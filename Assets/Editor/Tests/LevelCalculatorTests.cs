// Assets/Editor/Tests/LevelCalculatorTests.cs
//
// Dependências: QuestionTestHelpers.cs (na mesma pasta Helpers/)
// Sem dependências de Unity runtime — roda em Edit Mode puro.

using NUnit.Framework;
using System.Collections.Generic;
using QuestionSystem;

[TestFixture]
public class LevelCalculatorTests
{
    // =======================================================
    // CalculateCurrentLevel — casos de entrada inválida
    // =======================================================

    [Test]
    public void CalculateCurrentLevel_ListaNula_RetornaUm()
    {
        int result = LevelCalculator.CalculateCurrentLevel(null, null);
        Assert.AreEqual(1, result, "Lista nula deve retornar nível 1");
    }

    [Test]
    public void CalculateCurrentLevel_ListaVazia_RetornaUm()
    {
        int result = LevelCalculator.CalculateCurrentLevel(new List<Question>(), new List<string>());
        Assert.AreEqual(1, result, "Lista vazia deve retornar nível 1");
    }

    [Test]
    public void CalculateCurrentLevel_RespostasNulas_NaoCrasha()
    {
        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 2);

        // answeredQuestionsFromFirebase = null não deve lançar exceção
        Assert.DoesNotThrow(() =>
            LevelCalculator.CalculateCurrentLevel(questions, null));
    }

    // =======================================================
    // CalculateCurrentLevel — progressão de níveis
    // =======================================================

    [Test]
    public void CalculateCurrentLevel_SemRespostas_RetornaUm()
    {
        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 3, nivel2: 3);

        int result = LevelCalculator.CalculateCurrentLevel(questions, new List<string>());

        Assert.AreEqual(1, result);
    }

    [Test]
    public void CalculateCurrentLevel_Nivel1ParcialmteRespondido_RetornaUm()
    {
        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 4, nivel2: 2);

        // Só 2 das 4 questões do nível 1 respondidas
        var answered = new List<string> { "1", "2" };

        int result = LevelCalculator.CalculateCurrentLevel(questions, answered);

        Assert.AreEqual(1, result);
    }

    [Test]
    public void CalculateCurrentLevel_Nivel1Completo_RetornaDois()
    {
        var questions   = QuestionTestHelpers.MakeQuestions(nivel1: 2, nivel2: 2);
        var nivel1Ids   = QuestionTestHelpers.ToAnsweredIdsForLevel(questions, 1);

        int result = LevelCalculator.CalculateCurrentLevel(questions, nivel1Ids);

        Assert.AreEqual(2, result);
    }

    [Test]
    public void CalculateCurrentLevel_Niveis1e2Completos_RetornaTres()
    {
        var questions      = QuestionTestHelpers.MakeQuestions(nivel1: 2, nivel2: 2, nivel3: 2);
        var nivel1e2Ids    = QuestionTestHelpers.ToAnsweredIdsForLevel(questions, 1);
        nivel1e2Ids.AddRange(QuestionTestHelpers.ToAnsweredIdsForLevel(questions, 2));

        int result = LevelCalculator.CalculateCurrentLevel(questions, nivel1e2Ids);

        Assert.AreEqual(3, result);
    }

    [Test]
    public void CalculateCurrentLevel_TodosNiveisCompletos_RetornaMaiorNivel()
    {
        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 2, nivel2: 2, nivel3: 2);
        var allIds    = QuestionTestHelpers.ToAnsweredIds(questions);

        int result = LevelCalculator.CalculateCurrentLevel(questions, allIds);

        // Deve retornar o maior nível (3), não estourar para 4
        Assert.AreEqual(3, result);
    }

    // =======================================================
    // CalculateCurrentLevel — edge case: questionLevel = 0
    // =======================================================

    [Test]
    public void CalculateCurrentLevel_QuestaoSemNivelDefinido_TratadaComoNivelUm()
    {
        // questionLevel = 0 → a classe deve tratar como nível 1
        var questions = new List<Question>
        {
            QuestionTestHelpers.MakeQuestion(1, level: 0), // sem nível definido
            QuestionTestHelpers.MakeQuestion(2, level: 2)
        };

        int result = LevelCalculator.CalculateCurrentLevel(questions, new List<string>());

        Assert.AreEqual(1, result, "Questão sem nível (0) deve ser tratada como nível 1");
    }

    [Test]
    public void CalculateCurrentLevel_QuestaoSemNivelRespondida_AvancaParaNivelDois()
    {
        var questions = new List<Question>
        {
            QuestionTestHelpers.MakeQuestion(1, level: 0), // nível 0 → trata como 1
            QuestionTestHelpers.MakeQuestion(2, level: 2)
        };

        // Questão 1 respondida → deve avançar para nível 2
        int result = LevelCalculator.CalculateCurrentLevel(questions, new List<string> { "1" });

        Assert.AreEqual(2, result);
    }

    // =======================================================
    // IsLevelComplete
    // =======================================================

    [Test]
    public void IsLevelComplete_NivelSemQuestoes_RetornaTrue()
    {
        // Banco só com nível 1 — nível 2 inexistente deve ser considerado "completo"
        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 2);

        bool result = LevelCalculator.IsLevelComplete(questions, new List<string>(), level: 2);

        Assert.IsTrue(result, "Nível sem questões deve ser considerado completo");
    }

    [Test]
    public void IsLevelComplete_NivelParcialmteRespondido_RetornaFalse()
    {
        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 3);
        var answered  = new List<string> { "1", "2" }; // só 2 de 3

        bool result = LevelCalculator.IsLevelComplete(questions, answered, level: 1);

        Assert.IsFalse(result);
    }

    [Test]
    public void IsLevelComplete_NivelTotalmenteRespondido_RetornaTrue()
    {
        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 3);
        var answered  = QuestionTestHelpers.ToAnsweredIdsForLevel(questions, 1);

        bool result = LevelCalculator.IsLevelComplete(questions, answered, level: 1);

        Assert.IsTrue(result);
    }

    // =======================================================
    // GetLevelStats
    // =======================================================

    [Test]
    public void GetLevelStats_ListaVazia_RetornaDicionarioVazio()
    {
        var stats = LevelCalculator.GetLevelStats(new List<Question>(), new List<string>());
        Assert.AreEqual(0, stats.Count);
    }

    [Test]
    public void GetLevelStats_SemRespostas_PercentualZero()
    {
        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 4);
        var stats     = LevelCalculator.GetLevelStats(questions, new List<string>());

        Assert.AreEqual(0f, stats[1].ProgressPercentage, delta: 0.01f);
        Assert.AreEqual(0,  stats[1].AnsweredQuestions);
        Assert.IsFalse(stats[1].IsComplete);
    }

    [Test]
    public void GetLevelStats_MetadeRespondida_Percentual50()
    {
        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 4);
        var answered  = new List<string> { "1", "2" }; // 2 de 4 = 50%

        var stats = LevelCalculator.GetLevelStats(questions, answered);

        Assert.AreEqual(50f, stats[1].ProgressPercentage, delta: 0.01f);
    }

    [Test]
    public void GetLevelStats_Nivel1Completo_IsCompleteTrue()
    {
        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 2, nivel2: 2);
        var nivel1Ids = QuestionTestHelpers.ToAnsweredIdsForLevel(questions, 1);

        var stats = LevelCalculator.GetLevelStats(questions, nivel1Ids);

        Assert.IsTrue(stats[1].IsComplete);
        Assert.IsFalse(stats[2].IsComplete);
    }

    [Test]
    public void GetLevelStats_ContieneTodosOsNiveis()
    {
        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 2, nivel2: 3, nivel3: 1);
        var stats     = LevelCalculator.GetLevelStats(questions, new List<string>());

        Assert.IsTrue(stats.ContainsKey(1));
        Assert.IsTrue(stats.ContainsKey(2));
        Assert.IsTrue(stats.ContainsKey(3));
        Assert.AreEqual(2, stats[1].TotalQuestions);
        Assert.AreEqual(3, stats[2].TotalQuestions);
        Assert.AreEqual(1, stats[3].TotalQuestions);
    }

    // =======================================================
    // GetMaxLevel
    // =======================================================

    [Test]
    public void GetMaxLevel_ListaNula_RetornaUm()
    {
        Assert.AreEqual(1, LevelCalculator.GetMaxLevel(null));
    }

    [Test]
    public void GetMaxLevel_ListaVazia_RetornaUm()
    {
        Assert.AreEqual(1, LevelCalculator.GetMaxLevel(new List<Question>()));
    }

    [Test]
    public void GetMaxLevel_QuestoesComVariosNiveis_RetornaMaior()
    {
        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 2, nivel2: 2, nivel3: 2);
        Assert.AreEqual(3, LevelCalculator.GetMaxLevel(questions));
    }

    [Test]
    public void GetMaxLevel_ApenasNivelUm_RetornaUm()
    {
        var questions = QuestionTestHelpers.MakeQuestions(nivel1: 5);
        Assert.AreEqual(1, LevelCalculator.GetMaxLevel(questions));
    }
}