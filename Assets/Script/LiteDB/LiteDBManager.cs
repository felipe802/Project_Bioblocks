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

    public ILiteCollection<UserDataDB> Users
        => Database.GetCollection<UserDataDB>("users");

    public ILiteCollection<RankingDB> Rankings
        => Database.GetCollection<RankingDB>("rankings");

    public void Initialize()
    {
        if (IsInitialized) return;
        try
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

            string path = System.IO.Path.Combine(Application.persistentDataPath, DB_NAME);
            _db = new LiteDatabase(path, mapper);
            EnsureIndexes();
            IsInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LiteDBManager] Falha ao abrir banco: {e.Message}");
            throw;
        }
    }

    public ILiteCollection<CachedImageDB> CachedImages
    => Database.GetCollection<CachedImageDB>("cached_images");

    private void EnsureIndexes()
    {
        Users.EnsureIndex(x => x.UserId, unique: true);
        CachedImages.EnsureIndex(x => x.ImageUrl, unique: true);
        Rankings.EnsureIndex(x => x.Score);
        Rankings.EnsureIndex(x => x.WeekScore);
    }

    private void OnDestroy() => Close();
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

    public ILiteCollection<PendingUploadDB> PendingUploads
        => Database.GetCollection<PendingUploadDB>("pending_uploads");
}