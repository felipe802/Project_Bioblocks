using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FakeFirestoreRepository : IFirestoreRepository
{
    private readonly Dictionary<string, UserData> _users = new();
    private readonly List<UserData> _allUsers = new();

    public int UpdateUserFieldCallCount      { get; private set; }
    public int UpdateUserScoresCallCount     { get; private set; }
    public string LastUpdatedField           { get; private set; }
    public object LastUpdatedValue           { get; private set; }
    public bool UpdateProfileImageUrlCalled  { get; private set; }
    public string LastProfileImageUrl        { get; private set; }

    // -------------------------------------------------------
    // Rastreamento de deleções por coleção
    // -------------------------------------------------------

    /// <summary>
    /// Registra todos os documentos deletados: [coleção] → conjunto de documentIds.
    /// Permite que os testes verifiquem exatamente quais coleções/documentos foram apagados.
    /// </summary>
    public Dictionary<string, HashSet<string>> DeletedDocuments { get; } = new();

    /// <summary>
    /// Retorna true se o documento foi deletado da coleção especificada.
    /// </summary>
    public bool WasDocumentDeleted(string collection, string documentId)
        => DeletedDocuments.TryGetValue(collection, out var ids) && ids.Contains(documentId);

    /// <summary>Total de chamadas a DeleteDocument (qualquer coleção).</summary>
    public int DeleteDocumentCallCount { get; private set; }

    // -------------------------------------------------------
    // Listener state — fake
    // -------------------------------------------------------
    public bool IsListening { get; private set; }

    // -------------------------------------------------------
    // Configuração
    // -------------------------------------------------------

    /// <summary>
    /// Adiciona usuário ao _users E ao _allUsers.
    /// AreNicknameTaken retornará true para o nickname desse usuário.
    /// Use quando quiser simular um nickname já existente.
    /// </summary>
    public void SetFakeUser(UserData user)
    {
        _users[user.UserId] = user;
        if (!_allUsers.Exists(u => u.UserId == user.UserId))
            _allUsers.Add(user);
    }

    /// <summary>
    /// Adiciona usuário apenas ao _users (para GetUserData).
    /// NÃO adiciona ao _allUsers — AreNicknameTaken retornará false.
    /// Use nos testes de registro bem-sucedido, onde o usuário
    /// ainda não existe e será criado pelo RegisterUserAsync.
    /// </summary>
    public void SetFakeUserForGetUserData(UserData user)
    {
        _users[user.UserId] = user;
    }

    // -------------------------------------------------------
    // IFirestoreRepository
    // -------------------------------------------------------
    public bool IsInitialized => true;
    public void Initialize() { }

    public Task<UserData> GetUserData(string userId)
    {
        _users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task CreateUserDocument(UserData userData)
    {
        _users[userData.UserId] = userData;
        return Task.CompletedTask;
    }

    public Task UpdateUserData(UserData userData)
    {
        _users[userData.UserId] = userData;
        return Task.CompletedTask;
    }

    public Task UpdateUserScore(string userId, int newScore, int questionNumber,
                                string databankName, bool isCorrect)
    {
        if (_users.TryGetValue(userId, out var user))
            user.Score = newScore;
        return Task.CompletedTask;
    }

    public Task UpdateUserScores(string userId, int additionalScore, int questionNumber,
                                 string databankName, bool isCorrect, UserData capturedUserData)
    {
        UpdateUserScoresCallCount++;
        if (_users.TryGetValue(userId, out var user))
        {
            user.Score     += additionalScore;
            user.WeekScore += additionalScore;
        }
        return Task.CompletedTask;
    }

    public Task UpdateUserWeekScore(string userId, int additionalScore)
    {
        if (_users.TryGetValue(userId, out var user))
            user.WeekScore += additionalScore;
        return Task.CompletedTask;
    }

    public Task UpdateUserField(string userId, string fieldName, object value)
    {
        UpdateUserFieldCallCount++;
        LastUpdatedField = fieldName;
        LastUpdatedValue = value;

        if (_users.TryGetValue(userId, out var user))
        {
            switch (fieldName)
            {
                case "PlayerLevel":                  user.PlayerLevel = Convert.ToInt32(value); break;
                case "TotalValidQuestionsAnswered":  user.TotalValidQuestionsAnswered = Convert.ToInt32(value); break;
                case "TotalQuestionsInAllDatabanks": user.TotalQuestionsInAllDatabanks = Convert.ToInt32(value); break;
                case "Score":                        user.Score = Convert.ToInt32(value); break;
                case "WeekScore":                    user.WeekScore = Convert.ToInt32(value); break;
                case "ProfileImageUrl":              user.ProfileImageUrl = value?.ToString(); break;
            }
        }
        return Task.CompletedTask;
    }

    public Task UpdateUserProgress(string userId, int progress)
    {
        if (_users.TryGetValue(userId, out var user))
            user.QuestionTypeProgress = progress;
        return Task.CompletedTask;
    }

    public Task UpdateUserProfileImageUrl(string userId, string imageUrl)
    {
        UpdateProfileImageUrlCalled = true;
        LastProfileImageUrl = imageUrl;
        if (_users.TryGetValue(userId, out var user))
            user.ProfileImageUrl = imageUrl;
        return Task.CompletedTask;
    }

    public Task ResetAnsweredQuestions(string userId, string databankName)
    {
        if (_users.TryGetValue(userId, out var user))
            user.AnsweredQuestions?.Remove(databankName);
        return Task.CompletedTask;
    }

    public Task EnsureWeekScoreField() => Task.CompletedTask;

    public Task DeleteDocument(string collection, string documentId)
    {
        // Rastreia a deleção para verificação nos testes
        if (!DeletedDocuments.ContainsKey(collection))
            DeletedDocuments[collection] = new HashSet<string>();
        DeletedDocuments[collection].Add(documentId);
        DeleteDocumentCallCount++;

        // Remove do _users apenas quando a coleção for "Users"
        if (collection == "Users")
            _users.Remove(documentId);

        return Task.CompletedTask;
    }

    public Task<bool> AreNicknameTaken(string nickName)
    {
        bool taken = _allUsers.Exists(u => u.NickName == nickName);
        return Task.FromResult(taken);
    }

    public Task<List<UserData>> GetAllUsersData()
        => Task.FromResult(new List<UserData>(_allUsers));

    // -------------------------------------------------------
    // Listeners — fake dispara callbacks imediatamente
    // -------------------------------------------------------
    public void ListenToUserData(
        string userId,
        Action<int> onScoreChanged = null,
        Action<int> onWeekScoreChanged = null,
        Action<Dictionary<string, List<int>>> onAnsweredQuestionsChanged = null)
    {
        IsListening = true;
        if (!_users.TryGetValue(userId, out var user)) return;
        onScoreChanged?.Invoke(user.Score);
        onWeekScoreChanged?.Invoke(user.WeekScore);
        onAnsweredQuestionsChanged?.Invoke(user.AnsweredQuestions);
    }

    public IDisposable ListenToScore(string userId,
        Action<int> onScoreChanged,
        Action<int> onWeekScoreChanged)
    {
        IsListening = true;
        if (_users.TryGetValue(userId, out var user))
        {
            onScoreChanged?.Invoke(user.Score);
            onWeekScoreChanged?.Invoke(user.WeekScore);
        }
        return null;
    }

    public IDisposable ListenToAnsweredQuestions(string userId,
        Action<Dictionary<string, List<int>>> onChanged)
    {
        if (_users.TryGetValue(userId, out var user))
            onChanged?.Invoke(user.AnsweredQuestions);
        return null;
    }

    public void StopListening()
    {
        IsListening = false;
    }

    public void ResumeListening(string userId,
        Action<int> onScoreChanged = null,
        Action<int> onWeekScoreChanged = null,
        Action<Dictionary<string, List<int>>> onAnsweredQuestionsChanged = null)
    {
        IsListening = true;
    }
}