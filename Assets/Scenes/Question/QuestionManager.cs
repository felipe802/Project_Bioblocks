using UnityEngine;
using System;
using System.Threading.Tasks;
using QuestionSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class QuestionManager : MonoBehaviour
{
    [Header("UI Managers")]
    [SerializeField] private QuestionBottomUIManager questionBottomBarManager;
    [SerializeField] private QuestionUIManager questionUIManager;
    [SerializeField] private QuestionCanvasGroupManager questionCanvasGroupManager;
    [SerializeField] private FeedbackUIElements feedbackElements;
    [SerializeField] private QuestionTransitionManager transitionManager;

    [Header("Game Logic Managers")]
    [SerializeField] private QuestionTimerManager timerManager;
    [SerializeField] private QuestionLoadManager loadManager;
    [SerializeField] private QuestionAnswerManager answerManager;
    [SerializeField] private QuestionScoreManager scoreManager;
    [SerializeField] private QuestionCounterManager counterManager;

    private QuestionSession currentSession;
    private Question nextQuestionToShow;
    private List<Question> allDatabaseQuestions;
    private int maxLevelInDatabase = 1;
    private INavigationService _navigation;
    private ISceneDataService _sceneData;

    // -------------------------------------------------------
    // Ciclo de vida
    // -------------------------------------------------------
    private void Start()
    {
        _navigation = AppContext.Navigation;
        _sceneData  = AppContext.SceneData;

        if (!ValidateManagers())
        {
            Debug.LogError("Falha na validação dos managers necessários.");
            return;
        }

        InitializeAndStartSession();
    }

    private async void InitializeAndStartSession()
    {
        await InitializeSession();

        if (currentSession != null)
        {
            SetupEventHandlers();
            StartQuestion();
        }
        else
        {
            Debug.LogError("QuestionManager: currentSession é null após InitializeSession");
        }
    }

    private void OnDestroy()
    {
        if (timerManager != null)
            timerManager.OnTimerComplete -= HandleTimeUp;

        if (answerManager != null)
            answerManager.OnAnswerSelected -= CheckAnswer;

        if (transitionManager != null)
        {
            transitionManager.OnBeforeTransitionStart -= PrepareNextQuestion;
            transitionManager.OnTransitionMidpoint    -= ApplyPreparedQuestion;
        }
    }

    // -------------------------------------------------------
    // Validação
    // -------------------------------------------------------
    private bool ValidateManagers()
    {
        if (questionBottomBarManager == null)
            Debug.LogError("QuestionManager: questionBottomBarManager é null");
        if (questionUIManager == null)
            Debug.LogError("QuestionManager: questionUIManager é null");
        if (questionCanvasGroupManager == null)
            Debug.LogError("QuestionManager: questionCanvasGroupManager é null");
        if (timerManager == null)
            Debug.LogError("QuestionManager: timerManager é null");
        if (loadManager == null)
            Debug.LogError("QuestionManager: loadManager é null");
        if (answerManager == null)
            Debug.LogError("QuestionManager: answerManager é null");
        if (scoreManager == null)
            Debug.LogError("QuestionManager: scoreManager é null");
        if (feedbackElements == null)
            Debug.LogError("QuestionManager: feedbackElements é null");
        if (transitionManager == null)
            Debug.LogError("QuestionManager: transitionManager é null");
        if (counterManager == null)
            Debug.LogWarning("QuestionManager: counterManager é null (opcional)");

        return questionBottomBarManager != null &&
               questionUIManager        != null &&
               questionCanvasGroupManager != null &&
               timerManager             != null &&
               loadManager              != null &&
               answerManager            != null &&
               scoreManager             != null &&
               feedbackElements         != null &&
               transitionManager        != null &&
               counterManager           != null;
    }

    // -------------------------------------------------------
    // Inicialização da sessão
    // -------------------------------------------------------
    private async Task InitializeSession()
    {
        try
        {
            QuestionSet currentSet      = QuestionSetManager.GetCurrentQuestionSet();
            string currentDatabaseName  = TopicToDatabankName(currentSet);
            loadManager.databankName    = currentDatabaseName;

            // Lê as questões do LiteDB via QuestionSyncService 
            allDatabaseQuestions = AppContext.QuestionSync?
                                       .GetQuestionsForDatabankName(currentDatabaseName)
                                   ?? new List<Question>();

            if (allDatabaseQuestions.Count == 0)
            {
                Debug.LogError($"[QuestionManager] Nenhuma questão no LiteDB para: {currentDatabaseName}");
                _sceneData.SetData(new Dictionary<string, object>
                    { { "databankName", currentDatabaseName } });
                SceneManager.LoadScene("ResetDatabaseView");
                return;
            }

            maxLevelInDatabase = LevelCalculator.GetMaxLevel(allDatabaseQuestions);
            Debug.Log($"[QuestionManager] Banco: {currentDatabaseName} | " +
                      $"Questões: {allDatabaseQuestions.Count} | Níveis: {maxLevelInDatabase}");

            List<string> answeredQuestions = await AppContext.AnsweredQuestions?
                .FetchUserAnsweredQuestionsInTargetDatabase(currentDatabaseName);

            int answeredCount  = answeredQuestions?.Count ?? 0;
            int totalQuestions = QuestionBankStatistics.GetTotalQuestions(currentDatabaseName);

            if (totalQuestions <= 0)
            {
                totalQuestions = allDatabaseQuestions.Count;
                QuestionBankStatistics.SetTotalQuestions(currentDatabaseName, totalQuestions);
            }

            bool allQuestionsAnswered =
                QuestionBankStatistics.AreAllQuestionsAnswered(currentDatabaseName, answeredCount);

            if (allQuestionsAnswered)
            {
                _sceneData.SetData(new Dictionary<string, object>
                    { { "databankName", currentDatabaseName } });
                SceneManager.LoadScene("ResetDatabaseView");
                return;
            }

            var questions = await loadManager.LoadQuestionsForSet(currentSet);
            if (questions == null || questions.Count == 0)
            {
                Debug.LogError("[QuestionManager] Nenhuma questão disponível após LoadQuestionsForSet");
                _sceneData.SetData(new Dictionary<string, object>
                    { { "databankName", currentDatabaseName } });
                SceneManager.LoadScene("ResetDatabaseView");
                return;
            }

            currentSession = new QuestionSession(questions);

            if (counterManager != null)
            {
                counterManager.Initialize(allDatabaseQuestions, answeredQuestions);
                Debug.Log("[QuestionManager] QuestionCounterManager inicializado");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuestionManager] Erro em InitializeSession: {e.Message}\n{e.StackTrace}");
            string currentDatabaseName = loadManager.DatabankName;
            _sceneData.SetData(new Dictionary<string, object>
                { { "databankName", currentDatabaseName } });
            SceneManager.LoadScene("ResetDatabaseView");
        }
    }

    // Converte QuestionSet → databankName para manter compatibilidade com AnsweredQuestions
    private static string TopicToDatabankName(QuestionSet set) => set switch
    {
        QuestionSet.acidsBase      => "AcidBaseBufferQuestionDatabase",
        QuestionSet.aminoacids     => "AminoacidQuestionDatabase",
        QuestionSet.biochem        => "BiochemistryIntroductionQuestionDatabase",
        QuestionSet.carbohydrates  => "CarbohydratesQuestionDatabase",
        QuestionSet.enzymes        => "EnzymeQuestionDatabase",
        QuestionSet.lipids         => "LipidsQuestionDatabase",
        QuestionSet.membranes      => "MembranesQuestionDatabase",
        QuestionSet.nucleicAcids   => "NucleicAcidsQuestionDatabase",
        QuestionSet.proteins       => "ProteinQuestionDatabase",
        QuestionSet.water          => "WaterQuestionDatabase",
        _                          => set.ToString()
    };

    // -------------------------------------------------------
    // Event handlers
    // -------------------------------------------------------

    private void SetupEventHandlers()
    {
        timerManager.OnTimerComplete             += HandleTimeUp;
        answerManager.OnAnswerSelected           += CheckAnswer;
        transitionManager.OnBeforeTransitionStart += PrepareNextQuestion;
        transitionManager.OnTransitionMidpoint   += ApplyPreparedQuestion;
    }

    // -------------------------------------------------------
    // Resposta
    // -------------------------------------------------------
    private async void CheckAnswer(int selectedAnswerIndex)
    {
        timerManager.StopTimer();
        answerManager.DisableAllButtons();

        var currentQuestion = currentSession.GetCurrentQuestion();
        bool isCorrect      = selectedAnswerIndex == currentQuestion.correctIndex;
        answerManager.MarkSelectedButton(selectedAnswerIndex, isCorrect);

        try
        {
            if (isCorrect)
            {
                int baseScore   = 5;
                bool bonusActive = scoreManager.HasBonusActive();

                feedbackElements.ShowCorrectAnswer(bonusActive);

                await scoreManager.UpdateScore(baseScore, true, currentQuestion);

                if (counterManager != null)
                {
                    counterManager.MarkQuestionAsAnswered(currentQuestion.questionNumber);
                    counterManager.UpdateCounter(currentQuestion);
                }

                await CheckLevelCompletionAfterCorrectAnswer(currentQuestion);
            }
            else
            {
                feedbackElements.ShowWrongAnswer();
                await scoreManager.UpdateScore(-2, false, currentQuestion);
            }

            questionBottomBarManager.EnableNavigationButtons();
            SetupNavigationButtons();
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao processar resposta: {e.Message}");
        }
    }

    private async Task CheckLevelCompletionAfterCorrectAnswer(Question answeredQuestion)
    {
        try
        {
            string userId      = UserDataStore.CurrentUserData?.UserId;
            string databankName = loadManager.DatabankName;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(databankName)) return;

            int questionLevel = answeredQuestion.questionLevel > 0
                ? answeredQuestion.questionLevel : 1;

            // FetchUserAnsweredQuestionsInTargetDatabase lê do UserDataStore
            // — sem rede, sem delay necessário
            List<string> answeredQuestions = await AppContext.AnsweredQuestions?
                .FetchUserAnsweredQuestionsInTargetDatabase(databankName);

            bool isComplete = LevelCalculator.IsLevelComplete(
                allDatabaseQuestions, answeredQuestions, questionLevel);

            if (isComplete)
            {
                Debug.Log($"Nível {questionLevel} COMPLETO!");
                ShowLevelCompletionFeedback(questionLevel,
                    isLastLevel: questionLevel >= maxLevelInDatabase);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao verificar conclusão de nível: {e.Message}");
        }
    }

    // -------------------------------------------------------
    // Feedback de nível
    // -------------------------------------------------------
    private void ShowLevelCompletionFeedback(int completedLevel, bool isLastLevel)
    {
        string levelName = GetLevelName(completedLevel);
        string title;
        string bodyText;

        if (isLastLevel)
        {
            title    = "INCRÍVEL!";
            bodyText = $"Você completou o Nível {levelName}. Este é o nível mais difícil.";
        }
        else
        {
            string nextLevelName = GetLevelName(completedLevel + 1);
            title    = "PARABÉNS!";
            bodyText = $"Você completou o Nível {levelName}. O Nível {nextLevelName} foi desbloqueado.";
        }

        feedbackElements.ShowLevelCompletionFeedback(title, bodyText, true);
    }

    private string GetLevelName(int level) => level switch
    {
        1 => "Básico",
        2 => "Intermediário",
        3 => "Difícil",
        4 => "Avançado",
        5 => "Expert",
        _ => $"Nível {level}"
    };

    private void ShowAnswerFeedback(string message, bool isCorrect, bool isCompleted = false)
    {
        if (isCompleted)
        {
            feedbackElements.QuestionsCompletedFeedbackText.text = message;
            questionCanvasGroupManager.ShowCompletionFeedback();
            questionBottomBarManager.SetupNavigationButtons(
                () => _navigation.NavigateTo("PathwayScene"), null);
        }
    }

    // -------------------------------------------------------
    // Navegação entre questões
    // -------------------------------------------------------
    private async void PrepareNextQuestion()
    {
        if (!currentSession.IsLastQuestion())
        {
            currentSession.NextQuestion();
            nextQuestionToShow = currentSession.GetCurrentQuestion();
            await PreloadQuestionResources(nextQuestionToShow);
        }
        else
        {
            nextQuestionToShow = null;
        }
    }

    private async Task PreloadQuestionResources(Question question)
    {
        if (question.isImageQuestion)
            await questionUIManager.PreloadQuestionImage(question);
    }

    private void ApplyPreparedQuestion()
    {
        if (nextQuestionToShow != null)
        {
            answerManager.ResetButtonBackgrounds();
            answerManager.SetupAnswerButtons(nextQuestionToShow);
            questionCanvasGroupManager.ShowQuestion(
                nextQuestionToShow.isImageQuestion,
                nextQuestionToShow.isImageAnswer,
                nextQuestionToShow.questionLevel);
            questionUIManager.ShowQuestion(nextQuestionToShow);

            if (counterManager != null)
                counterManager.UpdateCounter(nextQuestionToShow);

            nextQuestionToShow = null;
        }
        else
        {
            StartCoroutine(HandleNoMoreQuestions());
        }
    }

    private IEnumerator HandleNoMoreQuestions()
    {
        var task = CheckAndLoadMoreQuestions();
        yield return new WaitUntil(() => task.IsCompleted);

        if (currentSession?.GetCurrentQuestion() != null)
        {
            var newQuestion = currentSession.GetCurrentQuestion();
            answerManager.SetupAnswerButtons(newQuestion);
            questionCanvasGroupManager.ShowQuestion(
                newQuestion.isImageQuestion,
                newQuestion.isImageAnswer,
                newQuestion.questionLevel);
            questionUIManager.ShowQuestion(newQuestion);

            if (counterManager != null)
                counterManager.UpdateCounter(newQuestion);
        }
    }

    private void StartQuestion()
    {
        try
        {
            var currentQuestion = currentSession.GetCurrentQuestion();
            answerManager.ResetButtonBackgrounds();
            answerManager.SetupAnswerButtons(currentQuestion);
            questionCanvasGroupManager.ShowQuestion(
                currentQuestion.isImageQuestion,
                currentQuestion.isImageAnswer,
                currentQuestion.questionLevel);
            questionUIManager.ShowQuestion(currentQuestion);

            if (counterManager != null)
                counterManager.UpdateCounter(currentQuestion);

            timerManager.StartTimer();
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao iniciar questão: {e.Message}\n{e.StackTrace}");
        }
    }

    private async void HandleTimeUp()
    {
        answerManager.DisableAllButtons();
        feedbackElements.ShowTimeout();
        await scoreManager.UpdateScore(-1, false, currentSession.GetCurrentQuestion());
        questionBottomBarManager.EnableNavigationButtons();
        SetupNavigationButtons();
    }

    private void SetupNavigationButtons()
    {
        questionBottomBarManager.SetupNavigationButtons(
            () => { HideAnswerFeedback(); _navigation.NavigateTo("PathwayScene"); },
            async () => { HideAnswerFeedback(); await HandleNextQuestion(); }
        );
    }

    public void ReturnToPathway() => _navigation.NavigateTo("PathwayScene");

    private void HideAnswerFeedback() => questionCanvasGroupManager.HideAnswerFeedback();

    private async Task HandleNextQuestion()
    {
        questionBottomBarManager.DisableNavigationButtons();

        if (currentSession.IsLastQuestion())
        {
            string userId              = UserDataStore.CurrentUserData?.UserId;
            string currentDatabaseName = loadManager.DatabankName;

            if (string.IsNullOrEmpty(userId)) return;

            // Lê do UserDataStore — sem rede
            List<string> answeredQuestions = await AppContext.AnsweredQuestions?
                .FetchUserAnsweredQuestionsInTargetDatabase(currentDatabaseName);

            int currentLevel = LevelCalculator.CalculateCurrentLevel(
                allDatabaseQuestions, answeredQuestions);

            var stats = LevelCalculator.GetLevelStats(allDatabaseQuestions, answeredQuestions);

            bool currentLevelComplete = stats.ContainsKey(currentLevel) &&
                                        stats[currentLevel].IsComplete;

            if (currentLevelComplete)
            {
                if (currentLevel < maxLevelInDatabase)
                {
                    ShowAnswerFeedback(
                        $"Nível {GetLevelName(currentLevel)} Completo!\n" +
                        $"Volte ao menu para acessar as questões do {GetLevelName(currentLevel + 1)}!",
                        true, true);
                    return;
                }

                int totalAnswered  = stats.Values.Sum(s => s.AnsweredQuestions);
                int totalQuestions = stats.Values.Sum(s => s.TotalQuestions);

                if (totalAnswered >= totalQuestions)
                {
                    try
                    {
                        await HandleDatabaseCompletion(currentDatabaseName);
                        ShowAnswerFeedback(
                            $"CONQUISTA DESBLOQUEADA!\n" +
                            $"Você completou TODAS as {totalQuestions} questões!\n" +
                            $"Todos os {maxLevelInDatabase} níveis foram dominados!\n" +
                            $"Bônus das Listas desbloqueado!",
                            true, true);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Erro ao processar bônus: {ex.Message}");
                        ShowAnswerFeedback("Parabéns! Você completou todos os níveis!", true, true);
                    }
                    return;
                }
            }
        }

        await transitionManager.TransitionToNextQuestion();
        timerManager.StartTimer();
    }

    private async Task CheckAndLoadMoreQuestions()
    {
        try
        {
            QuestionSet currentSet         = QuestionSetManager.GetCurrentQuestionSet();
            string currentDatabaseName     = loadManager.DatabankName;
            var newQuestions               = await loadManager.LoadQuestionsForSet(currentSet);

            if (newQuestions == null || newQuestions.Count == 0)
            {
                ShowAnswerFeedback("Não há mais questões disponíveis. Volte ao menu principal.",
                    false, true);
                return;
            }

            // Lê do UserDataStore — sem rede
            List<string> answeredQuestionsIds = await AppContext.AnsweredQuestions?
                .FetchUserAnsweredQuestionsInTargetDatabase(currentDatabaseName);

            var unansweredQuestions = newQuestions
                .Where(q => !answeredQuestionsIds.Contains(q.questionNumber.ToString()))
                .ToList();

            if (unansweredQuestions.Count > 0)
            {
                currentSession = new QuestionSession(unansweredQuestions);
                StartQuestion();
            }
            else
            {
                ShowAnswerFeedback(
                    "Não há mais questões não respondidas. Volte ao menu principal.",
                    false, true);
            }
        }
        catch (Exception)
        {
            ShowAnswerFeedback(
                "Ocorreu um erro ao buscar mais questões. Volte ao menu principal.",
                false, true);
        }
    }

    // -------------------------------------------------------
    // Conclusão de banco de questões
    // Operação rara — só acontece quando todas as questões são respondidas
    // -------------------------------------------------------
    private async Task HandleDatabaseCompletion(string databankName)
    {
        try
        {
            string userId = UserDataStore.CurrentUserData?.UserId;
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(databankName)) return;

            // Sem internet — registra localmente e sincroniza depois
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("[QuestionManager] Offline — conclusão de banco registrada localmente.");
                AppContext.UserDataLocal?.MarkAsDirty(userId);
                return;
            }

            bool isEligible = await AppContext.Firestore.IsDatabankEligibleForBonus(userId, databankName);

            if (isEligible)
            {
                await AppContext.Firestore.MarkDatabankAsCompleted(userId, databankName);
                var userBonusManager = new UserBonusManager();
                await userBonusManager.IncrementBonusCount(userId, "listCompletionBonus", 1, true);
            }
            else
            {
                Debug.LogWarning($"[QuestionManager] Databank '{databankName}' já foi completado anteriormente.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuestionManager] Erro ao processar conclusão do database: {e.Message}");
        }
    }
}