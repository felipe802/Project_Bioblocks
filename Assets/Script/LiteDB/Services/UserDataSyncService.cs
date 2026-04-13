using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UserDataSyncService : MonoBehaviour, IUserDataSyncService
{
    [SerializeField] private float cacheValidMinutes = 5f;

    private IUserDataLocalRepository _localRepository;
    private IFirestoreRepository _firestore;

    public bool IsSyncing { get; private set; }

    public void InjectDependencies(
        IUserDataLocalRepository localRepository,
        IFirestoreRepository firestore)
    {
        _localRepository = localRepository;
        _firestore       = firestore;
    }

    public async Task SyncFromFirestore(string userId)
    {
        if (IsSyncing) return;
        IsSyncing = true;

        try
        {
            var userData = await _firestore.GetUserData(userId).ConfigureAwait(false);
            await Task.Yield();
            
            if (userData == null)
            {
                Debug.LogWarning($"[SyncService] Usuário {userId} não encontrado no Firestore.");
                return;
            }

            _localRepository.UpdateUser(userData);
            _localRepository.MarkAsSynced(userId);
            UserDataStore.CurrentUserData = userData;
            Debug.Log("[SyncService] Dados sincronizados do Firestore para o cache local.");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SyncService] Falha ao sincronizar do Firestore: {e.Message}");
            throw;
        }
        finally
        {
            IsSyncing = false;
        }
    }

    public async Task SyncToFirestore(string userId)
    {
        if (IsSyncing) return;
        IsSyncing = true;

        try
        {
            var userData = _localRepository.GetUser(userId);
            if (userData == null)
            {
                Debug.LogWarning($"[SyncService] Usuário {userId} não encontrado no cache local.");
                return;
            }

            await _firestore.UpdateUserData(userData).ConfigureAwait(false);
            await Task.Yield();
            _localRepository.MarkAsSynced(userId);
            Debug.Log("[SyncService] Dados locais enviados ao Firestore com sucesso.");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SyncService] Falha ao enviar dados ao Firestore: {e.Message}");
            throw;
        }
        finally
        {
            IsSyncing = false;
        }
    }

    public async Task TrySyncPendingData(string userId)
    {
        try
        {
            bool hasLocal = _localRepository.HasUser(userId);

            // Sem cache local — dispositivo novo, busca do Firestore
            if (!hasLocal)
            {
                Debug.Log("[SyncService] Sem cache local — buscando do Firestore...");
                await SyncFromFirestore(userId);
                return;
            }

            // Cache dirty — há dados locais para enviar
            if (_localRepository.IsDirty(userId))
            {
                Debug.Log("[SyncService] Dados pendentes encontrados — enviando ao Firestore...");
                await SyncToFirestore(userId);
                return;
            }

            // Cache stale — verifica se Firestore tem dados mais recentes
            if (IsCacheStale(userId))
            {
                Debug.Log("[SyncService] Cache desatualizado — comparando com Firestore...");
                await MergeWithFirestore(userId);
                return;
            }

            // Cache válido — usa LiteDB diretamente
            Debug.Log("[SyncService] Cache válido — carregando do local.");
            var cached = _localRepository.GetUser(userId);
            if (cached != null)
                UserDataStore.CurrentUserData = cached;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SyncService] TrySyncPendingData falhou — usando cache local: {e.Message}");
            var cached = _localRepository.GetUser(userId);
            if (cached != null)
                UserDataStore.CurrentUserData = cached;
        }
    }

    // Método que compara SavedAt e usa o mais recente como fonte verdade
    private async Task MergeWithFirestore(string userId)
    {
        try
        {
            var localData = _localRepository.GetUser(userId);
            UserData remoteData = null;

            try
            {
                remoteData = await _firestore.GetUserData(userId).ConfigureAwait(false);
                await Task.Yield();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SyncService] Firestore indisponível: {e.Message}");
            }

            if (remoteData == null)
            {
                // Sem internet — usa LiteDB
                Debug.Log("[SyncService] Firestore indisponível — usando LiteDB.");
                UserDataStore.CurrentUserData = localData;
                return;
            }

            // Compara SavedAt — usa o mais recente
            if (localData.SavedAt.ToUniversalTime() >= remoteData.SavedAt.ToUniversalTime())
            {
                UserDataStore.CurrentUserData = localData;

                // Se local é mais recente, sincroniza ao Firestore
                if (localData.SavedAt > remoteData.SavedAt)
                    _localRepository.MarkAsDirty(userId);
            }
            else
            {
                remoteData.SavedAt = DateTime.UtcNow;
                _localRepository.UpdateUser(remoteData);
                _localRepository.MarkAsSynced(userId);
                UserDataStore.CurrentUserData = remoteData;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SyncService] MergeWithFirestore falhou: {e.Message}");
            var cached = _localRepository.GetUser(userId);
            if (cached != null)
                UserDataStore.CurrentUserData = cached;
        }
    }

    private bool IsCacheStale(string userId)
    {
        var lastSync = _localRepository.GetLastSyncedAt(userId);
        if (lastSync == DateTime.MinValue) return true;
        return (DateTime.Now - lastSync).TotalMinutes > cacheValidMinutes;
    }

    // UserDataSyncService.cs
    public async Task UpdateUserScores(
        string userId,
        int additionalScore,
        int questionNumber,
        string databankName,
        bool isCorrect)
    {
        await Task.Yield();

        var localUser = _localRepository.GetUser(userId);
        if (localUser == null)
        {
            Debug.LogWarning("[SyncService] Usuário não encontrado no cache local.");
            return;
        }

        int newScore     = Mathf.Max(0, localUser.Score     + additionalScore);
        int newWeekScore = Mathf.Max(0, localUser.WeekScore + additionalScore);

        // LiteDB — síncrono, seguro
        _localRepository.UpdateScore(userId, newScore, newWeekScore);
        if (isCorrect && !string.IsNullOrEmpty(databankName) && questionNumber > 0)
            _localRepository.AddAnsweredQuestion(userId, databankName, questionNumber);

        // UserDataStore — dispara UI, precisa estar no main thread
        var current = UserDataStore.CurrentUserData;
        if (current != null)
        {
            current.Score     = newScore;
            current.WeekScore = newWeekScore;

            if (isCorrect && !string.IsNullOrEmpty(databankName) && questionNumber > 0)
            {
                current.AnsweredQuestions ??= new Dictionary<string, List<int>>();
                if (!current.AnsweredQuestions.ContainsKey(databankName))
                    current.AnsweredQuestions[databankName] = new List<int>();
                if (!current.AnsweredQuestions[databankName].Contains(questionNumber))
                    current.AnsweredQuestions[databankName].Add(questionNumber);
            }

            UserDataStore.CurrentUserData = current;
        }

        var capturedUserData = UserDataStore.CurrentUserData;
        _ = SyncToFirestoreBackground(userId, additionalScore, questionNumber,
                                    databankName, isCorrect, capturedUserData);
    }

    private async Task SyncToFirestoreBackground(
    string userId,
    int additionalScore,
    int questionNumber,
    string databankName,
    bool isCorrect,
    UserData capturedUserData)
    {
        try
        {
            await _firestore.UpdateUserScores(
                userId, additionalScore,
                questionNumber, databankName,
                isCorrect, capturedUserData
            )
            .ConfigureAwait(false);
            await Task.Yield();
            _localRepository.MarkAsSynced(userId);
            Debug.Log("[SyncService] Score sincronizado com Firestore.");
        }
        catch (Exception e)
        {
             _localRepository.MarkAsDirty(userId); // LiteDB — seguro em qualquer thread

            await Task.Yield();
            Debug.LogError($"[SyncBackground] FALHA — {e.GetType().Name}: {e.Message}");
            Debug.LogError($"[SyncBackground] StackTrace: {e.StackTrace}");
        }
    }
}