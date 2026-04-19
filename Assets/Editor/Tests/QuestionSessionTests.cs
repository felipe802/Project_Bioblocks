// Assets/Editor/Tests/QuestionSessionTests.cs
// Testes unitários para QuestionSession.
//
// QuestionSession é um POJO puro (não herda de MonoBehaviour),
// portanto todos os testes rodam em Edit Mode sem setup de cena.

using NUnit.Framework;
using System;
using System.Collections.Generic;
using QuestionSystem;

[TestFixture]
public class QuestionSessionTests
{
    // =======================================================
    // Helpers locais
    // =======================================================

    private static List<Question> MakeSession(int count, string dbName = "TestDB")
    {
        var list = new List<Question>();
        for (int i = 1; i <= count; i++)
            list.Add(QuestionTestHelpers.MakeQuestion(i, databankName: dbName));
        return list;
    }

    // =======================================================
    // Construtor
    // =======================================================

    [Test]
    public void Constructor_ListaNula_HasMoreQuestionsEFalse()
    {
        var session = new QuestionSession(null);
        Assert.IsFalse(session.HasMoreQuestions);
    }

    [Test]
    public void Constructor_ListaNula_TotalQuestionsZero()
    {
        var session = new QuestionSession(null);
        Assert.AreEqual(0, session.GetTotalQuestions());
    }

    [Test]
    public void Constructor_ListaVazia_HasMoreQuestionsEFalse()
    {
        var session = new QuestionSession(new List<Question>());
        Assert.IsFalse(session.HasMoreQuestions);
    }

    [Test]
    public void Constructor_ListaValida_IndiceIniciaEmZero()
    {
        var session = new QuestionSession(MakeSession(3));
        Assert.AreEqual(0, session.CurrentQuestionIndex);
    }

    [Test]
    public void Constructor_ListaValida_DatabankNameVemDaPrimeiraQuestao()
    {
        var questions = MakeSession(3, dbName: "BioQuestions");
        var session   = new QuestionSession(questions);
        Assert.AreEqual("BioQuestions", session.DatabankName);
    }

    [Test]
    public void Constructor_ListaNula_DatabankNameENuloOuVazio()
    {
        var session = new QuestionSession(null);
        // DatabankName não deve lançar exceção e deve ser null ou vazio
        Assert.IsTrue(string.IsNullOrEmpty(session.DatabankName));
    }

    // =======================================================
    // GetCurrentQuestion
    // =======================================================

    [Test]
    public void GetCurrentQuestion_IndiceZero_RetornaPrimeiraQuestao()
    {
        var session = new QuestionSession(MakeSession(3));
        Assert.AreEqual(1, session.GetCurrentQuestion().questionNumber);
    }

    [Test]
    public void GetCurrentQuestion_AposNextQuestion_RetornaSegunda()
    {
        var session = new QuestionSession(MakeSession(3));
        session.NextQuestion();
        Assert.AreEqual(2, session.GetCurrentQuestion().questionNumber);
    }

    [Test]
    public void GetCurrentQuestion_ListaVazia_LancaInvalidOperationException()
    {
        var session = new QuestionSession(new List<Question>());
        Assert.Throws<InvalidOperationException>(() => session.GetCurrentQuestion());
    }

    [Test]
    public void GetCurrentQuestion_AposUltimaQuestao_LancaInvalidOperationException()
    {
        var session = new QuestionSession(MakeSession(2));
        session.NextQuestion(); // índice 1
        session.NextQuestion(); // índice 2 → fora da lista
        Assert.Throws<InvalidOperationException>(() => session.GetCurrentQuestion());
    }

    // =======================================================
    // NextQuestion
    // =======================================================

    [Test]
    public void NextQuestion_AvancaIndiceCorretamente()
    {
        var session = new QuestionSession(MakeSession(3));
        session.NextQuestion();
        Assert.AreEqual(1, session.CurrentQuestionIndex);
    }

