// Assets/Editor/Tests/QuestionSyncServiceTests.cs
//
// Testes unitários para QuestionSyncService.
// Cada região cobre um comportamento distinto do serviço.
//
// Cobertura planejada:
//   ✅ InitializeAsync — sem cache local
//   ✅ InitializeAsync — cache válido (< 7 dias)
//   ✅ InitializeAsync — cache expirado (> 7 dias)
//   ✅ InitializeAsync — Firestore indisponível, sem cache
//   ✅ InitializeAsync — Firestore indisponível, com cache antigo (fallback)
//   ✅ InitializeAsync — Firestore retorna lista vazia
//   ✅ GetQuestionsForDatabankName — retorna questões corretas do banco
//   ✅ GetQuestionsForDatabankName — antes de InitializeAsync (IsCacheReady = false)
//   ✅ QuestionLocalRepository — SaveQuestions e GetQuestionsByDatabankName (integração com FakeLiteDBManager)
//   ✅ QuestionLocalRepository — ClearAll, HasAnyQuestions, GetLatestCacheTimestamp

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using QuestionSystem;

[TestFixture]
public class QuestionSyncServiceTests
{
    // ── Fixtures ───────────────────────────────────────────────────────────────
    private FakeFirestoreQuestionRepository _fakeFirestore;
    private FakeQuestionLocalRepository     _fakeLocal;
    private QuestionSyncService             _syncService;
    private GameObject                      _syncServiceGO;

    [SetUp]
    public void Setup()
    {
        _fakeFirestore = new FakeFirestoreQuestionRepository();
        _fakeLocal     = new FakeQuestionLocalRepository();

        _syncServiceGO = new GameObject("QuestionSyncService");
        _syncService   = _syncServiceGO.AddComponent<QuestionSyncService>();
        _syncService.InjectDependencies(_fakeFirestore, _fakeLocal);
    }

    [TearDown]
    public void TearDown()
    {
        _fakeFirestore.Reset();
        _fakeLocal.Reset();
        if (_syncServiceGO != null)
            Object.DestroyImmediate(_syncServiceGO);
    }

    // =======================================================================
    // InitializeAsync — sem cache local
    // =======================================================================

    [UnityTest]
    public IEnumerator InitializeAsync_SemCacheLocal_BaixaDoFirestoreESalvaNoLocal()
    {
        // Arrange
        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 5, nivel2: 3, databankName: "TestDB");
        _fakeFirestore.SetQuestions(questions);
        // _fakeLocal está vazio por padrão

