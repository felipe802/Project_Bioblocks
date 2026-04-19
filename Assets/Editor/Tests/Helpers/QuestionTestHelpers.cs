using System.Collections.Generic;
using System.Linq;
using QuestionSystem;

public static class QuestionTestHelpers
{
    // =======================================================================
    // Fábricas de questões — campos originais (retrocompatível)
    // =======================================================================

    /// <summary>
    /// Cria uma lista de questões distribuídas por nível.
    /// Os questionNumbers são gerados sequencialmente a partir de 1.
    /// </summary>
    public static List<Question> MakeQuestions(
        int nivel1,
        int nivel2 = 0,
        int nivel3 = 0,
        string databankName = "TestDB")
    {
        var list = new List<Question>();
        int num = 1;

        for (int i = 0; i < nivel1; i++)
            list.Add(MakeQuestion(num++, level: 1, databankName: databankName));
        for (int i = 0; i < nivel2; i++)
            list.Add(MakeQuestion(num++, level: 2, databankName: databankName));
        for (int i = 0; i < nivel3; i++)
            list.Add(MakeQuestion(num++, level: 3, databankName: databankName));

        return list;
    }

    /// <summary>
    /// Cria uma única questão com valores mínimos válidos (campos originais).
    /// </summary>
    public static Question MakeQuestion(
        int number,
        int level = 1,
        string databankName = "TestDB",
        bool inDevelopment = false)
    {
        return new Question
        {
            questionNumber        = number,
            questionLevel         = level,
            questionDatabankName  = databankName,
            questionText          = $"Questão {number}",
            answers               = new[] { "A", "B", "C", "D" },
            correctIndex          = 0,
            questionInDevelopment = inDevelopment,
            // Novos campos com valores default seguros
            globalId              = $"{databankName}_{number:D3}",
            topic                 = "",
            displayName           = "",
            subtopic              = null,
            bloomLevel            = "unclassified",
            conceptTags           = new List<string>(),
            prerequisites         = new List<string>(),
            questionHint          = new QuestionHint()
        };
    }

    // =======================================================================
    // Fábricas — campos novos do Firestore
    // =======================================================================

    /// <summary>
    /// Cria uma questão com todos os campos do Firestore preenchidos
    /// (útil para testar conversão Firestore → LiteDB → Question).
    /// </summary>
    public static Question MakeFullQuestion(
        int number,
        int level = 1,
        string databankName = "TestDB",
        string topic = "testTopic",
        string bloomLevel = "remember",
        bool inDevelopment = false)
    {
        return new Question
        {
            questionNumber        = number,
            questionLevel         = level,
            questionDatabankName  = databankName,
            questionText          = $"Questão completa {number}",
            answers               = new[] { "Resposta A", "Resposta B", "Resposta C", "Resposta D" },
            correctIndex          = 0,
            isImageAnswer         = false,
            isImageQuestion       = false,
            questionImagePath     = "",
            questionInDevelopment = inDevelopment,
            globalId              = $"{databankName}_{number:D3}",
            topic                 = topic,
            displayName           = $"Display {databankName}",
            subtopic              = $"subtopico-{number}",
            bloomLevel            = bloomLevel,
            conceptTags           = new List<string> { "tag1", "tag2" },
            prerequisites         = new List<string>(),
            questionHint          = new QuestionHint
            {
                text     = $"Dica da questão {number}",
                videoUrl = null,
                imagePath= null,
                link     = null
            }
        };
    }

    /// <summary>
    /// Cria questões para múltiplos bancos — simula o estado do Firestore
    /// com a coleção completa (10 bancos).
    /// </summary>
    /// <param name="questionsPerBank">Número de questões por banco.</param>
    /// <param name="databankNames">Nomes dos bancos. Se null, usa os 10 bancos reais.</param>
    public static List<Question> MakeQuestionsForAllDatabankNames(
        int questionsPerBank = 5,
        IEnumerable<string> databankNames = null)
    {
        var banks = databankNames ?? RealDatabankNames;
        var all   = new List<Question>();

        foreach (var bank in banks)
        {
            for (int i = 1; i <= questionsPerBank; i++)
                all.Add(MakeQuestion(i, level: (i % 2) + 1, databankName: bank));
        }

        return all;
    }

    /// <summary>
    /// Cria questões para um banco com hint preenchida.
    /// Útil para testar se a hint sobrevive ao ciclo Firestore → LiteDB → Question.
    /// </summary>
    public static List<Question> MakeQuestionsWithHint(
        int count,
        string databankName = "TestDB",
        string hintText = "Dica de teste")
    {
        var list = new List<Question>();
        for (int i = 1; i <= count; i++)
        {
            var q = MakeQuestion(i, databankName: databankName);
            q.questionHint = new QuestionHint { text = hintText };
            list.Add(q);
        }
        return list;
    }

    // =======================================================================
    // Conversores (retrocompatível)
    // =======================================================================

    /// <summary>
    /// Converte lista de questões para IDs respondidos (questionNumber como string).
    /// </summary>
    public static List<string> ToAnsweredIds(List<Question> questions)
        => questions.Select(q => q.questionNumber.ToString()).ToList();

    /// <summary>
    /// Retorna os IDs respondidos apenas das questões de um nível específico.
    /// </summary>
    public static List<string> ToAnsweredIdsForLevel(List<Question> questions, int level)
        => questions
            .Where(q => q.questionLevel == level)
            .Select(q => q.questionNumber.ToString())
            .ToList();

    // =======================================================================
    // Nomes reais dos 10 bancos (evita strings mágicas nos testes)
    // =======================================================================

    public static readonly string[] RealDatabankNames =
    {
        "AcidBaseBufferQuestionDatabase",
        "AminoacidQuestionDataBase",
        "BiochemistryIntroductionQuestionDatabase",
        "CarbohydratesQuestionDataBase",
        "EnzymeQuestionDataBase",
        "LipidsQuestionDataBase",
        "MembranesQuestionDatabase",
        "NucleicAcidsQuestionDataBase",
        "ProteinQuestionDataBase",
        "WaterQuestionDataBase"
    };
}
