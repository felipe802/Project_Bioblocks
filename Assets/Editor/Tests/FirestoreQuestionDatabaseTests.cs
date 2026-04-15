// Assets/Editor/Tests/FirestoreQuestionDatabaseTests.cs
//
// Testes unitários para FirestoreQuestionDatabase.
// Usa FakeQuestionSyncService injetado via AppContext.OverrideForTests.
//
// Cobertura:
//   ✅ GetQuestions — cache pronto, retorna questões do banco correto
//   ✅ GetQuestions — cache não pronto, retorna lista vazia
//   ✅ GetQuestions — QuestionSync nulo no AppContext, retorna lista vazia
//   ✅ GetQuestions — delega o databankName correto ao sync service
//   ✅ GetQuestions — banco sem questões no cache, retorna lista vazia
//   ✅ GetDatabankName — retorna nome configurado
//   ✅ GetDisplayName — retorna nome de exibição configurado
//   ✅ IsDatabaseInDevelopment — false por padrão
//   ✅ IsDatabaseInDevelopment — true quando configurado

using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using QuestionSystem;
using UnityEngine.TestTools;

[TestFixture]
public class FirestoreQuestionDatabaseTests
{
    // ── Fixtures ───────────────────────────────────────────────────────────────
    private FakeQuestionSyncService _fakeSync;
    private FirestoreQuestionDatabase _db;
    private GameObject _dbGO;

    [SetUp]
    public void Setup()
    {
        _fakeSync = new FakeQuestionSyncService();

        _dbGO = new GameObject("FirestoreQuestionDatabase");
        _db   = _dbGO.AddComponent<FirestoreQuestionDatabase>();

        // Injeta o fake no AppContext (padrão: cache pronto)
        AppContext.OverrideForTests(questionSync: _fakeSync);
    }

    [TearDown]
    public void TearDown()
    {
        _fakeSync.Reset();

        // Reseta a propriedade estática QuestionSync para não vazar entre testes
        ResetAppContextProperty("QuestionSync");

        if (_dbGO != null)
            Object.DestroyImmediate(_dbGO);
    }

    // =======================================================================
    // GetQuestions — cache pronto
    // =======================================================================

    [Test]
    public void GetQuestions_CacheReady_RetornaQuestoesDoBanco()
    {
        // Arrange
        const string banco = "AcidBaseBufferQuestionDatabase";
        SetDatabankName(_db, banco);

        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 5, nivel2: 3, databankName: banco);
        _fakeSync.IsCacheReady = true;
        _fakeSync.SetQuestionsForDatabankName(banco, questions);

        // Act
        var result = _db.GetQuestions();

