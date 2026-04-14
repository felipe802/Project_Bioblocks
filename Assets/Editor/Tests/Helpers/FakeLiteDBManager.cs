using System;
using System.IO;
using LiteDB;

/// <summary>
/// Fake do ILiteDBManager para testes unitários.
/// Opera inteiramente em memória — sem arquivo em disco.
/// Descartado automaticamente ao fim de cada teste via Close().
///
/// Como usar:
///   var db = new FakeLiteDBManager();
///   var repo = new QuestionLocalRepository();
///   repo.InjectDependencies(db);
/// </summary>
public class FakeLiteDBManager : ILiteDBManager
{
    private LiteDatabase _db;

    public FakeLiteDBManager()
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

        _db = new LiteDatabase(new MemoryStream(), mapper);
        IsInitialized = true;
    }

    public bool IsInitialized { get; private set; }

    // ── Collections existentes ─────────────────────────────────────────────────
    public ILiteCollection<UserDataDB>      Users          => _db.GetCollection<UserDataDB>("users");
    public ILiteCollection<CachedImageDB>   CachedImages   => _db.GetCollection<CachedImageDB>("cached_images");
    public ILiteCollection<RankingDB>       Rankings       => _db.GetCollection<RankingDB>("rankings");
    public ILiteCollection<PendingUploadDB> PendingUploads => _db.GetCollection<PendingUploadDB>("pending_uploads");

    // ── Collection nova: questões ──────────────────────────────────────────────
    public ILiteCollection<QuestionDB>      Questions      => _db.GetCollection<QuestionDB>("questions");

    public void Initialize()  { IsInitialized = true; }

    public void Close()
    {
        _db?.Dispose();
        _db = null;
        IsInitialized = false;
    }
}