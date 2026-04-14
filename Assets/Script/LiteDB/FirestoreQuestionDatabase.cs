using System.Collections.Generic;
using UnityEngine;
using QuestionSystem;

/// <summary>
/// Implementação de IQuestionDatabase que lê questões do cache LiteDB
/// (sincronizado com o Firestore via QuestionSyncService).
///
/// Uso:
///   - Substitui cada um dos 10 MonoBehaviours de banco hardcoded na QuestionScene.
///   - Configure no Inspector: QuestionSet + DatabankName + DisplayName.
///   - A flag DatabaseInDevelopment controla se a sessão exibe questões de dev ou de prod.
///
/// Não há chamada de rede aqui: o QuestionSyncService já garantiu que o LiteDB
/// está populado durante a inicialização do app.
/// </summary>
public class FirestoreQuestionDatabase : MonoBehaviour, IQuestionDatabase
{
    [Header("Configuração do banco")]
    [Tooltip("Conjunto de questões que este componente representa.")]
    [SerializeField] private QuestionSet questionSet;

    [Tooltip("Nome do banco conforme questionDatabankName no Firestore (ex: 'AcidBaseBufferQuestionDatabase').")]
    [SerializeField] private string databankName;

    [Tooltip("Nome de exibição legível ao usuário (ex: 'Ácidos, Bases e Tampões').")]
    [SerializeField] private string displayName;

    [Tooltip("Ativa o modo desenvolvimento: exibe apenas questões com questionInDevelopment = true.")]
    [SerializeField] private bool databaseInDevelopment = false;

    // ── IQuestionDatabase ──────────────────────────────────────────────────────

    public bool IsDatabaseInDevelopment() => databaseInDevelopment;

    public QuestionSet GetQuestionSetType() => questionSet;

    public string GetDatabankName() => databankName;

    public string GetDisplayName() => displayName;

    public List<Question> GetQuestions()
    {
        if (AppContext.QuestionSync == null)
        {
            Debug.LogError("[FirestoreQuestionDatabase] QuestionSyncService não registrado no AppContext.");
            return new List<Question>();
        }

        if (!AppContext.QuestionSync.IsCacheReady)
        {
            Debug.LogError($"[FirestoreQuestionDatabase] Cache não está pronto para '{databankName}'. " +
                           "QuestionSyncService.InitializeAsync() deve ser concluído antes.");
            return new List<Question>();
        }

        return AppContext.QuestionSync.GetQuestionsForDatabankName(databankName);
    }
}
