// Assets/Editor/Tests/ScoreManagerTests.cs
// Testes unitários para QuestionScoreManager via AppContext.OverrideForTests().

// (requer PlayMode pois UpdateScore usa async/await com Task.Yield)

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using QuestionSystem;

[TestFixture]
public class QuestionScoreManagerTests
{
    // -------------------------------------------------------
    // Fixtures
    // -------------------------------------------------------

    private GameObject           _managerGO;
    private QuestionScoreManager _scoreManager;

    private FakeAuthRepository       _auth;
    private FakeFirestoreRepository  _firestore;
    private FakeLiteDBManager        _db;
    private UserDataLocalRepository  _localRepo;
    private UserDataSyncService      _syncService;
    private FakePlayerLevelService   _playerLevel;
    private GameObject               _syncServiceGO;
    private GameObject               _localRepoGO;

    private const string USER_ID = "test-user-01";

    [SetUp]
    public void Setup()
    {
        // 1. Fakes de infraestrutura
        _auth        = new FakeAuthRepository();
        _firestore   = new FakeFirestoreRepository();
        _db          = new FakeLiteDBManager();
        _playerLevel = new FakePlayerLevelService();

        // 2. Repositório local + SyncService (MonoBehaviours)
        _localRepoGO = new GameObject("UserDataLocalRepo");
        _localRepo   = _localRepoGO.AddComponent<UserDataLocalRepository>();
        _localRepo.InjectDependencies(_db);

        _syncServiceGO = new GameObject("SyncService");
        _syncService   = _syncServiceGO.AddComponent<UserDataSyncService>();
        _syncService.InjectDependencies(_localRepo, _firestore);

        // 3. Injeta tudo no AppContext via OverrideForTests (sem Firebase real)
        AppContext.OverrideForTests(
            auth:          _auth,
            firestore:     _firestore,
            localDatabase: _db,
            userDataLocal: _localRepo,
            userDataSync:  _syncService,
            playerLevel:   _playerLevel
        );

        // 4. Usuário logado com dados no LiteDB e no UserDataStore
        _auth.SetLoggedInUser(USER_ID);
        var user = new UserData(USER_ID, "Tester", "Tester", "t@test.com",
                                score: 0, weekScore: 0);
        _localRepo.SaveUser(user);
        UserDataStore.CurrentUserData = user;
        UserDataStore.Logger = _ => { };

        // 5. QuestionScoreManager
        _managerGO    = new GameObject("QuestionScoreManager");
        _scoreManager = _managerGO.AddComponent<QuestionScoreManager>();
    }

    [TearDown]
    public void Teardown()
    {
        _db.Close();
        UserDataStore.Clear();
        Object.DestroyImmediate(_managerGO);
        Object.DestroyImmediate(_syncServiceGO);
        Object.DestroyImmediate(_localRepoGO);
    }

    // -------------------------------------------------------
    // Helper: cria uma Question mínima válida
    // -------------------------------------------------------

    private static Question MakeQuestion(int number, string dbName = "BioQuestions")
        => new Question
        {
            questionNumber        = number,
            questionDatabankName  = dbName,
            questionLevel         = 1,
            questionInDevelopment = false
        };

    // =======================================================
    // Equivalente a: AddScore(10) → GetScore() == 10
    // =======================================================

    [UnityTest]
    public IEnumerator UpdateScore_Correto_AtualizaScoreNoStore()
    {
        var task = _scoreManager.UpdateScore(
            scoreChange:      10,
            isCorrect:        true,
            answeredQuestion: MakeQuestion(1));

        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(10, UserDataStore.CurrentUserData.Score,
            "Score deve ser 10 após resposta correta com +10");
    }

    [UnityTest]
    public IEnumerator UpdateScore_Correto_AtualizaScoreNoLiteDB()
    {
        var task = _scoreManager.UpdateScore(10, true, MakeQuestion(1));
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(10, _localRepo.GetUser(USER_ID).Score);
    }

    // =======================================================
    // Equivalente a: AddScore(5) + AddScore(15) → GetScore() == 20
    // =======================================================

    [UnityTest]
    public IEnumerator UpdateScore_MultiplasChamadas_AcumulaScore()
    {
        var task1 = _scoreManager.UpdateScore(5, true, MakeQuestion(1));
        yield return new WaitUntil(() => task1.IsCompleted);

        var task2 = _scoreManager.UpdateScore(15, true, MakeQuestion(2));
        yield return new WaitUntil(() => task2.IsCompleted);

        Assert.AreEqual(20, UserDataStore.CurrentUserData.Score,
            "Score acumulado de duas respostas corretas deve ser 20");
    }

    // =======================================================
    // Score negativo (penalidade por resposta errada)
    // =======================================================

    [UnityTest]
    public IEnumerator UpdateScore_Incorreto_SubtraiScore()
    {
        // Parte de score 10 para ter margem de subtração
        var user = _localRepo.GetUser(USER_ID);
        user.Score = 10;
        _localRepo.UpdateScore(USER_ID, 10, 10);
        UserDataStore.CurrentUserData = user;

        var task = _scoreManager.UpdateScore(-5, false, MakeQuestion(1));
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(5, UserDataStore.CurrentUserData.Score);
    }

