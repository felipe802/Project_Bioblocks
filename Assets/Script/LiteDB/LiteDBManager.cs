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

    public void Initialize()
    {
        if (IsInitialized) return;
        Debug.Log("[LiteDBManager] Initialize() chamado — IsInitialized será true após este método.");

        try
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, DB_NAME);
            _db = new LiteDatabase(path);
            EnsureIndexes();
            IsInitialized = true;
            Debug.Log($"[LiteDatabaseManager] Banco aberto em: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LiteDatabaseManager] Falha ao abrir banco: {e.Message}");
            throw;
        }
    }

    public ILiteCollection<CachedImageDB> CachedImages
    => Database.GetCollection<CachedImageDB>("cached_images");

    private void EnsureIndexes()
    {
        Users.EnsureIndex(x => x.UserId, unique: true);
        CachedImages.EnsureIndex(x => x.ImageUrl, unique: true);
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