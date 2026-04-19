const {onRequest} = require("firebase-functions/v2/https");
const {onDocumentWritten} = require("firebase-functions/v2/firestore");
const {onSchedule} = require("firebase-functions/v2/scheduler");
const admin = require("firebase-admin");

admin.initializeApp();

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

      const after  = event.data.after.data();
      const before = event.data.before?.data() ?? {};

      // Só atualiza Rankings se algum campo relevante mudou
      const relevantChanged =
          after.Score          !== before.Score          ||
          after.WeekScore      !== before.WeekScore      ||
          after.NickName       !== before.NickName       ||
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
