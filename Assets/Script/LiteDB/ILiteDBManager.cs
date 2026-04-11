using LiteDB;

public interface ILiteDBManager
{
    bool IsInitialized { get; }
    ILiteCollection<UserDataDB> Users { get; }
    ILiteCollection<CachedImageDB> CachedImages { get; }
    void Initialize();
    void Close();
}
