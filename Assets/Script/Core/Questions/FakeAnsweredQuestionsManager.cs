using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Implementação fake de IAnsweredQuestionsManager para o ambiente Dev.
/// Zero Firestore, zero LiteDB — estado somente em memória, começa do zero.
/// Permite testar o fluxo completo de questões sem contaminar dados reais.
/// </summary>
public class FakeAnsweredQuestionsManager : IAnsweredQuestionsManager
{
    // databankName → conjunto de questionNumbers respondidos (como string, igual ao real)
    private readonly Dictionary<string, HashSet<string>> _answered =
        new Dictionary<string, HashSet<string>>();

    // Sempre pronto — sem inicialização assíncrona.
    public bool IsManagerInitialized => true;

    public Task ForceUpdate()
    {
        Debug.Log("[FakeAnsweredQuestionsManager] ForceUpdate — no-op (memória já atual).");
        return Task.CompletedTask;
    }

    public Task<List<string>> FetchUserAnsweredQuestionsInTargetDatabase(string target)
    {
        if (_answered.TryGetValue(target, out var set))
            return Task.FromResult(new List<string>(set));

        return Task.FromResult(new List<string>());
    }

    public Task MarkQuestionAsAnswered(string databankName, int questionNumber)
    {
        if (!_answered.ContainsKey(databankName))
            _answered[databankName] = new HashSet<string>();

        _answered[databankName].Add(questionNumber.ToString());
        Debug.Log($"[FakeAnsweredQuestionsManager] Marcada em memória: {databankName} #{questionNumber}");
        return Task.CompletedTask;
    }

    public Task<bool> HasRemainingQuestions(string currentDatabase, List<string> currentQuestionList)
    {
        var answered = _answered.ContainsKey(currentDatabase)
            ? _answered[currentDatabase]
            : new HashSet<string>();

        bool hasRemaining = currentQuestionList.Except(answered).Any();
        return Task.FromResult(hasRemaining);
    }

    public void ResetManager()
    {
        _answered.Clear();
        Debug.Log("[FakeAnsweredQuestionsManager] Reset — memória limpa.");
    }
}
