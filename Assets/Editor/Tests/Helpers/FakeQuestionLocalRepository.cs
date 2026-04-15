using System;
using System.Collections.Generic;
using System.Linq;
using QuestionSystem;

/// <summary>
/// Fake do IQuestionLocalRepository para testes unitários.
/// Opera inteiramente em memória — sem LiteDB.
///
/// Cenários suportados:
///   - Cache vazio (estado inicial padrão)
///   - Cache populado (via SetQuestions ou SetQuestionsSavedAt)
///   - Cache com timestamp configurável (para testar lógica de staleness)
///   - Lançamento de exceção em SaveQuestions (simula disco cheio, corrupção, etc.)
///
/// Como usar:
///   var fakeLocal = new FakeQuestionLocalRepository();
///   fakeLocal.SetQuestions(QuestionTestHelpers.MakeQuestions(10), savedDaysAgo: 1);
///   // → cache válido com 10 questões salvas há 1 dia
///
///   var fakeLocal = new FakeQuestionLocalRepository();
///   fakeLocal.SetQuestions(QuestionTestHelpers.MakeQuestions(5), savedDaysAgo: 30);
///   // → cache expirado com 5 questões
/// </summary>
public class FakeQuestionLocalRepository : IQuestionLocalRepository
{
    // ── Storage em memória ─────────────────────────────────────────────────────
    // Chave: questionDatabankName   Valor: lista de questões daquele banco
    private readonly Dictionary<string, List<Question>> _storage =
        new Dictionary<string, List<Question>>();

    private DateTime _latestCacheTimestamp = DateTime.MinValue;

    // ── Comportamento de erro configurável ────────────────────────────────────
    /// <summary>Se true, SaveQuestions() lança ExceptionToThrow.</summary>
    public bool ShouldThrowOnSave { get; set; } = false;

    public Exception ExceptionToThrow { get; set; } =
        new Exception("Simulated local storage error");

    // ── Rastreamento de chamadas ───────────────────────────────────────────────
    public int SaveQuestionsCallCount { get; private set; }
    public int ClearAllCallCount      { get; private set; }

    /// <summary>Número total de questões passadas para SaveQuestions nas últimas chamadas.</summary>
    public int LastSaveCount          { get; private set; }

    // ── Configuração de cenários ───────────────────────────────────────────────

    /// <summary>
    /// Popula o cache com as questões fornecidas, como se tivessem sido salvas
    /// há <paramref name="savedDaysAgo"/> dias.
    /// </summary>
    public void SetQuestions(List<Question> questions, double savedDaysAgo = 0)
    {
        _storage.Clear();

        if (questions == null || questions.Count == 0)
        {
            _latestCacheTimestamp = DateTime.MinValue;
            return;
        }

        foreach (var q in questions)
        {
            if (!_storage.ContainsKey(q.questionDatabankName))
                _storage[q.questionDatabankName] = new List<Question>();
            _storage[q.questionDatabankName].Add(q);
        }

        _latestCacheTimestamp = DateTime.UtcNow.AddDays(-savedDaysAgo);
    }

    /// <summary>
    /// Define diretamente o timestamp retornado por GetLatestCacheTimestamp().
    /// Útil para testar a fronteira exata de expiração sem depender do relógio.
    /// </summary>
    public void SetCacheTimestamp(DateTime timestamp)
    {
        _latestCacheTimestamp = timestamp;
    }

    // ── IQuestionLocalRepository ───────────────────────────────────────────────
    public void SaveQuestions(List<Question> questions)
    {
        SaveQuestionsCallCount++;
        LastSaveCount = questions?.Count ?? 0;

        if (ShouldThrowOnSave)
            throw ExceptionToThrow;

        if (questions == null || questions.Count == 0) return;

        foreach (var q in questions)
        {
            if (!_storage.ContainsKey(q.questionDatabankName))
                _storage[q.questionDatabankName] = new List<Question>();

            // Upsert por questionNumber dentro do banco
            var existing = _storage[q.questionDatabankName]
                .FindIndex(x => x.questionNumber == q.questionNumber);
            if (existing >= 0)
                _storage[q.questionDatabankName][existing] = q;
            else
                _storage[q.questionDatabankName].Add(q);
        }

        _latestCacheTimestamp = DateTime.UtcNow;
    }

    public List<Question> GetQuestionsByDatabankName(string databankName)
    {
        if (_storage.TryGetValue(databankName, out var questions))
            return new List<Question>(questions);
        return new List<Question>();
    }

    public List<Question> GetAllQuestions()
    {
        return _storage.Values
                       .SelectMany(list => list)
                       .ToList();
    }

    public bool HasAnyQuestions()
    {
        return _storage.Values.Any(list => list.Count > 0);
    }

    public DateTime GetLatestCacheTimestamp()
    {
        return _latestCacheTimestamp;
    }

    public void ClearAll()
    {
        ClearAllCallCount++;
        _storage.Clear();
        _latestCacheTimestamp = DateTime.MinValue;
    }

    // ── Utilitário ─────────────────────────────────────────────────────────────
    /// <summary>Zera tudo para reutilização entre testes.</summary>
    public void Reset()
    {
        _storage.Clear();
        _latestCacheTimestamp   = DateTime.MinValue;
        ShouldThrowOnSave       = false;
        SaveQuestionsCallCount  = 0;
        ClearAllCallCount       = 0;
        LastSaveCount           = 0;
    }
}