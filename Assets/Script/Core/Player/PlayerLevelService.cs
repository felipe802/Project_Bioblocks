using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PlayerLevelService : MonoBehaviour, IPlayerLevelService
{
    public event Action<int, int> OnLevelChanged;
    public event Action<int> OnLevelProgressUpdated;

    private IFirestoreRepository _firestore;
    private IStatisticsProvider _statistics;
    private UserData _currentUserData;
    private bool _isInitialized = false;
    private bool _isMigrating = false;
    private bool _migrationChecked = false;
    private IUserDataSyncService _userDataSync;

    // -------------------------------------------------------
    // Ciclo de vida Unity
    // -------------------------------------------------------
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Debug.Log("[PlayerLevelService] Start() chamado");

        if (!AppContext.IsReady)
        {
            Debug.LogWarning("[PlayerLevelService] AppContext não está pronto. Aguardando...");
            AppContext.OnReady += OnAppContextReady;
            return;
        }

        InitializeDependencies();
    }

    private void OnAppContextReady()
    {
        AppContext.OnReady -= OnAppContextReady;
        InitializeDependencies();
    }

    private void InitializeDependencies()
    {
        _firestore    = AppContext.Firestore;
        _statistics   = AppContext.Statistics;
        _userDataSync = AppContext.UserDataSync;

        if (_firestore == null)
            Debug.LogError("[PlayerLevelService] _firestore é null");

        if (_statistics == null)
            Debug.LogWarning("[PlayerLevelService] _statistics é null");

        _migrationChecked = false;

        UserDataStore.OnUserDataChanged += OnUserDataChanged;
        _currentUserData = UserDataStore.CurrentUserData;

        if (_currentUserData == null)
            Debug.LogWarning("[PlayerLevelService] CurrentUserData é null. Aguardando OnUserDataChanged...");
        else
        {
            Debug.Log($"[PlayerLevelService] CurrentUserData encontrado: " +
                      $"{_currentUserData.UserId}, Level: {_currentUserData.PlayerLevel}");
            _migrationChecked = true;
            PerformMigrationIfNeeded();
        }

        _isInitialized = true;
        Debug.Log("[PlayerLevelService] Inicialização completa");
    }

    private void OnDestroy()
    {
        UserDataStore.OnUserDataChanged -= OnUserDataChanged;
        _migrationChecked     = false;
        _cachedTotalQuestions = 0;
    }

    // -------------------------------------------------------
    // IPlayerLevelService — carregamento de dados
    // -------------------------------------------------------
    public void OnUserDataLoaded(UserData userData)
    {
        Debug.Log($"[PlayerLevelService] OnUserDataLoaded. UserId: {userData?.UserId}, " +
                  $"Level: {userData?.PlayerLevel}");
        _currentUserData = userData;

        if (_currentUserData != null && _isInitialized)
            PerformMigrationIfNeeded();
    }

    private void OnUserDataChanged(UserData userData)
    {
        _currentUserData      = userData;
        _cachedTotalQuestions = 0;

        if (_currentUserData == null || string.IsNullOrEmpty(_currentUserData.UserId)) return;

        if (_isInitialized && !_isMigrating && !_migrationChecked)
        {
            _migrationChecked = true;
            Debug.Log("[PlayerLevelService] Dados carregados pela primeira vez. Verificando migração...");
            PerformMigrationIfNeeded();
        }

        if (_isInitialized)
            OnLevelProgressUpdated?.Invoke(_currentUserData.TotalValidQuestionsAnswered);
    }

    // -------------------------------------------------------
    // Migração de dados legados
    // -------------------------------------------------------
    private async void PerformMigrationIfNeeded()
    {
        if (_currentUserData == null) return;
        if (string.IsNullOrEmpty(_currentUserData.UserId)) return;
        if (_firestore == null) return;
        if (_isMigrating) return;

        int realTotal  = CalculateValidAnsweredQuestionsFromData(_currentUserData);
        int savedTotal = _currentUserData.TotalValidQuestionsAnswered;

        if (realTotal == savedTotal)
        {
            Debug.Log($"[PlayerLevelService] Dados consistentes: {savedTotal}. Nenhuma migração necessária.");
            return;
        }

        Debug.Log($"[PlayerLevelService] Inconsistência: salvo={savedTotal}, real={realTotal}. Corrigindo...");

        _isMigrating = true;
        try
        {
            int totalQuestions = GetTotalQuestionsCount();
            int newLevel       = PlayerLevelConfig.CalculateLevel(realTotal, totalQuestions);

            _currentUserData.TotalValidQuestionsAnswered = realTotal;
            _currentUserData.PlayerLevel                 = newLevel;

            // Persiste no LiteDB imediatamente — funciona offline
            AppContext.UserDataLocal?.UpdateUser(_currentUserData);

            // Tenta Firestore — sem bloqueio se offline
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                try
                {
                    await _firestore.UpdateUserField(_currentUserData.UserId,
                        "TotalValidQuestionsAnswered", realTotal).ConfigureAwait(false);
                    await _firestore.UpdateUserField(_currentUserData.UserId,
                        "PlayerLevel", newLevel).ConfigureAwait(false);
                    AppContext.UserDataLocal?.MarkAsSynced(_currentUserData.UserId);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[PlayerLevelService] Firestore offline na migração: {e.Message}");
                    AppContext.UserDataLocal?.MarkAsDirty(_currentUserData.UserId);
                }
            }
            else
            {
                Debug.Log("[PlayerLevelService] Offline — migração salva localmente.");
                AppContext.UserDataLocal?.MarkAsDirty(_currentUserData.UserId);
            }

            var captured = _currentUserData;
            MainThreadDispatcher.Enqueue(() =>
            {
                UserDataStore.CurrentUserData = captured;
                Debug.Log($"[PlayerLevelService] Migração concluída. Level: {newLevel}, Questões: {realTotal}");
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerLevelService] Erro na migração: {e.Message}");
        }
        finally
        {
            _isMigrating = false;
        }
    }

    private int CalculateValidAnsweredQuestionsFromData(UserData userData)
    {
        if (userData?.AnsweredQuestions == null) return 0;

        int total = 0;
        foreach (var kvp in userData.AnsweredQuestions)
        {
            int count = new HashSet<int>(kvp.Value).Count;
            total += count;
            Debug.Log($"[PlayerLevelService] Banco '{kvp.Key}': {count} questões únicas");
        }

        Debug.Log($"[PlayerLevelService] Total calculado: {total}");
        return total;
    }

    // -------------------------------------------------------
    // IPlayerLevelService — progressão
    // -------------------------------------------------------
    public async Task IncrementTotalAnswered()
    {
        if (_userDataSync == null) _userDataSync = AppContext.UserDataSync;
        if (_firestore == null)    _firestore    = AppContext.Firestore;
        if (!_isInitialized || _currentUserData == null) return;

        _currentUserData.TotalValidQuestionsAnswered++;
        var total  = _currentUserData.TotalValidQuestionsAnswered;
        var userId = _currentUserData.UserId;

        // Persiste no LiteDB imediatamente — funciona offline
        AppContext.UserDataLocal?.UpdateUser(_currentUserData);

        // Tenta Firestore — sem bloqueio se offline
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            try
            {
                await _firestore.UpdateUserField(userId,
                    "TotalValidQuestionsAnswered", total).ConfigureAwait(false);
                AppContext.UserDataLocal?.MarkAsSynced(userId);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PlayerLevelService] Firestore offline: {e.Message}");
                AppContext.UserDataLocal?.MarkAsDirty(userId);
            }
        }
        else
        {
            Debug.Log("[PlayerLevelService] Offline — TotalAnswered salvo localmente.");
            AppContext.UserDataLocal?.MarkAsDirty(userId);
        }

        MainThreadDispatcher.Enqueue(() =>
        {
            UserDataStore.UpdateTotalValidQuestionsAnswered(total);
            OnLevelProgressUpdated?.Invoke(total);
        });
    }

    public async Task CheckAndHandleLevelUp()
    {
        if (!_isInitialized || _currentUserData == null) return;

        int totalQuestions = GetTotalQuestionsCount();
        int oldLevel       = _currentUserData.PlayerLevel;
        int newLevel       = PlayerLevelConfig.CalculateLevel(
            _currentUserData.TotalValidQuestionsAnswered, totalQuestions);

        if (newLevel <= oldLevel) return;

        _currentUserData.PlayerLevel = newLevel;
        var userId = _currentUserData.UserId;

        int totalBonus = 0;
        for (int level = oldLevel + 1; level <= newLevel; level++)
            totalBonus += PlayerLevelConfig.GetBonusForLevel(level);

        await GrantLevelUpBonus(totalBonus);

        // Persiste no LiteDB imediatamente
        AppContext.UserDataLocal?.UpdateUser(_currentUserData);

        // Tenta Firestore — sem bloqueio se offline
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            try
            {
                await _firestore.UpdateUserField(userId, "PlayerLevel", newLevel)
                                .ConfigureAwait(false);
                AppContext.UserDataLocal?.MarkAsSynced(userId);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PlayerLevelService] Firestore offline: {e.Message}");
                AppContext.UserDataLocal?.MarkAsDirty(userId);
            }
        }
        else
        {
            Debug.Log("[PlayerLevelService] Offline — PlayerLevel salvo localmente.");
            AppContext.UserDataLocal?.MarkAsDirty(userId);
        }

        int capturedOld = oldLevel;
        int capturedNew = newLevel;
        MainThreadDispatcher.Enqueue(() =>
        {
            UserDataStore.UpdatePlayerLevel(capturedNew);
            OnLevelChanged?.Invoke(capturedOld, capturedNew);
        });
    }

    public async Task RecalculateTotalAnswered()
    {
        if (!_isInitialized || _currentUserData == null) return;

        int realTotal = CalculateValidAnsweredQuestionsFromData(_currentUserData);
        int oldTotal  = _currentUserData.TotalValidQuestionsAnswered;

        if (realTotal == oldTotal)
        {
            Debug.Log($"[PlayerLevelService] Já consistente: {oldTotal}");
            return;
        }

        _currentUserData.TotalValidQuestionsAnswered = realTotal;
        var userId = _currentUserData.UserId;

        // Persiste no LiteDB imediatamente
        AppContext.UserDataLocal?.UpdateUser(_currentUserData);

        // Tenta Firestore — sem bloqueio se offline
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            try
            {
                await _firestore.UpdateUserField(userId,
                    "TotalValidQuestionsAnswered", realTotal).ConfigureAwait(false);
                AppContext.UserDataLocal?.MarkAsSynced(userId);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PlayerLevelService] Firestore offline: {e.Message}");
                AppContext.UserDataLocal?.MarkAsDirty(userId);
            }
        }
        else
        {
            Debug.Log("[PlayerLevelService] Offline — RecalculateTotalAnswered salvo localmente.");
            AppContext.UserDataLocal?.MarkAsDirty(userId);
        }

        int capturedReal = realTotal;
        int capturedOld  = oldTotal;
        MainThreadDispatcher.Enqueue(() =>
        {
            UserDataStore.UpdateTotalValidQuestionsAnswered(capturedReal);
            OnLevelProgressUpdated?.Invoke(capturedReal);
            Debug.Log($"[PlayerLevelService] Recalculado: {capturedOld} → {capturedReal}");
        });
    }

    // -------------------------------------------------------
    // IPlayerLevelService — getters
    // -------------------------------------------------------
    public int GetCurrentLevel()                => _currentUserData?.PlayerLevel ?? 1;
    public int GetTotalValidAnswered()           => _currentUserData?.TotalValidQuestionsAnswered ?? 0;
    public int GetTotalQuestionsInAllDatabanks() => _currentUserData?.TotalQuestionsInAllDatabanks ?? 0;

    public float GetProgressInCurrentLevel()
    {
        if (_currentUserData == null) return 0f;

        int totalQuestions = GetTotalQuestionsCount();
        int currentLevel   = _currentUserData.PlayerLevel;
        var threshold      = PlayerLevelConfig.GetThresholdForLevel(currentLevel);

        float currentPercentage = (float)_currentUserData.TotalValidQuestionsAnswered / totalQuestions;
        float levelRange        = threshold.MaxPercentage - threshold.MinPercentage;
        float progressInLevel   = (currentPercentage - threshold.MinPercentage) / levelRange;

        return Mathf.Clamp01(progressInLevel);
    }

    public int GetQuestionsUntilNextLevel()
    {
        if (_currentUserData == null) return 0;
        if (_currentUserData.PlayerLevel >= 10) return 0;

        int totalQuestions = GetTotalQuestionsCount();
        int nextLevel      = _currentUserData.PlayerLevel + 1;
        var nextThreshold  = PlayerLevelConfig.GetThresholdForLevel(nextLevel);

        int questionsNeeded = nextThreshold.GetMinRequiredQuestions(totalQuestions);
        int remaining       = questionsNeeded - _currentUserData.TotalValidQuestionsAnswered;

        return Mathf.Max(0, remaining);
    }

    public int GetQuestionsAtLevelStart()
    {
        if (_currentUserData == null) return 0;

        int totalQuestions   = GetTotalQuestionsCount();
        int currentLevel     = _currentUserData.PlayerLevel;
        var currentThreshold = PlayerLevelConfig.GetThresholdForLevel(currentLevel);

        return currentThreshold.GetMinRequiredQuestions(totalQuestions);
    }

    // -------------------------------------------------------
    // Helpers privados
    // -------------------------------------------------------
    private async Task GrantLevelUpBonus(int bonusPoints)
    {
        _currentUserData.Score     += bonusPoints;
        _currentUserData.WeekScore += bonusPoints;

        // UpdateUserScores já gerencia LiteDB + Firestore com fallback offline
        await _userDataSync.UpdateUserScores(
            _currentUserData.UserId,
            bonusPoints,
            0, "", false
        );

        // UpdateUserScores já retorna no main thread (tem await Task.Yield internamente)
        UserDataStore.CurrentUserData = _currentUserData;
        Debug.Log($"[PlayerLevelService] Bônus concedido: {bonusPoints} pontos");
    }

    private int _cachedTotalQuestions = 0;
    private int GetTotalQuestionsCount()
    {
        if (_cachedTotalQuestions > 0) return _cachedTotalQuestions;

        // 1. Tenta do UserData (já sincronizado com o Firestore)
        int total = _currentUserData?.TotalQuestionsInAllDatabanks ?? 0;

        // 2. Tenta do DatabaseStatisticsManager — só se já estiver inicializado
        if (total <= 0 && _statistics != null && _statistics.IsInitialized)
        {
            total = _statistics.GetTotalQuestionsCount();
            Debug.Log($"[PlayerLevelService] Total obtido do IStatisticsProvider: {total}");
        }

        // Não cacheia zero — tenta novamente na próxima chamada quando as
        // estatísticas estiverem prontas (ex: logo após o registro de novo usuário)
        if (total <= 0)
        {
            Debug.LogWarning("[PlayerLevelService] Total de questões ainda não disponível.");
            return 0;
        }

        _cachedTotalQuestions = total;
        return total;
    }
}