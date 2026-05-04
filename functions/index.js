const {onRequest} = require("firebase-functions/v2/https");
const {onDocumentWritten} = require("firebase-functions/v2/firestore");
const {onSchedule} = require("firebase-functions/v2/scheduler");
const admin = require("firebase-admin");
const {FieldValue} = require("firebase-admin/firestore");

admin.initializeApp();

// ─────────────────────────────────────────────────────────────
// Caminho canônico das estatísticas de questões
// Lido pelo cliente (DatabaseStatisticsManager → FirestoreRepository.GetQuestionStats)
// ESCRITO APENAS pelas Cloud Functions abaixo.
// ─────────────────────────────────────────────────────────────
const QUESTION_STATS_COLLECTION = "Config";
const QUESTION_STATS_DOC_ID = "QuestionStats";
const QUESTIONS_COLLECTION = "Questions";
const DATABANK_FIELD = "questionDatabankName";

// ─────────────────────────────────────────────────────────────
// TRIGGER: Sincroniza Rankings sempre que um Users/{uid} muda
//
// Responsabilidade: manter Rankings com os campos
// MÍNIMOS e PÚBLICOS necessários para exibir o ranking.
// Dados sensíveis (Email, AnsweredQuestions, Name) nunca
// são copiados para esta coleção.
// ─────────────────────────────────────────────────────────────
exports.syncRankingOnUserWrite = onDocumentWritten(
    "Users/{userId}",
    async (event) => {
      // Documento deletado → remove entrada do ranking
      if (!event.data.after.exists) {
        const beforeData = event.data.before.data();
        const nickName = beforeData?.NickName ?? "";
        if (!nickName) return;

        await admin.firestore()
            .collection("Rankings")
            .doc(nickName.toLowerCase())
            .delete();

        console.log(`[syncRanking] Entrada removida para nickName: ${nickName}`);
        return;
      }

      const after = event.data.after.data();
      const before = event.data.before?.data() ?? {};

      // Só atualiza Rankings se algum campo relevante mudou
      const relevantChanged =
          after.Score !== before.Score ||
          after.WeekScore !== before.WeekScore ||
          after.NickName !== before.NickName ||
          after.ProfileImageUrl !== before.ProfileImageUrl;

      if (!relevantChanged) {
        console.log("[syncRanking] Nenhum campo relevante alterado — ignorando.");
        return;
      }

      const nickName = after.NickName ?? "";
      if (!nickName) return;

      const rankingEntry = {
        nickName: after.NickName ?? "",
        score: after.Score ?? 0,
        weekScore: after.WeekScore ?? 0,
        profileImageUrl: after.ProfileImageUrl ?? "",
      };

      await admin.firestore()
          .collection("Rankings")
          .doc(nickName.toLowerCase())
          .set(rankingEntry, {merge: true});

      console.log(`[syncRanking] Rankings/${nickName} atualizado — score: ${rankingEntry.score}`);
    },
);

// ─────────────────────────────────────────────────────────────
// SCHEDULE: Reseta WeekScore toda segunda-feira às 00:00 (Brasília)
// Tlmente automático.
// ─────────────────────────────────────────────────────────────
exports.resetWeeklyScores = onSchedule(
    {
      schedule: "0 3 * * 1", // 03:00 UTC = 00:00 Brasília (segunda)
      timeZone: "UTC",
      timeoutSeconds: 300,
    },
    async () => {
      const db = admin.firestore();
      const BATCH_MAX = 450; // limite seguro por batch

      // Reseta Users
      const usersSnap = await db.collection("Users").get();
      await _batchUpdate(db, usersSnap.docs, {WeekScore: 0}, BATCH_MAX);

      // Reseta Rankings (espelho)
      const rankSnap = await db.collection("Rankings").get();
      await _batchUpdate(db, rankSnap.docs, {weekScore: 0}, BATCH_MAX);

      console.log(
          `[resetWeekly] WeekScore zerado — ${usersSnap.size} usuários, ${rankSnap.size} rankings.`,
      );
    },
);

