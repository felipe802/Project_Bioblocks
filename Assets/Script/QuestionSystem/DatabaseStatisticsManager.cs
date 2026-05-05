using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuestionSystem;
using UnityEngine;

/// <summary>
/// Lê o total de questões a partir da fonte única de verdade
/// (Firestore: Config/QuestionStats), mantida pelo Cloud Function
/// <c>syncQuestionStats</c>. O cliente NUNCA escreve nesse documento.
///
/// Preenche o cache estático <see cref="QuestionBankStatistics"/>
/// para manter compatibilidade com a UI existente
/// (CircularProgressIndicator, ProfileManager, PathwayManager etc.).
///
/// Fallback: se Config/QuestionStats estiver indisponível (documento ausente
/// ou erro de rede), conta localmente via QuestionSyncService apenas para a UI.
/// Esse fallback NÃO escreve em Firestore — evita o bug de "denominador
/// encolhido" causado por dispositivos com cache LiteDB desatualizado.
/// </summary>
public class DatabaseStatisticsManager : MonoBehaviour, IStatisticsProvider
{
    private bool isInitialized  = false;
    private bool isInitializing = false;

    /// <summary>
    /// Último snapshot canônico lido de Config/QuestionStats. Pode ser null
    /// caso o documento esteja indisponível — nesse caso o GetTotalQuestionsCount
    /// cai no cache local (QuestionBankStatistics).
    /// </summary>
    private QuestionStats _canonicalStats;

    public bool IsInitialized => isInitialized;

    /// <summary>
    /// Versão do documento Config/QuestionStats que foi aplicada.
    /// Permite que observers detectem invalidação de cache.
    /// </summary>
    public long LastAppliedVersion => _canonicalStats?.Version ?? 0;

    // Mapeamento QuestionSet → databankName.
    // Usado para semear QuestionBankStatistics com zero caso o Cloud Function
    // ainda não tenha populado o banco no documento canônico (UI não quebra).
    private static readonly Dictionary<QuestionSet, string> TopicToDatabankName =
        new Dictionary<QuestionSet, string>
        {
            { QuestionSet.acidsBase,      "AcidBaseBufferQuestionDatabase" },
            { QuestionSet.aminoacids,     "AminoacidQuestionDatabase" },
            { QuestionSet.biochem,        "BiochemistryIntroductionQuestionDatabase" },
            { QuestionSet.carbohydrates,  "CarbohydratesQuestionDatabase" },
            { QuestionSet.enzymes,        "EnzymeQuestionDatabase" },
            { QuestionSet.lipids,         "LipidsQuestionDatabase" },
            { QuestionSet.membranes,      "MembranesQuestionDatabase" },
            { QuestionSet.nucleicAcids,   "NucleicAcidsQuestionDatabase" },
            { QuestionSet.proteins,       "ProteinQuestionDatabase" },
            { QuestionSet.water,          "WaterQuestionDatabase" },
        };

    public async Task Initialize()
    {
        if (isInitialized || isInitializing) return;

        // Em preview mode não há Config/QuestionStats no Firebase — pula.
        // Dev usa o mesmo caminho que Prod: lê de Config/QuestionStats via Firestore.
        var envConfig = EnvironmentConfig.Load();
        if (envConfig?.QuestionPreviewMode == true)
        {
            Debug.Log("[DatabaseStatisticsManager] Preview mode — inicialização ignorada.");

            isInitialized  = true;
            isInitializing = false;
            return;
        }

        isInitializing = true;
        Debug.Log("[DatabaseStatisticsManager] Inicializando...");

        try
        {
            await Task.Yield();
            await LoadCanonicalStats();

            isInitialized  = true;
            isInitializing = false;

            Debug.Log("[DatabaseStatisticsManager] Inicializado com sucesso.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DatabaseStatisticsManager] Erro na inicialização: {e.Message}");
            isInitializing = false;
            isInitialized  = false;
        }
    }

    /// <summary>
    /// Recarrega Config/QuestionStats sob demanda (ex.: após level-up do jogador
    /// ou ao detectar versão nova). Atualiza o cache estático
    /// <see cref="QuestionBankStatistics"/> com os novos contadores por banco.
    /// </summary>
    public async Task ReloadCanonicalStats()
    {
        await LoadCanonicalStats();
    }

