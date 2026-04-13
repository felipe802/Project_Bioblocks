// Assets/Editor/Tests/ImageCacheServiceTests.cs
// Testes unitários para ImageCacheService — cobrindo apenas a lógica pura.
//
// O que É testado aqui (sem filesystem nem Texture2D):
//   - GetCachedImagePath: URL nula/vazia, cache não inicializado, cache expirado,
//     entrada inexistente no DB
//   - GetCachedImagesCount e GetTotalCacheSize: com entradas no FakeLiteDB
//   - ClearAllCache: esvazia a coleção no DB
//   - IsInitialized: estado após InjectDependencies
//
// O que NÃO é testado aqui (requer Play Mode ou filesystem real):
//   - SaveImageToCache (depende de Texture2D.EncodeToPNG e File.WriteAllBytes)
//   - LoadImageFromCache (depende de File.ReadAllBytes e Texture2D.LoadImage)
//   - GetCachedImagePath com arquivo em disco (depende de File.Exists)
//   - ResizeTexture (depende de RenderTexture e Graphics.Blit)

using NUnit.Framework;
using System;
using UnityEngine;

[TestFixture]
public class ImageCacheServiceTests
{
    // -------------------------------------------------------
    // Fixtures
    // -------------------------------------------------------

    private FakeLiteDBManager  _db;
    private ImageCacheService  _service;
    private GameObject         _serviceGO;

    [SetUp]
    public void Setup()
    {
        _db = new FakeLiteDBManager();

        _serviceGO = new GameObject("ImageCacheService");
        _service   = _serviceGO.AddComponent<ImageCacheService>();
        _service.InjectDependencies(_db);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Close();
        if (_serviceGO != null)
            UnityEngine.Object.DestroyImmediate(_serviceGO);
    }

    // -------------------------------------------------------
    // Helper: insere uma entrada diretamente no FakeLiteDB
    // sem passar pelo filesystem
    // -------------------------------------------------------

    private void InsertCacheEntry(
        string url,
        long   sizeBytes,
        bool   expired     = false,
        string localPath   = "/fake/path/img.png")
    {
        _db.CachedImages.Upsert(new CachedImageDB
        {
            ImageUrl      = url,
            LocalPath     = localPath,
            CachedAt      = DateTime.UtcNow.AddDays(-1),
            ExpiresAt     = expired
                                ? DateTime.UtcNow.AddDays(-1)   // já expirou
                                : DateTime.UtcNow.AddDays(7),   // válido por 7 dias
            FileSizeBytes = sizeBytes
        });
    }

    // =======================================================
    // IsInitialized
    // =======================================================

    [Test]
    public void IsInitialized_AposInjectDependencies_ETrue()
    {
        Assert.IsTrue(_service.IsInitialized);
    }

    [Test]
    public void IsInitialized_SemInjectDependencies_EFalse()
    {
        var go      = new GameObject("Uninit");
        var service = go.AddComponent<ImageCacheService>();

        Assert.IsFalse(service.IsInitialized);

        UnityEngine.Object.DestroyImmediate(go);
    }

    // =======================================================
    // GetCachedImagePath — guards de entrada
    // =======================================================

    [Test]
    public void GetCachedImagePath_UrlNula_RetornaNull()
    {
        var result = _service.GetCachedImagePath(null);
        Assert.IsNull(result);
    }

    [Test]
    public void GetCachedImagePath_UrlVazia_RetornaNull()
    {
        var result = _service.GetCachedImagePath(string.Empty);
        Assert.IsNull(result);
    }

    [Test]
    public void GetCachedImagePath_EntradaInexistenteNoDB_RetornaNull()
    {
        // Nenhuma entrada no FakeLiteDB para esta URL
        var result = _service.GetCachedImagePath("https://example.com/foto.png");
        Assert.IsNull(result);
    }

    [Test]
    public void GetCachedImagePath_EntradaExpirada_RetornaNull()
    {
        // Entrada existe no DB mas já passou do ExpiresAt
        // File.Exists retornará false para "/fake/path" — o comportamento
        // de expiração também cobre este caso (ExpiresAt < UtcNow → remove e retorna null)
        InsertCacheEntry("https://example.com/foto.png", sizeBytes: 1024, expired: true);

        var result = _service.GetCachedImagePath("https://example.com/foto.png");

        Assert.IsNull(result, "Cache expirado deve retornar null");
    }

