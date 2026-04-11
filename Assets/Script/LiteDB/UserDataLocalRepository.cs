using System;
using System.Collections.Generic;
using UnityEngine;

public class UserDataLocalRepository : MonoBehaviour, IUserDataLocalRepository
{
    private ILiteDBManager _db;

    public void InjectDependencies(ILiteDBManager db)
    {
        _db = db;
    }

    public UserData GetUser(string userId)
    {
        try
        {
            var doc = _db.Users.FindById(userId);
            return doc?.ToDomain();
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataLocalRepository] Erro ao buscar usuário: {e.Message}");
            return null;
        }
    }

    public void SaveUser(UserData userData)
    {
        try
        {
            var doc = UserDataDB.FromDomain(userData);
            _db.Users.Insert(doc);
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataLocalRepository] Erro ao salvar usuário: {e.Message}");
            throw;
        }
    }

    public void UpdateUser(UserData userData)
    {
        try
        {
            var existing = _db.Users.FindById(userData.UserId);
            if (existing == null)
            {
                SaveUser(userData);
                return;
            }

            var doc = UserDataDB.FromDomain(userData);
            doc.IsDirty     = existing.IsDirty;
            doc.LastSyncedAt = existing.LastSyncedAt;
            _db.Users.Update(doc);
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataLocalRepository] Erro ao atualizar usuário: {e.Message}");
            throw;
        }
    }

    public void MarkAsDirty(string userId)
    {
        try
        {
            var doc = _db.Users.FindById(userId);
            if (doc == null) return;
            doc.IsDirty = true;
            _db.Users.Update(doc);
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataLocalRepository] Erro ao marcar dirty: {e.Message}");
            throw;
        }
    }

    public void MarkAsSynced(string userId)
    {
        try
        {
            var doc = _db.Users.FindById(userId);
            if (doc == null) return;
            doc.IsDirty     = false;
            doc.LastSyncedAt = DateTime.UtcNow;
            _db.Users.Update(doc);
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataLocalRepository] Erro ao marcar synced: {e.Message}");
            throw;
        }
    }

    public bool HasUser(string userId)
    {
        try
        {
            return _db.Users.FindById(userId) != null;
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataLocalRepository] Erro ao verificar usuário: {e.Message}");
            return false;
        }
    }

    public bool IsDirty(string userId)
    {
        try
        {
            var doc = _db.Users.FindById(userId);
            return doc?.IsDirty ?? false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataLocalRepository] Erro ao verificar dirty: {e.Message}");
            return false;
        }
    }

    public void DeleteUser(string userId)
    {
        try
        {
            _db.Users.Delete(userId);
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataLocalRepository] Erro ao deletar usuário: {e.Message}");
            throw;
        }
    }

    public DateTime GetLastSyncedAt(string userId)
    {
        try
        {
            var doc = _db.Users.FindById(userId);
            return doc?.LastSyncedAt ?? DateTime.MinValue;
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataLocalRepository] Erro ao buscar LastSyncedAt: {e.Message}");
            return DateTime.MinValue;
        }
    }

    public void UpdateScore(string userId, int newScore, int newWeekScore)
    {
        try
        {
            var doc = _db.Users.FindById(userId);
            if (doc == null) return;
            doc.Score      = newScore;
            doc.WeekScore  = newWeekScore;
            doc.IsDirty    = true;
            _db.Users.Update(doc);
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataLocalRepository] Erro ao atualizar score: {e.Message}");
            throw;
        }
    }

    public void AddAnsweredQuestion(string userId, string databankName, int questionNumber)
    {
        try
        {
            var doc = _db.Users.FindById(userId);
            if (doc == null) return;

            doc.AnsweredQuestions ??= new Dictionary<string, List<int>>();

            if (!doc.AnsweredQuestions.ContainsKey(databankName))
                doc.AnsweredQuestions[databankName] = new List<int>();

            if (!doc.AnsweredQuestions[databankName].Contains(questionNumber))
                doc.AnsweredQuestions[databankName].Add(questionNumber);

            doc.IsDirty = true;
            _db.Users.Update(doc);
        }
        catch (Exception e)
        {
            Debug.LogError($"[UserDataLocalRepository] Erro ao adicionar questão respondida: {e.Message}");
            throw;
        }
    }
}
