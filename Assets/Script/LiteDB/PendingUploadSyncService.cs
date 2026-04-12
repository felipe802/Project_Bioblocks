using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

public class PendingUploadSyncService : MonoBehaviour
{
    private IImageUploadService _imageUpload;
    private IFirestoreRepository _firestore;
    private NetworkReachability _lastReachability;
    private float _checkInterval = 30f;
    private float _elapsed = 0f;

    private void Start()
    {
        _imageUpload = AppContext.ImageUpload;
        _firestore   = AppContext.Firestore;
        _lastReachability = Application.internetReachability;
    }

    private void Update()
    {
        // Lazy-init — garante que as dependências estão disponíveis
        if (_imageUpload == null) _imageUpload = AppContext.ImageUpload;
        if (_firestore == null)   _firestore   = AppContext.Firestore;

        var current = Application.internetReachability;

        if (_lastReachability == NetworkReachability.NotReachable &&
            current != NetworkReachability.NotReachable)
        {
            Debug.Log("[PendingUploadSync] Internet voltou — sincronizando.");
            _ = TrySyncPendingUploads();
        }
        _lastReachability = current;

        _elapsed += Time.deltaTime;
        if (_elapsed < _checkInterval) return;
        _elapsed = 0f;

        if (current != NetworkReachability.NotReachable)
            _ = TrySyncPendingUploads();
    }

    public async Task TrySyncPendingUploads()
    {
        var db = AppContext.LocalDatabase;
        if (db == null) return;

        var pending = db.PendingUploads.FindAll().ToList();

        foreach (var upload in pending)
        {
            if (!File.Exists(upload.LocalPath))
            {
                db.PendingUploads.Delete(upload.Id);
                continue;
            }

            try
            {
                Debug.Log($"[PendingUploadSync] Sincronizando upload pendente para {upload.UserId}");

                var config = new ImageUploadConfig
                {
                    ImagePath        = upload.LocalPath,
                    DestinationFolder = "profile_images",
                    FileNamePrefix   = upload.UserId,
                    MaxSizeBytes     = 5 * 1024 * 1024,
                    OldImageUrl      = upload.OldImageUrl,
                    OnCompleted = async url =>
                    {
                        Debug.Log($"[PendingUploadSync] OnCompleted. url={url}");

                        // 1. Firestore
                        try
                        {
                            await _firestore.UpdateUserProfileImageUrl(upload.UserId, url)
                                            .ConfigureAwait(false);
                            Debug.Log("[PendingUploadSync] Firestore atualizado");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[PendingUploadSync] Firestore falhou: {e.Message}");
                        }

                        // 2. LiteDB — thread seguro
                        try
                        {
                            var localRepo  = AppContext.UserDataLocal;
                            var cachedUser = localRepo?.GetUser(upload.UserId);
                            if (cachedUser != null)
                            {
                                cachedUser.ProfileImageUrl = url;
                                localRepo.UpdateUser(cachedUser);
                                localRepo.MarkAsSynced(upload.UserId);
                                Debug.Log("[PendingUploadSync] LiteDB atualizado");
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[PendingUploadSync] LiteDB falhou: {e.Message}");
                        }

                        // 3. Arquivo local e PendingUploadDB — thread seguro
                        try
                        {
                            if (File.Exists(upload.LocalPath))
                                File.Delete(upload.LocalPath);
                            AppContext.LocalDatabase?.PendingUploads.Delete(upload.Id);
                            Debug.Log("[PendingUploadSync] Arquivo local e PendingUploadDB removidos");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[PendingUploadSync] Deleção falhou: {e.Message}");
                        }

                        // 4. UserDataStore e UI — precisa do main thread
                        var capturedUrl    = url;
                        var capturedUserId = upload.UserId;
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            var current = UserDataStore.CurrentUserData;
                            if (current != null && current.UserId == capturedUserId)
                            {
                                current.ProfileImageUrl       = capturedUrl;
                                UserDataStore.CurrentUserData = current;
                                UserAvatarSyncHelper.NotifyAvatarChanged(capturedUrl);
                                Debug.Log("[PendingUploadSync] UserDataStore atualizado");
                            }
                        });
                    },
                };

                await _imageUpload.UploadAsync(config);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PendingUploadSync] Erro ao sincronizar upload: {e.Message}");
            }
        }
    }
}