    [Test]
    public void GetCachedImagePath_EntradaExpirada_RemoveDoDb()
    {
        InsertCacheEntry("https://example.com/foto.png", sizeBytes: 1024, expired: true);

        _service.GetCachedImagePath("https://example.com/foto.png");

        // Após retornar null por expiração, a entrada deve ter sido removida do DB
        Assert.AreEqual(0, _db.CachedImages.Count(),
            "Entrada expirada deve ser removida do DB ao ser acessada");
    }

    // =======================================================
    // GetCachedImagesCount
    // =======================================================

    [Test]
    public void GetCachedImagesCount_DBVazio_RetornaZero()
    {
        Assert.AreEqual(0, _service.GetCachedImagesCount());
    }

    [Test]
    public void GetCachedImagesCount_ComDuasEntradas_RetornaDois()
    {
        InsertCacheEntry("https://example.com/img1.png", sizeBytes: 1024);
        InsertCacheEntry("https://example.com/img2.png", sizeBytes: 2048);

        Assert.AreEqual(2, _service.GetCachedImagesCount());
    }

    [Test]
    public void GetCachedImagesCount_NaoInicializado_RetornaZero()
    {
        var go      = new GameObject("Uninit");
        var service = go.AddComponent<ImageCacheService>();
        // InjectDependencies não foi chamado — IsInitialized = false

        Assert.AreEqual(0, service.GetCachedImagesCount());

        UnityEngine.Object.DestroyImmediate(go);
    }

    // =======================================================
    // GetTotalCacheSize
    // =======================================================

    [Test]
    public void GetTotalCacheSize_DBVazio_RetornaZero()
    {
        Assert.AreEqual(0L, _service.GetTotalCacheSize());
    }

    [Test]
    public void GetTotalCacheSize_SomaCorretamente()
    {
        InsertCacheEntry("https://example.com/img1.png", sizeBytes: 1_000_000);
        InsertCacheEntry("https://example.com/img2.png", sizeBytes: 2_000_000);
        InsertCacheEntry("https://example.com/img3.png", sizeBytes:   500_000);

        long total = _service.GetTotalCacheSize();

        Assert.AreEqual(3_500_000L, total);
    }

    [Test]
    public void GetTotalCacheSize_NaoInicializado_RetornaZero()
    {
        var go      = new GameObject("Uninit");
        var service = go.AddComponent<ImageCacheService>();

        Assert.AreEqual(0L, service.GetTotalCacheSize());

        UnityEngine.Object.DestroyImmediate(go);
    }

    // =======================================================
    // ClearAllCache
    // =======================================================

    [Test]
    public void ClearAllCache_DBVazio_NaoLancaExcecao()
    {
        Assert.DoesNotThrow(() => _service.ClearAllCache());
    }

    [Test]
    public void ClearAllCache_ComEntradas_EsvaziaCachedImagesNoDB()
    {
        InsertCacheEntry("https://example.com/img1.png", sizeBytes: 1024);
        InsertCacheEntry("https://example.com/img2.png", sizeBytes: 2048);
        InsertCacheEntry("https://example.com/img3.png", sizeBytes: 512);

        _service.ClearAllCache();

        Assert.AreEqual(0, _db.CachedImages.Count(),
            "ClearAllCache deve remover todas as entradas do DB");
    }

    [Test]
    public void ClearAllCache_GetCachedImagesCountVoltaAZero()
    {
        InsertCacheEntry("https://example.com/img1.png", sizeBytes: 1024);
        InsertCacheEntry("https://example.com/img2.png", sizeBytes: 2048);

        _service.ClearAllCache();

        Assert.AreEqual(0, _service.GetCachedImagesCount());
    }

    [Test]
    public void ClearAllCache_GetTotalCacheSizeVoltaAZero()
    {
        InsertCacheEntry("https://example.com/img1.png", sizeBytes: 1_000_000);

        _service.ClearAllCache();

        Assert.AreEqual(0L, _service.GetTotalCacheSize());
    }

    [Test]
    public void ClearAllCache_NaoInicializado_NaoLancaExcecao()
    {
        var go      = new GameObject("Uninit");
        var service = go.AddComponent<ImageCacheService>();

        Assert.DoesNotThrow(() => service.ClearAllCache());

        UnityEngine.Object.DestroyImmediate(go);
    }
}