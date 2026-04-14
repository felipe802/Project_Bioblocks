using System.Collections.Generic;
using System.Threading.Tasks;
using QuestionSystem;

/// <summary>
/// Fake do IQuestionSyncService para testes unitários.
/// Usado para testar componentes que dependem do QuestionSyncService
/// (ex: FirestoreQuestionDatabase) sem precisar do Firestore ou LiteDB.
///
/// Como usar:
///   var fakeSync = new FakeQuestionSyncService();
///   fakeSync.IsCacheReady = true;
///   fakeSync.SetQuestionsForDatabankName("AcidBaseBufferQuestionDatabase",
///       QuestionTestHelpers.MakeQuestions(nivel1: 5, nivel2: 3,
///           databankName: "AcidBaseBufferQuestionDatabase"));
///
///   // Injeta via AppContext.OverrideForTests (se necessário) ou diretamente
/// </summary>
public class FakeQuestionSyncService : IQuestionSyncService
{
    // ── Estado configurável ────────────────────────────────────────────────────
    public bool IsSyncing    { get; set; } = false;
    public bool IsCacheReady { get; set; } = true;

    /// <summary>Valor retornado por InitializeAsync().</summary>
    public bool InitializeReturnValue { get; set; } = true;

    // ── Dados configuráveis por banco ──────────────────────────────────────────
    private readonly Dictionary<string, List<Question>> _questionsByDatabankName =
        new Dictionary<string, List<Question>>();

    /// <summary>
    /// Define as questões retornadas para um banco específico.
    /// </summary>
    public void SetQuestionsForDatabankName(string databankName, List<Question> questions)
    {
        _questionsByDatabankName[databankName] = questions ?? new List<Question>();
    }

    /// <summary>
    /// Atalho: define questões para vários bancos de uma vez.
    /// </summary>
    public void SetQuestionsForAllDatabankNames(Dictionary<string, List<Question>> allQuestions)
    {
        _questionsByDatabankName.Clear();
        foreach (var kvp in allQuestions)
            _questionsByDatabankName[kvp.Key] = kvp.Value ?? new List<Question>();
    }

    // ── Rastreamento de chamadas ───────────────────────────────────────────────
    public bool   InitializeAsyncWasCalled             { get; private set; }
    public int    GetQuestionsForDatabankNameCallCount  { get; private set; }
    public string LastRequestedDatabankName             { get; private set; }

    // ── IQuestionSyncService ───────────────────────────────────────────────────

    public Task<bool> InitializeAsync()
    {
        InitializeAsyncWasCalled = true;
        IsCacheReady = InitializeReturnValue;
        return Task.FromResult(InitializeReturnValue);
    }

    public List<Question> GetQuestionsForDatabankName(string databankName)
    {
        GetQuestionsForDatabankNameCallCount++;
        LastRequestedDatabankName = databankName;

        if (_questionsByDatabankName.TryGetValue(databankName, out var questions))
            return new List<Question>(questions);

        return new List<Question>();
    }

    // ── Utilitário ─────────────────────────────────────────────────────────────

    /// <summary>Zera contadores e estado para reutilização entre testes.</summary>
    public void Reset()
    {
        IsSyncing            = false;
        IsCacheReady         = true;
        InitializeReturnValue= true;
        InitializeAsyncWasCalled = false;
        GetQuestionsForDatabankNameCallCount = 0;
        LastRequestedDatabankName = null;
        _questionsByDatabankName.Clear();
    }
}
