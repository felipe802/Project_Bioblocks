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
    private bool isCheckingLevelCompletion = false;

    private void Start()
    {
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
            Debug.LogWarning("QuestionManager: counterManager é null (opcional, mas recomendado)");

        bool isValid = questionBottomBarManager != null &&
               questionUIManager != null &&
               questionCanvasGroupManager != null &&
               timerManager != null &&
               loadManager != null &&
               answerManager != null &&
               scoreManager != null &&
               feedbackElements != null &&
               transitionManager != null &&
               counterManager != null;

        return isValid;
    }

    private async Task InitializeSession()
    {
        try
        {
            QuestionSet currentSet = QuestionSetManager.GetCurrentQuestionSet();
            IQuestionDatabase database = FindQuestionDatabase(currentSet);
            if (database == null)
            {
                Debug.LogError($"Nenhum database encontrado para o QuestionSet: {currentSet}");
                return;
            }

            string currentDatabaseName = database.GetDatabankName();
            loadManager.databankName = currentDatabaseName;

            allDatabaseQuestions = QuestionFilterService.FilterQuestions(database);
            maxLevelInDatabase = LevelCalculator.GetMaxLevel(allDatabaseQuestions);
            Debug.Log($"📚 Banco {currentDatabaseName} possui {maxLevelInDatabase} níveis");

            List<string> answeredQuestions = await AnsweredQuestionsManager.Instance
                .FetchUserAnsweredQuestionsInTargetDatabase(currentDatabaseName);
            int answeredCount = answeredQuestions.Count;
            int totalQuestions = QuestionBankStatistics.GetTotalQuestions(currentDatabaseName);

            if (totalQuestions <= 0)
            {
                totalQuestions = allDatabaseQuestions.Count;
                QuestionBankStatistics.SetTotalQuestions(currentDatabaseName, totalQuestions);
            }

            bool allQuestionsAnswered = QuestionBankStatistics.AreAllQuestionsAnswered(currentDatabaseName, answeredCount);

            if (allQuestionsAnswered)
            {
                SceneDataManager.Instance.SetData(new Dictionary<string, object> { { "databankName", currentDatabaseName } });
                SceneManager.LoadScene("ResetDatabaseView");
                return;
            }

            var questions = await loadManager.LoadQuestionsForSet(currentSet);
            if (questions == null || questions.Count == 0)
            {
                Debug.LogError("QuestionManager: Nenhuma questão disponível");
                SceneDataManager.Instance.SetData(new Dictionary<string, object> { { "databankName", currentDatabaseName } });
                SceneManager.LoadScene("ResetDatabaseView");
                return;
            }

            currentSession = new QuestionSession(questions);

            if (counterManager != null)
            {
                counterManager.Initialize(allDatabaseQuestions, answeredQuestions);
                Debug.Log("QuestionCounterManager inicializado");
            }
            else
            {
                Debug.LogWarning("QuestionCounterManager não está atribuído - contador não será exibido");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"QuestionManager: Erro em InitializeSession: {e.Message}\n{e.StackTrace}");
            string currentDatabaseName = loadManager.DatabankName;
            SceneDataManager.Instance.SetData(new Dictionary<string, object> { { "databankName", currentDatabaseName } });
            SceneManager.LoadScene("ResetDatabaseView");
        }
    }

    private IQuestionDatabase FindQuestionDatabase(QuestionSet targetSet)
    {
        try
        {
            MonoBehaviour[] allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            
            foreach (MonoBehaviour behaviour in allBehaviours)
            {
                if (behaviour is IQuestionDatabase database)
                {
                    if (database.GetQuestionSetType() == targetSet)
                    {
                        return database;
                    }
                }
            }

            Debug.LogError($"QuestionManager: Nenhum database encontrado para o set: {targetSet}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao procurar database: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    private void SetupEventHandlers()
    {
        timerManager.OnTimerComplete += HandleTimeUp;
        answerManager.OnAnswerSelected += CheckAnswer;
        transitionManager.OnBeforeTransitionStart += PrepareNextQuestion;
        transitionManager.OnTransitionMidpoint += ApplyPreparedQuestion;
    }

    private async void CheckAnswer(int selectedAnswerIndex)
    {
        timerManager.StopTimer();
        answerManager.DisableAllButtons();
        var currentQuestion = currentSession.GetCurrentQuestion();
        bool isCorrect = selectedAnswerIndex == currentQuestion.correctIndex;
        answerManager.MarkSelectedButton(selectedAnswerIndex, isCorrect);

        try
        {
            if (isCorrect)
            {
                int baseScore = 5;
                int actualScore = baseScore;
                bool bonusActive = false;

                if (scoreManager.HasBonusActive())
                {
                    bonusActive = true;
                    actualScore = scoreManager.CalculateBonusScore(baseScore);
                }

                feedbackElements.ShowCorrectAnswer(bonusActive);
                await scoreManager.UpdateScore(baseScore, true, currentQuestion);

                if (counterManager != null)
                {
                    counterManager.MarkQuestionAsAnswered(currentQuestion.questionNumber);
                    counterManager.UpdateCounter(currentQuestion);
                    Debug.Log($"Questão {currentQuestion.questionNumber} marcada no contador");
                }

                await CheckLevelCompletionAfterCorrectAnswer(currentQuestion);
            }
            else
            {
                Debug.Log($"Q{currentQuestion.questionNumber} (Nível {currentQuestion.questionLevel}) - ERRADA");
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
            string userId = UserDataStore.CurrentUserData?.UserId;
            string databankName = loadManager.DatabankName;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(databankName))
            {
                return;
            }

            await Task.Delay(1000);

            int questionLevel = answeredQuestion.questionLevel > 0 ? answeredQuestion.questionLevel : 1;

            Debug.Log($"\n Verificando se nível {questionLevel} foi completado...");

            List<string> answeredQuestions = await AnsweredQuestionsManager.Instance
                .FetchUserAnsweredQuestionsInTargetDatabase(databankName);

            bool isComplete = LevelCalculator.IsLevelComplete(
                allDatabaseQuestions,
                answeredQuestions,
                questionLevel
            );

            if (isComplete)
            {
                Debug.Log($"Nível {questionLevel} COMPLETO!");

                if (questionLevel >= maxLevelInDatabase)
                {
                    ShowLevelCompletionFeedback(questionLevel, isLastLevel: true);
                }
                else
                {
                    ShowLevelCompletionFeedback(questionLevel, isLastLevel: false);
                }
            }
            else
            {
                Debug.Log($"Nível {questionLevel} ainda não completo");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao verificar conclusão de nível: {e.Message}");
        }
    }

    private void ShowLevelCompletionFeedback(int completedLevel, bool isLastLevel)
    {
        string levelName = GetLevelName(completedLevel);
        string title;
        string bodyText;

        if (isLastLevel)
        {
            title = "INCRÍVEL!";
            bodyText = $"Você completou o Nível {levelName}. Este é o nível mais difícil.";
        }
        else
        {
            int nextLevel = completedLevel + 1;
            string nextLevelName = GetLevelName(nextLevel);

            title = "PARABÉNS!";
            bodyText = $"Você completou o Nível {levelName}. O Nível {nextLevelName} foi desbloqueado.";
        }

        feedbackElements.ShowLevelCompletionFeedback(title, bodyText, true);
    }

    private string GetLevelName(int level)
    {
        return level switch
        {
            1 => "Básico",
            2 => "Intermediário",
            3 => "Difícil",
            4 => "Avançado",
            5 => "Expert",
            _ => $"Nível {level}"
        };
    }

    private void ShowAnswerFeedback(string message, bool isCorrect, bool isCompleted = false)
    {
        if (isCompleted)
        {
            feedbackElements.QuestionsCompletedFeedbackText.text = message;
            questionCanvasGroupManager.ShowCompletionFeedback();
            questionBottomBarManager.SetupNavigationButtons(
                () => NavigationManager.Instance.NavigateTo("PathwayScene"),
                null
            );
        }
    }

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
        {
            await questionUIManager.PreloadQuestionImage(question);
        }

        if (question.isImageAnswer)
        {
        }
    }

    private void ApplyPreparedQuestion()
    {
        if (nextQuestionToShow != null)
        {
            answerManager.ResetButtonBackgrounds();
            answerManager.SetupAnswerButtons(nextQuestionToShow);
            questionCanvasGroupManager.ShowQuestion(
                isImageQuestion: nextQuestionToShow.isImageQuestion,
                isImageAnswer: nextQuestionToShow.isImageAnswer,
                questionLevel: nextQuestionToShow.questionLevel
            );
            questionUIManager.ShowQuestion(nextQuestionToShow);

            if (counterManager != null)
            {
                counterManager.UpdateCounter(nextQuestionToShow);
            }

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

        if (currentSession != null && currentSession.GetCurrentQuestion() != null)
        {
            var newQuestion = currentSession.GetCurrentQuestion();
            answerManager.SetupAnswerButtons(newQuestion);
            questionCanvasGroupManager.ShowQuestion(
                isImageQuestion: newQuestion.isImageQuestion,
                isImageAnswer: newQuestion.isImageAnswer,
                questionLevel: newQuestion.questionLevel
            );
            questionUIManager.ShowQuestion(newQuestion);

            if (counterManager != null)
            {
                counterManager.UpdateCounter(newQuestion);
            }
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
                isImageQuestion: currentQuestion.isImageQuestion,
                isImageAnswer: currentQuestion.isImageAnswer,
                questionLevel: currentQuestion.questionLevel
            );
            questionUIManager.ShowQuestion(currentQuestion);

            if (counterManager != null)
            {
                counterManager.UpdateCounter(currentQuestion);
                Debug.Log($"Contador atualizado para questão {currentQuestion.questionNumber}");
            }

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
            () =>
            {
                HideAnswerFeedback();
                NavigationManager.Instance.NavigateTo("PathwayScene");
            },
            async () =>
            {
                HideAnswerFeedback();
                await HandleNextQuestion();
            }
        );
    }

    public void ReturnToPathway()
    {
        NavigationManager.Instance.NavigateTo("PathwayScene");
    }

    private void HideAnswerFeedback()
    {
        questionCanvasGroupManager.HideAnswerFeedback();
    }

    private async Task HandleNextQuestion()
    {
        questionBottomBarManager.DisableNavigationButtons();

        if (currentSession.IsLastQuestion())
        {
            string userId = UserDataStore.CurrentUserData?.UserId;
            string currentDatabaseName = loadManager.DatabankName;

            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogError("UserId não disponível");
                return;
            }

            List<string> answeredQuestions = await AnsweredQuestionsManager.Instance
                .FetchUserAnsweredQuestionsInTargetDatabase(currentDatabaseName);

            int currentLevel = LevelCalculator.CalculateCurrentLevel(
                allDatabaseQuestions,
                answeredQuestions
            );

            var stats = LevelCalculator.GetLevelStats(
                allDatabaseQuestions,
                answeredQuestions
            );

            bool currentLevelComplete = stats.ContainsKey(currentLevel) &&
                                       stats[currentLevel].IsComplete;

            if (currentLevelComplete)
            {
                if (currentLevel < maxLevelInDatabase)
                {
                    string message = $"Nível {GetLevelName(currentLevel)} Completo!\n" +
                        $"Volte ao menu para acessar as questões do {GetLevelName(currentLevel + 1)}!";

                    ShowAnswerFeedback(message, true, true);
                    return;
                }
                else
                {
                    int totalAnswered = stats.Values.Sum(s => s.AnsweredQuestions);
                    int totalQuestions = stats.Values.Sum(s => s.TotalQuestions);

                    if (totalAnswered >= totalQuestions)
                    {
                        try
                        {
                            await HandleDatabaseCompletion(currentDatabaseName);

                            string completionMessage = $"CONQUISTA DESBLOQUEADA!\n" +
                                $"Você completou TODAS as {totalQuestions} questões!\n" +
                                $"Todos os {maxLevelInDatabase} níveis foram dominados!\n" +
                                $"Bônus das Listas desbloqueado!";

                            ShowAnswerFeedback(completionMessage, true, true);
                        }
                        catch (Exception bonusEx)
                        {
                            Debug.LogError($"Erro ao processar bônus: {bonusEx.Message}");
                            ShowAnswerFeedback($"Parabéns! Você completou todos os níveis!", true, true);
                        }

                        return;
                    }
                }
            }
        }

        await transitionManager.TransitionToNextQuestion();
        timerManager.StartTimer();
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
            transitionManager.OnTransitionMidpoint -= ApplyPreparedQuestion;
        }
    }

    private async Task CheckAndLoadMoreQuestions()
    {
        try
        {
            QuestionSet currentSet = QuestionSetManager.GetCurrentQuestionSet();
            string currentDatabaseName = loadManager.DatabankName;
            var newQuestions = await loadManager.LoadQuestionsForSet(currentSet);

            if (newQuestions == null || newQuestions.Count == 0)
            {
                ShowAnswerFeedback("Não há mais questões disponíveis. Volte ao menu principal.", false, true);
                return;
            }

            List<string> answeredQuestionsIds = await AnsweredQuestionsManager.Instance.FetchUserAnsweredQuestionsInTargetDatabase(currentDatabaseName);
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
                ShowAnswerFeedback("Não há mais questões não respondidas disponíveis. Volte ao menu principal.", false, true);
            }
        }
        catch (Exception)
        {
            ShowAnswerFeedback("Ocorreu um erro ao buscar mais questões. Volte ao menu principal.", false, true);
        }
    }

    private async Task HandleDatabaseCompletion(string databankName)
    {
        try
        {
            if (string.IsNullOrEmpty(databankName) || string.IsNullOrEmpty(UserDataStore.CurrentUserData?.UserId))
            {
                return;
            }

            string userId = UserDataStore.CurrentUserData.UserId;
            UserBonusManager userBonusManager = new UserBonusManager();
            bool isEligible = await CheckIfDatabankEligibleForBonus(userId, databankName);

            if (isEligible)
            {
                await MarkDatabankAsCompleted(userId, databankName);
                await userBonusManager.IncrementBonusCount(userId, "listCompletionBonus", 1, true);
            }
            else
            {
                Debug.LogWarning($"Databank {databankName} já foi marcado como completado anteriormente");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao processar conclusão do database: {e.Message}");
        }
    }

    private async Task<bool> CheckIfDatabankEligibleForBonus(string userId, string databankName)
    {
        UserBonusManager userBonusManager = new UserBonusManager();

        try
        {
            var docRef = Firebase.Firestore.FirebaseFirestore.DefaultInstance
                .Collection("UserBonus").Document(userId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();

                if (data.ContainsKey("CompletedDatabanks"))
                {
                    List<object> completedDatabanks = data["CompletedDatabanks"] as List<object>;

                    if (completedDatabanks != null && completedDatabanks.Contains(databankName))
                    {
                        return false;
                    }
                }
                return true;
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao verificar elegibilidade do databank: {e.Message}");
            return false;
        }
    }

    private async Task MarkDatabankAsCompleted(string userId, string databankName)
    {
        try
        {
            var docRef = Firebase.Firestore.FirebaseFirestore.DefaultInstance
                .Collection("UserBonus").Document(userId);
            var snapshot = await docRef.GetSnapshotAsync();

            List<string> completedDatabanks = new List<string>();

            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();

                if (data.ContainsKey("CompletedDatabanks"))
                {
                    List<object> existingList = data["CompletedDatabanks"] as List<object>;

                    if (existingList != null)
                    {
                        completedDatabanks = existingList.Select(item => item.ToString()).ToList();
                    }
                }
            }

            if (!completedDatabanks.Contains(databankName))
            {
                completedDatabanks.Add(databankName);
                Dictionary<string, object> updateData = new Dictionary<string, object>
                {
                    { "CompletedDatabanks", completedDatabanks }
                };

                await docRef.UpdateAsync(updateData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao marcar databank como completo: {e.Message}");
        }
    }

    private Color HexToColor(string hex)
    {
        Color color = new Color();
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }
}