// ─────────────────────────────────────────────────────────────
// HTTP: Mantido como fallback manual (se quiser usar a// Te secreta)
// ─────────────────────────────────────────────────────────────
exports.resetWeeklyScoresManual = onRequest(async (req, res) => {
  const secretKey = process.env.RESET_SECRET_KEY;

  if (!secretKey) {
    res.status(500).send("RESET_SECRET_KEY não configurada.");
    return;
  }
  if (req.query.key !== secretKey) {
    res.status(403).send("Acesso não autorizado.");
    return;
  }

  try {
    const db = admin.firestore();
    const BATCH_MAX = 450;

    const [usersSnap, rankSnap] = await Promise.all([
      db.collection("Users").get(),
      db.collection("Rankings").get(),
    ]);

    await Promise.all([
      _batchUpdate(db, usersSnap.docs, {WeekScore: 0}, BATCH_MAX),
      _batchUpdate(db, rankSnap.docs, {weekScore: 0}, BATCH_MAX),
    ]);

    const msg = `Reset manual concluído — ${usersSnap.size} usuários.`;
    console.log(msg);
    res.status(200).send(msg);
  } catch (err) {
    console.error("[resetManual] Erro:", err);
    res.status(500).send(`Erro: ${err.message}`);
  }
});

// ─────────────────────────────────────────────────────────────
// TRIGGER: Mantém Config/QuestionStats em sincronia com a coleção Questions.
//
// Fonte única de verdade do TOTAL de questões do app. Consumido pelo cliente
// em DatabaseStatisticsManager (via FirestoreRepository.GetQuestionStats) e
// usado pelo PlayerLevelService como denominador nos cálculos de nível.
//
// Racional: contar questões localmente no cliente (como antes) permitia que um
// dispositivo com LiteDB defasado reportasse um TOTAL menor que o real e, ao
// gravar esse valor em UserData.TotalQuestionsInAllDatabanks, inflacionasse a
// porcentagem de progresso do jogador — causando o bug que pulou jogadores
// direto para o nível máximo.
//
// Incrementa/decrementa atômica via FieldValue.increment — seguro sob escritas
// concorrentes. Também incrementa Version para invalidar caches no cliente.
// ─────────────────────────────────────────────────────────────
exports.syncQuestionStats = onDocumentWritten(
    `${QUESTIONS_COLLECTION}/{questionId}`,
    async (event) => {
      const before = event.data.before?.exists ? event.data.before.data() : null;
      const after = event.data.after?.exists ? event.data.after.data() : null;

      const beforeBank = before?.[DATABANK_FIELD] ?? null;
      const afterBank = after?.[DATABANK_FIELD] ?? null;

      // Determina o delta aplicado ao total e a cada banco
      let totalDelta = 0;
      const perBankDelta = {};

      if (!before && after) {
        // Criação
        totalDelta = 1;
        if (afterBank) perBankDelta[afterBank] = 1;
      } else if (before && !after) {
        // Deleção
        totalDelta = -1;
        if (beforeBank) perBankDelta[beforeBank] = -1;
      } else if (before && after) {
        // Atualização — só mexe no stats se o banco mudou
        if (beforeBank === afterBank) {
          console.log(`[syncQuestionStats] Sem mudança de banco em ${event.params.questionId} — ignorando.`);
          return;
        }
        if (beforeBank) perBankDelta[beforeBank] = (perBankDelta[beforeBank] || 0) - 1;
        if (afterBank) perBankDelta[afterBank] = (perBankDelta[afterBank] || 0) + 1;
      } else {
        // Sem before nem after — não deveria acontecer
        return;
      }

      const updatePayload = {
        Version: FieldValue.increment(1),
        UpdatedAt: FieldValue.serverTimestamp(),
      };

      if (totalDelta !== 0) {
        updatePayload.TotalQuestions = FieldValue.increment(totalDelta);
      }
      for (const [bank, delta] of Object.entries(perBankDelta)) {
        if (delta === 0) continue;
        updatePayload[`PerBank.${bank}`] = FieldValue.increment(delta);
      }

      const ref = admin.firestore()
          .collection(QUESTION_STATS_COLLECTION)
          .doc(QUESTION_STATS_DOC_ID);

      // Garante existência do doc antes do increment (increment falha em doc ausente
      // para alguns clientes, mas admin SDK aceita; ainda assim, {merge:true} é seguro).
      await ref.set(updatePayload, {merge: true});

      console.log(
          `[syncQuestionStats] question=${event.params.questionId} totalDelta=${totalDelta} ` +
          `perBankDelta=${JSON.stringify(perBankDelta)}`,
      );
    },
);

