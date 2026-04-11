using System.Threading.Tasks;

public interface IUserDataSyncService
{
    bool IsSyncing { get; }
    Task SyncFromFirestore(string userId);
    Task SyncToFirestore(string userId);
    Task TrySyncPendingData(string userId);
}