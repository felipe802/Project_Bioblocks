using System.Collections.Generic;
using QuestionSystem;
using UnityEngine;

/// <summary>
/// IQuestionSource que delega para o IQuestionSyncService (Firestore + LiteDB).
/// Usada no ambiente de Produção.
/// Requer que AppContext.QuestionSync já tenha sido inicializado.
/// </summary>
public class FirestoreQuestionSource : IQuestionSource
{
    private readonly IQuestionSyncService _sync;

    public FirestoreQuestionSource(IQuestionSyncService sync)
    {
        _sync = sync;
    }

    public List<Question> GetQuestionsForDatabankName(string databankName)
    {
        if (_sync == null)
        {
            Debug.LogError("[FirestoreQuestionSource] IQuestionSyncService é null.");
            return new List<Question>();
        }

        return _sync.GetQuestionsForDatabankName(databankName) ?? new List<Question>();
    }
}
