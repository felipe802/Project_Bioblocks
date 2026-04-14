using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QuestionSystem;

/// <summary>
/// Repositório LiteDB para questões. Segue o mesmo padrão de UserDataLocalRepository.
/// Todas as operações são síncronas (LiteDB é síncrono por design).
/// </summary>
public class QuestionLocalRepository : MonoBehaviour, IQuestionLocalRepository
{
    private ILiteDBManager _db;

    public void InjectDependencies(ILiteDBManager db)
    {
        _db = db;
    }

    // ── Escrita ────────────────────────────────────────────────────────────────

    public void SaveQuestions(List<Question> questions)
    {
        if (questions == null || questions.Count == 0)
        {
            Debug.LogWarning("[QuestionLocalRepository] Lista de questões vazia — nada salvo.");
            return;
        }

        try
        {
            int saved = 0;
            foreach (var q in questions)
            {
                var doc = QuestionDB.FromDomain(q);
                _db.Questions.Upsert(doc);   // Insert ou Update baseado no GlobalId
                saved++;
            }
            Debug.Log($"[QuestionLocalRepository] {saved} questões salvas/atualizadas no LiteDB.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuestionLocalRepository] Erro ao salvar questões: {e.Message}");
            throw;
        }
    }

    // ── Leitura ────────────────────────────────────────────────────────────────

    public List<Question> GetQuestionsByDatabankName(string databankName)
    {
        try
        {
            var docs = _db.Questions
                          .Find(q => q.QuestionDatabankName == databankName)
                          .ToList();

            return docs.Select(d => d.ToDomain()).ToList();
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuestionLocalRepository] Erro ao buscar questões de '{databankName}': {e.Message}");
            return new List<Question>();
        }
    }

    public List<Question> GetAllQuestions()
    {
        try
        {
            return _db.Questions.FindAll()
                      .Select(d => d.ToDomain())
                      .ToList();
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuestionLocalRepository] Erro em GetAllQuestions: {e.Message}");
            return new List<Question>();
        }
    }

    // ── Metadados de cache ─────────────────────────────────────────────────────

    public bool HasAnyQuestions()
    {
        try
        {
            return _db.Questions.Count() > 0;
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuestionLocalRepository] Erro em HasAnyQuestions: {e.Message}");
            return false;
        }
    }

    public DateTime GetLatestCacheTimestamp()
    {
        try
        {
            var latest = _db.Questions.FindAll()
                            .OrderByDescending(q => q.CachedAt)
                            .FirstOrDefault();

            return latest?.CachedAt ?? DateTime.MinValue;
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuestionLocalRepository] Erro em GetLatestCacheTimestamp: {e.Message}");
            return DateTime.MinValue;
        }
    }

    // ── Limpeza ────────────────────────────────────────────────────────────────

    public void ClearAll()
    {
        try
        {
            int deleted = _db.Questions.DeleteAll();
            Debug.Log($"[QuestionLocalRepository] {deleted} questões removidas do cache local.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuestionLocalRepository] Erro em ClearAll: {e.Message}");
            throw;
        }
    }
}