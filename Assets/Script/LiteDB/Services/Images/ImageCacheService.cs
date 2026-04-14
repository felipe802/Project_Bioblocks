using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class ImageCacheService : MonoBehaviour, IImageCacheService
{
    private ILiteDBManager _dbManager;
    private string _cacheDirectory;

    private const long MAX_CACHE_SIZE_BYTES = 50 * 1024 * 1024;
    private const int MAX_IMAGE_DIMENSION   = 512;
    private const int MAX_IMAGE_BYTES       = 5 * 1024 * 1024;
    private const int CACHE_EXPIRY_DAYS     = 7;
    private const float CLEANUP_FRACTION    = 0.25f;

    public bool IsInitialized { get; private set; }

    public void InjectDependencies(ILiteDBManager dbManager)
    {
        _dbManager = dbManager;
        Initialize();
    }

    private void Initialize()
    {
        if (IsInitialized) return;

        try
        {
            _cacheDirectory = Path.Combine(Application.persistentDataPath, "ImageCache");

            if (!Directory.Exists(_cacheDirectory))
                Directory.CreateDirectory(_cacheDirectory);

            IsInitialized = true;
            Debug.Log("[ImageCacheService] Inicializado com sucesso.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ImageCacheService] Erro ao inicializar: {e.Message}");
            IsInitialized = false;
        }
    }

    public string GetCachedImagePath(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl) || !IsInitialized) return null;
        if (_dbManager == null || !_dbManager.IsInitialized) return null;

        try
        {
            var cached = _dbManager.CachedImages.FindById(imageUrl);

            if (cached == null) return null;

            if (DateTime.UtcNow < cached.ExpiresAt && File.Exists(cached.LocalPath))
            {
                Debug.Log($"[ImageCacheService] Cache hit: {imageUrl}");
                return cached.LocalPath;
            }

            DeleteCachedImage(cached);
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"[ImageCacheService] Erro ao buscar cache: {e.Message}");
            return null;
        }
    }

    public void SaveImageToCache(string imageUrl, Texture2D texture)
    {
        if (string.IsNullOrEmpty(imageUrl) || texture == null || !IsInitialized) return;
        if (_dbManager == null || !_dbManager.IsInitialized) return;

        try
        {
            string fileName  = GetHashedFileName(imageUrl);
            string localPath = Path.Combine(_cacheDirectory, fileName);

            bool needsResize      = texture.width > MAX_IMAGE_DIMENSION || texture.height > MAX_IMAGE_DIMENSION;
            Texture2D toSave      = needsResize ? ResizeTexture(texture, MAX_IMAGE_DIMENSION, MAX_IMAGE_DIMENSION) : texture;
            byte[] imageBytes     = toSave.EncodeToPNG();

            if (needsResize && toSave != texture)
                Destroy(toSave);

            if (imageBytes.Length > MAX_IMAGE_BYTES)
            {
                Debug.LogWarning($"[ImageCacheService] Imagem muito grande, não será cacheada: {imageUrl}");
                return;
            }

            File.WriteAllBytes(localPath, imageBytes);

            _dbManager.CachedImages.Upsert(new CachedImageDB
            {
                ImageUrl      = imageUrl,
                LocalPath     = localPath,
                CachedAt      = DateTime.UtcNow,
                ExpiresAt     = DateTime.UtcNow.AddDays(CACHE_EXPIRY_DAYS),
                FileSizeBytes = imageBytes.Length
            });

            Debug.Log($"[ImageCacheService] Imagem cacheada: {imageUrl} ({imageBytes.Length} bytes)");
            CleanupOldCacheIfNeeded();
        }
        catch (OutOfMemoryException)
        {
            Debug.LogError("[ImageCacheService] OutOfMemory ao salvar imagem.");
            CleanupOldCacheIfNeeded();
        }
        catch (Exception e)
        {
            Debug.LogError($"[ImageCacheService] Erro ao salvar cache: {e.Message}");
        }
    }

    public Texture2D LoadImageFromCache(string localPath)
    {
        try
        {
            if (!File.Exists(localPath)) return null;

            byte[]    imageBytes = File.ReadAllBytes(localPath);
            Texture2D texture    = new Texture2D(2, 2);

            if (texture.LoadImage(imageBytes))
                return texture;

            Destroy(texture);
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"[ImageCacheService] Erro ao carregar cache: {e.Message}");
            return null;
        }
    }

    public void ClearAllCache()
    {
        if (!IsInitialized) return;

        try
        {
            var all = _dbManager.CachedImages.FindAll().ToList();
            foreach (var image in all)
                DeleteCachedImage(image);

            Debug.Log($"[ImageCacheService] Cache limpo ({all.Count} imagens removidas).");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ImageCacheService] Erro ao limpar cache: {e.Message}");
        }
    }

    public long GetTotalCacheSize()
    {
        if (!IsInitialized) return 0;
        try { return _dbManager.CachedImages.FindAll().Sum(x => x.FileSizeBytes); }
        catch { return 0; }
    }

    public int GetCachedImagesCount()
    {
        if (!IsInitialized) return 0;
        try { return _dbManager.CachedImages.Count(); }
        catch { return 0; }
    }

    private void DeleteCachedImage(CachedImageDB cached)
    {
        try
        {
            if (File.Exists(cached.LocalPath))
                File.Delete(cached.LocalPath);

            _dbManager.CachedImages.Delete(cached.ImageUrl);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ImageCacheService] Erro ao deletar cache: {e.Message}");
        }
    }

    private void CleanupOldCacheIfNeeded()
    {
        try
        {
            var all       = _dbManager.CachedImages.FindAll().ToList();
            long totalSize = all.Sum(x => x.FileSizeBytes);

            if (totalSize > MAX_CACHE_SIZE_BYTES)
            {
                var toDelete = all
                    .OrderBy(x => x.CachedAt)
                    .Take((int)(all.Count * CLEANUP_FRACTION))
                    .ToList();

                foreach (var image in toDelete)
                    DeleteCachedImage(image);

                Debug.Log($"[ImageCacheService] Cleanup: {toDelete.Count} imagens removidas.");
            }

            foreach (var expired in all.Where(x => DateTime.UtcNow >= x.ExpiresAt))
                DeleteCachedImage(expired);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ImageCacheService] Erro no cleanup: {e.Message}");
        }
    }

    private Texture2D ResizeTexture(Texture2D source, int maxWidth, int maxHeight)
    {
        float ratio = Mathf.Min((float)maxWidth / source.width, (float)maxHeight / source.height);
        if (ratio >= 1f) return source;

        int newWidth  = Mathf.RoundToInt(source.width  * ratio);
        int newHeight = Mathf.RoundToInt(source.height * ratio);

        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode    = FilterMode.Bilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    private string GetHashedFileName(string url)
        => $"img_{Math.Abs(url.GetHashCode()):X8}.png";
}