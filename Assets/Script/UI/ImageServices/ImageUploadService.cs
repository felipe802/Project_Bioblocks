using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;

public class ImageUploadService : MonoBehaviour, IImageUploadService
{
    public bool IsUploading { get; private set; } = false;

    private IStorageRepository _storage;

    public void InjectDependencies(IStorageRepository storage)
    {
        _storage = storage;
    }

    public async Task<string> UploadAsync(ImageUploadConfig config)
    {
        if (IsUploading)
        {
            Debug.LogWarning("[ImageUploadService] Upload já em andamento.");
            return null;
        }

        IsUploading = true;

        try
        {
            config.OnProgress?.Invoke("Validando imagem...");
            byte[] imageBytes = ReadAndValidate(config);

            // Sem internet — salva localmente e avisa o usuário
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                string localPath = SaveImageLocally(imageBytes, config.FileNamePrefix);
                SavePendingUpload(localPath, config);

                // Notifica que foi salvo localmente
                config.OnProgress?.Invoke("Sem internet — imagem salva localmente.");
                if (config.OnSavedOffline != null)
                    await config.OnSavedOffline.Invoke(localPath);

                Debug.Log($"[ImageUploadService] Imagem salva localmente: {localPath}");
                return null;
            }

            // Com internet — fluxo normal
            if (!string.IsNullOrEmpty(config.OldImageUrl))
            {
                config.OnProgress?.Invoke("Removendo imagem anterior...");
                await DeleteOldImage(config.OldImageUrl);
            }

            config.OnProgress?.Invoke("Enviando imagem...");
            string fileName = BuildFileName(config);
            string imageUrl = await _storage.UploadImageAsync(fileName, imageBytes).ConfigureAwait(false);
            await Task.Yield();

            RemovePendingUpload(config.FileNamePrefix);
            Debug.Log($"[ImageUploadService] ✅ Upload concluído: {imageUrl}");
            
            if (config.OnCompleted != null)
                await config.OnCompleted.Invoke(imageUrl);

            return imageUrl;
        }
        catch (ImageTooLargeException)
        {
            string msg = $"Imagem muito grande. Máximo: {config.MaxSizeBytes / (1024 * 1024)}MB.";
            Fail(config, msg);
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ImageUploadService] Exception tipo: {ex.GetType().Name}");
            Debug.LogError($"[ImageUploadService] Mensagem: {ex.Message}");
            Debug.LogError($"[ImageUploadService] StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
                Debug.LogError($"[ImageUploadService] InnerException: {ex.InnerException.Message}");
            Fail(config, $"Falha no upload: {ex.Message}");
            return null;
        }
        finally
        {
            IsUploading = false;
        }
    }

    private string SaveImageLocally(byte[] imageBytes, string prefix)
    {
        string dir = Path.Combine(Application.persistentDataPath, "PendingUploads");
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, $"{prefix}_{DateTime.UtcNow.Ticks}.jpg");
        File.WriteAllBytes(path, imageBytes);
        return path;
    }

    private void SavePendingUpload(string localPath, ImageUploadConfig config)
    {
        var db = AppContext.LocalDatabase;
        if (db == null) return;

        var pending = new PendingUploadDB
        {
            Id           = config.FileNamePrefix, // userId como chave única
            UserId       = config.FileNamePrefix,
            LocalPath    = localPath,
            OldImageUrl  = config.OldImageUrl,
            CreatedAt    = DateTime.UtcNow
        };

        db.PendingUploads.Upsert(pending); // substitui se já havia um pendente
        Debug.Log("[ImageUploadService] Upload pendente registrado no LiteDB.");
    }

    private void RemovePendingUpload(string userId)
    {
        AppContext.LocalDatabase?.PendingUploads.Delete(userId);
    }

    private byte[] ReadAndValidate(ImageUploadConfig config)
    {
        var fileInfo = new FileInfo(config.ImagePath);
        if (fileInfo.Length > config.MaxSizeBytes)
            throw new ImageTooLargeException(
                $"Tamanho: {fileInfo.Length / (1024 * 1024)}MB"
            );
        return File.ReadAllBytes(config.ImagePath);
    }

    private async Task DeleteOldImage(string oldImageUrl)
    {
        try
        {
            await _storage.DeleteProfileImageAsync(oldImageUrl).ConfigureAwait(false);
            await Task.Yield();
            Debug.Log("[ImageUploadService] Imagem antiga deletada.");
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            await Task.Yield();
            Debug.LogWarning($"[ImageUploadService] Não foi possível deletar imagem antiga: {msg}");
        }
    }

    private string BuildFileName(ImageUploadConfig config) =>
        $"{config.DestinationFolder}/{config.FileNamePrefix}_{DateTime.UtcNow.Ticks}.jpg";

    private void Fail(ImageUploadConfig config, string message)
    {
        Debug.LogError($"[ImageUploadService] {message}");
        if (config.OnFailed != null)
            MainThreadDispatcher.Enqueue(() => config.OnFailed.Invoke(message));
    }
}
