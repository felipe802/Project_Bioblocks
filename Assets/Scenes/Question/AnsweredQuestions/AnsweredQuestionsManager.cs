using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// Gerencia as questões respondidas corretamente pelo usuário.
/// Fonte primária de dados: UserDataStore (memória) → LiteDB (cache local)
/// Firestore: apenas para escrita e sincronização inicial
/// </summary>
public class AnsweredQuestionsManager : MonoBehaviour, IAnsweredQuestionsManager
{
    public delegate void AnsweredQuestionsUpdatedHandler(Dictionary<string, int> answeredCounts);
    public static event AnsweredQuestionsUpdatedHandler OnAnsweredQuestionsUpdated;

    private IFirestoreRepository _firestore;
    private IAuthRepository _auth;

    private string userId;
    private bool isInitialized = false;

    // -------------------------------------------------------
    // Ciclo de vida
    // -------------------------------------------------------

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        if (!AppContext.IsReady)
        {
            Debug.Log("[AnsweredQuestionsManager] Aguardando AppContext...");
            float elapsed = 0f;
            while (!AppContext.IsReady && elapsed < 15f)
            {
                await Task.Delay(100);
                elapsed += 0.1f;
            }

            if (!AppContext.IsReady)
            {
                Debug.LogError("[AnsweredQuestionsManager] Timeout aguardando AppContext.");
                return;
            }
        }

        _firestore = AppContext.Firestore;
        _auth      = AppContext.Auth;

        if (_firestore == null || _auth == null)
        {
            Debug.LogError("[AnsweredQuestionsManager] Serviços não disponíveis após AppContext.IsReady");
            return;
        }

