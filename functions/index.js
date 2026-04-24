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
