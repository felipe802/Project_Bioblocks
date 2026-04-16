using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuestionSystem;
using UnityEngine;

public class DatabaseStatisticsManager : MonoBehaviour, IStatisticsProvider
{
    private bool isInitialized  = false;
    private bool isInitializing = false;

    public bool IsInitialized => isInitialized;

    public async Task Initialize()
    {
        if (isInitialized || isInitializing) return;

        isInitializing = true;
        Debug.Log("[DatabaseStatisticsManager] Inicializando...");

        try
        {
            await Task.Yield();
            await LoadAllDatabaseStatistics();

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

    private async Task LoadAllDatabaseStatistics()
    {
        // Mapeamento QuestionSet → databankName
        // Necessário para manter compatibilidade com AnsweredQuestions no Firestore,
        // que ainda usa databankName como chave
        var topicToDatabankName = new Dictionary<QuestionSet, string>
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

        // Aguarda o QuestionSyncService estar pronto
        // (é inicializado antes do DatabaseStatisticsManager no AppContext)
        if (AppContext.QuestionSync == null || !AppContext.QuestionSync.IsCacheReady)
        {
            Debug.LogWarning("[DatabaseStatisticsManager] QuestionSyncService não está pronto.");
            return;
        }

        Dictionary<string, int> questionCounts = new Dictionary<string, int>();

        foreach (var kvp in topicToDatabankName)
        {
            string databankName = kvp.Value;

            try
            {
                // Lê as questões do LiteDB via QuestionSyncService —
                // sem reflection, sem GameObjects temporários
                var questions = AppContext.QuestionSync.GetQuestionsForDatabankName(databankName);
                int count     = questions?.Count ?? 0;

                QuestionBankStatistics.SetTotalQuestions(databankName, count);
                questionCounts[databankName] = count;

                Debug.Log($"[DatabaseStatisticsManager] {databankName}: {count} questões");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DatabaseStatisticsManager] Erro ao carregar {databankName}: {e.Message}");
                questionCounts[databankName] = 0;
            }
        }

        int totalQuestions = 0;
        foreach (var kvp in questionCounts)
            totalQuestions += kvp.Value;

        Debug.Log($"[DatabaseStatisticsManager] Total geral: {totalQuestions} questões");

        if (totalQuestions > 0 && UserDataStore.CurrentUserData != null)
        {
            UserDataStore.UpdateTotalQuestionsInAllDatabanks(totalQuestions);

            await AppContext.Firestore.UpdateUserField(
                UserDataStore.CurrentUserData.UserId,
                "TotalQuestionsInAllDatabanks",
                totalQuestions
            );

            Debug.Log("[DatabaseStatisticsManager] Total salvo no UserData e Firebase");
        }
    }

    public int GetTotalQuestionsCount()
    {
        var topicToDatabankName = new Dictionary<QuestionSet, string>
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

        int total = 0;
        foreach (var kvp in topicToDatabankName)
            total += QuestionBankStatistics.GetTotalQuestions(kvp.Value);

        return total;
    }
}