// Assets/Editor/Tests/Helpers/QuestionTestHelpers.cs
using System.Collections.Generic;
using System.Linq;
using QuestionSystem;

public static class QuestionTestHelpers
{
    // -------------------------------------------------------
    // Fábrica de questões
    // -------------------------------------------------------

    /// <summary>
    /// Cria uma lista de questões distribuídas por nível.
    /// Os questionNumbers são gerados sequencialmente a partir de 1.
    /// </summary>
    /// <param name="nivel1">Quantas questões de nível 1 criar</param>
    /// <param name="nivel2">Quantas questões de nível 2 criar</param>
    /// <param name="nivel3">Quantas questões de nível 3 criar</param>
    /// <param name="databankName">Nome do banco (padrão: "TestDB")</param>
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
    /// Cria uma única questão com valores mínimos válidos.
    /// </summary>
    public static Question MakeQuestion(
        int number,
        int level = 1,
        string databankName = "TestDB",
        bool inDevelopment = false)
    {
        return new Question
        {
            questionNumber       = number,
            questionLevel        = level,
            questionDatabankName = databankName,
            questionText         = $"Questão {number}",
            answers              = new[] { "A", "B", "C", "D" },
            correctIndex         = 0,
            questionInDevelopment = inDevelopment
        };
    }

    // -------------------------------------------------------
    // Conversores
    // -------------------------------------------------------

    /// <summary>
    /// Converte uma lista de questões para o formato de IDs respondidos
    /// usado pelo Firebase / LevelCalculator (questionNumber como string).
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
}