    [Test]
    public void NextQuestion_NaoUltrapassaLimiteDaLista()
    {
        var session = new QuestionSession(MakeSession(2));

        // Chama mais vezes do que existem questões
        session.NextQuestion();
        session.NextQuestion();
        session.NextQuestion();
        session.NextQuestion();

        // Índice máximo deve ser Count (2), nunca acima disso
        Assert.AreEqual(2, session.CurrentQuestionIndex,
            "NextQuestion não deve ultrapassar o tamanho da lista");
    }

    [Test]
    public void NextQuestion_AposTodasRespondidas_HasMoreQuestionsEFalse()
    {
        var session = new QuestionSession(MakeSession(2));
        session.NextQuestion();
        session.NextQuestion();
        Assert.IsFalse(session.HasMoreQuestions);
    }

    // =======================================================
    // HasMoreQuestions
    // =======================================================

    [Test]
    public void HasMoreQuestions_NoInicio_ETrue()
    {
        var session = new QuestionSession(MakeSession(3));
        Assert.IsTrue(session.HasMoreQuestions);
    }

    [Test]
    public void HasMoreQuestions_AposTodasAsQuestoes_EFalse()
    {
        var session = new QuestionSession(MakeSession(1));
        session.NextQuestion();
        Assert.IsFalse(session.HasMoreQuestions);
    }

    // =======================================================
    // IsLastQuestion
    // =======================================================

    [Test]
    public void IsLastQuestion_SessaoComUmaQuestao_TrueDesdeOInicio()
    {
        var session = new QuestionSession(MakeSession(1));
        Assert.IsTrue(session.IsLastQuestion());
    }

    [Test]
    public void IsLastQuestion_NaoPrimeiraQuestao_RetornaFalse()
    {
        var session = new QuestionSession(MakeSession(3));
        Assert.IsFalse(session.IsLastQuestion());
    }

    [Test]
    public void IsLastQuestion_NaUltimaQuestao_RetornaTrue()
    {
        var session = new QuestionSession(MakeSession(3));
        session.NextQuestion(); // índice 1
        session.NextQuestion(); // índice 2 = última (0-based, count=3)
        Assert.IsTrue(session.IsLastQuestion());
    }

    // =======================================================
    // Reset
    // =======================================================

    [Test]
    public void Reset_ApesAvancar_VoltaAoIndiceZero()
    {
        var session = new QuestionSession(MakeSession(3));
        session.NextQuestion();
        session.NextQuestion();

        session.Reset();

        Assert.AreEqual(0, session.CurrentQuestionIndex);
    }

    [Test]
    public void Reset_RestaurasHasMoreQuestions()
    {
        var session = new QuestionSession(MakeSession(2));
        session.NextQuestion();
        session.NextQuestion(); // esgota a lista

        session.Reset();

        Assert.IsTrue(session.HasMoreQuestions);
    }

    [Test]
    public void Reset_PermiteRelerPrimeiraQuestao()
    {
        var session   = new QuestionSession(MakeSession(2));
        var primeira  = session.GetCurrentQuestion().questionNumber;
        session.NextQuestion();
        session.Reset();

        Assert.AreEqual(primeira, session.GetCurrentQuestion().questionNumber);
    }

    // =======================================================
    // GetTotalQuestions
    // =======================================================

    [Test]
    public void GetTotalQuestions_RetornaContagemCorreta()
    {
        var session = new QuestionSession(MakeSession(5));
        Assert.AreEqual(5, session.GetTotalQuestions());
    }

    [Test]
    public void GetTotalQuestions_NaoMudaAposNextQuestion()
    {
        var session = new QuestionSession(MakeSession(3));
        session.NextQuestion();
        session.NextQuestion();
        Assert.AreEqual(3, session.GetTotalQuestions(),
            "GetTotalQuestions deve permanecer fixo independente do índice atual");
    }
}