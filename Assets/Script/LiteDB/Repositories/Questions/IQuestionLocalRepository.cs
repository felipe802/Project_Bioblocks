using System;
using System.Collections.Generic;
using QuestionSystem;

public interface IQuestionLocalRepository
{
    void SaveQuestions(List<Question> questions);
    List<Question> GetQuestionsByDatabankName(string databankName);
    List<Question> GetAllQuestions();
    bool HasAnyQuestions();
    DateTime GetLatestCacheTimestamp();
    void ClearAll();
}