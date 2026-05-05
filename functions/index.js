const {onRequest} = require("firebase-functions/v2/https");
const {onDocumentWritten} = require("firebase-functions/v2/firestore");
const {onSchedule} = require("firebase-functions/v2/scheduler");
const admin = require("firebase-admin");
const {FieldValue} = require("firebase-admin/firestore");
const {defineSecret} = require("firebase-functions/params");

const resetSecretKey = defineSecret("RESET_SECRET_KEY");

admin.initializeApp();

// ─────────────────────────────────────────────────────────────
// Caminho canônico das estatísticas de questões
// ─────────────────────────────────────────────────────────────
const QUESTION_STATS_COLLECTION = "Config";
const QUESTION_STATS_DOC_ID = "QuestionStats";
const QUESTIONS_COLLECTION = "Questions";
const DATABANK_FIELD = "questionDatabankName";

// ─────────────────────────────────────────────────────────────
// TRIGGER: Sincroniza Rankings sempre que um Users/{uid} muda
// ─────────────────────────────────────────────────────────────
exports.syncRankingOnUserWrite = onDocumentWritten(
    "Users/{userId}",
    async (event) => {
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
// ─────────────────────────────────────────────────────────────
exports.resetWeeklyScores = onSchedule(
    {
      schedule: "0 3 * * 1",
      timeZone: "UTC",
      timeoutSeconds: 300,
    },
    async () => {
      const db = admin.firestore();
      const BATCH_MAX = 450;

      const usersSnap = await db.collection("Users").get();
      await _batchUpdate(db, usersSnap.docs, {WeekScore: 0}, BATCH_MAX);

      const rankSnap = await db.collection("Rankings").get();
      await _batchUpdate(db, rankSnap.docs, {weekScore: 0}, BATCH_MAX);

      console.log(
          `[resetWeekly] WeekScore zerado — ${usersSnap.size} usuários, ${rankSnap.size} rankings.`,
      );
    },
);

// ─────────────────────────────────────────────────────────────
// HTTP: Mantido como fallback manual
// ─────────────────────────────────────────────────────────────
exports.resetWeeklyScoresManual = onRequest(
    {secrets: [resetSecretKey]},
    async (req, res) => {
      const secretKey = resetSecretKey.value();

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
    },
);

// ─────────────────────────────────────────────────────────────
// TRIGGER: Mantém Config/QuestionStats em sincronia com Questions
// ─────────────────────────────────────────────────────────────
exports.syncQuestionStats = onDocumentWritten(
    `${QUESTIONS_COLLECTION}/{questionId}`,
    async (event) => {
      const before = event.data.before?.exists ? event.data.before.data() : null;
      const after = event.data.after?.exists ? event.data.after.data() : null;

      const beforeBank = before?.[DATABANK_FIELD] ?? null;
      const afterBank = after?.[DATABANK_FIELD] ?? null;

      let totalDelta = 0;
      const perBankDelta = {};

      if (!before && after) {
        totalDelta = 1;
        if (afterBank) perBankDelta[afterBank] = 1;
      } else if (before && !after) {
        totalDelta = -1;
        if (beforeBank) perBankDelta[beforeBank] = -1;
      } else if (before && after) {
        if (beforeBank === afterBank) {
          console.log(`[syncQuestionStats] Sem mudança de banco em ${event.params.questionId} — ignorando.`);
          return;
        }
        if (beforeBank) perBankDelta[beforeBank] = (perBankDelta[beforeBank] || 0) - 1;
        if (afterBank) perBankDelta[afterBank] = (perBankDelta[afterBank] || 0) + 1;
      } else {
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

      await ref.set(updatePayload, {merge: true});

      console.log(
          `[syncQuestionStats] question=${event.params.questionId} totalDelta=${totalDelta} ` +
          `perBankDelta=${JSON.stringify(perBankDelta)}`,
      );
    },
);

// ─────────────────────────────────────────────────────────────
// HTTP: rebuild completo de Config/QuestionStats
// ─────────────────────────────────────────────────────────────
exports.rebuildQuestionStats = onRequest(
    {secrets: [resetSecretKey]},
    async (req, res) => {
      const secretKey = resetSecretKey.value();

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
    },
);

// ─────────────────────────────────────────────────────────────
// HTTP: auditoria e correção de PlayerLevel/Score inflados
// ─────────────────────────────────────────────────────────────
exports.auditPlayerLevels = onRequest(
    {timeoutSeconds: 540, memory: "512MiB", secrets: [resetSecretKey]},
    async (req, res) => {
      const secretKey = resetSecretKey.value();

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
// Espelho server-side de PlayerLevelConfig
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

// ─────────────────────────────────────────────────────────────
// HTTP: Bulk upload de questões dos bancos de dados para Firestore
// ─────────────────────────────────────────────────────────────
exports.uploadQuestionBanks = onRequest(
    {timeoutSeconds: 540, memory: "512MiB", secrets: [resetSecretKey]},
    async (req, res) => {
      const secretKey = resetSecretKey.value();

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
        const payload = req.body;

        if (!payload.questionBanks || !Array.isArray(payload.questionBanks)) {
          res.status(400).send(
              "Body inválido. Esperado: { questionBanks: [...] }",
          );
          return;
        }

        const report = {
          mode: "UPLOAD",
          totalBanks: payload.questionBanks.length,
          totalQuestionsProcessed: 0,
          totalQuestionsWritten: 0,
          bankReports: [],
          errors: [],
        };

        const BATCH_MAX = 450;
        let batch = db.batch();
        let batchCount = 0;
        const commitBatch = async () => {
          if (batchCount === 0) return;
          await batch.commit();
          batch = db.batch();
          batchCount = 0;
        };

        for (const bank of payload.questionBanks) {
          const bankName = bank.bankName || "unknown";
          const questions = bank.questions || [];

          const bankReport = {
            bankName,
            totalQuestions: questions.length,
            writtenQuestions: 0,
            skippedQuestions: 0,
            failedQuestions: 0,
            failedIds: [],
          };

          for (const q of questions) {
            try {
              report.totalQuestionsProcessed++;

              if (!q.globalId) {
                throw new Error("globalId obrigatório");
              }

              const firestoreDoc = {
                globalId: q.globalId,
                questionDatabankName: q.questionDatabankName || "",
                questionNumber: q.questionNumber || 0,
                questionText: q.questionText || "",
                answers: q.answers || [],
                correctIndex: q.correctIndex || 0,
                isImageQuestion: q.isImageQuestion || false,
                isImageAnswer: q.isImageAnswer || false,
                questionImagePath: q.questionImagePath || "",
                questionLevel: q.questionLevel || 1,
                topic: q.topic || "",
                subtopic: q.subtopic || null,
                displayName: q.displayName || "",
                bloomLevel: q.bloomLevel || "unclassified",
                conceptTags: Array.isArray(q.conceptTags) ? q.conceptTags : [],
                prerequisites: Array.isArray(q.prerequisites) ? q.prerequisites : [],
                questionHint: {
                  imagePath: (q.questionHint?.imagePath) || "",
                  link: (q.questionHint?.link) || "",
                  text: (q.questionHint?.text) || "",
                  videoUrl: (q.questionHint?.videoUrl) || "",
                },
                questionInDevelopment: q.questionInDevelopment || false,
                UploadedAt: FieldValue.serverTimestamp(),
              };

              const ref = db.collection(QUESTIONS_COLLECTION).doc(q.globalId);
              batch.set(ref, firestoreDoc, {merge: false});
              batchCount++;
              bankReport.writtenQuestions++;
              report.totalQuestionsWritten++;

              if (batchCount >= BATCH_MAX) {
                await commitBatch();
              }
            } catch (err) {
              bankReport.failedQuestions++;
              bankReport.failedIds.push({
                globalId: q.globalId || "unknown",
                error: err.message,
              });
              report.errors.push({
                bankName,
                globalId: q.globalId || "unknown",
                error: err.message,
              });
            }
          }

          report.bankReports.push(bankReport);
        }

        await commitBatch();

        console.log("[uploadQuestionBanks] Reconstruindo Config/QuestionStats...");
        const snap = await db.collection(QUESTIONS_COLLECTION).get();
        const perBank = {};
        let total = 0;
        for (const doc of snap.docs) {
          const bank = doc.get("questionDatabankName");
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

        report.statsRebuilt = {
          totalQuestions: total,
          banksCount: Object.keys(perBank).length,
          perBank,
        };

        console.log(
            `[uploadQuestionBanks] Upload completo — ` +
            `${report.totalQuestionsWritten}/${report.totalQuestionsProcessed} questões escritas, ` +
            `${report.errors.length} erros.`,
        );
        res.status(200).json(report);
      } catch (err) {
        console.error("[uploadQuestionBanks] Erro:", err);
        res.status(500).json({error: err.message, stack: err.stack});
      }
    },
);
