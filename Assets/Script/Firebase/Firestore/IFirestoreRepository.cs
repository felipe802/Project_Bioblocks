using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IFirestoreRepository
{
    bool IsInitialized { get; }
    void Initialize();
    Task<UserData> GetUserData(string userId);
    Task CreateUserDocument(UserData userData);
    Task UpdateUserData(UserData userData);
    Task UpdateUserScore(string userId, int newScore, int questionNumber, string databankName, bool isCorrect);
    Task UpdateUserScores(string userId, int additionalScore, int questionNumber, string databankName, bool isCorrect, UserData capturedUserData);
    Task UpdateUserWeekScore(string userId, int additionalScore);
    Task UpdateUserField(string userId, string fieldName, object value);
    Task UpdateUserProgress(string userId, int progress);
    Task UpdateUserProfileImageUrl(string userId, string imageUrl);
    Task ResetAnsweredQuestions(string userId, string databankName);
    Task EnsureWeekScoreField();
    Task DeleteDocument(string collection, string documentId);
    Task<bool> AreNicknameTaken(string nickName);
    Task<List<UserData>> GetAllUsersData();

    /// <summary>
    /// Lê Config/QuestionStats — fonte única de verdade do total de questões.
    /// Mantido pelo Cloud Function <c>syncQuestionStats</c>. Cliente NUNCA escreve.
    /// Retorna null se o documento não existir ou se houver falha de rede.
    /// </summary>
    Task<QuestionStats> GetQuestionStats();

    // ── UserBonus / CompletedDatabanks ────────────────────────────────────────

    /// <summary>
    /// Retorna true se o databank ainda não foi marcado como completo para este usuário
    /// (ou seja, o bônus de conclusão ainda não foi concedido).
    /// Lê UserBonus/{userId}.CompletedDatabanks.
    /// </summary>
    Task<bool> IsDatabankEligibleForBonus(string userId, string databankName);

    /// <summary>
    /// Adiciona <paramref name="databankName"/> à lista CompletedDatabanks do usuário,
    /// evitando duplicatas. Escreve em UserBonus/{userId}.
    /// </summary>
    Task MarkDatabankAsCompleted(string userId, string databankName);

    IDisposable ListenToScore(string userId, Action<int> onScoreChanged, Action<int> onWeekScoreChanged);
    IDisposable ListenToAnsweredQuestions(string userId, Action<Dictionary<string, List<int>>> onChanged);
    void StopListening();
    
    void ResumeListening(string userId,
        Action<int> onScoreChanged = null,
        Action<int> onWeekScoreChanged = null,
        Action<Dictionary<string, List<int>>> onAnsweredQuestionsChanged = null)
    ;
    bool IsListening { get; }

    void ListenToUserData(string userId,
        Action<int> onScoreChanged = null,
        Action<int> onWeekScoreChanged = null,
        Action<Dictionary<string, List<int>>> onAnsweredQuestionsChanged = null
    );

    // ── Rankings ────────────────────────────────────────────────────────────
    Task<List<Ranking>> GetRankingsAsync(int limit = 50);
    Task<List<Ranking>> GetWeekRankingsAsync(int limit = 50);
}
    
    
    
    

    
    
    
    
    
    
    