// ─────────────────────────────────────────────────────────────
// HTTP: rebuild completo de Config/QuestionStats varrendo toda a
// coleção Questions. Útil para:
//   - Seed inicial do documento.
//   - Recuperação após qualquer drift (corrige Total e PerBank).
//
// Protegido por chave secreta (env RESET_SECRET_KEY) — mesma usada no
// reset manual de WeekScore para evitar multiplicação de segredos.
// ─────────────────────────────────────────────────────────────
exports.rebuildQuestionStats = onRequest(async (req, res) => {
  const secretKey = process.env.RESET_SECRET_KEY;

  if (!secretKey) {
    res.status(500).send("RESET_SECRET_KEY não configurada.");
    return;
  }
  if (req.query.key !== secretKey) {
    res.status(403).send("Acesso não autorizado.");
    return;
  }

  try {
    const db = admin.firestore();
    const snap = await db.collection(QUESTIONS_COLLECTION).get();

    const perBank = {};
    let total = 0;
    for (const doc of snap.docs) {
      const bank = doc.get(DATABANK_FIELD);
      total += 1;
      if (bank) perBank[bank] = (perBank[bank] || 0) + 1;
    }

    await db.collection(QUESTION_STATS_COLLECTION)
        .doc(QUESTION_STATS_DOC_ID)
        .set({
          TotalQuestions: total,
          PerBank: perBank,
          Version: FieldValue.increment(1),
          UpdatedAt: FieldValue.serverTimestamp(),
        }, {merge: true});

    const msg = `[rebuildQuestionStats] Total=${total}, Bancos=${Object.keys(perBank).length}`;
    console.log(msg);
    res.status(200).send(msg);
  } catch (err) {
    console.error("[rebuildQuestionStats] Erro:", err);
    res.status(500).send(`Erro: ${err.message}`);
  }
});

