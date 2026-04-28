// Assets/Editor/Tests/QuestionLoadManagerTests.cs
// Testes unitários para QuestionLoadManager — fluxo de carregamento de questões.
//
// O que É testado:
//   - LoadQuestionsForSet: leitura do LiteDB, filtragem, cálculo de nível
//   - Integração com QuestionBankStatistics (SetTotalQuestions, SetQuestionsPerLevel)
//   - Guards: LiteDB vazio, usuário sem UserId
//   - Progressão de níveis: sem respondidas → nível 1, nível 1 completo → nível 2
//   - Remoção de questões já respondidas do resultado

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using QuestionSystem;

[TestFixture]
public class QuestionLoadManagerTests
{
    private GameObject _managerGO;
    private QuestionLoadManager _loadManager;
    private ConfigurableFakeAnsweredQuestionsManager _fakeAnswered;
    private FakeQuestionSyncService _fakeSync;

    private const string DB_NAME = "AminoacidQuestionDatabase";
    private const string USER_ID = "user-load-test";

    // QuestionSet que mapeia para DB_NAME no TopicToDatabankName
    private const QuestionSet TARGET_SET = QuestionSet.aminoacids;

    [SetUp]
    public void Setup()
    {
        _fakeAnswered = new ConfigurableFakeAnsweredQuestionsManager();
        _fakeSync     = new FakeQuestionSyncService { IsCacheReady = true };

        AppContext.OverrideForTests(
            answeredQuestions: _fakeAnswered,
            questionSync:      _fakeSync,
            questionSource: new FirestoreQuestionSource(_fakeSync)
        );

        var user = new UserData(USER_ID, "Tester", "Tester", "t@test.com");
        UserDataStore.CurrentUserData = user;
        UserDataStore.Logger = _ => { };

        QuestionBankStatistics.ClearAllStatistics();

        _managerGO = new GameObject("QuestionLoadManager");
        _loadManager = _managerGO.AddComponent<QuestionLoadManager>();
        _loadManager.databankName = DB_NAME;
    }

    [TearDown]
    public void TearDown()
    {
        QuestionBankStatistics.ClearAllStatistics();
        UserDataStore.Clear();
        _fakeSync.Reset();
        _fakeAnswered.Reset();
        EnvironmentConfig.ClearTestOverride();
        Object.DestroyImmediate(_managerGO);
    }

    // -------------------------------------------------------
    // Helper
    // -------------------------------------------------------
    private void SetupLiteDBWithQuestions(int nivel1, int nivel2 = 0, int nivel3 = 0)
    {
        var questions = QuestionTestHelpers.MakeQuestions(nivel1, nivel2, nivel3, databankName: DB_NAME);
        _fakeSync.SetQuestionsForDatabankName(DB_NAME, questions);
    }

    // =======================================================
    // Guard: LiteDB vazio
    // =======================================================
    [UnityTest]
    public IEnumerator LoadQuestionsForSet_DatabaseNulo_RetornaListaVazia()
    {
        // Arrange — LiteDB retorna lista vazia para o banco
        _fakeSync.SetQuestionsForDatabankName(DB_NAME, new List<Question>());

        LogAssert.Expect(LogType.Error,
            new System.Text.RegularExpressions.Regex(
                @"\[QuestionLoadManager\] ❌ Nenhuma questão no LiteDB"));

        // Act
        var task = _loadManager.LoadQuestionsForSet(TARGET_SET);
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.AreEqual(0, task.Result.Count);
    }

    [UnityTest]
    public IEnumerator LoadQuestionsForSet_QuestoesVazias_RetornaListaVazia()
    {
        // LiteDB sem dados para o banco
        // (FakeQuestionSyncService retorna lista vazia por padrão)
        LogAssert.Expect(LogType.Error,
            new System.Text.RegularExpressions.Regex(
                @"\[QuestionLoadManager\] ❌ Nenhuma questão no LiteDB"));

        var task = _loadManager.LoadQuestionsForSet(TARGET_SET);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(0, task.Result.Count);
    }