    [UnityTest]
    public IEnumerator UpdateScore_Incorreto_ScoreNaoVaiAbaixoDeZero()
    {
        // Score começa em 0, subtrai 10 → deve ficar em 0
        var task = _scoreManager.UpdateScore(-10, false, MakeQuestion(1));
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.GreaterOrEqual(UserDataStore.CurrentUserData.Score, 0,
            "Score não pode ser negativo");
    }

    // =======================================================
    // Questão respondida corretamente é registrada
    // =======================================================

    [UnityTest]
    public IEnumerator UpdateScore_Correto_RegistraQuestaoRespondida()
    {
        var task = _scoreManager.UpdateScore(10, true, MakeQuestion(42, "BioQuestions"));
        yield return new WaitUntil(() => task.IsCompleted);

        var user = _localRepo.GetUser(USER_ID);
        Assert.IsTrue(
            user.AnsweredQuestions.ContainsKey("BioQuestions") &&
            user.AnsweredQuestions["BioQuestions"].Contains(42),
            "Questão 42 deve estar em AnsweredQuestions após resposta correta");
    }

    [UnityTest]
    public IEnumerator UpdateScore_Incorreto_NaoRegistraQuestaoRespondida()
    {
        var task = _scoreManager.UpdateScore(-5, false, MakeQuestion(42, "BioQuestions"));
        yield return new WaitUntil(() => task.IsCompleted);

        var user    = _localRepo.GetUser(USER_ID);
        bool wasAdded = user.AnsweredQuestions.ContainsKey("BioQuestions") &&
                        user.AnsweredQuestions["BioQuestions"].Contains(42);

        Assert.IsFalse(wasAdded,
            "Questão respondida incorretamente não deve ser registrada");
    }

    // =======================================================
    // Modo desenvolvimento — não salva no Firebase
    // =======================================================

    [UnityTest]
    public IEnumerator UpdateScore_ModoDev_NaoDisparaUpdateNoFirestore()
    {
        var devDB        = new FakeQuestionDatabase { IsInDevelopmentMode = true };
        var fakeAnswered = new FakeAnsweredQuestionsManager();
        AppContext.OverrideForTests(answeredQuestions: fakeAnswered);

        int callsAntes = _firestore.UpdateUserScoresCallCount;

        var task = _scoreManager.UpdateScore(10, true, MakeQuestion(1), database: devDB);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(callsAntes, _firestore.UpdateUserScoresCallCount,
            "Modo desenvolvimento não deve chamar UpdateUserScores no Firestore");
    }

    // =======================================================
    // Usuário não autenticado
    // =======================================================

    [UnityTest]
    public IEnumerator UpdateScore_UsuarioNaoAutenticado_NaoLancaExcecao()
    {
        _auth.SetLoggedOut();

        var task = _scoreManager.UpdateScore(10, true, MakeQuestion(1));
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(task.IsCompletedSuccessfully,
            "UpdateScore deve completar sem exceção quando usuário não está autenticado");
    }

    // =======================================================
    // HasBonusActive / CalculateBonusScore (síncronos, sem async)
    // =======================================================

    [Test]
    public void HasBonusActive_SemBonusNaCena_RetornaFalse()
    {
        Assert.IsFalse(_scoreManager.HasBonusActive());
    }

    [Test]
    public void CalculateBonusScore_SemBonus_RetornaValorOriginal()
    {
        Assert.AreEqual(10, _scoreManager.CalculateBonusScore(10));
    }

    [Test]
    public void CalculateBonusScore_ScoreNegativo_SemBonus_PassaIntacto()
    {
        Assert.AreEqual(-5, _scoreManager.CalculateBonusScore(-5));
    }
}

// -------------------------------------------------------
// Fake mínimo para IAnsweredQuestionsManager
// Necessário apenas para o teste de modo desenvolvimento.
// Pode ser movido para Assets/Editor/Tests/Helpers/ futuramente.
// -------------------------------------------------------
public class FakeAnsweredQuestionsManager : IAnsweredQuestionsManager
{
    public bool IsManagerInitialized  => true;
    public int  MarkQuestionCallCount { get; private set; }

    public System.Threading.Tasks.Task ForceUpdate()
        => System.Threading.Tasks.Task.CompletedTask;

    public System.Threading.Tasks.Task<List<string>> FetchUserAnsweredQuestionsInTargetDatabase(string target)
        => System.Threading.Tasks.Task.FromResult(new List<string>());

    public System.Threading.Tasks.Task MarkQuestionAsAnswered(string databankName, int questionNumber)
    {
        MarkQuestionCallCount++;
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task<bool> HasRemainingQuestions(string db, List<string> list)
        => System.Threading.Tasks.Task.FromResult(true);

    public void ResetManager() { }
}