// ─────────────────────────────────────────────────────────────
// HTTP: auditoria e correção de PlayerLevel/Score inflados pelo bug
// antigo (old client → CalculateLevel fallthrough → jump para nível 10).
//
// Modo default (sem ?apply): DRY-RUN. Só lê, retorna relatório completo.
// Modo ?apply=true: aplica as correções em batches.
//
// Política (acordada com o time):
//   1. Denominador de referência = Config/QuestionStats.TotalQuestions
//      (fonte única de verdade server-side). Aceita rebaixar usuários
//      que chegaram ao nível atual com totais menores no passado.
//   2. Nível correto = CalculateLevel(answered, realTotal), onde
//      answered = sum( |set(AnsweredQuestions[bank])| para cada banco ).
//   3. Bônus excedente = soma de GetBonusForLevel(i) para i de
//      correctLevel+1 até storedLevel, inclusive — replica EXATAMENTE o
//      caminho antigo (commit bc39007^) que premiava em loop.
//   4. Para TODOS os usuários: TotalQuestionsInAllDatabanks e
//      LevelSnapshotDenominator são sobrescritos com realTotal, fechando
//      o vetor de stale-cache de clientes antigos.
//   5. Score/WeekScore clampados em 0 após o desconto.
//
// Autenticação por RESET_SECRET_KEY (mesma chave dos demais endpoints
// administrativos — evita multiplicação de segredos).
// Query params:
//   ?key=<RESET_SECRET_KEY>   obrigatório
//   ?apply=true               aplica fixes (default = dry-run)
//   ?limit=<N>                limita quantos usuários escanear (útil p/ teste)
// ─────────────────────────────────────────────────────────────
exports.auditPlayerLevels = onRequest(
    {timeoutSeconds: 540, memory: "512MiB"},
    async (req, res) => {
      const secretKey = process.env.RESET_SECRET_KEY;

      if (!secretKey) {
        res.status(500).send("RESET_SECRET_KEY não configurada.");
        return;
      }
      if (req.query.key !== secretKey) {
        res.status(403).send("Acesso não autorizado.");
        return;
      }

      const apply = req.query.apply === "true";
      const limit = req.query.limit ? parseInt(req.query.limit, 10) : 0;

      try {
        const db = admin.firestore();

        // 1. Lê fonte de verdade do total de questões
        const statsDoc = await db.collection(QUESTION_STATS_COLLECTION)
            .doc(QUESTION_STATS_DOC_ID).get();
        if (!statsDoc.exists) {
          res.status(500).send(
              "Config/QuestionStats não existe. Rode rebuildQuestionStats antes.",
          );
          return;
        }
        const realTotal = statsDoc.get("TotalQuestions");
        if (!realTotal || realTotal <= 0) {
          res.status(500).send(
              `Config/QuestionStats.TotalQuestions inválido: ${realTotal}`,
          );
          return;
        }

        // 2. Varre Users
        let query = db.collection("Users");
        if (limit > 0) query = query.limit(limit);
        const usersSnap = await query.get();

        const report = {
          mode: apply ? "APPLY" : "DRY_RUN",
          realTotalQuestions: realTotal,
          usersScanned: 0,
          usersInflated: 0,
          usersUnchanged: 0,
          cacheRefreshedOnly: 0,
          totalExcessBonusFound: 0,
          totalScoreReclaimed: 0,
          anomalies: [],
          inflatedUsers: [],
        };

        const BATCH_MAX = 400;
        let batch = db.batch();
        let batchCount = 0;
        const commitBatch = async () => {
          if (batchCount === 0) return;
          await batch.commit();
          batch = db.batch();
          batchCount = 0;
        };

        for (const doc of usersSnap.docs) {
          report.usersScanned++;
          const data = doc.data();
          const storedLevel = Number(data.PlayerLevel) || 1;
          const storedScore = Number(data.Score) || 0;
          const storedWeekScore = Number(data.WeekScore) || 0;
          const storedTotalInAll = Number(data.TotalQuestionsInAllDatabanks) || 0;
          const storedSnapshot = Number(data.LevelSnapshotDenominator) || 0;

          const answered = _countUniqueAnswered(data.AnsweredQuestions);
          const correctLevel = _calculateLevel(answered, realTotal);

          const updates = {};

          if (storedLevel > correctLevel) {
            // INFLADO — corrige PlayerLevel, Score e WeekScore.
            let excessBonus = 0;
            for (let lvl = correctLevel + 1; lvl <= storedLevel; lvl++) {
              excessBonus += _getBonusForLevel(lvl);
            }

            const newScore = Math.max(0, storedScore - excessBonus);
            const newWeekScore = Math.max(0, storedWeekScore - excessBonus);
            const actualReclaimed = storedScore - newScore;

            const entry = {
              userId: doc.id,
              nickName: data.NickName || "",
              answered,
              storedLevel,
              correctLevel,
              levelsRemoved: storedLevel - correctLevel,
              excessBonus,
              storedScore,
              newScore,
              scoreDelta: -actualReclaimed,
              storedWeekScore,
              newWeekScore,
              storedSnapshot,
              storedTotalInAll,
            };

            if (storedScore < excessBonus) {
              entry.scoreUnderflowed = true;
              report.anomalies.push({
                userId: doc.id,
                nickName: data.NickName || "",
                issue: "Score < excessBonus — clampado em 0",
                storedScore,
                excessBonus,
                lostDelta: excessBonus - storedScore,
              });
            }

            report.inflatedUsers.push(entry);
            report.usersInflated++;
            report.totalExcessBonusFound += excessBonus;
            report.totalScoreReclaimed += actualReclaimed;

            updates.PlayerLevel = correctLevel;
            updates.Score = newScore;
            updates.WeekScore = newWeekScore;
            updates.TotalQuestionsInAllDatabanks = realTotal;
            updates.LevelSnapshotDenominator = realTotal;
          } else {
            // Não inflado — só refresca os dois caches p/ fechar o vetor.
            report.usersUnchanged++;

            if (storedTotalInAll !== realTotal) {
              updates.TotalQuestionsInAllDatabanks = realTotal;
            }
            if (storedSnapshot !== realTotal) {
              updates.LevelSnapshotDenominator = realTotal;
            }

            if (Object.keys(updates).length > 0) {
              report.cacheRefreshedOnly++;
            }
          }

          if (apply && Object.keys(updates).length > 0) {
            batch.update(doc.ref, updates);
            batchCount++;
            if (batchCount >= BATCH_MAX) {
              await commitBatch();
            }
          }
        }

        if (apply) await commitBatch();

        console.log(
            `[auditPlayerLevels] mode=${report.mode} ` +
            `scanned=${report.usersScanned} inflated=${report.usersInflated} ` +
            `cacheOnly=${report.cacheRefreshedOnly} ` +
            `reclaimed=${report.totalScoreReclaimed}`,
        );
        res.status(200).json(report);
      } catch (err) {
        console.error("[auditPlayerLevels] Erro:", err);
        res.status(500).json({error: err.message, stack: err.stack});
      }
    },
);

