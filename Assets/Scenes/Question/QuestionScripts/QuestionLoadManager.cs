using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuestionSystem;
using System;
using System.Linq;
 
public class QuestionLoadManager : MonoBehaviour
{
    private List<Question> questions;
    public string databankName;
    private bool isInitialized = false;
    public string DatabankName => databankName;
 
    private async void Start()
    {
        await Initialize();
    }
 
    private async Task Initialize()
    {
        if (isInitialized) return;
 
        try
        {
            await WaitForAnsweredQuestionsManager();
            isInitialized = true;
            Debug.Log("[QuestionLoadManager] Inicializado com sucesso");
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuestionLoadManager] Erro ao inicializar: {e.Message}");
        }
    }
 
    private async Task WaitForAnsweredQuestionsManager()
    {
        if (AppContext.AnsweredQuestions == null)
            throw new Exception("[QuestionLoadManager] AnsweredQuestionsManager não registrado no AppContext.");
 
        int maxAttempts = 10;
        int currentAttempt = 0;
 
        while (!AppContext.AnsweredQuestions.IsManagerInitialized && currentAttempt < maxAttempts)
        {
            Debug.Log($"[QuestionLoadManager] Aguardando AnsweredQuestionsManager... tentativa {currentAttempt + 1}/{maxAttempts}");
            await Task.Delay(500);
            currentAttempt++;
        }
 
        if (!AppContext.AnsweredQuestions.IsManagerInitialized)
            throw new Exception("[QuestionLoadManager] AnsweredQuestionsManager não inicializou a tempo.");
 
        Debug.Log("[QuestionLoadManager] AnsweredQuestionsManager pronto.");
    }
 
    public async Task<List<Question>> LoadQuestionsForSet(QuestionSet targetSet)
    {
        try
        {
            if (!isInitialized)
                await Initialize();

            // Lê via IQuestionSource — Firestore/LiteDB em Prod, fake local em Dev.
            // A implementação é escolhida no bootstrap do AppContext (EnvironmentConfig).
            List<Question> allQuestions = AppContext.QuestionSource?
                                              .GetQuestionsForDatabankName(databankName)
                                          ?? new List<Question>();

            if (allQuestions == null || allQuestions.Count == 0)
            {
                Debug.LogError($"[QuestionLoadManager] ❌ Nenhuma questão no LiteDB para: {databankName}");
                return new List<Question>();
            }

            Debug.Log($"\n📚 PASSO 1: LITEDB");
            Debug.Log($"  Banco: {databankName} | Total: {allQuestions.Count}");

            int totalQuestions = allQuestions.Count;
            QuestionBankStatistics.SetTotalQuestions(databankName, totalQuestions);

            var questionsByLevel = GetQuestionCountByLevel(allQuestions);
            QuestionBankStatistics.SetQuestionsPerLevel(databankName, questionsByLevel);

            foreach (var kvp in questionsByLevel.OrderBy(x => x.Key))
                Debug.Log($"    Nível {kvp.Key}: {kvp.Value} questões");

            // Preview Mode — retorna todas as questões de todos os níveis,
            // sem filtrar por respondidas ou nível atual.
            var envCfg = EnvironmentConfig.Load();
            if (envCfg != null && envCfg.QuestionPreviewMode)
            {
                Debug.Log($"[QuestionLoadManager] Preview Mode — {allQuestions.Count} questões de todos os níveis carregadas.");
                questions = allQuestions;
                return questions;
            }

            string userId = UserDataStore.CurrentUserData?.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogWarning("[QuestionLoadManager] ⚠️ UserId não disponível — carregando nível 1");
                questions = allQuestions.Where(q => GetQuestionLevel(q) == 1).ToList();
                return questions;
            }

            List<string> answeredQuestionsFromFirebase = await AppContext.AnsweredQuestions
                .FetchUserAnsweredQuestionsInTargetDatabase(databankName);

            Debug.Log($"\n🔥 PASSO 2: QUESTÕES RESPONDIDAS");
            Debug.Log($"  Respondidas corretamente: {answeredQuestionsFromFirebase.Count}");

            if (answeredQuestionsFromFirebase.Count > 0 && answeredQuestionsFromFirebase.Count <= 20)
                Debug.Log($"  IDs: [{string.Join(", ", answeredQuestionsFromFirebase)}]");

            Debug.Log($"\n🔢 PASSO 3: CÁLCULO DO NÍVEL ATUAL");

            int currentLevel = LevelCalculator.CalculateCurrentLevel(
                allQuestions, answeredQuestionsFromFirebase);

            HashSet<string> answeredSet = new HashSet<string>(answeredQuestionsFromFirebase);

            List<Question> questionsNotAnswered = allQuestions
                .Where(q => !answeredSet.Contains(q.questionNumber.ToString()))
                .ToList();

            Debug.Log($"\n🗑️ PASSO 4: REMOVER QUESTÕES RESPONDIDAS");
            Debug.Log($"  Questões restantes: {questionsNotAnswered.Count}");

            List<Question> questionsForCurrentLevel = questionsNotAnswered
                .Where(q => GetQuestionLevel(q) == currentLevel)
                .ToList();

            Debug.Log($"\n✅ PASSO 5: FILTRAR POR NÍVEL {currentLevel}");
            Debug.Log($"  Questões disponíveis: {questionsForCurrentLevel.Count}");

            if (questionsForCurrentLevel.Count > 0)
            {
                var questionNumbers = questionsForCurrentLevel
                    .Select(q => q.questionNumber)
                    .OrderBy(n => n)
                    .ToList();

                if (questionNumbers.Count <= 20)
                    Debug.Log($"  IDs: [{string.Join(", ", questionNumbers)}]");
                else
                    Debug.Log($"  IDs: [{string.Join(", ", questionNumbers.Take(10))}... +{questionNumbers.Count - 10} mais]");
            }
            else
            {
                Debug.Log($"  ⚠️ NENHUMA questão disponível no nível {currentLevel}!");

                var stats = LevelCalculator.GetLevelStats(allQuestions, answeredQuestionsFromFirebase);
                Debug.Log($"\n📊 ESTATÍSTICAS:");
                foreach (var stat in stats.Values.OrderBy(s => s.Level))
                    Debug.Log($"  {stat}");
            }

            Debug.Log($"╚══════════════════════════════════════════════════════╝\n");

            questions = questionsForCurrentLevel;
            return questions;
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuestionLoadManager] ❌ Erro em LoadQuestionsForSet: {e.Message}\n{e.StackTrace}");
            return new List<Question>();
        }
    }
 
    private int GetQuestionLevel(Question question)
    {
        return question.questionLevel <= 0 ? 1 : question.questionLevel;
    }
 
    private Dictionary<int, int> GetQuestionCountByLevel(List<Question> allQuestions)
    {
        var stats = new Dictionary<int, int>();
 
        if (allQuestions == null || allQuestions.Count == 0)
            return stats;
 
        foreach (var question in allQuestions)
        {
            int level = GetQuestionLevel(question);
            if (!stats.ContainsKey(level))
                stats[level] = 0;
            stats[level]++;
        }
 
        return stats;
    }
}