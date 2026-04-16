using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Gerencia a cena principal de seleção de trilhas (Pathway).
/// </summary>
public class PathwayManager : MonoBehaviour
{
    [SerializeField] private NavigationManager navigationManager;

    // -------------------------------------------------------
    // Dependências — obtidas do AppContext no Start()
    // -------------------------------------------------------
    private IFirestoreRepository _firestore;
    private IStatisticsProvider _statistics;

    private void Start()
    {
        _firestore  = AppContext.Firestore;
        _statistics = AppContext.Statistics;

        if (UserDataStore.CurrentUserData == null)
        {
            SceneManager.LoadScene("Login");
            return;
        }

        InitializeTopBar();

        AnsweredQuestionsManager.OnAnsweredQuestionsUpdated += HandleAnsweredQuestionsUpdated;

        // AppContext.Statistics já foi inicializado antes de IsReady = true.
        // Se por algum motivo ainda não estiver pronto, inicializa e aguarda.
        if (_statistics.IsInitialized)
        {
            UpdateAnsweredQuestionsPercentages();
        }
        else
        {
            StartCoroutine(InitializeDatabaseStatistics());
        }
    }

    private void OnDestroy()
    {
        AnsweredQuestionsManager.OnAnsweredQuestionsUpdated -= HandleAnsweredQuestionsUpdated;
    }

    // -------------------------------------------------------
    // TopBar
    // -------------------------------------------------------

    private void InitializeTopBar()
    {
        if (TopBarManager.Instance != null)
        {
            TopBarManager.Instance.AddSceneToButtonVisibility("HubButton", "ProfileScene");
            TopBarManager.Instance.AddSceneToButtonVisibility("EngineButton", "ProfileScene");
        }
    }

    // -------------------------------------------------------
    // Estatísticas — fallback caso não estejam prontas ainda
    // -------------------------------------------------------

    private IEnumerator InitializeDatabaseStatistics()
    {
        yield return null;
        var task = (_statistics as DatabaseStatisticsManager)?.Initialize();
        if (task == null) yield break;
        while (!task.IsCompleted) yield return null;
        UpdateAnsweredQuestionsPercentages();
    }

    // -------------------------------------------------------
    // Atualização de progresso por banco de questões
    // -------------------------------------------------------

    private void HandleAnsweredQuestionsUpdated(Dictionary<string, int> answeredCounts)
    {
        if (this == null) return;

        if (UserDataStore.CurrentUserData != null)
        {
            string userId = UserDataStore.CurrentUserData.UserId;
            foreach (var kvp in answeredCounts)
                AnsweredQuestionsListStore.UpdateAnsweredQuestionsCount(userId, kvp.Key, kvp.Value);
        }

        UpdateAnsweredQuestionsPercentages();
    }

    private void UpdateAnsweredQuestionsPercentages()
    {
        if (UserDataStore.CurrentUserData == null) return;

        string userId    = UserDataStore.CurrentUserData.UserId;
        var userCounts   = AnsweredQuestionsListStore.GetAnsweredQuestionsCountForUser(userId);

        string[] allDatabases =
        {
            "AcidBaseBufferQuestionDatabase",
            "AminoacidQuestionDatabase",
            "BiochemistryIntroductionQuestionDatabase",
            "CarbohydratesQuestionDatabase",
            "EnzymeQuestionDatabase",
            "LipidsQuestionDatabase",
            "MembranesQuestionDatabase",
            "NucleicAcidsQuestionDatabase",
            "ProteinQuestionDatabase",
            "WaterQuestionDatabase"
        };

        foreach (string databankName in allDatabases)
        {
            int count          = userCounts.ContainsKey(databankName) ? userCounts[databankName] : 0;
            int totalQuestions = QuestionBankStatistics.GetTotalQuestions(databankName);

            // Estatísticas ainda não carregadas — pula para não sobrescrever com 0%
            // O CircularProgressIndicator será atualizado pelo OnAnsweredQuestionsUpdated
            // quando as estatísticas estiverem prontas
            if (totalQuestions <= 0)
            {
                // Só seta 0% explicitamente se confirmamos que não há questões respondidas
                if (count == 0)
                {
                    string progressObjectName0 = $"{databankName}Porcentage";
                    var go0 = GameObject.Find(progressObjectName0);
                    go0?.GetComponent<CircularProgressIndicator>()?.SetProgress(0);
                }
                continue;
            }

            int percentageAnswered = Mathf.Min((count * 100) / totalQuestions, 100);

            string progressObjectName = $"{databankName}Porcentage";
            GameObject progressObject = GameObject.Find(progressObjectName);

            if (progressObject != null)
            {
                var progressIndicator = progressObject.GetComponent<CircularProgressIndicator>();
                if (progressIndicator != null)
                    progressIndicator.SetProgress(percentageAnswered);
                else
                    Debug.LogWarning($"CircularProgressIndicator não encontrado em {progressObjectName}");
            }
            else
            {
                Debug.LogWarning($"[PathwayManager] GameObject {progressObjectName} não encontrado");
            }
        }
    }

    // -------------------------------------------------------
    // Navegação
    // -------------------------------------------------------

    public void Navigate(string sceneName)
    {
        if (navigationManager != null)
            navigationManager.NavigateTo(sceneName);
        else
            Debug.LogError("[PathwayManager] NavigationManager não configurado no Inspector.");
    }
}