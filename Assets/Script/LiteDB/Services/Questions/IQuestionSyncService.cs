using System.Collections.Generic;
using System.Threading.Tasks;
using QuestionSystem;

public interface IQuestionSyncService
{
    /// <summary>
    /// Inicializa o serviço: verifica o cache LiteDB e sincroniza com o Firestore se necessário.
    /// Deve ser chamado uma vez durante AppContext.InitializeServices().
    /// Retorna true se as questões estão disponíveis (online ou offline).
    /// </summary>
    Task<bool> InitializeAsync();

    /// <summary>
    /// Retorna as questões de um banco específico a partir do cache LiteDB (síncrono).
    /// Requer que InitializeAsync() tenha sido concluído antes.
    /// </summary>
    List<Question> GetQuestionsForDatabankName(string databankName);

    /// <summary>Indica se uma sincronização com o Firestore está em andamento.</summary>
    bool IsSyncing { get; }

    /// <summary>Indica se o cache local está disponível e pronto para uso.</summary>
    bool IsCacheReady { get; }
}
