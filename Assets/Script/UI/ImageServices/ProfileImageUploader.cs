using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections;

[RequireComponent(typeof(ProfileImageLoader))]
public class ProfileImageUploader : MonoBehaviour
{
    [Header("Upload Configuration")]
    [SerializeField] private Button uploadButton;
    [SerializeField] private bool enableUploadOnStart = true;

    [Header("Validation")]
    [SerializeField] private int maxImageSizeBytes = 1024 * 1024;

    private ProfileImageLoader imageLoader;
    private UserData currentUserData;
    private bool isProcessing = false;
    private IFirestoreRepository _firestore;
    private IImageUploadService _imageUpload;

    private void Awake()
    {
        _firestore = AppContext.Firestore;
        imageLoader = GetComponent<ProfileImageLoader>();

        if (imageLoader == null)
        {
            Debug.LogError("[ProfileImageUploader] ProfileImageLoader não encontrado!");
            return;
        }

        imageLoader.Initialize();
    }

    private void Start()
    {
        _firestore = AppContext.Firestore;
        _imageUpload = AppContext.ImageUpload;
        currentUserData = UserDataStore.CurrentUserData;

        if (enableUploadOnStart)
        {
            EnableUpload(true);
        }

        LoadCurrentProfileImage();
    }

    public void EnableUpload(bool enable)
    {
        if (uploadButton == null)
        {
            Debug.LogWarning("[ProfileImageUploader] Upload button não configurado");
            return;
        }

        if (enable)
        {
            uploadButton.onClick.RemoveAllListeners();
            uploadButton.onClick.AddListener(OnUploadButtonClick);
            uploadButton.interactable = true;
        }
        else
        {
            uploadButton.onClick.RemoveAllListeners();
            uploadButton.interactable = false;
        }
    }

    private void LoadCurrentProfileImage()
    {
        if (currentUserData != null && !string.IsNullOrEmpty(currentUserData.ProfileImageUrl))
        {
            imageLoader.LoadProfileImage(currentUserData.ProfileImageUrl);
        }
        else
        {
            imageLoader.LoadStandardProfileImage();
        }
    }

    private void OnUploadButtonClick()
    {
        if (isProcessing || NativeGallery.IsMediaPickerBusy())
        {
            Debug.Log("[ProfileImageUploader] Upload já em andamento ou galeria ocupada");
            return;
        }

        RequestGalleryPermission();
    }

    private void RequestGalleryPermission()
    {
        NativeGallery.RequestPermissionAsync((permission) =>
        {
            Debug.Log($"[ProfileImageUploader] Permissão: {permission}");

            if (permission == NativeGallery.Permission.Granted)
            {
               OpenGallery();
            }
            else if (permission == NativeGallery.Permission.ShouldAsk)
            {
                OpenGallery();
            }
            else
            {
                ShowAlert("Permissão negada.\nAcesse as configurações do dispositivo para liberar o acesso à galeria.");
                NativeGallery.OpenSettings();
            }
        },
        NativeGallery.PermissionType.Read,
        NativeGallery.MediaType.Image);
    }