    // =======================================================
    // Sem userId — retorna apenas questões de nível 1
    // =======================================================
    [UnityTest]
    public IEnumerator LoadQuestionsForSet_SemUserId_RetornaApenasNivel1()
    {
        // Arrange
        UserDataStore.CurrentUserData = new UserData(); // UserId vazio
        SetupLiteDBWithQuestions(nivel1: 3, nivel2: 3);

        LogAssert.Expect(LogType.Warning,
            new System.Text.RegularExpressions.Regex(
                @"\[QuestionLoadManager\] ⚠️ UserId não disponível"));

        // Act
        var task = _loadManager.LoadQuestionsForSet(TARGET_SET);
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.IsTrue(task.Result.All(q => q.questionLevel == 1 || q.questionLevel == 0),
            "Sem userId, apenas questões de nível 1 devem ser retornadas");
        Assert.AreEqual(3, task.Result.Count);
    }

    // =======================================================
    // Progressão de níveis
    // =======================================================
    [UnityTest]
    public IEnumerator LoadQuestionsForSet_SemRespondidas_RetornaNivel1()
    {
        // Arrange
        _fakeAnswered.SetAnsweredQuestions(DB_NAME, new List<string>());
        SetupLiteDBWithQuestions(nivel1: 3, nivel2: 3);

        // Act
        var task = _loadManager.LoadQuestionsForSet(TARGET_SET);
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.IsTrue(task.Result.All(q => q.questionLevel == 1),
            "Sem respondidas, todas as questões retornadas devem ser de nível 1");
        Assert.AreEqual(3, task.Result.Count);
    }

    [UnityTest]
    public IEnumerator LoadQuestionsForSet_Nivel1Completo_RetornaNivel2()
    {
        // Arrange — questões 1, 2, 3 (nível 1) respondidas → avança para nível 2
        _fakeAnswered.SetAnsweredQuestions(DB_NAME, new List<string> { "1", "2", "3" });
        SetupLiteDBWithQuestions(nivel1: 3, nivel2: 3);

        // Act
        var task = _loadManager.LoadQuestionsForSet(TARGET_SET);
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.IsTrue(task.Result.All(q => q.questionLevel == 2),
            "Com nível 1 completo, as questões retornadas devem ser de nível 2");
        Assert.AreEqual(3, task.Result.Count);
    }

    [UnityTest]
    public IEnumerator LoadQuestionsForSet_RemoveQuestoesJaRespondidas()
    {
        // Arrange — questão 1 (nível 1) já foi respondida
        _fakeAnswered.SetAnsweredQuestions(DB_NAME, new List<string> { "1" });
        SetupLiteDBWithQuestions(nivel1: 3);

        // Act
        var task = _loadManager.LoadQuestionsForSet(TARGET_SET);
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        bool temQ1 = task.Result.Any(q => q.questionNumber == 1);
        Assert.IsFalse(temQ1, "Questão já respondida não deve aparecer no resultado");
        Assert.AreEqual(2, task.Result.Count);
    }

    [UnityTest]
    public IEnumerator LoadQuestionsForSet_TodosRespondidos_RetornaListaVazia()
    {
        // Arrange
        _fakeAnswered.SetAnsweredQuestions(DB_NAME, new List<string> { "1", "2", "3" });
        SetupLiteDBWithQuestions(nivel1: 3);

        // Act
        var task = _loadManager.LoadQuestionsForSet(TARGET_SET);
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.AreEqual(0, task.Result.Count,
            "Com todas as questões respondidas, a lista deve estar vazia");
    }

    // =======================================================
    // Integração com QuestionBankStatistics
    // =======================================================
    [UnityTest]
    public IEnumerator LoadQuestionsForSet_RegistraTotalNoBankStatistics()
    {
        // Arrange
        _fakeAnswered.SetAnsweredQuestions(DB_NAME, new List<string>());
        SetupLiteDBWithQuestions(nivel1: 3, nivel2: 2);

        // Act
        var task = _loadManager.LoadQuestionsForSet(TARGET_SET);
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.AreEqual(5, QuestionBankStatistics.GetTotalQuestions(DB_NAME),
            "QuestionBankStatistics deve receber o total de questões do banco");
    }

