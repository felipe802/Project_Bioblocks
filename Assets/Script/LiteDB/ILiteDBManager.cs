using LiteDB;

public interface ILiteDBManager
{
    bool IsInitialized { get; }
    void Initialize();
    void Close();

    // ── Acesso direto ao banco (necessário para transações) ────────────────────
    LiteDatabase Database { get; }

    ILiteCollection<UserDataDB>      Users          { get; }
    ILiteCollection<CachedImageDB>   CachedImages   { get; }
    ILiteCollection<RankingDB>       Rankings       { get; }

    // ── Novo: cache de questões ────────────────────────────────────────────────
    ILiteCollection<QuestionDB>      Questions      { get; }
}
