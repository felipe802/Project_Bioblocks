using System;

public interface IUserDataLocalRepository
{
    UserData GetUser(string userId);
    void SaveUser(UserData userData);
    void UpdateUser(UserData userData);
    void MarkAsDirty(string userId);
    void MarkAsSynced(string userId);
    bool HasUser(string userId);
    bool IsDirty(string userId);
    void DeleteUser(string userId);
    DateTime GetLastSyncedAt(string userId); // ← adicionar
}