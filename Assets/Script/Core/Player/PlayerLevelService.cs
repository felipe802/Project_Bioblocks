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
    //
    // Duas migrações possíveis (idempotentes):
    //   1. TotalValidQuestionsAnswered desacoplado de AnsweredQuestions
    //      — recalcula via CalculateValidAnsweredQuestionsFromData.
    //   2. LevelSnapshotDenominator ausente (==0) — cliente pré-fix do bug
    //      de "pulo de nível". Deriva snapshot que preserva o PlayerLevel
    //      atual do usuário; a partir desse ponto o nível só muda quando
    //      o jogador responde questões suficientes para cruzar o threshold
    //      no snapshot congelado.
    // -------------------------------------------------------
    private async void PerformMigrationIfNeeded()
    {
        if (_currentUserData == null) return;
        if (string.IsNullOrEmpty(_currentUserData.UserId)) return;
        if (_firestore == null) return;
        if (_isMigrating) return;

        int realTotal  = CalculateValidAnsweredQuestionsFromData(_currentUserData);
        int savedTotal = _currentUserData.TotalValidQuestionsAnswered;

        bool answeredMismatch = (realTotal != savedTotal);
        bool snapshotMissing  = (_currentUserData.LevelSnapshotDenominator <= 0);

        if (!answeredMismatch && !snapshotMissing)
        {
            Debug.Log($"[PlayerLevelService] Dados consistentes: {savedTotal}, snapshot={_currentUserData.LevelSnapshotDenominator}. Nenhuma migração necessária.");
            return;
        }

        Debug.Log($"[PlayerLevelService] Migração necessária: answeredMismatch={answeredMismatch}, snapshotMissing={snapshotMissing}. Corrigindo...");

        _isMigrating = true;
        try
        {
            int correctedTotal = answeredMismatch ? realTotal : savedTotal;
            int storedLevel    = _currentUserData.PlayerLevel;

            _currentUserData.TotalValidQuestionsAnswered = correctedTotal;

            // --- Snapshot: preserva o nível atual se o campo ainda não existe.
            //     Nunca diminui o nível (regra de produto).
            int snapshot = _currentUserData.LevelSnapshotDenominator;
            if (snapshot <= 0)
            {
                int currentTotal = GetTotalQuestionsCount();
                snapshot = ComputeInitialSnapshotForLevel(storedLevel, correctedTotal, currentTotal);
                _currentUserData.LevelSnapshotDenominator = snapshot;
                Debug.Log($"[PlayerLevelService] Snapshot inicial derivado: storedLevel={storedLevel}, answered={correctedTotal}, currentTotal={currentTotal} → snapshot={snapshot}");
            }

            // --- PlayerLevel: monotônico. Nunca desce.
            //     Usa snapshot como denominador — comportamento correto pós-fix.
            int computedLevel = PlayerLevelConfig.CalculateLevel(correctedTotal, snapshot);
            int newLevel      = Mathf.Max(storedLevel, computedLevel);
            _currentUserData.PlayerLevel = newLevel;

            // Persiste no LiteDB imediatamente — funciona offline
            AppContext.UserDataLocal?.UpdateUser(_currentUserData);

            // Tenta Firestore — sem bloqueio se offline
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                try
                {
                    if (answeredMismatch)
                    {
                        await _firestore.UpdateUserField(_currentUserData.UserId,
                            "TotalValidQuestionsAnswered", correctedTotal).ConfigureAwait(false);
                    }
                    if (newLevel != storedLevel)
                    {
                        await _firestore.UpdateUserField(_currentUserData.UserId,
                            "PlayerLevel", newLevel).ConfigureAwait(false);
                    }
                    if (snapshotMissing)
                    {
                        await _firestore.UpdateUserField(_currentUserData.UserId,
                            "LevelSnapshotDenominator", snapshot).ConfigureAwait(false);
                    }
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
                Debug.Log($"[PlayerLevelService] Migração concluída. Level: {newLevel}, Questões: {correctedTotal}, Snapshot: {snapshot}");
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

    /// <summary>
    /// Calcula um <c>LevelSnapshotDenominator</c> inicial que preserva
    /// o nível atual do usuário. Usado exclusivamente na migração de
    /// clientes pré-fix (onde o campo ainda não existia).
    ///
    /// Estratégia: escolhe o MAIOR entre:
    ///   - snapshotFromLevel: denominador que coloca o usuário na base
    ///     do seu nível atual (<c>answered / minPct(level)</c>).
    ///   - currentTotal: total atual canônico do app.
    ///
    /// Dessa forma o snapshot nunca encolhe o nível do usuário e respeita
    /// o total canônico quando maior. Para nível 1 ou dados degenerados,
    /// cai no total atual com piso de 1 para evitar divisão por zero.
    /// </summary>
    private int ComputeInitialSnapshotForLevel(int storedLevel, int answered, int currentTotal)
    {
        int safeCurrentTotal = Mathf.Max(1, currentTotal);

        if (storedLevel <= 1 || answered <= 0)
            return safeCurrentTotal;

        var threshold = PlayerLevelConfig.GetThresholdForLevel(storedLevel);
        float minPct  = threshold.MinPercentage;

        if (minPct <= 0f)
            return safeCurrentTotal;

        // snapshot que coloca o jogador na base do nível atual
        int snapshotFromLevel = Mathf.CeilToInt(answered / minPct);

        return Mathf.Max(snapshotFromLevel, safeCurrentTotal);
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

        int answered    = _currentUserData.TotalValidQuestionsAnswered;
        int oldLevel    = _currentUserData.PlayerLevel;
        int snapshot    = GetEffectiveSnapshotDenominator();
        if (snapshot <= 0) return;

        // Level-up em passos — um nível por vez — e refresca o snapshot
        // no momento EXATO do level-up (fonte única de verdade).
        //
        // Regras:
        //   - Snapshot é monotonicamente não-decrescente: newSnapshot = max(currentTotal, oldSnapshot).
        //     Assim, se o total do app encolher depois, o jogador mantém o nível sem pular.
        //   - Para cada nível ganho, concede bônus do nível DESTINO.
        //   - Nunca ultrapassa <see cref="PlayerLevelConfig.MaxLevel"/>.
        //   - Nunca diminui o nível.
        int currentLevel     = oldLevel;
        int effectiveSnapshot = snapshot;
        int totalBonus        = 0;

        while (PlayerLevelConfig.CanLevelUp(currentLevel, answered, effectiveSnapshot))
        {
            int nextLevel = currentLevel + 1;
            totalBonus   += PlayerLevelConfig.GetBonusForLevel(nextLevel);
            currentLevel  = nextLevel;

            // Refresca snapshot — monotônico não-decrescente
            int currentTotal = GetTotalQuestionsCount();
            if (currentTotal > effectiveSnapshot)
                effectiveSnapshot = currentTotal;

            if (currentLevel >= PlayerLevelConfig.MaxLevel) break;
        }

        if (currentLevel == oldLevel) return;

        int newLevel = currentLevel;
        _currentUserData.PlayerLevel              = newLevel;
        _currentUserData.LevelSnapshotDenominator = effectiveSnapshot;
        var userId = _currentUserData.UserId;

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
                await _firestore.UpdateUserField(userId, "LevelSnapshotDenominator", effectiveSnapshot)
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

        // Usa snapshot — denominador consistente com o nível atual do jogador.
        int denominator    = GetEffectiveSnapshotDenominator();
        if (denominator <= 0) return 0f;

        int currentLevel   = _currentUserData.PlayerLevel;
        var threshold      = PlayerLevelConfig.GetThresholdForLevel(currentLevel);

        float currentPercentage = (float)_currentUserData.TotalValidQuestionsAnswered / denominator;
        float levelRange        = threshold.MaxPercentage - threshold.MinPercentage;
        if (levelRange <= 0f) return 1f;
        float progressInLevel   = (currentPercentage - threshold.MinPercentage) / levelRange;

        return Mathf.Clamp01(progressInLevel);
    }

    public int GetQuestionsUntilNextLevel()
    {
        if (_currentUserData == null) return 0;
        if (_currentUserData.PlayerLevel >= PlayerLevelConfig.MaxLevel) return 0;

        int denominator    = GetEffectiveSnapshotDenominator();
        if (denominator <= 0) return 0;

        int nextLevel      = _currentUserData.PlayerLevel + 1;
        var nextThreshold  = PlayerLevelConfig.GetThresholdForLevel(nextLevel);

        int questionsNeeded = nextThreshold.GetMinRequiredQuestions(denominator);
        int remaining       = questionsNeeded - _currentUserData.TotalValidQuestionsAnswered;

        return Mathf.Max(0, remaining);
    }

    public int GetQuestionsAtLevelStart()
    {
        if (_currentUserData == null) return 0;

        int denominator      = GetEffectiveSnapshotDenominator();
        if (denominator <= 0) return 0;

        int currentLevel     = _currentUserData.PlayerLevel;
        var currentThreshold = PlayerLevelConfig.GetThresholdForLevel(currentLevel);

        return currentThreshold.GetMinRequiredQuestions(denominator);
    }

    /// <summary>
    /// Retorna o denominador efetivo para cálculos de nível/progresso.
    /// Prioriza o <c>LevelSnapshotDenominator</c> persistido no <see cref="UserData"/>;
    /// se ausente (cliente antigo), deriva um snapshot que preserva o nível atual.
    /// </summary>
    private int GetEffectiveSnapshotDenominator()
    {
        int snapshot = _currentUserData?.LevelSnapshotDenominator ?? 0;
        if (snapshot > 0) return snapshot;

        int answered     = _currentUserData?.TotalValidQuestionsAnswered ?? 0;
        int storedLevel  = _currentUserData?.PlayerLevel ?? 1;
        int currentTotal = GetTotalQuestionsCount();
        return ComputeInitialSnapshotForLevel(storedLevel, answered, currentTotal);
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
    /// <summary>
    /// Retorna o total atual (canônico) de questões — obtido preferencialmente
    /// do <see cref="IStatisticsProvider"/> (que lê Config/QuestionStats no Firestore).
    /// O campo <c>UserData.TotalQuestionsInAllDatabanks</c> é apenas um cache
    /// da última leitura canônica e deve ser usado somente como fallback offline.
    /// </summary>
    private int GetTotalQuestionsCount()
    {
        if (_cachedTotalQuestions > 0) return _cachedTotalQuestions;

        // 1. Fonte canônica — IStatisticsProvider (Config/QuestionStats).
        int total = 0;
        if (_statistics != null && _statistics.IsInitialized)
        {
            total = _statistics.GetTotalQuestionsCount();
            if (total > 0)
                Debug.Log($"[PlayerLevelService] Total obtido do IStatisticsProvider: {total}");
        }

        // 2. Fallback offline — cache em UserData (última leitura sincronizada).
        if (total <= 0)
            total = _currentUserData?.TotalQuestionsInAllDatabanks ?? 0;

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