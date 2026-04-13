// Assets/Editor/Tests/QuestionCounterManagerTests.cs
// Testes unitários para QuestionCounterManager — lógica de nível e contagem.
//
// QuestionCounterManager tem dois blocos distintos:
//   (A) Lógica pura: cálculo de nível, GetLevelName, GetCurrentLevelProgress
//       → testável em Edit Mode sem UI
//   (B) Renderização: atualiza TextMeshProUGUI e ProgressBarManager
//       → não testada aqui (depende de SerializeField)
//
// Para rodar: Window → General → Test Runner → EditMode → Run All

using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using QuestionSystem;

[TestFixture]
public class QuestionCounterManagerTests
{
    private GameObject           _managerGO;
    private QuestionCounterManager _manager;

    [SetUp]
    public void Setup()
    {
        _managerGO = new GameObject("QuestionCounterManager");
        _manager   = _managerGO.AddComponent<QuestionCounterManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_managerGO);
    }

    // -------------------------------------------------------
    // Helper
    // -------------------------------------------------------

    private static List<Question> MakeQuestions(int nivel1, int nivel2 = 0, int nivel3 = 0)
        => QuestionTestHelpers.MakeQuestions(nivel1, nivel2, nivel3);

    private static List<string> Answered(params int[] numbers)
    {
        var list = new List<string>();
        foreach (var n in numbers) list.Add(n.ToString());
        return list;
    }

    // =======================================================
    // Initialize + GetCurrentLevelProgress — estado inicial
    // =======================================================

    [Test]
    public void GetCurrentLevelProgress_SemInitialize_RetornaNull()
    {
        // Antes de Initialize, não há nível atual
        var progress = _manager.GetCurrentLevelProgress();
        Assert.IsNull(progress);
    }

    [Test]
    public void GetCurrentLevelProgress_AposInitialize_NaoENull()
    {
        var questions = MakeQuestions(nivel1: 3);
        _manager.Initialize(questions, new List<string>());

        // Precisamos chamar UpdateCounter para definir o nível atual
        _manager.UpdateCounter(questions[0]);

        var progress = _manager.GetCurrentLevelProgress();
        Assert.IsNotNull(progress);
    }

    // =======================================================
    // GetCurrentLevelProgress — cálculo de progresso
    // =======================================================

    [Test]
    public void GetCurrentLevelProgress_SemRespostas_AnsweredZero()
    {
        var questions = MakeQuestions(nivel1: 4);
        _manager.Initialize(questions, new List<string>());
        _manager.UpdateCounter(questions[0]);

        var progress = _manager.GetCurrentLevelProgress();

        Assert.AreEqual(0, progress.AnsweredQuestions);
        Assert.AreEqual(4, progress.TotalQuestions);
        Assert.AreEqual(0f, progress.ProgressPercentage, delta: 0.01f);
    }

    [Test]
    public void GetCurrentLevelProgress_MetadeRespondida_Percentual50()
    {
        var questions = MakeQuestions(nivel1: 4);
        _manager.Initialize(questions, Answered(1, 2)); // 2 de 4 = 50%
        _manager.UpdateCounter(questions[0]);

        var progress = _manager.GetCurrentLevelProgress();

        Assert.AreEqual(2,   progress.AnsweredQuestions);
        Assert.AreEqual(50f, progress.ProgressPercentage, delta: 0.01f);
    }

    [Test]
    public void GetCurrentLevelProgress_TodasRespondidas_Percentual100()
    {
        var questions = MakeQuestions(nivel1: 3);
        _manager.Initialize(questions, Answered(1, 2, 3));
        _manager.UpdateCounter(questions[0]);

        var progress = _manager.GetCurrentLevelProgress();

        Assert.AreEqual(100f, progress.ProgressPercentage, delta: 0.01f);
        Assert.IsTrue(progress.AnsweredQuestions >= progress.TotalQuestions);
    }

    [Test]
    public void GetCurrentLevelProgress_Level_CorrespondeAoNivelDaQuestaoAtual()
    {
        var questions = MakeQuestions(nivel1: 2, nivel2: 2);
        _manager.Initialize(questions, new List<string>());
        _manager.UpdateCounter(questions[0]); // questão de nível 1

        var progress = _manager.GetCurrentLevelProgress();

        Assert.AreEqual(1, progress.Level);
    }

    // =======================================================
    // LevelName
    // =======================================================

    [Test]
    public void GetCurrentLevelProgress_Nivel1_LevelNameBasico()
    {
        var questions = MakeQuestions(nivel1: 2);
        _manager.Initialize(questions, new List<string>());
        _manager.UpdateCounter(questions[0]);

        var progress = _manager.GetCurrentLevelProgress();

        Assert.AreEqual("Nível Básico", progress.LevelName);
    }

    [Test]
    public void GetCurrentLevelProgress_Nivel2_LevelNameIntermediario()
    {
        var questions = MakeQuestions(nivel1: 0, nivel2: 2);
        _manager.Initialize(questions, new List<string>());
        _manager.UpdateCounter(questions[0]); // questão de nível 2

        var progress = _manager.GetCurrentLevelProgress();

        Assert.AreEqual("Nível Intermediário", progress.LevelName);
    }

    [Test]
    public void GetCurrentLevelProgress_Nivel3_LevelNameDificil()
    {
        var questions = MakeQuestions(nivel1: 0, nivel2: 0, nivel3: 2);
        _manager.Initialize(questions, new List<string>());
        _manager.UpdateCounter(questions[0]); // questão de nível 3

        var progress = _manager.GetCurrentLevelProgress();

        Assert.AreEqual("Nível Difícil", progress.LevelName);
    }

    // =======================================================
    // MarkQuestionAsAnswered
    // =======================================================

    [Test]
    public void MarkQuestionAsAnswered_AdicionaAoConjuntoDeRespondidas()
    {
        var questions = MakeQuestions(nivel1: 3);
        _manager.Initialize(questions, new List<string>());
        _manager.UpdateCounter(questions[0]);

        _manager.MarkQuestionAsAnswered(1);
        _manager.UpdateCounter(questions[0]); // re-renderiza com novo estado

        var progress = _manager.GetCurrentLevelProgress();
        Assert.AreEqual(1, progress.AnsweredQuestions);
    }

    [Test]
    public void MarkQuestionAsAnswered_MesmaQuestaoduasVezes_NaoDuplica()
    {
        var questions = MakeQuestions(nivel1: 3);
        _manager.Initialize(questions, new List<string>());

        _manager.MarkQuestionAsAnswered(1);
        _manager.MarkQuestionAsAnswered(1); // segunda vez não deve duplicar

        _manager.UpdateCounter(questions[0]);
        var progress = _manager.GetCurrentLevelProgress();

        Assert.AreEqual(1, progress.AnsweredQuestions,
            "A mesma questão não deve ser contada duas vezes");
    }

    // =======================================================
    // UpdateAnsweredQuestions
    // =======================================================

    [Test]
    public void UpdateAnsweredQuestions_SubstituiListaAnterior()
    {
        var questions = MakeQuestions(nivel1: 4);
        _manager.Initialize(questions, Answered(1)); // começa com 1 respondida
        _manager.UpdateCounter(questions[0]);

        _manager.UpdateAnsweredQuestions(Answered(1, 2, 3)); // atualiza para 3
        _manager.UpdateCounter(questions[0]);

        var progress = _manager.GetCurrentLevelProgress();
        Assert.AreEqual(3, progress.AnsweredQuestions);
    }

    [Test]
    public void UpdateAnsweredQuestions_ListaNula_NaoCrasha()
    {
        var questions = MakeQuestions(nivel1: 3);
        _manager.Initialize(questions, new List<string>());

        Assert.DoesNotThrow(() => _manager.UpdateAnsweredQuestions(null));
    }

    // =======================================================
    // UpdateCounter — questão com nível 0 tratada como nível 1
    // =======================================================

    [Test]
    public void UpdateCounter_QuestaoSemNivelDefinido_TratadaComoNivel1()
    {
        var questionSemNivel = QuestionTestHelpers.MakeQuestion(99, level: 0);
        var questions = new List<Question>
        {
            QuestionTestHelpers.MakeQuestion(1, level: 1),
            questionSemNivel
        };

        _manager.Initialize(questions, new List<string>());

        Assert.DoesNotThrow(() => _manager.UpdateCounter(questionSemNivel),
            "Questão com level=0 não deve causar exceção");
    }

    [Test]
    public void UpdateCounter_QuestaoNula_NaoCrasha()
    {
        var questions = MakeQuestions(nivel1: 2);
        _manager.Initialize(questions, new List<string>());

        Assert.DoesNotThrow(() => _manager.UpdateCounter(null));
    }

    // =======================================================
    // LevelProgressInfo.ToString
    // =======================================================

    [Test]
    public void LevelProgressInfo_ToString_FormatoCorreto()
    {
        var info = new LevelProgressInfo
        {
            Level              = 1,
            LevelName          = "Nível Básico",
            AnsweredQuestions  = 2,
            TotalQuestions     = 4,
            ProgressPercentage = 50f
        };

        string result = info.ToString();

        StringAssert.Contains("Nível Básico", result);
        StringAssert.Contains("2",            result);
        StringAssert.Contains("4",            result);
        StringAssert.Contains("50",           result);
    }
}