    private void OpenGallery()
    {
        Debug.Log("[ProfileImageUploader] Abrindo galeria...");

        NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log($"[ProfileImageUploader] Callback da galeria. Path: {path ?? "NULL"}");

            if (string.IsNullOrEmpty(path))
            {
                MainThreadDispatcher.Enqueue(() => isProcessing = false);
                return;
            }

            // Copia o arquivo imediatamente no callback — antes do MainThreadDispatcher
            // O iOS pode limpar o arquivo temporário antes do próximo frame
            string safePath = path;
            try
            {
                string cacheDir  = Path.Combine(Application.persistentDataPath, "ImageTemp");
                Directory.CreateDirectory(cacheDir);
                string destPath  = Path.Combine(cacheDir, "selected_image.png");
                File.Copy(path, destPath, overwrite: true);
                safePath = destPath;
                Debug.Log($"[ProfileImageUploader] Arquivo copiado para: {safePath}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ProfileImageUploader] Não foi possível copiar arquivo: {e.Message}. Usando path original.");
            }

            Debug.Log("[ProfileImageUploader] Enfileirando ProcessSelectedImage no MainThreadDispatcher");
            string capturedPath = safePath;
            MainThreadDispatcher.Enqueue(() =>
            {
                Debug.Log($"[ProfileImageUploader] MainThreadDispatcher executando. " +
                        $"this null={this == null}, " +
                        $"gameObject active={this != null && gameObject.activeInHierarchy}");

                if (this == null || !gameObject.activeInHierarchy)
                {
                    Debug.LogWarning("[ProfileImageUploader] Componente destruído antes do processamento");
                    return;
                }
                ProcessSelectedImage(capturedPath);
            });
        },
        "Selecione uma imagem",
        "image/*");
    }

    private async void ProcessSelectedImage(string imagePath)
    {
        isProcessing = true;
        if (uploadButton != null) uploadButton.interactable = false;

        // Captura tudo que precisa ANTES de qualquer await
        // — após um await o componente pode não existir mais
        string userId      = currentUserData.UserId;
        string oldImageUrl = currentUserData.ProfileImageUrl;

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("[ProfileImageUploader] UserId nulo");
            FinishProcessing();
            return;
        }

        try
        {
            byte[] previewBytes = File.ReadAllBytes(imagePath);
            Texture2D preview   = new Texture2D(2, 2);
            preview.LoadImage(previewBytes);
            imageLoader.SetTexture(preview);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[ProfileImageUploader] Erro ao gerar preview: {e.Message}");
        }

        // Sem internet — salva localmente e retorna imediatamente
        // O PendingUploadSyncService completará quando a internet voltar
       if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            try
            {
                string dir = Path.Combine(Application.persistentDataPath, "PendingUploads");
                Directory.CreateDirectory(dir);
                string localPath = Path.Combine(dir, $"{userId}_{DateTime.UtcNow.Ticks}.jpg");
                File.Copy(imagePath, localPath, overwrite: true);
                Debug.Log($"[ProfileImageUploader] Arquivo copiado para: {localPath}");

                var pending = new PendingUploadDB
                {
                    Id          = userId,
                    UserId      = userId,
                    LocalPath   = localPath,
                    OldImageUrl = oldImageUrl,
                    CreatedAt   = DateTime.UtcNow
                };
                AppContext.LocalDatabase?.PendingUploads.Upsert(pending);
                Debug.Log("[ProfileImageUploader] PendingUploadDB salvo no LiteDB");

                var userData = UserDataStore.CurrentUserData;
                Debug.Log($"[ProfileImageUploader] userData null={userData == null}");
                
                if (userData != null)
                {
                    Debug.Log($"[ProfileImageUploader] ProfileImageUrl ANTES: {userData.ProfileImageUrl}");
                    userData.ProfileImageUrl      = localPath;
                    Debug.Log($"[ProfileImageUploader] ProfileImageUrl DEPOIS: {userData.ProfileImageUrl}");
                    
                    UserDataStore.CurrentUserData = userData;
                    Debug.Log("[ProfileImageUploader] UserDataStore atualizado");
                    
                    AppContext.UserDataLocal?.UpdateUser(userData); // ← sem var, sem atribuição
                    var litedbCheck = AppContext.UserDataLocal?.GetUser(userId);
                    Debug.Log($"[ProfileImageUploader] LiteDB após UpdateUser: " +
                            $"ProfileImageUrl={litedbCheck?.ProfileImageUrl}");
                    Debug.Log($"[ProfileImageUploader] LiteDB após UpdateUser: " +
                            $"ProfileImageUrl={litedbCheck?.ProfileImageUrl}");
                }

                UserAvatarSyncHelper.NotifyAvatarChanged(localPath);
                Debug.Log("[ProfileImageUploader] NotifyAvatarChanged chamado");

                ShowAlert("Sem internet. Foto salva e será enviada quando a conexão voltar.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProfileImageUploader] Erro: {e.Message}\n{e.StackTrace}");
            }

            FinishProcessing();
            return;
        }
        // Com internet — monta o config e faz upload
        var config = new ImageUploadConfig
        {
            ImagePath         = imagePath,
            DestinationFolder = "profile_images",
            FileNamePrefix    = userId,
            MaxSizeBytes      = maxImageSizeBytes,
            OldImageUrl       = oldImageUrl,
            OnProgress        = msg => Debug.Log($"[ProfileImageUploader] {msg}"),
            OnCompleted = async url =>
            {
                Debug.Log($"[ProfileImageUploader] OnCompleted. url={url}");

                try
                {
                    await _firestore.UpdateUserProfileImageUrl(userId, url).ConfigureAwait(false);
                    Debug.Log("[ProfileImageUploader] Firestore atualizado");
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    MainThreadDispatcher.Enqueue(() =>
                        Debug.LogWarning($"[ProfileImageUploader] Firestore falhou: {msg}"));
                }

                var capturedUserData = currentUserData;
                var capturedUrl      = url;
                MainThreadDispatcher.Enqueue(() =>
                {
                    capturedUserData.ProfileImageUrl = capturedUrl;
                    UserDataStore.CurrentUserData    = capturedUserData;
                    UserAvatarSyncHelper.NotifyAvatarChanged(capturedUrl);
                    Debug.Log("[ProfileImageUploader] UserDataStore atualizado com nova URL");
                });
            },
            OnFailed = error =>
            {
                if (this != null && gameObject.activeInHierarchy)
                    ShowAlert(error);
            }
        };

        try
        {
            await _imageUpload.UploadAsync(config);
        }
        catch
        {
            // Erro já tratado pelo OnFailed
        }

        // Verifica se o componente ainda existe antes de tocar na UI
        if (this != null && gameObject.activeInHierarchy)
            FinishProcessing();
    }

    private void ShowAlert(string message)
    {
        if (AlertManager.Instance != null)
        {
            AlertManager.Instance.ShowAlert(message);
        }
        else
        {
            Debug.LogWarning($"[ProfileImageUploader] AlertManager não disponível. Mensagem: {message}");
        }
    }

    private void FinishProcessing()
    {
        isProcessing = false;

        if (uploadButton != null)
        {
            uploadButton.interactable = true;
        }
    }

    private void OnDestroy()
    {
        if (uploadButton != null)
        {
            uploadButton.onClick.RemoveAllListeners();
        }
    }

    public ProfileImageLoader ImageLoader => imageLoader;
    public bool IsProcessing => isProcessing;
}