        // Act
        var task = _syncService.InitializeAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.IsTrue(task.Result,                          "Deve retornar true quando baixa com sucesso.");
        Assert.IsTrue(_syncService.IsCacheReady,            "IsCacheReady deve ser true.");
        Assert.AreEqual(1, _fakeFirestore.GetAllQuestionsCallCount, "GetAllQuestions deve ter sido chamado 1x.");
        Assert.AreEqual(1, _fakeLocal.SaveQuestionsCallCount,       "SaveQuestions deve ter sido chamado 1x.");
        Assert.AreEqual(8, _fakeLocal.LastSaveCount,                "Deve ter salvo as 8 questões.");
    }

    [UnityTest]
    public IEnumerator InitializeAsync_SemCacheLocal_FirestoreVazio_RetornaFalse()
    {
        // Arrange
        _fakeFirestore.SetQuestions(new List<Question>());  // lista vazia

        // Act
        var task = _syncService.InitializeAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.IsFalse(task.Result,             "Deve retornar false quando Firestore retorna vazio.");
        Assert.IsFalse(_syncService.IsCacheReady, "IsCacheReady deve ser false.");
    }

    [UnityTest]
    public IEnumerator InitializeAsync_SemCacheLocal_FirestoreIndisponivel_RetornaFalse()
    {
        // Arrange
        _fakeFirestore.ShouldThrowOnGetAll = true;
        // _fakeLocal está vazio — sem fallback

        // O serviço loga um erro quando o Firestore falha — precisamos declarar que esperamos esse log
        LogAssert.Expect(LogType.Error,
            new System.Text.RegularExpressions.Regex(@"\[QuestionSyncService\] Falha ao baixar"));

        // Act
        var task = _syncService.InitializeAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.IsFalse(task.Result,               "Deve retornar false quando Firestore falha e não há cache.");
        Assert.IsFalse(_syncService.IsCacheReady, "IsCacheReady deve ser false.");
    }

    // =======================================================================
    // InitializeAsync — cache válido (< 7 dias)
    // =======================================================================

    [UnityTest]
    public IEnumerator InitializeAsync_CacheValido_NaoChamaFirestore()
    {
        // Arrange — cache salvo há 1 dia (válido)
        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 5, databankName: "TestDB");
        _fakeLocal.SetQuestions(questions, savedDaysAgo: 1);

        // Act
        var task = _syncService.InitializeAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert
        Assert.IsTrue(task.Result,                              "Deve retornar true com cache válido.");
        Assert.IsTrue(_syncService.IsCacheReady,                "IsCacheReady deve ser true.");
        Assert.AreEqual(0, _fakeFirestore.GetAllQuestionsCallCount, "Não deve chamar Firestore com cache válido.");
        Assert.AreEqual(0, _fakeLocal.SaveQuestionsCallCount,       "Não deve salvar novamente no local.");
    }

    // =======================================================================
    // InitializeAsync — cache expirado (> 7 dias)
    // =======================================================================

    [UnityTest]
    public IEnumerator InitializeAsync_CacheExpirado_RetornaTrueComCacheAntigoEAtualizaEmBackground()
    {
        // Arrange — cache salvo há 10 dias (expirado, threshold = 7)
        var oldQuestions = QuestionTestHelpers.MakeQuestions(nivel1: 3, databankName: "TestDB");
        _fakeLocal.SetQuestions(oldQuestions, savedDaysAgo: 10);

        var newQuestions = QuestionTestHelpers.MakeQuestions(nivel1: 5, databankName: "TestDB");
        _fakeFirestore.SetQuestions(newQuestions);

        // Act
        var task = _syncService.InitializeAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert — retorna imediatamente com cache antigo, atualização em background
        Assert.IsTrue(task.Result,            "Deve retornar true mesmo com cache expirado (usa o antigo).");
        Assert.IsTrue(_syncService.IsCacheReady, "IsCacheReady deve ser true.");

        // Aguarda a atualização em background concluir
        yield return new WaitForSeconds(0.5f);
        Assert.AreEqual(1, _fakeFirestore.GetAllQuestionsCallCount, "Deve ter chamado Firestore para atualizar.");
    }

    // =======================================================================
    // InitializeAsync — Firestore indisponível com cache antigo (fallback)
    // =======================================================================

    [UnityTest]
    public IEnumerator InitializeAsync_FirestoreIndisponivel_ComCacheAntigo_UsaFallback()
    {
        // Arrange — cache expirado + Firestore fora do ar
        var oldQuestions = QuestionTestHelpers.MakeQuestions(nivel1: 5, databankName: "TestDB");
        _fakeLocal.SetQuestions(oldQuestions, savedDaysAgo: 30);
        _fakeFirestore.ShouldThrowOnGetAll = true;

        // Act
        var task = _syncService.InitializeAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        // Assert — usa cache antigo como fallback
        Assert.IsTrue(task.Result,            "Deve retornar true usando cache antigo como fallback.");
        Assert.IsTrue(_syncService.IsCacheReady, "IsCacheReady deve ser true (cache antigo disponível).");
    }

    // =======================================================================
    // GetQuestionsForDatabankName
    // =======================================================================

    [UnityTest]
    public IEnumerator GetQuestionsForDatabankName_RetornaQuestoesDoBancoCorreto()
    {
        // Arrange
        var acidsQuestions = QuestionTestHelpers.MakeQuestions(nivel1: 5,
            databankName: "AcidBaseBufferQuestionDatabase");
        var waterQuestions = QuestionTestHelpers.MakeQuestions(nivel1: 3,
            databankName: "WaterQuestionDataBase");

        var allQuestions = new List<Question>(acidsQuestions);
        allQuestions.AddRange(waterQuestions);

        _fakeFirestore.SetQuestions(allQuestions);

        var task = _syncService.InitializeAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        // Act
        var result = _syncService.GetQuestionsForDatabankName("AcidBaseBufferQuestionDatabase");

        // Assert
        Assert.AreEqual(5, result.Count, "Deve retornar apenas as questões do banco solicitado.");
        Assert.IsTrue(result.All(q => q.questionDatabankName == "AcidBaseBufferQuestionDatabase"),
            "Todas as questões devem ser do banco correto.");
    }

    [UnityTest]
    public IEnumerator GetQuestionsForDatabankName_SemInicializar_RetornaListaVazia()
    {
        // Arrange — NÃO chama InitializeAsync

        // O serviço loga um erro quando IsCacheReady = false
        LogAssert.Expect(LogType.Error,
            new System.Text.RegularExpressions.Regex(@"\[QuestionSyncService\] Cache não está pronto"));

        // Act
        var result = _syncService.GetQuestionsForDatabankName("TestDB");

        // Assert
        Assert.IsNotNull(result,            "Não deve retornar null.");
        Assert.AreEqual(0, result.Count,    "Deve retornar lista vazia se IsCacheReady = false.");
        yield return null;
    }

    // =======================================================================
    // QuestionLocalRepository (integração com FakeLiteDBManager)
    // =======================================================================

    [Test]
    public void QuestionLocalRepository_SaveEGetQuestions_CicloCompleto()
    {
        // Arrange
        var db   = new FakeLiteDBManager();
        var repo = new QuestionLocalRepository();
        repo.InjectDependencies(db);

        var questions = QuestionTestHelpers.MakeQuestions(
            nivel1: 4, nivel2: 2,
            databankName: "MembranesQuestionDatabase");

        // Act
        repo.SaveQuestions(questions);
        var result = repo.GetQuestionsByDatabankName("MembranesQuestionDatabase");

        // Assert
        Assert.AreEqual(6, result.Count, "Deve retornar as 6 questões salvas.");
        Assert.IsTrue(repo.HasAnyQuestions(), "HasAnyQuestions deve ser true após salvar.");

        db.Close();
    }

    [Test]
    public void QuestionLocalRepository_ClearAll_RemoveTodasAsQuestoes()
    {
        // Arrange
        var db   = new FakeLiteDBManager();
        var repo = new QuestionLocalRepository();
        repo.InjectDependencies(db);
        repo.SaveQuestions(QuestionTestHelpers.MakeQuestions(nivel1: 5, databankName: "TestDB"));

        // Act
        repo.ClearAll();

        // Assert
        Assert.IsFalse(repo.HasAnyQuestions(),     "HasAnyQuestions deve ser false após ClearAll.");
        Assert.AreEqual(0, repo.GetAllQuestions().Count, "GetAllQuestions deve retornar vazio.");

        db.Close();
    }

    [Test]
    public void QuestionLocalRepository_GetLatestCacheTimestamp_SemQuestoes_RetornaMinValue()
    {
        // Arrange
        var db   = new FakeLiteDBManager();
        var repo = new QuestionLocalRepository();
        repo.InjectDependencies(db);

        // Act
        var timestamp = repo.GetLatestCacheTimestamp();

        // Assert
        Assert.AreEqual(System.DateTime.MinValue, timestamp,
            "Sem questões, deve retornar DateTime.MinValue.");

        db.Close();
    }

    [Test]
    public void QuestionLocalRepository_NovoCamposDOFirestore_SobrevivemAoCicloSaveGet()
    {
        // Arrange — garante que os campos novos (globalId, bloomLevel, etc.) são persistidos
        var db   = new FakeLiteDBManager();
        var repo = new QuestionLocalRepository();
        repo.InjectDependencies(db);

        var question = QuestionTestHelpers.MakeFullQuestion(
            number: 1,
            databankName: "TestDB",
            topic: "acidsBase",
            bloomLevel: "understand");

        // Act
        repo.SaveQuestions(new List<Question> { question });
        var result = repo.GetQuestionsByDatabankName("TestDB");

        // Assert
        Assert.AreEqual(1, result.Count);
        var saved = result[0];
        Assert.AreEqual("TestDB_001",  saved.globalId,   "globalId deve ser preservado.");
        Assert.AreEqual("acidsBase",   saved.topic,      "topic deve ser preservado.");
        Assert.AreEqual("understand",  saved.bloomLevel, "bloomLevel deve ser preservado.");
        Assert.IsNotNull(saved.conceptTags,              "conceptTags não deve ser null.");
        Assert.IsNotNull(saved.questionHint,             "questionHint não deve ser null.");

        db.Close();
    }
}
