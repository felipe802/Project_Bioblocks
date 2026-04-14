using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuestionSystem;

/// <summary>
/// Fake do IFirestoreQuestionRepository para testes unitários.
/// Simula chamadas de rede sem dependência do Firebase SDK.
///
/// Cenários suportados:
///   - Retorno normal com lista configurável de questões
///   - Retorno de lista vazia
///   - Lançamento de exceção (simula sem internet / erro de rede)
///
/// Como usar:
///   var fakeFirestore = new FakeFirestoreQuestionRepository();
///   fakeFirestore.SetQuestions(QuestionTestHelpers.MakeQuestions(10));
///   var sync = new QuestionSyncService();
///   sync.InjectDependencies(fakeFirestore, fakeLocal);
/// </summary>
public class FakeFirestoreQuestionRepository : IFirestoreQuestionRepository
{
    // ── Dados configuráveis ────────────────────────────────────────────────────
    private readonly List<Question> _questions = new List<Question>();

    /// <summary>Substitui a lista de questões retornada pelo fake.</summary>
    public void SetQuestions(List<Question> questions)
    {
        _questions.Clear();
        if (questions != null)
            _questions.AddRange(questions);
    }

    /// <summary>Adiciona questões à lista existente.</summary>
    public void AddQuestions(List<Question> questions)
    {
        if (questions != null)
            _questions.AddRange(questions);
    }

    // ── Comportamento de erro configurável ────────────────────────────────────
    /// <summary>Se true, GetAllQuestions() lança ExceptionToThrow.</summary>
    public bool ShouldThrowOnGetAll { get; set; } = false;

    /// <summary>Se true, GetQuestionsByDatabankName() lança ExceptionToThrow.</summary>
    public bool ShouldThrowOnGetByDatabankName { get; set; } = false;

    /// <summary>Exceção lançada quando ShouldThrow* é true. Padrão: erro de rede genérico.</summary>
    public Exception ExceptionToThrow { get; set; } =
        new Exception("Simulated Firestore network error");

    // ── Rastreamento de chamadas ───────────────────────────────────────────────
    public int GetAllQuestionsCallCount             { get; private set; }
    public int GetQuestionsByDatabankNameCallCount  { get; private set; }

    /// <summary>Lista de argumentos databankName passados para GetQuestionsByDatabankName.</summary>
    public List<string> DatabankNamesRequested      { get; private set; } = new List<string>();

    // ── IFirestoreQuestionRepository ──────────────────────────────────────────

    public Task<List<Question>> GetAllQuestions()
    {
        GetAllQuestionsCallCount++;

        if (ShouldThrowOnGetAll)
            throw ExceptionToThrow;

        return Task.FromResult(new List<Question>(_questions));
    }

    public Task<List<Question>> GetQuestionsByDatabankName(string databankName)
    {
        GetQuestionsByDatabankNameCallCount++;
        DatabankNamesRequested.Add(databankName);

        if (ShouldThrowOnGetByDatabankName)
            throw ExceptionToThrow;

        var filtered = _questions
            .Where(q => q.questionDatabankName == databankName)
            .ToList();

        return Task.FromResult(filtered);
    }

    // ── Utilitário ─────────────────────────────────────────────────────────────

    /// <summary>Zera contadores e configurações de erro para reutilização entre testes.</summary>
    public void Reset()
    {
        _questions.Clear();
        ShouldThrowOnGetAll            = false;
        ShouldThrowOnGetByDatabankName = false;
        GetAllQuestionsCallCount       = 0;
        GetQuestionsByDatabankNameCallCount = 0;
        DatabankNamesRequested.Clear();
    }
}