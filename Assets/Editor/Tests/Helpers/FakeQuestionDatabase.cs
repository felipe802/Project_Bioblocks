// Assets/Editor/Tests/Helpers/FakeQuestionDatabase.cs
// Mock de IQuestionDatabase para testes da Sprint 2.
// Coloque este arquivo em: Assets/Editor/Tests/Helpers/

using System.Collections.Generic;
using QuestionSystem;

/// <summary>
/// Implementação fake de IQuestionDatabase para uso em testes unitários.
/// Permite configurar modo de desenvolvimento, nome do banco e lista de questões
/// sem depender de nenhuma classe Unity ou MonoBehaviour.
/// </summary>
public class FakeQuestionDatabase : IQuestionDatabase
{
    // -------------------------------------------------------
    // Configuração
    // -------------------------------------------------------

    /// <summary>Define se o banco está em modo de desenvolvimento.</summary>
    public bool IsInDevelopmentMode { get; set; } = false;

    /// <summary>Nome interno do banco (usado em logs e Firebase).</summary>
    public string DatabaseName { get; set; } = "FakeDB";

    /// <summary>Nome de exibição do banco.</summary>
    public string DisplayName { get; set; } = "Fake Database";

    /// <summary>Tipo do QuestionSet associado.</summary>
    public QuestionSet QuestionSetType { get; set; } = QuestionSet.biochem;

    /// <summary>Lista de questões retornada por GetQuestions().</summary>
    public List<Question> Questions { get; set; } = new List<Question>();

    // -------------------------------------------------------
    // IQuestionDatabase
    // -------------------------------------------------------

    public bool IsDatabaseInDevelopment() => IsInDevelopmentMode;
    public List<Question> GetQuestions()   => Questions;
    public QuestionSet GetQuestionSetType() => QuestionSetType;
    public string GetDatabankName()         => DatabaseName;
    public string GetDisplayName()          => DisplayName;

    // -------------------------------------------------------
    // Fábricas estáticas para cenários comuns
    // -------------------------------------------------------

    /// <summary>
    /// Banco em modo PRODUÇÃO com questões mistas (prod + dev).
    /// </summary>
    public static FakeQuestionDatabase ProductionWith(
        int prodCount,
        int devCount = 0,
        string name = "FakeDB")
    {
        var db = new FakeQuestionDatabase
        {
            IsInDevelopmentMode = false,
            DatabaseName        = name
        };

        int num = 1;
        for (int i = 0; i < prodCount; i++)
            db.Questions.Add(QuestionTestHelpers.MakeQuestion(num++, inDevelopment: false));
        for (int i = 0; i < devCount; i++)
            db.Questions.Add(QuestionTestHelpers.MakeQuestion(num++, inDevelopment: true));

        return db;
    }

    /// <summary>
    /// Banco em modo DESENVOLVIMENTO com questões mistas.
    /// </summary>
    public static FakeQuestionDatabase DevelopmentWith(
        int devCount,
        int prodCount = 0,
        string name = "FakeDB")
    {
        var db = new FakeQuestionDatabase
        {
            IsInDevelopmentMode = true,
            DatabaseName        = name
        };

        int num = 1;
        for (int i = 0; i < devCount; i++)
            db.Questions.Add(QuestionTestHelpers.MakeQuestion(num++, inDevelopment: true));
        for (int i = 0; i < prodCount; i++)
            db.Questions.Add(QuestionTestHelpers.MakeQuestion(num++, inDevelopment: false));

        return db;
    }

    /// <summary>
    /// Banco vazio em modo produção.
    /// </summary>
    public static FakeQuestionDatabase Empty()
        => new FakeQuestionDatabase { IsInDevelopmentMode = false };
}