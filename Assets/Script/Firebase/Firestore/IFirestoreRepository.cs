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
}
    
    
    
    

    
    
    
    
    
    
    