// ─────────────────────────────────────────────────────────────
// Espelho server-side de PlayerLevelConfig (thresholds, bônus).
// Mantido sincronizado MANUALMENTE — se a tabela mudar em C#, atualize
// aqui também. Os testes unitários em C# continuam sendo a fonte de
// verdade; esta cópia serve apenas para a auditoria offline.
// ─────────────────────────────────────────────────────────────
const LEVEL_THRESHOLDS = [
  {level: 1, min: 0.00, max: 0.10, bonus: 1000},
  {level: 2, min: 0.10, max: 0.20, bonus: 2000},
  {level: 3, min: 0.20, max: 0.30, bonus: 3000},
  {level: 4, min: 0.30, max: 0.40, bonus: 4000},
  {level: 5, min: 0.40, max: 0.50, bonus: 5000},
  {level: 6, min: 0.50, max: 0.60, bonus: 6000},
  {level: 7, min: 0.60, max: 0.70, bonus: 7000},
  {level: 8, min: 0.70, max: 0.80, bonus: 8000},
  {level: 9, min: 0.80, max: 0.90, bonus: 9000},
  {level: 10, min: 0.90, max: 1.00, bonus: 10000},
];
const MAX_LEVEL = 10;

function _calculateLevel(answered, totalQuestions) {
  if (totalQuestions <= 0) return 1;
  if (answered <= 0) return 1;
  // JS numbers são float64 — sem o drift do float32 C#. Cálculo direto é seguro.
  const pct = Math.min(1.0, answered / totalQuestions);
  if (pct >= 1.0) return MAX_LEVEL;
  for (const t of LEVEL_THRESHOLDS) {
    if (pct >= t.min && pct < t.max) return t.level;
  }
  return 1;
}

function _getBonusForLevel(level) {
  const t = LEVEL_THRESHOLDS.find((x) => x.level === level);
  return t ? t.bonus : 0;
}

function _countUniqueAnswered(answeredQuestionsMap) {
  if (!answeredQuestionsMap || typeof answeredQuestionsMap !== "object") {
    return 0;
  }
  let total = 0;
  for (const list of Object.values(answeredQuestionsMap)) {
    if (!Array.isArray(list)) continue;
    total += new Set(list).size;
  }
  return total;
}

// ─────────────────────────────────────────────────────────────
// Helper: executa batch updates em lotes de tamanho seguro
// ─────────────────────────────────────────────────────────────
async function _batchUpdate(db, docs, fields, batchMax) {
  let batch = db.batch();
  let count = 0;

  for (const doc of docs) {
    batch.update(doc.ref, fields);
    count++;

    if (count >= batchMax) {
      await batch.commit();
      batch = db.batch();
      count = 0;
    }
  }

  if (count > 0) {
    await batch.commit();
  }
}
