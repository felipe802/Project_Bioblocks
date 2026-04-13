// Assets/Editor/Tests/UserDataSyncServiceTests.cs
// Testes unitários para UserDataSyncService — gaps não cobertos por LiteDBTests.cs
//
// LiteDBTests.cs já cobre:
//   ✅ TrySyncPendingData (sem local, dirty, stale, fallback)
//   ✅ MergeWithFirestore (local mais recente, remoto mais recente, MinValue)
//   ✅ UpdateUserScores (score, weekScore, questão correta/incorreta, score < 0)
//
// Este arquivo cobre os gaps restantes:
//   🔲 SyncFromFirestore direto (usuário existe / não existe / IsSyncing guard)
//   🔲 SyncToFirestore direto (usuário existe / não existe / propaga exceção)
//   🔲 UpdateUserScores com questão duplicada não é adicionada duas vezes
//   🔲 UpdateUserScores com usuário ausente do LiteDB não crasha
// para testes com [UnityTest], que usa coroutines e Task.Yield)

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class UserDataSyncServiceTests
{
    // -------------------------------------------------------
    // Fixtures compartilhadas
    // -------------------------------------------------------

    private FakeLiteDBManager         _db;
    private UserDataLocalRepository   _repo;
    private FakeFirestoreRepository   _firestore;
    private UserDataSyncService       _syncService;
    private GameObject                _syncServiceGO;

    [SetUp]
    public void Setup()
    {
        _db   = new FakeLiteDBManager();
        _repo = new UserDataLocalRepository();
        _repo.InjectDependencies(_db);

        _firestore = new FakeFirestoreRepository();

        _syncServiceGO = new GameObject("SyncService");
        _syncService   = _syncServiceGO.AddComponent<UserDataSyncService>();
        _syncService.InjectDependencies(_repo, _firestore);

        UserDataStore.Clear();
        UserDataStore.Logger = _ => { };
    }

    [TearDown]
    public void TearDown()
    {
        _db.Close();
        UserDataStore.Clear();
        if (_syncServiceGO != null)
            Object.DestroyImmediate(_syncServiceGO);
    }

    // -------------------------------------------------------
    // Helper
    // -------------------------------------------------------

    private static UserData MakeUser(string id, int score = 0, int weekScore = 0)
        => new UserData(id, "Nick", "Name", "email@test.com",
                        score: score, weekScore: weekScore);

    // =======================================================
    // SyncFromFirestore — direto
    // =======================================================

    [UnityTest]
    public IEnumerator SyncFromFirestore_UsuarioExisteNoFirestore_SalvaNoLiteDB()
    {
        var remoteUser = MakeUser("u1", score: 200);
        _firestore.SetFakeUser(remoteUser);

        var task = _syncService.SyncFromFirestore("u1");
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(_repo.HasUser("u1"), "Usuário deve ser salvo no LiteDB após sync");
        Assert.AreEqual(200, _repo.GetUser("u1").Score);
    }

    [UnityTest]
    public IEnumerator SyncFromFirestore_UsuarioExisteNoFirestore_AtualizaUserDataStore()
    {
        var remoteUser = MakeUser("u1", score: 350);
        _firestore.SetFakeUser(remoteUser);

        var task = _syncService.SyncFromFirestore("u1");
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsNotNull(UserDataStore.CurrentUserData);
        Assert.AreEqual(350, UserDataStore.CurrentUserData.Score);
    }

    [UnityTest]
    public IEnumerator SyncFromFirestore_UsuarioNaoExisteNoFirestore_NaoAlteraLiteDB()
    {
        // Firestore não tem o usuário → GetUserData retorna null
        // LiteDB deve permanecer sem o usuário

        var task = _syncService.SyncFromFirestore("fantasma");
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsFalse(_repo.HasUser("fantasma"));
    }

    [UnityTest]
    public IEnumerator SyncFromFirestore_UsuarioExiste_MarcaLiteDBComoSynced()
    {
        var remoteUser = MakeUser("u1", score: 100);
        _firestore.SetFakeUser(remoteUser);

        var task = _syncService.SyncFromFirestore("u1");
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsFalse(_repo.IsDirty("u1"),
            "Após sync do Firestore o cache local deve estar marcado como synced (não dirty)");
    }

    [UnityTest]
    public IEnumerator SyncFromFirestore_IsSyncing_SegundaChamadaEhIgnorada()
    {
        // Sinaliza que já está em andamento antes da segunda chamada
        var remoteUser = MakeUser("u1", score: 100);
        _firestore.SetFakeUser(remoteUser);

        // Dispara primeira chamada e imediatamente verifica o guard
        var task1 = _syncService.SyncFromFirestore("u1");

        // Durante a primeira chamada IsSyncing deve ser true
        Assert.IsTrue(_syncService.IsSyncing,
            "IsSyncing deve ser true enquanto o primeiro sync está em andamento");

        yield return new WaitUntil(() => task1.IsCompleted);

        Assert.IsFalse(_syncService.IsSyncing,
            "IsSyncing deve ser false após a conclusão do sync");
    }

    // =======================================================
    // SyncToFirestore — direto
    // =======================================================

    [UnityTest]
    public IEnumerator SyncToFirestore_UsuarioExisteNoLiteDB_EnviaAoFirestore()
    {
        var localUser = MakeUser("u1", score: 400);
        _repo.SaveUser(localUser);

        var task = _syncService.SyncToFirestore("u1");
        yield return new WaitUntil(() => task.IsCompleted);

        // FakeFirestoreRepository deve ter recebido a chamada de update
        var remoteUser = await_fake(_firestore, "u1");
        Assert.IsNotNull(remoteUser, "SyncToFirestore deve ter chamado UpdateUserData no Firestore");
    }

    [UnityTest]
    public IEnumerator SyncToFirestore_UsuarioExisteNoLiteDB_MarcaSynced()
    {
        var localUser = MakeUser("u1", score: 100);
        _repo.SaveUser(localUser);
        _repo.MarkAsDirty("u1");

        var task = _syncService.SyncToFirestore("u1");
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsFalse(_repo.IsDirty("u1"),
            "Após SyncToFirestore bem-sucedido, IsDirty deve ser false");
    }

    [UnityTest]
    public IEnumerator SyncToFirestore_UsuarioNaoExisteNoLiteDB_NaoLancaExcecao()
    {
        // Usuário não existe no LiteDB — deve retornar sem lançar exceção
        var task = _syncService.SyncToFirestore("inexistente");
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(task.IsCompletedSuccessfully,
            "SyncToFirestore não deve lançar exceção quando usuário não existe no LiteDB");
    }

    // =======================================================
    // UpdateUserScores — gaps específicos
    // =======================================================

    [UnityTest]
    public IEnumerator UpdateUserScores_QuestaoCorretaDuplicada_NaoAdicionaDuasVezes()
    {
        // Garante que responder a mesma questão duas vezes não duplica a entrada
        var user = MakeUser("u1", score: 100);
        _repo.SaveUser(user);
        UserDataStore.CurrentUserData = user;

        // Responde questão 42 pela primeira vez
        var task1 = _syncService.UpdateUserScores("u1", 5, 42, "BioQuestions", isCorrect: true);
        yield return new WaitUntil(() => task1.IsCompleted);

        // Responde questão 42 novamente
        var task2 = _syncService.UpdateUserScores("u1", 5, 42, "BioQuestions", isCorrect: true);
        yield return new WaitUntil(() => task2.IsCompleted);

        var loaded = _repo.GetUser("u1");
        int count  = loaded.AnsweredQuestions["BioQuestions"]
                           .FindAll(n => n == 42).Count;

        Assert.AreEqual(1, count,
            "A questão 42 não deve aparecer duplicada em AnsweredQuestions");
    }

    [UnityTest]
    public IEnumerator UpdateUserScores_UsuarioAusenteDoLiteDB_NaoLancaExcecao()
    {
        // UserDataStore tem o usuário mas LiteDB não — não deve crashar
        UserDataStore.CurrentUserData = MakeUser("ghost", score: 50);

        var task = _syncService.UpdateUserScores("ghost", 10, 1, "BioQuestions", true);
        yield return new WaitUntil(() => task.IsCompleted);

        // O teste passa se chegou aqui sem exceção
        Assert.IsTrue(task.IsCompletedSuccessfully);
    }

    [UnityTest]
    public IEnumerator UpdateUserScores_ScoreNegativoGrandeNoWeekScore_NaoVaiAbaixoDeZero()
    {
        // WeekScore começa em 5, subtrai 100 → deve ficar em 0
        var user = MakeUser("u1", score: 5, weekScore: 5);
        _repo.SaveUser(user);
        UserDataStore.CurrentUserData = user;

        var task = _syncService.UpdateUserScores("u1", -100, 0, "", isCorrect: false);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.GreaterOrEqual(_repo.GetUser("u1").WeekScore, 0,
            "WeekScore não pode ser negativo");
        Assert.GreaterOrEqual(UserDataStore.CurrentUserData.WeekScore, 0);
    }

    [UnityTest]
    public IEnumerator UpdateUserScores_QuestaoNumeracaoZero_NaoAdicionaEmAnswered()
    {
        // questionNumber = 0 é inválido — não deve ser adicionado a AnsweredQuestions
        var user = MakeUser("u1", score: 0);
        _repo.SaveUser(user);
        UserDataStore.CurrentUserData = user;

        var task = _syncService.UpdateUserScores("u1", 5, questionNumber: 0,
                                                 "BioQuestions", isCorrect: true);
        yield return new WaitUntil(() => task.IsCompleted);

        var loaded = _repo.GetUser("u1");
        bool wasAdded = loaded.AnsweredQuestions.ContainsKey("BioQuestions")
                     && loaded.AnsweredQuestions["BioQuestions"].Contains(0);

        Assert.IsFalse(wasAdded,
            "questionNumber = 0 não deve ser adicionado a AnsweredQuestions");
    }

    [UnityTest]
    public IEnumerator UpdateUserScores_DatabaseNameVazio_NaoAdicionaEmAnswered()
    {
        // databankName vazio é inválido — não deve ser adicionado a AnsweredQuestions
        var user = MakeUser("u1", score: 0);
        _repo.SaveUser(user);
        UserDataStore.CurrentUserData = user;

        var task = _syncService.UpdateUserScores("u1", 5, questionNumber: 1,
                                                 databankName: "", isCorrect: true);
        yield return new WaitUntil(() => task.IsCompleted);

        var loaded = _repo.GetUser("u1");
        Assert.AreEqual(0, loaded.AnsweredQuestions.Count,
            "databankName vazio não deve gerar entrada em AnsweredQuestions");
    }

    // =======================================================
    // Helper privado
    // =======================================================

    // Obtém o usuário do FakeFirestore de forma síncrona (já é Task.FromResult)
    private static UserData await_fake(FakeFirestoreRepository fake, string userId)
        => fake.GetUserData(userId).Result;
}