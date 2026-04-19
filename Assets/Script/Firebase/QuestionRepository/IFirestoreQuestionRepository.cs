using System.Collections.Generic;
using System.Threading.Tasks;
using QuestionSystem;

public interface IFirestoreQuestionRepository
{
    Task<List<Question>> GetAllQuestions();
    Task<List<Question>> GetQuestionsByDatabankName(string databankName);
}