        // Assert
        Assert.AreEqual(8, result.Count, "Deve retornar todas as 8 questões do banco.");
    }

    [Test]
    public void GetQuestions_DelegaODatabankNameCorretoAoSync()
    {
        // Arrange
        const string banco = "WaterQuestionDataBase";
        SetDatabankName(_db, banco);
        _fakeSync.IsCacheReady = true;

        // Act
        _db.GetQuestions();

        // Assert
        Assert.AreEqual(banco, _fakeSync.LastRequestedDatabankName,
            "Deve passar o databankName configurado para o sync service.");
        Assert.AreEqual(1, _fakeSync.GetQuestionsForDatabankNameCallCount,
            "Deve chamar GetQuestionsForDatabankName exatamente 1 vez.");
    }

    [Test]
    public void GetQuestions_BancoSemQuestoesNoCachee_RetornaListaVazia()
    {
        // Arrange — banco sem questões configuradas no fake
        const string banco = "LipidsQuestionDataBase";
        SetDatabankName(_db, banco);
        _fakeSync.IsCacheReady = true;
        // (não chama SetQuestionsForDatabankName)

        // Act
        var result = _db.GetQuestions();

        // Assert
        Assert.IsNotNull(result,         "Não deve retornar null.");
        Assert.AreEqual(0, result.Count, "Deve retornar lista vazia para banco sem questões.");
    }

    // =======================================================================
    // GetQuestions — cache não pronto
    // =======================================================================

    [Test]
    public void GetQuestions_CacheNotReady_RetornaListaVazia()
    {
        // Arrange
        SetDatabankName(_db, "TestDB");
        _fakeSync.IsCacheReady = false;

        LogAssert.Expect(LogType.Error,
            new System.Text.RegularExpressions.Regex(@"\[FirestoreQuestionDatabase\] Cache não está pronto"));

        // Act
        var result = _db.GetQuestions();

        // Assert
        Assert.IsNotNull(result,         "Não deve retornar null.");
        Assert.AreEqual(0, result.Count, "Deve retornar lista vazia quando cache não está pronto.");
        Assert.AreEqual(0, _fakeSync.GetQuestionsForDatabankNameCallCount,
            "Não deve chamar GetQuestionsForDatabankName quando cache não está pronto.");
    }

    // =======================================================================
    // GetQuestions — QuestionSync nulo no AppContext
    // =======================================================================

    [Test]
    public void GetQuestions_QuestionSyncNuloNoAppContext_RetornaListaVazia()
    {
        // Arrange — remove o QuestionSync do AppContext
        ResetAppContextProperty("QuestionSync");
        SetDatabankName(_db, "TestDB");

        LogAssert.Expect(LogType.Error,
            new System.Text.RegularExpressions.Regex(@"\[FirestoreQuestionDatabase\] QuestionSyncService não registrado"));

        // Act
        var result = _db.GetQuestions();

        // Assert
        Assert.IsNotNull(result,         "Não deve retornar null.");
        Assert.AreEqual(0, result.Count, "Deve retornar lista vazia quando QuestionSync é null.");
    }

    // =======================================================================
    // Getters de configuração
    // =======================================================================

    [Test]
    public void GetDatabankName_RetornaNomeConfigurado()
    {
        // Arrange
        const string esperado = "MembranesQuestionDatabase";
        SetDatabankName(_db, esperado);

        // Act + Assert
        Assert.AreEqual(esperado, _db.GetDatabankName());
    }

    [Test]
    public void GetDisplayName_RetornaNomeDeExibicao()
    {
        // Arrange
        const string esperado = "Membranas Biológicas";
        SetDisplayName(_db, esperado);

        // Act + Assert
        Assert.AreEqual(esperado, _db.GetDisplayName());
    }

    [Test]
    public void IsDatabaseInDevelopment_PorPadrao_RetornaFalse()
    {
        Assert.IsFalse(_db.IsDatabaseInDevelopment(),
            "databaseInDevelopment deve ser false por padrão.");
    }

    [Test]
    public void IsDatabaseInDevelopment_QuandoTrue_RetornaTrue()
    {
        // Arrange
        SetDatabaseInDevelopment(_db, true);

        // Act + Assert
        Assert.IsTrue(_db.IsDatabaseInDevelopment());
    }

    // =======================================================================
    // Helpers — reflexão para setar campos [SerializeField] privados
    // =======================================================================

    private static void SetDatabankName(FirestoreQuestionDatabase target, string value)
        => SetPrivateField(target, "databankName", value);

    private static void SetDisplayName(FirestoreQuestionDatabase target, string value)
        => SetPrivateField(target, "displayName", value);

    private static void SetDatabaseInDevelopment(FirestoreQuestionDatabase target, bool value)
        => SetPrivateField(target, "databaseInDevelopment", value);

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.IsNotNull(field, $"Campo '{fieldName}' não encontrado via reflexão.");
        field.SetValue(target, value);
    }

    /// <summary>
    /// Reseta uma propriedade estática do AppContext para null via reflexão.
    /// Necessário porque AppContext.OverrideForTests não aceita null como valor.
    /// </summary>
    private static void ResetAppContextProperty(string propertyName)
    {
        var prop = typeof(AppContext).GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Static);

        prop?.SetValue(null, null);
    }
}
