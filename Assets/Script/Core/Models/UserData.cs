using System;
using System.Collections.Generic;

[System.Serializable]
public class UserData
{
    public string UserId               { get; set; }
    public string NickName             { get; set; }
    public string Name                 { get; set; }
    public string Email                { get; set; }
    public string ProfileImageUrl      { get; set; }
    public int    Score                { get; set; }
    public int    WeekScore            { get; set; }
    public int    QuestionTypeProgress { get; set; }
    public DateTime CreatedTime        { get; set; }
    public bool   IsUserRegistered     { get; set; }
    public int    PlayerLevel          { get; set; } = 1;
    public int    TotalValidQuestionsAnswered  { get; set; } = 0;
    public int    TotalQuestionsInAllDatabanks { get; set; } = 0;
    public Dictionary<string, List<int>> AnsweredQuestions  { get; set; }
    public Dictionary<string, bool>      ResetDatabankFlags { get; set; }

    // -------------------------------------------------------
    // Construtores
    // -------------------------------------------------------
    public UserData()
    {
        AnsweredQuestions  = new Dictionary<string, List<int>>();
        ResetDatabankFlags = new Dictionary<string, bool>();
        IsUserRegistered   = false;
        PlayerLevel        = 1;
        TotalValidQuestionsAnswered  = 0;
        TotalQuestionsInAllDatabanks = 0;
    }

    public UserData(
        string userId,
        string nickName,
        string name,
        string email,
        string profileImageUrl    = null,
        int    score              = 0,
        int    weekScore          = 0,
        int    questionTypeProgress = 0,
        bool   isRegistered       = false)
    {
        UserId             = userId;
        NickName           = nickName;
        Name               = name;
        Email              = email;
        ProfileImageUrl    = profileImageUrl;
        Score              = score;
        WeekScore          = weekScore;
        QuestionTypeProgress = questionTypeProgress;
        CreatedTime        = DateTime.UtcNow; // ← sem Timestamp.FromDateTime
        IsUserRegistered   = isRegistered;
        PlayerLevel        = 1;
        TotalValidQuestionsAnswered  = 0;
        TotalQuestionsInAllDatabanks = 0;
        AnsweredQuestions  = new Dictionary<string, List<int>>();
        ResetDatabankFlags = new Dictionary<string, bool>();
    }

    // -------------------------------------------------------
    // Helpers de data — sem dependência externa
    // -------------------------------------------------------
    public DateTime GetCreatedDateTime()
        => CreatedTime;

    public string GetFormattedCreatedTime()
        => CreatedTime.ToLocalTime().ToString("dd/MM/yyyy");

    public string GetFormattedCreatedTimeWithHour()
        => CreatedTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");

    // -------------------------------------------------------
    // Mutation helpers
    // -------------------------------------------------------
    public void SetUserRegistered(bool registered)
        => IsUserRegistered = registered;
}