    [UnityTest]
    public IEnumerator LoadQuestionsForSet_RegistraQuestoesPorNivel()
    {
        // Arrange
        _fakeAnswered.SetAnsweredQuestions(DB_NAME, new List<string>());
        SetupLiteDBWithQuestions(nivel1: 3, nivel2: 2, nivel3: 1);

        // Act
        var task = _loadManager.LoadQuestionsForSet(TARGET_SET);
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.AreEqual(3, QuestionBankStatistics.GetQuestionsForLevel(DB_NAME, 1));
        Assert.AreEqual(2, QuestionBankStatistics.GetQuestionsForLevel(DB_NAME, 2));
        Assert.AreEqual(1, QuestionBankStatistics.GetQuestionsForLevel(DB_NAME, 3));
    }

    // =======================================================
    // Preview Mode
    // =======================================================

    [UnityTest]
    public IEnumerator LoadQuestionsForSet_PreviewMode_RetornaTodosOsNiveis()
    {
        // Arrange
        EnvironmentConfig.OverridePreviewModeForTests(true);
        SetupLiteDBWithQuestions(nivel1: 3, nivel2: 3, nivel3: 3);

        // Act
        var task = _loadManager.LoadQuestionsForSet(TARGET_SET);
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.AreEqual(9, task.Result.Count,
            "Preview Mode deve retornar questões de todos os níveis sem filtrar");
    }

    [UnityTest]
    public IEnumerator LoadQuestionsForSet_PreviewMode_NaoFiltraRespondidas()
    {
        // Arrange — questões de nível 1 marcadas como respondidas
        EnvironmentConfig.OverridePreviewModeForTests(true);
        _fakeAnswered.SetAnsweredQuestions(DB_NAME, new List<string> { "1", "2", "3" });
        SetupLiteDBWithQuestions(nivel1: 3, nivel2: 3);

        // Act
        var task = _loadManager.LoadQuestionsForSet(TARGET_SET);
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.AreEqual(6, task.Result.Count,
            "Preview Mode não deve filtrar questões já respondidas");
    }

    [UnityTest]
    public IEnumerator LoadQuestionsForSet_PreviewMode_SemUserId_RetornaTodas()
    {
        // Arrange — sem usuário logado
        EnvironmentConfig.OverridePreviewModeForTests(true);
        UserDataStore.CurrentUserData = new UserData(); // UserId vazio
        SetupLiteDBWithQuestions(nivel1: 3, nivel2: 3);

        // Act
        var task = _loadManager.LoadQuestionsForSet(TARGET_SET);
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.AreEqual(6, task.Result.Count,
            "Preview Mode não deve cair no guard de nível 1 quando UserId está vazio");
    }
}

// -------------------------------------------------------
// Fake especializado para QuestionLoadManagerTests.
// -------------------------------------------------------
public class ConfigurableFakeAnsweredQuestionsManager : IAnsweredQuestionsManager
{
    private readonly Dictionary<string, List<string>> _answeredByDb
        = new Dictionary<string, List<string>>();

    public bool IsManagerInitialized => true;

    public void SetAnsweredQuestions(string databankName, List<string> answered)
        => _answeredByDb[databankName] = answered ?? new List<string>();

    public void Reset() => _answeredByDb.Clear();

    public System.Threading.Tasks.Task<List<string>> FetchUserAnsweredQuestionsInTargetDatabase(string target)
    {
        _answeredByDb.TryGetValue(target, out var list);
        return System.Threading.Tasks.Task.FromResult(list ?? new List<string>());
    }

    public System.Threading.Tasks.Task ForceUpdate()
        => System.Threading.Tasks.Task.CompletedTask;

    public System.Threading.Tasks.Task MarkQuestionAsAnswered(string db, int number)
        => System.Threading.Tasks.Task.CompletedTask;

    public System.Threading.Tasks.Task<bool> HasRemainingQuestions(string db, List<string> list)
        => System.Threading.Tasks.Task.FromResult(true);

    public void ResetManager() { }
}