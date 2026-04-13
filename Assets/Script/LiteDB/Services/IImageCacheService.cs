using UnityEngine;

public interface IImageCacheService
{
    string GetCachedImagePath(string url);
    Texture2D LoadImageFromCache(string path);
    void SaveImageToCache(string url, Texture2D texture);
    void ClearAllCache();
    long GetTotalCacheSize();
    int GetCachedImagesCount();
}