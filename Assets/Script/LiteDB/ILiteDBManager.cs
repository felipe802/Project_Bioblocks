using LiteDB;

public interface ILiteDBManager
{
    bool IsInitialized { get; }
    void Initialize();
    void Close();

    ILiteCollection<UserDataDB>     Users          { get; }
    ILiteCollection<CachedImageDB>  CachedImages   { get; }
    ILiteCollection<RankingDB>      Rankings       { get; }
    ILiteCollection<PendingUploadDB> PendingUploads { get; }

    // ── Novo: cache de questões ────────────────────────────────────────────────
    ILiteCollection<QuestionDB>     Questions      { get; }
}
