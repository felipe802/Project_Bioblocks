using System;
using System.Collections.Generic;
using LiteDB;

public class UserDataDB
{
    [BsonId]
    public string UserId { get; set; }
    public string NickName { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string ProfileImageUrl { get; set; }
    public int Score { get; set; }
    public int WeekScore { get; set; }
    public int QuestionTypeProgress { get; set; }
    public DateTime CreatedTime { get; set; }
    public bool IsUserRegistered { get; set; }
    public int PlayerLevel { get; set; }
    public int TotalValidQuestionsAnswered { get; set; }
    public int TotalQuestionsInAllDatabanks { get; set; }
    public Dictionary<string, List<int>> AnsweredQuestions { get; set; }
    public Dictionary<string, bool> ResetDatabankFlags { get; set; }
    public DateTime LastSyncedAt { get; set; }
    public bool IsDirty { get; set; }
    public DateTime SavedAt { get; set; }

    public UserDataDB()
    {
        AnsweredQuestions = new Dictionary<string, List<int>>();
        ResetDatabankFlags = new Dictionary<string, bool>();
        IsDirty = false;
    }

    public static UserDataDB FromDomain(UserData domain)
    {
        return new UserDataDB
        {
            UserId                      = domain.UserId,
            NickName                    = domain.NickName,
            Name                        = domain.Name,
            Email                       = domain.Email,
            ProfileImageUrl             = domain.ProfileImageUrl ?? "",
            Score                       = domain.Score,
            WeekScore                   = domain.WeekScore,
            QuestionTypeProgress        = domain.QuestionTypeProgress,
            CreatedTime                 = domain.CreatedTime,
            IsUserRegistered            = domain.IsUserRegistered,
            PlayerLevel                 = domain.PlayerLevel,
            TotalValidQuestionsAnswered = domain.TotalValidQuestionsAnswered,
            TotalQuestionsInAllDatabanks= domain.TotalQuestionsInAllDatabanks,
            AnsweredQuestions           = domain.AnsweredQuestions  ?? new Dictionary<string, List<int>>(),
            ResetDatabankFlags          = domain.ResetDatabankFlags ?? new Dictionary<string, bool>(),
            LastSyncedAt                = DateTime.UtcNow,
            IsDirty                     = false,
            SavedAt                     = domain.SavedAt
        };
    }

    public UserData ToDomain()
    {
        return new UserData
        {
            UserId                      = UserId,
            NickName                    = NickName,
            Name                        = Name,
            Email                       = Email,
            ProfileImageUrl             = ProfileImageUrl,
            Score                       = Score,
            WeekScore                   = WeekScore,
            QuestionTypeProgress        = QuestionTypeProgress,
            CreatedTime                 = CreatedTime,
            IsUserRegistered            = IsUserRegistered,
            PlayerLevel                 = PlayerLevel,
            TotalValidQuestionsAnswered = TotalValidQuestionsAnswered,
            TotalQuestionsInAllDatabanks= TotalQuestionsInAllDatabanks,
            AnsweredQuestions           = AnsweredQuestions  ?? new Dictionary<string, List<int>>(),
            ResetDatabankFlags          = ResetDatabankFlags ?? new Dictionary<string, bool>(),
            SavedAt = SavedAt
        };
    }
}