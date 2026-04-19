using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;
using QuestionSystem;

public class FirestoreQuestionRepository : MonoBehaviour, IFirestoreQuestionRepository
{
    private FirebaseFirestore db;
    private bool isInitialized;

    private const string COLLECTION = "Questions";

    // ── Inicialização ──────────────────────────────────────────────────────────

    public void Initialize()
    {
        if (isInitialized) return;

        try
        {
            db = FirebaseFirestore.DefaultInstance;
            isInitialized = true;
            Debug.Log("[FirestoreQuestionRepository] Inicializado com sucesso.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirestoreQuestionRepository] Falha na inicialização: {e.Message}");
            throw;
        }
    }

    // ── Leitura ────────────────────────────────────────────────────────────────

    public async Task<List<Question>> GetAllQuestions()
    {
        EnsureInitialized();

        try
        {
            Debug.Log("[FirestoreQuestionRepository] Baixando todas as questões do Firestore...");

            QuerySnapshot snapshot = await db.Collection(COLLECTION).GetSnapshotAsync();
            var questions = new List<Question>(snapshot.Count);

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (!doc.Exists) continue;

                Question q = ConvertFromFirestore(doc);
                if (q != null)
                    questions.Add(q);
            }

            Debug.Log($"[FirestoreQuestionRepository] {questions.Count} questões carregadas do Firestore.");
            return questions;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirestoreQuestionRepository] Erro em GetAllQuestions: {e.Message}");
            throw;
        }
    }

    public async Task<List<Question>> GetQuestionsByDatabankName(string databankName)
    {
        EnsureInitialized();

        try
        {
            Debug.Log($"[FirestoreQuestionRepository] Baixando questões do banco '{databankName}'...");

            Query query = db.Collection(COLLECTION)
                            .WhereEqualTo("questionDatabankName", databankName);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();
            var questions = new List<Question>(snapshot.Count);

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (!doc.Exists) continue;

                Question q = ConvertFromFirestore(doc);
                if (q != null)
                    questions.Add(q);
            }

            Debug.Log($"[FirestoreQuestionRepository] {questions.Count} questões carregadas para '{databankName}'.");
            return questions;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirestoreQuestionRepository] Erro em GetQuestionsByDatabankName('{databankName}'): {e.Message}");
            throw;
        }
    }

    // ── Conversão Firestore → Question ─────────────────────────────────────────

    private Question ConvertFromFirestore(DocumentSnapshot doc)
    {
        try
        {
            var data = doc.ToDictionary();

            var question = new Question
            {
                globalId              = GetString(data, "globalId",              doc.Id),
                questionDatabankName  = GetString(data, "questionDatabankName",  ""),
                questionText          = GetString(data, "questionText",           ""),
                correctIndex          = GetInt   (data, "correctIndex",           0),
                questionNumber        = GetInt   (data, "questionNumber",         0),
                isImageAnswer         = GetBool  (data, "isImageAnswer",          false),
                isImageQuestion       = GetBool  (data, "isImageQuestion",        false),
                questionImagePath     = GetString(data, "questionImagePath",      ""),
                questionLevel         = GetInt   (data, "questionLevel",          1),
                questionInDevelopment = GetBool  (data, "questionInDevelopment",  false),
                topic                 = GetString(data, "topic",                  ""),
                displayName           = GetString(data, "displayName",            ""),
                subtopic              = GetString(data, "subtopic",               null),
                bloomLevel            = GetString(data, "bloomLevel",             "unclassified"),
                conceptTags           = GetStringList(data, "conceptTags"),
                prerequisites         = GetStringList(data, "prerequisites"),
                answers               = GetStringArray(data, "answers"),
                questionHint          = GetHint(data, "questionHint")
            };

            return question;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[FirestoreQuestionRepository] Erro ao converter documento '{doc.Id}': {e.Message}");
            return null;
        }
    }

    // ── Helpers de extração de tipos ───────────────────────────────────────────

    private static string GetString(Dictionary<string, object> data, string key, string defaultValue)
    {
        if (data.TryGetValue(key, out object value) && value != null)
            return value.ToString();
        return defaultValue;
    }

    private static int GetInt(Dictionary<string, object> data, string key, int defaultValue)
    {
        if (data.TryGetValue(key, out object value) && value != null)
        {
            if (value is long l)   return (int)l;
            if (value is int  i)   return i;
            if (int.TryParse(value.ToString(), out int parsed)) return parsed;
        }
        return defaultValue;
    }

    private static bool GetBool(Dictionary<string, object> data, string key, bool defaultValue)
    {
        if (data.TryGetValue(key, out object value) && value != null)
        {
            if (value is bool b) return b;
            if (bool.TryParse(value.ToString(), out bool parsed)) return parsed;
        }
        return defaultValue;
    }

    private static string[] GetStringArray(Dictionary<string, object> data, string key)
    {
        if (data.TryGetValue(key, out object value) && value is List<object> list)
        {
            var result = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
                result[i] = list[i]?.ToString() ?? "";
            return result;
        }

        return new string[0];
    }

    private static List<string> GetStringList(Dictionary<string, object> data, string key)
    {
        if (data.TryGetValue(key, out object value) && value is List<object> list)
        {
            var result = new List<string>(list.Count);
            foreach (var item in list)
                if (item != null) result.Add(item.ToString());
            return result;
        }
        return new List<string>();
    }

    private static QuestionHint GetHint(Dictionary<string, object> data, string key)
    {
        if (data.TryGetValue(key, out object value) && value is Dictionary<string, object> map)
        {
            return new QuestionHint
            {
                imagePath = GetString(map, "imagePath", null),
                link      = GetString(map, "Link",      null),  // nota: "Link" com L maiúsculo conforme Firestore
                text      = GetString(map, "text",      null),
                videoUrl  = GetString(map, "videoUrl",  null)
            };
        }
        return new QuestionHint();
    }

    // ── Utilitário ─────────────────────────────────────────────────────────────

    private void EnsureInitialized()
    {
        if (!isInitialized)
            throw new InvalidOperationException("[FirestoreQuestionRepository] Não inicializado. Chame Initialize() antes de usar.");
    }
}