        await Initialize();
    }

    // -------------------------------------------------------
    // Inicialização
    // -------------------------------------------------------

    private async Task Initialize()
    {
        if (isInitialized) return;

        try
        {
            await Task.Yield();

            if (_firestore == null || !_firestore.IsInitialized)
            {
                Debug.LogError("[AnsweredQuestionsManager] FirestoreRepository não está inicializado");
                return;
            }

            if (_auth == null || !_auth.IsUserLoggedIn())
            {
                Debug.LogError("[AnsweredQuestionsManager] Nenhum usuário está autenticado");
                return;
            }

            userId = _auth.CurrentUserId;
            Debug.Log($"[AnsweredQuestionsManager] Inicializando para usuário: {userId}");

            // Listener para atualizações em tempo real do Firestore
            // (só dispara quando há internet — inofensivo offline)
            _firestore.ListenToAnsweredQuestions(userId, HandleAnsweredQuestionsUpdate);

            // Popula a partir do UserDataStore — já carregado pelo SyncService na inicialização
            await FetchUserAnsweredQuestions();

            isInitialized = true;
            Debug.Log("[AnsweredQuestionsManager] Inicializado com sucesso");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AnsweredQuestionsManager] Erro na inicialização: {e.Message}");
            isInitialized = false;
        }
    }

    // -------------------------------------------------------
    // Fonte primária de dados — UserDataStore → LiteDB → Firestore
    // -------------------------------------------------------

    /// <summary>
    /// Retorna UserData da fonte mais rápida disponível.
    /// Nunca bloqueia: sem internet usa cache local imediatamente.
    /// </summary>
    private UserData GetLocalUserData()
    {
        // 1. Memória — mais rápido, sempre atualizado pelo SyncService
        var inMemory = UserDataStore.CurrentUserData;
        if (inMemory != null) return inMemory;

        // 2. LiteDB — fallback se memória estiver vazia
        if (!string.IsNullOrEmpty(userId))
        {
            var cached = AppContext.UserDataLocal?.GetUser(userId);
            if (cached != null)
            {
                Debug.Log("[AnsweredQuestionsManager] Usando dados do LiteDB.");
                return cached;
            }
        }

        return null;
    }

    // -------------------------------------------------------
    // Listener de atualizações em tempo real (Firestore → memória)
    // -------------------------------------------------------

    private void HandleAnsweredQuestionsUpdate(Dictionary<string, List<int>> answeredQuestions)
    {
        try
        {
            if (answeredQuestions == null) return;

            Dictionary<string, int> answeredCounts = new Dictionary<string, int>();

            foreach (var kvp in answeredQuestions)
            {
                string databankName    = kvp.Key;
                List<int> questionsList = kvp.Value;
                if (questionsList == null) continue;

                var distinctQuestions = questionsList.Distinct().ToList();
                int totalQuestions    = QuestionBankStatistics.GetTotalQuestions(databankName);
                if (totalQuestions <= 0) totalQuestions = 50;

                int count = Mathf.Min(distinctQuestions.Count, totalQuestions);
                answeredCounts[databankName] = count;
                AnsweredQuestionsListStore.UpdateAnsweredQuestionsCount(userId, databankName, count);

                Debug.Log($"[AnsweredQuestionsManager] Listener atualizou {databankName}: {count} questões");
            }

            if (answeredCounts.Count > 0)
                OnAnsweredQuestionsUpdated?.Invoke(answeredCounts);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AnsweredQuestionsManager] Erro ao processar atualização: {ex.Message}");
        }
    }

    // -------------------------------------------------------
    // Busca de dados — usa UserDataStore/LiteDB, sem chamadas de rede
    // -------------------------------------------------------

    /// <summary>
    /// Atualiza os contadores locais a partir do UserDataStore.
    /// Sem chamadas de rede — instantâneo online e offline.
    /// </summary>
    private async Task FetchUserAnsweredQuestions()
    {
        try
        {
            // Lê da memória/LiteDB — sem chamada de rede
            UserData userData = GetLocalUserData();

            if (userData?.AnsweredQuestions == null)
            {
                Debug.LogWarning("[AnsweredQuestionsManager] UserData sem AnsweredQuestions no cache local.");
                return;
            }

            Dictionary<string, int> answeredCounts = new Dictionary<string, int>();

            foreach (var kvp in userData.AnsweredQuestions)
            {
                string databankName    = kvp.Key;
                List<int> questionsList = kvp.Value;
                if (questionsList == null) continue;

                var distinctQuestions = questionsList.Distinct().ToList();

                if (distinctQuestions.Count != questionsList.Count)
                    Debug.LogWarning($"[AnsweredQuestionsManager] " +
                        $"{questionsList.Count - distinctQuestions.Count} questões duplicadas em {databankName}");

                int totalQuestions = QuestionBankStatistics.GetTotalQuestions(databankName);
                int count          = Mathf.Min(distinctQuestions.Count, totalQuestions);

                answeredCounts[databankName] = count;
                AnsweredQuestionsListStore.UpdateAnsweredQuestionsCount(userId, databankName, count);
                Debug.Log($"[AnsweredQuestionsManager] {databankName}: {count} questões respondidas");
            }

            Debug.Log($"[AnsweredQuestionsManager] OnAnsweredQuestionsUpdated com {answeredCounts.Count} bancos");
            OnAnsweredQuestionsUpdated?.Invoke(answeredCounts);

            // Task.Yield mantido para não bloquear o frame
            await Task.Yield();
        }
        catch (Exception e)
        {
            Debug.LogError($"[AnsweredQuestionsManager] Erro ao buscar dados: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// Retorna questões respondidas de um banco específico.
    /// Lê do UserDataStore/LiteDB — sem chamada de rede.
    /// </summary>
    public async Task<List<string>> FetchUserAnsweredQuestionsInTargetDatabase(string target)
    {
        var answeredQuestions = new List<string>();

        try
        {
            if (!isInitialized)
            {
                await Initialize();
                if (!isInitialized) return answeredQuestions;
            }

            // Lê da memória/LiteDB — sem chamada de rede
            UserData userData = GetLocalUserData();

            if (userData?.AnsweredQuestions != null &&
                userData.AnsweredQuestions.ContainsKey(target))
            {
                answeredQuestions = userData.AnsweredQuestions[target]
                    .Select(q => q.ToString())
                    .ToList();

                Debug.Log($"[AnsweredQuestionsManager] {answeredQuestions.Count} questões respondidas " +
                          $"em {target} (cache local)");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[AnsweredQuestionsManager] Erro ao buscar questões de {target}: {e.Message}");
        }

        return answeredQuestions;
    }

    // -------------------------------------------------------
    // Marcar questão como respondida
    // -------------------------------------------------------

    /// <summary>
    /// Marca uma questão como respondida.
    /// Verifica duplicata no cache local — só vai ao Firestore para escrever.
    /// </summary>
    public async Task MarkQuestionAsAnswered(string databankName, int questionNumber)
    {
        try
        {
            if (!isInitialized)
            {
                await Initialize();
                if (!isInitialized) return;
            }

            if (string.IsNullOrEmpty(userId))
            {
                Debug.LogError("[AnsweredQuestionsManager] Usuário não autenticado");
                return;
            }

            // Verifica duplicata no cache local — sem chamada de rede
            UserData userData = GetLocalUserData();

            if (userData == null)
            {
                Debug.LogError("[AnsweredQuestionsManager] Dados do usuário não encontrados no cache");
                return;
            }

            bool alreadyAnswered = userData.AnsweredQuestions != null &&
                                   userData.AnsweredQuestions.ContainsKey(databankName) &&
                                   userData.AnsweredQuestions[databankName].Contains(questionNumber);

            if (alreadyAnswered)
            {
                Debug.Log($"[AnsweredQuestionsManager] Questão {questionNumber} já estava marcada em {databankName}");
                return;
            }

            // Atualiza cache local imediatamente
            userData.AnsweredQuestions ??= new Dictionary<string, List<int>>();
            if (!userData.AnsweredQuestions.ContainsKey(databankName))
                userData.AnsweredQuestions[databankName] = new List<int>();
            userData.AnsweredQuestions[databankName].Add(questionNumber);

            // Persiste no LiteDB
            AppContext.UserDataLocal?.AddAnsweredQuestion(userId, databankName, questionNumber);

            // Tenta Firestore — sem bloqueio se offline
            if (AppContext.Connectivity?.IsOnline == true)
            {
                try
                {
                    await _firestore.UpdateUserScore(userId, userData.Score, questionNumber, databankName, true).ConfigureAwait(false);
                    await Task.Yield();
                    AppContext.UserDataLocal?.MarkAsSynced(userId);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[AnsweredQuestionsManager] Firestore offline, marcado localmente: {e.Message}");
                    AppContext.UserDataLocal?.MarkAsDirty(userId);
                }
            }
            else
            {
                Debug.Log("[AnsweredQuestionsManager] Offline — questão marcada localmente.");
                AppContext.UserDataLocal?.MarkAsDirty(userId);
            }

            await Task.Yield();
            await ForceUpdate();

            Debug.Log($"[AnsweredQuestionsManager] Questão {questionNumber} marcada em {databankName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AnsweredQuestionsManager] Erro ao marcar questão: {ex.Message}");
        }
    }

    // -------------------------------------------------------
    // ForceUpdate — relê do cache local, sem rede
    // -------------------------------------------------------
    public async Task ForceUpdate()
    {
        try
        {
            Debug.Log("[AnsweredQuestionsManager] ForceUpdate iniciado");

            if (!isInitialized)
            {
                await Initialize();
                if (!isInitialized)
                {
                    Debug.LogError("[AnsweredQuestionsManager] Falha ao inicializar durante ForceUpdate");
                    return;
                }
            }

            // FetchUserAnsweredQuestions agora lê do UserDataStore — sem rede
            await FetchUserAnsweredQuestions();

            Debug.Log("[AnsweredQuestionsManager] ForceUpdate concluído");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AnsweredQuestionsManager] Erro em ForceUpdate: {e.Message}");
        }
    }

    // -------------------------------------------------------
    // Questões restantes
    // -------------------------------------------------------

    public async Task<bool> HasRemainingQuestions(string currentDatabase, List<string> currentQuestionList)
    {
        try
        {
            if (!isInitialized)
            {
                await Initialize();
                if (!isInitialized) return false;
            }

            List<string> answeredQuestions =
                await FetchUserAnsweredQuestionsInTargetDatabase(currentDatabase);

            bool hasRemaining = currentQuestionList.Except(answeredQuestions).Any();

            Debug.Log($"[AnsweredQuestionsManager] {currentDatabase}: " +
                      $"Total={currentQuestionList.Count}, " +
                      $"Respondidas={answeredQuestions.Count}, " +
                      $"Restantes={hasRemaining}");

            return hasRemaining;
        }
        catch (Exception e)
        {
            Debug.LogError($"[AnsweredQuestionsManager] Erro ao verificar questões restantes: {e.Message}");
            return false;
        }
    }

    // -------------------------------------------------------
    // Reset
    // -------------------------------------------------------

    public void ResetManager()
    {
        isInitialized = false;
        userId        = null;
    }

    public bool IsManagerInitialized => isInitialized;
}