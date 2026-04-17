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

    [Header("Avatar Picker")]
    [SerializeField] private AvatarPickerPanel avatarPickerPanel;

    [Header("Validation")]
    [SerializeField] private int maxImageSizeBytes = 1024 * 1024;

    private ProfileImageLoader imageLoader;
    private UserData currentUserData;
    private bool isProcessing = false;
    private IFirestoreRepository _firestore;
    private IImageUploadService _imageUpload;

    // Estado do picker: rastreia seleção enquanto o painel está aberto
    private string _oldImageUrlBeforePicker;
    private string _lastSelectedResourceName;

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

        if (avatarPickerPanel != null)
        {
            avatarPickerPanel.OnAvatarSelected += OnPresetAvatarSelected;
            avatarPickerPanel.OnPanelClosed += OnPickerPanelClosed;
        }

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
        if (isProcessing)
        {
            Debug.Log("[ProfileImageUploader] Processamento já em andamento");
            return;
        }

        if (avatarPickerPanel != null)
        {
            // Captura a URL atual antes de abrir o picker
            // (usada como oldImageUrl para deletar a imagem antiga do Storage)
            _oldImageUrlBeforePicker = currentUserData?.ProfileImageUrl;
            _lastSelectedResourceName = null;

            avatarPickerPanel.ShowPanel();
        }
        else
        {
            Debug.LogError("[ProfileImageUploader] AvatarPickerPanel não atribuído no Inspector");
        }
    }

    /// <summary>
    /// Callback chamado pelo AvatarPickerPanel a cada clique em um avatar.
    /// O painel permanece aberto — o usuário pode experimentar vários avatares.
    ///
    /// Fluxo 100% instantâneo (mesmo frame, zero I/O):
    ///   1. Carrega textura de Resources (já em memória)
    ///   2. Mostra a textura no imageLoader (preview imediato)
    ///   3. Salva "preset:resourceName" no UserDataStore + LiteDB (apenas string)
    ///   4. Notifica TopBar para atualizar em todas as cenas
    ///
    /// O upload para Firebase só acontece quando o usuário fecha o picker
    /// (via OnPickerPanelClosed).
    /// </summary>
    private void OnPresetAvatarSelected(string resourceName)
    {
        Debug.Log($"[ProfileImageUploader] Avatar preset selecionado: {resourceName}");

        // 1. Carrega textura de Resources (muito rápido, já em memória/cache do Unity)
        Texture2D texture = Resources.Load<Texture2D>($"AvatarPresets/{resourceName}");
        if (texture == null)
        {
            ShowAlert("Erro ao carregar avatar selecionado.");
            Debug.LogError($"[ProfileImageUploader] Textura não encontrada: AvatarPresets/{resourceName}");
            return;
        }

        // 2. Mostra a textura imediatamente (fromResources: true evita Destroy no asset)
        imageLoader.SetTexture(texture, fromResources: true);

        // 3. Salva identificador do preset no UserDataStore + LiteDB (zero I/O de disco)
        string presetUrl = $"preset:{resourceName}";
        currentUserData.ProfileImageUrl = presetUrl;
        UserDataStore.CurrentUserData = currentUserData;
        AppContext.UserDataLocal?.UpdateUser(currentUserData);

        // 4. Notifica TopBar (carregará via "preset:" → Resources, sem I/O)
        UserAvatarSyncHelper.NotifyAvatarChanged(presetUrl);

        // 5. Rastreia seleção para o upload quando o picker fechar
        _lastSelectedResourceName = resourceName;

        Debug.Log($"[ProfileImageUploader] Preview instantâneo: {presetUrl}");
    }

    /// <summary>
    /// Callback chamado quando o AvatarPickerPanel fecha (CloseButton).
    /// Dispara o upload para Firebase Storage em background (fire-and-forget).
    /// Necessário apenas para a RankingScene (compartilhada com outros usuários).
    /// </summary>
    private void OnPickerPanelClosed()
    {
        // Se o usuário não selecionou nenhum avatar, nada a fazer
        if (string.IsNullOrEmpty(_lastSelectedResourceName))
        {
            Debug.Log("[ProfileImageUploader] Picker fechado sem seleção — nenhum upload necessário");
            return;
        }

        Debug.Log($"[ProfileImageUploader] Picker fechado. Iniciando upload do preset: {_lastSelectedResourceName}");

        // Carrega textura para EncodeToPNG (já em cache do Unity, rápido)
        Texture2D texture = Resources.Load<Texture2D>($"AvatarPresets/{_lastSelectedResourceName}");
        if (texture == null)
        {
            Debug.LogError($"[ProfileImageUploader] Textura não encontrada para upload: {_lastSelectedResourceName}");
            return;
        }

        string resourceName = _lastSelectedResourceName;
        string userId = currentUserData.UserId;
        string oldImageUrl = _oldImageUrlBeforePicker;

        // Limpa estado do picker
        _lastSelectedResourceName = null;
        _oldImageUrlBeforePicker = null;

        // Dispara coroutine para EncodeToPNG + file save + upload (background, não bloqueia)
        StartCoroutine(DeferredUploadPreset(resourceName, texture, userId, oldImageUrl));
    }

    /// <summary>
    /// Coroutine que salva o preset em arquivo e faz upload para Firebase Storage.
    /// Necessário apenas para a RankingScene (compartilhada com outros usuários).
    /// Roda em frames separados — nunca trava a UI.
    /// </summary>
    private IEnumerator DeferredUploadPreset(string resourceName, Texture2D texture,
                                              string userId, string oldImageUrl)
    {
        isProcessing = true;

        // --- Frame seguinte: EncodeToPNG + WriteAllBytes ---
        yield return null;

        string localPath = null;
        try
        {
            byte[] pngBytes = texture.EncodeToPNG();
            string avatarDir = Path.Combine(Application.persistentDataPath, "AvatarPresets");
            Directory.CreateDirectory(avatarDir);
            localPath = Path.Combine(avatarDir, $"{resourceName}.png");
            File.WriteAllBytes(localPath, pngBytes);
            Debug.Log($"[ProfileImageUploader] Arquivo temporário salvo para upload: {localPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ProfileImageUploader] Erro ao salvar arquivo para upload: {e.Message}");
            isProcessing = false;
            yield break;
        }

        // --- Frame seguinte: dispara upload fire-and-forget ---
        yield return null;

        UploadAvatarInBackground(localPath, userId, oldImageUrl);
        isProcessing = false;
    }

    /// <summary>
    /// Faz upload do avatar para Firebase Storage em background.
    /// Fire-and-forget — não trava a UI e não afeta a experiência do usuário.
    /// Necessário apenas para a RankingScene (compartilhada com outros usuários).
    /// </summary>
    private async void UploadAvatarInBackground(string imagePath, string userId, string oldImageUrl)
    {
        try
        {
            var config = new ImageUploadConfig
            {
                ImagePath         = imagePath,
                DestinationFolder = "profile_images",
                FileNamePrefix    = userId,
                MaxSizeBytes      = maxImageSizeBytes,
                OldImageUrl       = oldImageUrl,
                OnProgress        = msg => Debug.Log($"[ProfileImageUploader] Background: {msg}"),
                OnCompleted = async url =>
                {
                    Debug.Log($"[ProfileImageUploader] Upload background concluído: {url}");

                    try
                    {
                        await _firestore.UpdateUserProfileImageUrl(userId, url).ConfigureAwait(false);
                        Debug.Log("[ProfileImageUploader] Firestore atualizado com URL do Storage");
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[ProfileImageUploader] Firestore falhou: {e.Message}");
                    }
                },
                OnFailed = error =>
                {
                    Debug.LogWarning($"[ProfileImageUploader] Upload background falhou: {error}");
                }
            };

            await _imageUpload.UploadAsync(config);
        }
        catch (Exception e)
        {
            // Falha no upload não afeta a experiência local
            Debug.LogWarning($"[ProfileImageUploader] Erro no upload background: {e.Message}");
        }
    }

    // -------------------------------------------------------
    // Galeria nativa (desativado — preservado para uso futuro)
    // -------------------------------------------------------
    /*
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

            string capturedPath = safePath;
            MainThreadDispatcher.Enqueue(() =>
            {
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
    */

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
       if (AppContext.Connectivity?.IsOnline == false)
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
            uploadButton.onClick.RemoveAllListeners();

        if (avatarPickerPanel != null)
        {
            avatarPickerPanel.OnAvatarSelected -= OnPresetAvatarSelected;
            avatarPickerPanel.OnPanelClosed -= OnPickerPanelClosed;
        }
    }

    public ProfileImageLoader ImageLoader => imageLoader;
    public bool IsProcessing => isProcessing;
}