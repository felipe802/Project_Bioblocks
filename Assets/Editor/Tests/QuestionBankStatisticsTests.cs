// Assets/Editor/Tests/QuestionBankStatisticsTests.cs
// Testes unitários para QuestionBankStatistics.
//
// ATENÇÃO: QuestionBankStatistics é uma classe estática com estado global.
// O [SetUp] e [TearDown] DEVEM chamar ClearAllStatistics() para garantir
// isolamento entre testes. Não remova essas chamadas.

using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class QuestionBankStatisticsTests
{
    private const string DB_BIO   = "BiochemistryDB";
    private const string DB_WATER = "WaterDB";

    [SetUp]
    public void Setup()
    {
        QuestionBankStatistics.ClearAllStatistics();
    }

    [TearDown]
    public void TearDown()
    {
        QuestionBankStatistics.ClearAllStatistics();
    }

    // =======================================================
    // SetTotalQuestions / GetTotalQuestions
    // =======================================================

    [Test]
    public void GetTotalQuestions_BancoNaoRegistrado_RetornaZero()
    {
        int result = QuestionBankStatistics.GetTotalQuestions(DB_BIO);
        Assert.AreEqual(0, result);
    }

    [Test]
    public void GetTotalQuestions_StringNula_RetornaZeroSemCrash()
    {
        // A classe chama Debug.LogError internamente — declaramos que é esperado
        LogAssert.Expect(LogType.Error, "Nome do banco de dados inválido");

        int result = QuestionBankStatistics.GetTotalQuestions(null);

        Assert.AreEqual(0, result);
    }

    [Test]
    public void GetTotalQuestions_StringVazia_RetornaZeroSemCrash()
    {
        LogAssert.Expect(LogType.Error, "Nome do banco de dados inválido");

        int result = QuestionBankStatistics.GetTotalQuestions(string.Empty);

        Assert.AreEqual(0, result);
    }

    [Test]
    public void SetAndGetTotalQuestions_ValorArmazenadoCorretamente()
    {
        QuestionBankStatistics.SetTotalQuestions(DB_BIO, 30);
        Assert.AreEqual(30, QuestionBankStatistics.GetTotalQuestions(DB_BIO));
    }

    [Test]
    public void SetTotalQuestions_Sobrescreve_ValorAnterior()
    {
        QuestionBankStatistics.SetTotalQuestions(DB_BIO, 10);
        QuestionBankStatistics.SetTotalQuestions(DB_BIO, 25);
        Assert.AreEqual(25, QuestionBankStatistics.GetTotalQuestions(DB_BIO));
    }

    [Test]
    public void SetTotalQuestions_BancosDiferentes_NaoInterfere()
    {
        QuestionBankStatistics.SetTotalQuestions(DB_BIO,   30);
        QuestionBankStatistics.SetTotalQuestions(DB_WATER, 15);

        Assert.AreEqual(30, QuestionBankStatistics.GetTotalQuestions(DB_BIO));
        Assert.AreEqual(15, QuestionBankStatistics.GetTotalQuestions(DB_WATER));
    }

    // =======================================================
    // SetQuestionsPerLevel / GetQuestionsForLevel
    // =======================================================

    [Test]
    public void GetQuestionsForLevel_BancoNaoRegistrado_RetornaZero()
    {
        int result = QuestionBankStatistics.GetQuestionsForLevel(DB_BIO, 1);
        Assert.AreEqual(0, result);
    }

    [Test]
    public void GetQuestionsForLevel_NivelInvalidoZero_RetornaZeroSemCrash()
    {
        QuestionBankStatistics.SetQuestionsPerLevel(DB_BIO, MakeLevelCounts(10, 10, 10));
        LogAssert.Expect(LogType.Error, "Nível inválido: 0. Deve ser 1, 2 ou 3.");

        int result = QuestionBankStatistics.GetQuestionsForLevel(DB_BIO, 0);

        Assert.AreEqual(0, result);
    }

    [Test]
    public void GetQuestionsForLevel_NivelInvalidoQuatro_RetornaZeroSemCrash()
    {
        QuestionBankStatistics.SetQuestionsPerLevel(DB_BIO, MakeLevelCounts(10, 10, 10));
        LogAssert.Expect(LogType.Error, "Nível inválido: 4. Deve ser 1, 2 ou 3.");

        int result = QuestionBankStatistics.GetQuestionsForLevel(DB_BIO, 4);

        Assert.AreEqual(0, result);
    }

    [Test]
    public void SetAndGetQuestionsPerLevel_Nivel1Correto()
    {
        QuestionBankStatistics.SetQuestionsPerLevel(DB_BIO, MakeLevelCounts(10, 8, 5));
        Assert.AreEqual(10, QuestionBankStatistics.GetQuestionsForLevel(DB_BIO, 1));
    }

    [Test]
    public void SetAndGetQuestionsPerLevel_Nivel2Correto()
    {
        QuestionBankStatistics.SetQuestionsPerLevel(DB_BIO, MakeLevelCounts(10, 8, 5));
        Assert.AreEqual(8, QuestionBankStatistics.GetQuestionsForLevel(DB_BIO, 2));
    }

    [Test]
    public void SetAndGetQuestionsPerLevel_Nivel3Correto()
    {
        QuestionBankStatistics.SetQuestionsPerLevel(DB_BIO, MakeLevelCounts(10, 8, 5));
        Assert.AreEqual(5, QuestionBankStatistics.GetQuestionsForLevel(DB_BIO, 3));
    }

    [Test]
    public void SetQuestionsPerLevel_Sobrescreve_ValoresAnteriores()
    {
        QuestionBankStatistics.SetQuestionsPerLevel(DB_BIO, MakeLevelCounts(10, 10, 10));
        QuestionBankStatistics.SetQuestionsPerLevel(DB_BIO, MakeLevelCounts(3,  3,  3));

        Assert.AreEqual(3, QuestionBankStatistics.GetQuestionsForLevel(DB_BIO, 1));
    }

    // =======================================================
    // AreAllLevelQuestionsAnswered
    // =======================================================

    [Test]
    public void AreAllLevelQuestionsAnswered_TotalZero_RetornaFalse()
    {
        // Banco não registrado → total = 0 → NÃO pode retornar true
        bool result = QuestionBankStatistics.AreAllLevelQuestionsAnswered(DB_BIO, 1, answeredCount: 0);
        Assert.IsFalse(result, "Total 0 nunca deve ser considerado 'todos respondidos'");
    }

    [Test]
    public void AreAllLevelQuestionsAnswered_RespostasMenorQueTotal_RetornaFalse()
    {
        QuestionBankStatistics.SetQuestionsPerLevel(DB_BIO, MakeLevelCounts(10, 0, 0));

        bool result = QuestionBankStatistics.AreAllLevelQuestionsAnswered(DB_BIO, 1, answeredCount: 9);

        Assert.IsFalse(result);
    }

    [Test]
    public void AreAllLevelQuestionsAnswered_RespostasIgualAoTotal_RetornaTrue()
    {
        QuestionBankStatistics.SetQuestionsPerLevel(DB_BIO, MakeLevelCounts(10, 0, 0));

        bool result = QuestionBankStatistics.AreAllLevelQuestionsAnswered(DB_BIO, 1, answeredCount: 10);

        Assert.IsTrue(result);
    }

    [Test]
    public void AreAllLevelQuestionsAnswered_RespostasMaiorQueTotal_RetornaTrue()
    {
        // Edge case: mais respostas do que questões (ex: migração de dados)
        QuestionBankStatistics.SetQuestionsPerLevel(DB_BIO, MakeLevelCounts(10, 0, 0));

        bool result = QuestionBankStatistics.AreAllLevelQuestionsAnswered(DB_BIO, 1, answeredCount: 12);

        Assert.IsTrue(result);
    }

    // =======================================================
    // AreAllQuestionsAnswered
    // =======================================================

    [Test]
    public void AreAllQuestionsAnswered_TotalZero_RetornaFalse()
    {
        bool result = QuestionBankStatistics.AreAllQuestionsAnswered(DB_BIO, answeredCount: 0);
        Assert.IsFalse(result, "Total 0 nunca deve ser considerado completo");
    }

    [Test]
    public void AreAllQuestionsAnswered_RespostasIgualAoTotal_RetornaTrue()
    {
        QuestionBankStatistics.SetTotalQuestions(DB_BIO, 30);

        bool result = QuestionBankStatistics.AreAllQuestionsAnswered(DB_BIO, answeredCount: 30);

        Assert.IsTrue(result);
    }

    [Test]
    public void AreAllQuestionsAnswered_RespostasMenorQueTotal_RetornaFalse()
    {
        QuestionBankStatistics.SetTotalQuestions(DB_BIO, 30);

        bool result = QuestionBankStatistics.AreAllQuestionsAnswered(DB_BIO, answeredCount: 29);

        Assert.IsFalse(result);
    }

    // =======================================================
    // HasStatistics / ClearAllStatistics / GetAllRegisteredDatabases
    // =======================================================

    [Test]
    public void HasStatistics_BancoNaoRegistrado_RetornaFalse()
    {
        Assert.IsFalse(QuestionBankStatistics.HasStatistics(DB_BIO));
    }

    [Test]
    public void HasStatistics_AposRegistrar_RetornaTrue()
    {
        QuestionBankStatistics.SetTotalQuestions(DB_BIO, 10);
        Assert.IsTrue(QuestionBankStatistics.HasStatistics(DB_BIO));
    }

    [Test]
    public void ClearAllStatistics_RemoveTodosOsBancos()
    {
        QuestionBankStatistics.SetTotalQuestions(DB_BIO,   30);
        QuestionBankStatistics.SetTotalQuestions(DB_WATER, 15);

        QuestionBankStatistics.ClearAllStatistics();

        Assert.IsFalse(QuestionBankStatistics.HasStatistics(DB_BIO));
        Assert.IsFalse(QuestionBankStatistics.HasStatistics(DB_WATER));
    }

    [Test]
    public void ClearAllStatistics_GetTotalQuestionsVoltaAZero()
    {
        QuestionBankStatistics.SetTotalQuestions(DB_BIO, 30);
        QuestionBankStatistics.ClearAllStatistics();

        Assert.AreEqual(0, QuestionBankStatistics.GetTotalQuestions(DB_BIO));
    }

    [Test]
    public void GetAllRegisteredDatabases_RetornaListaCorreta()
    {
        QuestionBankStatistics.SetTotalQuestions(DB_BIO,   30);
        QuestionBankStatistics.SetTotalQuestions(DB_WATER, 15);

        var dbs = QuestionBankStatistics.GetAllRegisteredDatabases();

        Assert.AreEqual(2, dbs.Count);
        Assert.IsTrue(dbs.Contains(DB_BIO));
        Assert.IsTrue(dbs.Contains(DB_WATER));
    }

    [Test]
    public void GetAllRegisteredDatabases_AposClear_ListaVazia()
    {
        QuestionBankStatistics.SetTotalQuestions(DB_BIO, 10);
        QuestionBankStatistics.ClearAllStatistics();

        var dbs = QuestionBankStatistics.GetAllRegisteredDatabases();

        Assert.AreEqual(0, dbs.Count);
    }

    // =======================================================
    // Helper
    // =======================================================

    private static Dictionary<int, int> MakeLevelCounts(int l1, int l2, int l3)
        => new Dictionary<int, int> { { 1, l1 }, { 2, l2 }, { 3, l3 } };
}