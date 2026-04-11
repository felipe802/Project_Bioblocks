using System;
using System.Collections.Generic;

public static class UserDataStore
{
    // -------------------------------------------------------
    // Logger injetável — sem UnityEngine
    // -------------------------------------------------------
    public static Action<string> Logger { get; set; } = _ => { }; // no-op por padrão

    // -------------------------------------------------------
    // Estado
    // -------------------------------------------------------
    private static UserData _currentUserData;
    public static event Action<UserData> OnUserDataChanged;

    public static UserData CurrentUserData
    {
        get => _currentUserData;
        set
        {
            _currentUserData = value;
            var data = value;
            MainThreadDispatcher.Enqueue(() => OnUserDataChanged?.Invoke(data));
        }
    }

    // -------------------------------------------------------
    // Mutations
    // -------------------------------------------------------
    public static void UpdateScore(int newScore)
    {
        if (_currentUserData == null) return;
        _currentUserData.Score = newScore;
        MainThreadDispatcher.Enqueue(() => OnUserDataChanged?.Invoke(_currentUserData));
        Logger($"[UserDataStore] Score atualizado para: {newScore}");
    }

    public static void UpdateWeekScore(int newWeekScore)
    {
        if (_currentUserData == null) return;
        _currentUserData.WeekScore = newWeekScore;
        MainThreadDispatcher.Enqueue(() => OnUserDataChanged?.Invoke(_currentUserData));
        Logger($"[UserDataStore] WeekScore atualizado para: {newWeekScore}");
    }

    public static void AddScore(int additionalScore)
    {
        if (_currentUserData == null) return;
        _currentUserData.Score     += additionalScore;
        _currentUserData.WeekScore += additionalScore;
        MainThreadDispatcher.Enqueue(() => OnUserDataChanged?.Invoke(_currentUserData));
        Logger($"[UserDataStore] Score incrementado em {additionalScore}. " +
               $"Score: {_currentUserData.Score}, WeekScore: {_currentUserData.WeekScore}");
    }

    public static void UpdateUserData(UserData userData)
    {
        if (userData == null) return;
        _currentUserData = userData;
        MainThreadDispatcher.Enqueue(() => OnUserDataChanged?.Invoke(_currentUserData));
        Logger($"[UserDataStore] UserData substituído → UserId: {_currentUserData?.UserId}");
    }

    public static void UpdatePlayerLevel(int newLevel)
    {
        if (_currentUserData == null) return;
        _currentUserData.PlayerLevel = newLevel;
        MainThreadDispatcher.Enqueue(() => OnUserDataChanged?.Invoke(_currentUserData));
        Logger($"[UserDataStore] PlayerLevel atualizado para: {newLevel}");
    }

    public static void UpdateTotalValidQuestionsAnswered(int newTotal)
    {
        if (_currentUserData == null) return;
        _currentUserData.TotalValidQuestionsAnswered = newTotal;
        MainThreadDispatcher.Enqueue(() => OnUserDataChanged?.Invoke(_currentUserData));
        Logger($"[UserDataStore] TotalValidQuestionsAnswered atualizado para: {newTotal}");
    }

    public static void UpdateTotalQuestionsInAllDatabanks(int newTotal)
    {
        if (_currentUserData == null) return;
        _currentUserData.TotalQuestionsInAllDatabanks = newTotal;
        MainThreadDispatcher.Enqueue(() => OnUserDataChanged?.Invoke(_currentUserData));
        Logger($"[UserDataStore] TotalQuestionsInAllDatabanks atualizado para: {newTotal}");
    }

    public static void MarkDatabankAsReset(string databankName, bool isReset)
    {
        if (_currentUserData == null) return;
        _currentUserData.ResetDatabankFlags ??= new Dictionary<string, bool>();
        _currentUserData.ResetDatabankFlags[databankName] = isReset;
        MainThreadDispatcher.Enqueue(() => OnUserDataChanged?.Invoke(_currentUserData));
        Logger($"[UserDataStore] Databank '{databankName}' marcado como resetado: {isReset}");
    }

    public static bool IsDatabankReset(string databankName)
    {
        if (_currentUserData?.ResetDatabankFlags == null) return false;
        return _currentUserData.ResetDatabankFlags.TryGetValue(databankName, out var value) && value;
    }

    // -------------------------------------------------------
    // Reset — útil em testes
    // -------------------------------------------------------
    public static void Clear()
    {
        _currentUserData = null;
        MainThreadDispatcher.Enqueue(() => OnUserDataChanged?.Invoke(null));
    }
}