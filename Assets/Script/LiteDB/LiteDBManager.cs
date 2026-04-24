using System;
using UnityEngine;
using LiteDB;

public class LiteDBManager : MonoBehaviour, ILiteDBManager
{
    private LiteDatabase _db;
    private const string DB_NAME = "app_cache.db";
    public bool IsInitialized { get; private set; }

    public LiteDatabase Database
    {
        get
        {
            if (_db == null) throw new Exception("[LiteDatabaseManager] Banco não inicializado.");
            return _db;
        }
    }

    // ── Collections existentes ─────────────────────────────────────────────────
    public ILiteCollection<UserDataDB>      Users          => Database.GetCollection<UserDataDB>("users");
    public ILiteCollection<RankingDB>       Rankings       => Database.GetCollection<RankingDB>("rankings");
    public ILiteCollection<CachedImageDB>   CachedImages   => Database.GetCollection<CachedImageDB>("cached_images");

    // ── Collection nova: questões ──────────────────────────────────────────────
    public ILiteCollection<QuestionDB>      Questions      => Database.GetCollection<QuestionDB>("questions");

    // ── Inicialização ──────────────────────────────────────────────────────────

    public void Initialize()
    {
        if (IsInitialized) return;

        string path = System.IO.Path.Combine(Application.persistentDataPath, DB_NAME);

        try
        {
            OpenDatabase(path);
        }
        catch (Exception e)
        {
            // Banco corrompido (ex: arquivo de log órfão após deleção manual).
            // Deleta todos os arquivos relacionados e cria banco limpo.
            Debug.LogWarning($"[LiteDBManager] Banco corrompido ({e.Message}) — recriando banco limpo...");
            DeleteDatabaseFiles(path);

            try
            {
                OpenDatabase(path);
                Debug.Log("[LiteDBManager] Banco recriado com sucesso.");
            }
            catch (Exception e2)
            {
                Debug.LogError($"[LiteDBManager] Falha ao recriar banco: {e2.Message}");
                throw;
            }
        }
    }

    private void OpenDatabase(string path)
    {
        var mapper = new BsonMapper();
        mapper.ResolveMember += (type, memberInfo, memberMapper) =>
        {
            if (memberMapper.DataType == typeof(DateTime))
            {
                memberMapper.Serialize   = (obj, m) => new BsonValue(((DateTime)obj).ToUniversalTime());
                memberMapper.Deserialize = (val, m) => DateTime.SpecifyKind(val.AsDateTime, DateTimeKind.Utc);
            }
        };

        // Fecha handle anterior antes de abrir novo (evita leaks no recovery)
        _db?.Dispose();
        _db = null;

        _db = new LiteDatabase(path, mapper);
        EnsureIndexes();
        IsInitialized = true;
    }

    private static void DeleteDatabaseFiles(string dbPath)
    {
        string[] relatedFiles =
        {
            dbPath,
            dbPath.Replace(".db", "-log.db"),
            dbPath.Replace(".db", "-tmp.db")
        };
        foreach (var file in relatedFiles)
        {
            if (System.IO.File.Exists(file))
            {
                System.IO.File.Delete(file);
                Debug.Log($"[LiteDBManager] Arquivo deletado: {file}");
            }
        }
    }

    private void EnsureIndexes()
    {
        // Índices existentes
        Users.EnsureIndex(x => x.UserId, unique: true);
        CachedImages.EnsureIndex(x => x.ImageUrl, unique: true);
        Rankings.EnsureIndex(x => x.Score);
        Rankings.EnsureIndex(x => x.WeekScore);

        // Índices novos para questões
        Questions.EnsureIndex(x => x.QuestionDatabankName);   // busca por banco (hot path)
        Questions.EnsureIndex(x => x.Topic);                  // busca por tópico (futuro)
        Questions.EnsureIndex(x => x.CachedAt);               // verificação de staleness
    }

    private void OnDestroy()          => Close();
    private void OnApplicationQuit() => Close();

    public void Close()
    {
        if (_db != null)
        {
            _db.Dispose();
            _db = null;
            IsInitialized = false;
            Debug.Log("[LiteDatabaseManager] Banco fechado.");
        }
    }
}
