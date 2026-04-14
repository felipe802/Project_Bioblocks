using QuestionSystem;
using System.Collections.Generic;

namespace QuestionSystem
{
    [System.Serializable]
    public class Question
    {
        public string globalId;
        public string questionDatabankName;
        public string topic;
        public string subtopic;
        public string displayName;
        public string questionText;
        public string[] answers;
        public int correctIndex;
        public int questionNumber;
        public bool isImageAnswer;
        public bool isImageQuestion;
        public string questionImagePath;
        public int questionLevel;
        public bool questionInDevelopment;
        public string bloomLevel;
        public List<string> conceptTags;
        public List<string> prerequisites;
        public QuestionHint questionHint;
    }
}

