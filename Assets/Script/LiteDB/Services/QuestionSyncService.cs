using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using QuestionSystem;

/// <summary>
/// Serviço que orquestra a sincronização de questões entre o Firestore e o cache LiteDB.
///
/// Estratégia offline-first:
///   - Primeira abertura (sem cache) → baixa tudo do Firestore → salva no LiteDB
///   - Cache válido (< cacheDaysThreshold)  → usa LiteDB diretamente, sem rede
///   - Cache expirado                       → baixa do Firestore em background e atualiza LiteDB
///   - Sem internet + cache presente        → usa LiteDB (modo offline transparente)
///   - Sem internet + sem cache             → IsCacheReady = false (app deve tratar este estado)
///
/// Segue o mesmo padrão de UserDataSyncService.
/// </summary>
public class QuestionSyncService : MonoBehaviour, IQuestionSyncService
{
    [Tooltip("Número de dias antes de considerar o cache de questões desatualizado.")]
    [SerializeField] private float cacheDaysThreshold = 7f;

    private IFirestoreQuestionRepository _firestore;
    private IQuestionLocalRepository     _local;

    public bool IsSyncing    { get; private set; }
    public bool IsCacheReady { get; private set; }

    // ── Injeção de dependências ────────────────────────────────────────────────

    public void InjectDependencies(
        IFirestoreQuestionRepository firestore,
        IQuestionLocalRepository     local)
    {
        _firestore = firestore;
        _local     = local;
    }

    // ── Inicialização ──────────────────────────────────────────────────────────

    public async Task<bool> InitializeAsync()
    {
        if (IsSyncing) return IsCacheReady;
        IsSyncing = true;

        try
        {
            bool hasCache = _local.HasAnyQuestions();

            if (!hasCache)
            {
                // ── Primeira abertura: sem cache local ────────────────────────
                Debug.Log("[QuestionSyncService] Sem cache local — baixando questões do Firestore...");
                bool success = await DownloadAndCacheAll();
                IsCacheReady = success;
                return IsCacheReady;
            }

            // ── Cache existe: verificar validade ──────────────────────────────
            if (IsCacheStale())
            {
                Debug.Log("[QuestionSyncService] Cache expirado — atualizando em background...");
                IsCacheReady = true;   // usa o cache antigo enquanto atualiza
                _ = RefreshCacheInBackground();
            }
            else
            {
                Debug.Log("[QuestionSyncService] Cache válido — usando LiteDB diretamente.");
                IsCacheReady = true;
            }

            return IsCacheReady;
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuestionSyncService] Erro na inicialização: {e.Message}");

            // Último recurso: usa cache antigo se existir
            IsCacheReady = _local.HasAnyQuestions();
            if (IsCacheReady)
                Debug.LogWarning("[QuestionSyncService] Usando cache antigo como fallback.");

            return IsCacheReady;
        }
        finally
        {
            IsSyncing = false;
        }
    }

    // ── Leitura (síncrona, chamada pelos IQuestionDatabase) ────────────────────

    public List<Question> GetQuestionsForDatabankName(string databankName)
    {
        if (!IsCacheReady)
        {
            Debug.LogError("[QuestionSyncService] Cache não está pronto. InitializeAsync() deve ser concluído primeiro.");
            return new List<Question>();
        }

        var questions = _local.GetQuestionsByDatabankName(databankName);
        Debug.Log($"[QuestionSyncService] {questions.Count} questões carregadas do LiteDB para '{databankName}'.");
        return questions;
    }

    // ── Sincronização ──────────────────────────────────────────────────────────

    /// <summary>Baixa todas as questões do Firestore e armazena no LiteDB.</summary>
    private async Task<bool> DownloadAndCacheAll()
    {
        try
        {
            List<Question> questions = await _firestore.GetAllQuestions().ConfigureAwait(false);
            await Task.Yield();  // retorna ao main thread

            if (questions == null || questions.Count == 0)
            {
                Debug.LogWarning("[QuestionSyncService] Firestore retornou lista vazia de questões.");
                return false;
            }

            _local.SaveQuestions(questions);
            Debug.Log($"[QuestionSyncService] {questions.Count} questões cacheadas no LiteDB.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuestionSyncService] Falha ao baixar questões do Firestore: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Atualização em background: não bloqueia o usuário.
    /// Limpa o cache antigo e salva as questões novas.
    /// </summary>
    private async Task RefreshCacheInBackground()
    {
        if (IsSyncing) return;
        IsSyncing = true;

        try
        {
            List<Question> questions = await _firestore.GetAllQuestions().ConfigureAwait(false);
            await Task.Yield();

            if (questions == null || questions.Count == 0)
            {
                Debug.LogWarning("[QuestionSyncService] Refresh em background retornou lista vazia — cache antigo mantido.");
                return;
            }

            _local.ClearAll();
            _local.SaveQuestions(questions);
            Debug.Log($"[QuestionSyncService] Cache atualizado em background com {questions.Count} questões.");
        }
        catch (Exception e)
        {
            // Falha silenciosa: o app continua com o cache antigo
            Debug.LogWarning($"[QuestionSyncService] Refresh em background falhou (usando cache antigo): {e.Message}");
        }
        finally
        {
            IsSyncing = false;
        }
    }

    // ── Utilitários ────────────────────────────────────────────────────────────

    private bool IsCacheStale()
    {
        DateTime latestCache = _local.GetLatestCacheTimestamp();
        if (latestCache == DateTime.MinValue) return true;

        double daysSinceCache = (DateTime.Now - latestCache).TotalDays;
        return daysSinceCache > cacheDaysThreshold;
    }
}