    private async Task LoadCanonicalStats()
    {
        // 1) Lê a fonte única de verdade (com timeout para evitar hang por Firestore rules/rede)
        QuestionStats stats = null;
        try
        {
            var statsTask   = AppContext.Firestore.GetQuestionStats();
            var timeoutTask = Task.Delay(6000);

            if (await Task.WhenAny(statsTask, timeoutTask) == timeoutTask)
            {
                Debug.LogWarning("[DatabaseStatisticsManager] GetQuestionStats timeout (6s) — usando fallback local.");
            }
            else
            {
                stats = await statsTask;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DatabaseStatisticsManager] Falha ao ler Config/QuestionStats: {e.Message}");
        }

        if (stats != null && stats.TotalQuestions > 0)
        {
            _canonicalStats = stats;
            ApplyStatsToCache(stats);

            // Espelha no UserDataStore apenas para UI compat — NÃO escreve no Firestore.
            // O campo UserData.TotalQuestionsInAllDatabanks deixa de ser fonte de verdade.
            if (UserDataStore.CurrentUserData != null)
                UserDataStore.UpdateTotalQuestionsInAllDatabanks(stats.TotalQuestions);

            Debug.Log($"[DatabaseStatisticsManager] Stats canônicas aplicadas: " +
                      $"Total={stats.TotalQuestions}, Version={stats.Version}, " +
                      $"Bancos={stats.PerBank?.Count ?? 0}");
            return;
        }

        // 2) Fallback local — Config/QuestionStats indisponível.
        //    Conta via QuestionSyncService apenas para popular a UI.
        //    ATENÇÃO: esse valor NÃO é gravado no Firestore — evita o bug
        //    onde um dispositivo com cache defasado sobrescrevia o total canônico.
        Debug.LogWarning("[DatabaseStatisticsManager] Config/QuestionStats indisponível — usando fallback local (somente UI).");
        LoadFromLocalCacheFallback();
    }

    private void ApplyStatsToCache(QuestionStats stats)
    {
        // Garante uma entrada por banco conhecido (mesmo que zero),
        // para que GetTotalQuestions não retorne "não encontrado".
        foreach (var kvp in TopicToDatabankName)
        {
            string databankName = kvp.Value;
            int count = (stats.PerBank != null &&
                         stats.PerBank.TryGetValue(databankName, out int c)) ? c : 0;
            QuestionBankStatistics.SetTotalQuestions(databankName, count);
        }

        // Bancos extras presentes em PerBank que não estão no enum local
        // (ex.: novo banco liberado server-side) — também ficam cacheados.
        if (stats.PerBank != null)
        {
            foreach (var kvp in stats.PerBank)
            {
                if (!QuestionBankStatistics.HasStatistics(kvp.Key))
                    QuestionBankStatistics.SetTotalQuestions(kvp.Key, kvp.Value);
            }
        }
    }

    private void LoadFromLocalCacheFallback()
    {
        if (AppContext.QuestionSync == null || !AppContext.QuestionSync.IsCacheReady)
        {
            Debug.LogWarning("[DatabaseStatisticsManager] QuestionSyncService não está pronto — nada a fazer.");
            return;
        }

        int totalQuestions = 0;
        foreach (var kvp in TopicToDatabankName)
        {
            string databankName = kvp.Value;
            try
            {
                var questions = AppContext.QuestionSync.GetQuestionsForDatabankName(databankName);
                int count = questions?.Count ?? 0;
                QuestionBankStatistics.SetTotalQuestions(databankName, count);
                totalQuestions += count;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DatabaseStatisticsManager] Erro ao carregar {databankName}: {e.Message}");
                QuestionBankStatistics.SetTotalQuestions(databankName, 0);
            }
        }

        // Somente UI — UserDataStore reflete, Firestore NÃO.
        if (totalQuestions > 0 && UserDataStore.CurrentUserData != null)
            UserDataStore.UpdateTotalQuestionsInAllDatabanks(totalQuestions);

        Debug.Log($"[DatabaseStatisticsManager] Fallback local aplicado: Total={totalQuestions}");
    }

    /// <summary>
    /// Total canônico de questões — consumido pelo PlayerLevelService e pela UI.
    /// Prioriza o valor canônico lido de Config/QuestionStats; se ausente,
    /// soma o cache local como fallback para não quebrar a UI.
    /// </summary>
    public int GetTotalQuestionsCount()
    {
        if (_canonicalStats != null && _canonicalStats.TotalQuestions > 0)
            return _canonicalStats.TotalQuestions;

        int total = 0;
        foreach (var kvp in TopicToDatabankName)
            total += QuestionBankStatistics.GetTotalQuestions(kvp.Value);

        return total;
    }
}
