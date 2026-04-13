// Assets/Editor/Tests/QuestionLoadManagerTests.cs
// Testes unitários para QuestionLoadManager — fluxo de carregamento de questões.
//
// QuestionLoadManager usa AppContext.AnsweredQuestions, o que permite
// testá-lo via AppContext.OverrideForTests + FakeAnsweredQuestionsManager.
//
// O que É testado:
//   - LoadQuestionsFromDatabase: filtragem, cálculo de nível, remoção de respondidas
//   - Integração com QuestionBankStatistics (SetTotalQuestions, SetQuestionsPerLevel)
//   - Guards: database nulo, lista vazia, usuário sem UserId
//
// O que NÃO é testado:
//   - LoadQuestionsForSet: usa FindObjectsByType — requer cena montada
//   - Initialize/WaitForAnsweredQuestionsManager: usa Task.Delay em loop
//
// Para rodar: Window → General → Test Runner → PlayMode → Run All
// (requer Play Mode pois LoadQuestionsFromDatabase é async com Task.Yield)

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
    private GameObject               _managerGO;
    private QuestionLoadManager      _loadManager;
    private ConfigurableFakeAnsweredQuestionsManager _fakeAnswered;

    private const string DB_NAME = "BioQuestions";
    private const string USER_ID = "user-load-test";

    [SetUp]
    public void Setup()
    {
        _fakeAnswered = new ConfigurableFakeAnsweredQuestionsManager();
        AppContext.OverrideForTests(answeredQuestions: _fakeAnswered);

        var user = new UserData(USER_ID, "Tester", "Tester", "t@test.com");
        UserDataStore.CurrentUserData = user;
        UserDataStore.Logger = _ => { };

        QuestionBankStatistics.ClearAllStatistics();

        _managerGO   = new GameObject("QuestionLoadManager");
        _loadManager = _managerGO.AddComponent<QuestionLoadManager>();
        _loadManager.databankName = DB_NAME;
    }

    [TearDown]
    public void TearDown()
    {
        QuestionBankStatistics.ClearAllStatistics();
        UserDataStore.Clear();
        Object.DestroyImmediate(_managerGO);
    }

    // -------------------------------------------------------
    // Helper: cria IQuestionDatabase fake com questões
    // -------------------------------------------------------

    private static FakeQuestionDatabase MakeDB(
        int nivel1, int nivel2 = 0, int nivel3 = 0,
        string name = DB_NAME)
    {
        var db = new FakeQuestionDatabase
        {
            IsInDevelopmentMode = false,
            DatabaseName        = name
        };
        db.Questions.AddRange(
            QuestionTestHelpers.MakeQuestions(nivel1, nivel2, nivel3, databankName: name));
        return db;
    }

    // -------------------------------------------------------
    // Expõe LoadQuestionsFromDatabase via reflection
    // (método privado — necessário para teste unitário isolado)
    // -------------------------------------------------------

    private System.Threading.Tasks.Task<List<Question>> InvokeLoadFromDatabase(
        FakeQuestionDatabase db)
    {
        var method = typeof(QuestionLoadManager)
            .GetMethod("LoadQuestionsFromDatabase",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

        return (System.Threading.Tasks.Task<List<Question>>)
            method.Invoke(_loadManager, new object[] { db });
    }

    // =======================================================
    // Guards de entrada
    // =======================================================

    [UnityTest]
    public IEnumerator LoadQuestionsFromDatabase_DatabaseNulo_RetornaListaVazia()
    {
        LogAssert.Expect(LogType.Error, "[QuestionLoadManager] Database é null");

        var task = InvokeLoadFromDatabase(null);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(0, task.Result.Count);
    }

    [UnityTest]
    public IEnumerator LoadQuestionsFromDatabase_QuestoesVazias_RetornaListaVazia()
    {
        var db = FakeQuestionDatabase.Empty();
        db.DatabaseName = DB_NAME;

        LogAssert.Expect(LogType.Error,
            "[QuestionLoadManager] ❌ Database retornou lista nula ou vazia");

        var task = InvokeLoadFromDatabase(db);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(0, task.Result.Count);
    }

    // =======================================================
    // Sem userId — retorna apenas questões de nível 1
    // =======================================================

    [UnityTest]
    public IEnumerator LoadQuestionsFromDatabase_SemUserId_RetornaApenasNivel1()
    {
        UserDataStore.CurrentUserData = new UserData(); // UserId vazio
        var db = MakeDB(nivel1: 3, nivel2: 3);

        var task = InvokeLoadFromDatabase(db);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(task.Result.All(q => q.questionLevel == 1 || q.questionLevel == 0),
            "Sem userId, apenas questões de nível 1 devem ser retornadas");
    }

    // =======================================================
    // Com userId — filtra respondidas e calcula nível
    // =======================================================

    [UnityTest]
    public IEnumerator LoadQuestionsFromDatabase_SemRespondidas_RetornaNivel1()
    {
        // Nenhuma questão respondida → nível atual = 1
        _fakeAnswered.SetAnsweredQuestions(DB_NAME, new List<string>());
        var db = MakeDB(nivel1: 3, nivel2: 3);

        var task = InvokeLoadFromDatabase(db);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(task.Result.All(q => q.questionLevel == 1),
            "Sem respondidas, todas as questões retornadas devem ser de nível 1");
        Assert.AreEqual(3, task.Result.Count);
    }

    [UnityTest]
    public IEnumerator LoadQuestionsFromDatabase_Nivel1Completo_RetornaNivel2()
    {
        // Questões 1, 2, 3 (nível 1) respondidas → avança para nível 2
        _fakeAnswered.SetAnsweredQuestions(DB_NAME, new List<string> { "1", "2", "3" });
        var db = MakeDB(nivel1: 3, nivel2: 3);

        var task = InvokeLoadFromDatabase(db);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(task.Result.All(q => q.questionLevel == 2),
            "Com nível 1 completo, as questões retornadas devem ser de nível 2");
    }

    [UnityTest]
    public IEnumerator LoadQuestionsFromDatabase_RemoveQuestoesJaRespondidas()
    {
        // Questão 1 (nível 1) já foi respondida — não deve aparecer no resultado
        _fakeAnswered.SetAnsweredQuestions(DB_NAME, new List<string> { "1" });
        var db = MakeDB(nivel1: 3);

        var task = InvokeLoadFromDatabase(db);
        yield return new WaitUntil(() => task.IsCompleted);

        bool temQ1 = task.Result.Any(q => q.questionNumber == 1);
        Assert.IsFalse(temQ1, "Questão já respondida não deve aparecer na lista de retorno");
        Assert.AreEqual(2, task.Result.Count);
    }

    [UnityTest]
    public IEnumerator LoadQuestionsFromDatabase_TodosRespondidos_RetornaListaVazia()
    {
        _fakeAnswered.SetAnsweredQuestions(DB_NAME, new List<string> { "1", "2", "3" });
        var db = MakeDB(nivel1: 3); // todos de nível 1, todos respondidos

        var task = InvokeLoadFromDatabase(db);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(0, task.Result.Count,
            "Com todas as questões respondidas, a lista deve estar vazia");
    }

    // =======================================================
    // Integração com QuestionBankStatistics
    // =======================================================

    [UnityTest]
    public IEnumerator LoadQuestionsFromDatabase_RegistraTotalNoBankStatistics()
    {
        _fakeAnswered.SetAnsweredQuestions(DB_NAME, new List<string>());
        var db = MakeDB(nivel1: 3, nivel2: 2);

        var task = InvokeLoadFromDatabase(db);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(5, QuestionBankStatistics.GetTotalQuestions(DB_NAME),
            "QuestionBankStatistics deve receber o total de questões do banco");
    }

    [UnityTest]
    public IEnumerator LoadQuestionsFromDatabase_RegistraQuestoesPorNivel()
    {
        _fakeAnswered.SetAnsweredQuestions(DB_NAME, new List<string>());
        var db = MakeDB(nivel1: 3, nivel2: 2, nivel3: 1);

        var task = InvokeLoadFromDatabase(db);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(3, QuestionBankStatistics.GetQuestionsForLevel(DB_NAME, 1));
        Assert.AreEqual(2, QuestionBankStatistics.GetQuestionsForLevel(DB_NAME, 2));
        Assert.AreEqual(1, QuestionBankStatistics.GetQuestionsForLevel(DB_NAME, 3));
    }
}

// -------------------------------------------------------
// Fake especializado para QuestionLoadManagerTests.
// Sobrescreve FetchUserAnsweredQuestionsInTargetDatabase
// para retornar questões configuradas por banco de dados.
// Declarado como classe separada para não colidir com o
// FakeAnsweredQuestionsManager do QuestionScoreManagerTests.
// -------------------------------------------------------
public class ConfigurableFakeAnsweredQuestionsManager : IAnsweredQuestionsManager
{
    private readonly Dictionary<string, List<string>> _answeredByDb
        = new Dictionary<string, List<string>>();

    public bool IsManagerInitialized => true;

    public void SetAnsweredQuestions(string databankName, List<string> answered)
        => _answeredByDb[databankName] = answered ?? new List<string>();

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