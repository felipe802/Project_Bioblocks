using System;
using System.Threading.Tasks;

/// <summary>
/// Implementação fake do IImageUploadService para testes.
/// Não faz nenhuma chamada real ao Firebase Storage.
///
/// Como usar:
///   var fakeUpload = new FakeImageUploadService();
///   AppContext.OverrideForTests(imageUpload: fakeUpload);
///
///   // Verificar se upload foi chamado:
///   Assert.AreEqual(1, fakeUpload.UploadCallCount);
///   Assert.AreEqual("profile_images", fakeUpload.LastConfig.DestinationFolder);
///
///   // Simular falha:
///   fakeUpload.ShouldThrowOnUpload = true;
/// </summary>
public class FakeImageUploadService : IImageUploadService
{
    // ── Contadores e captura para assertions ─────────────────────────────────
    public int UploadCallCount { get; private set; }
    public ImageUploadConfig LastConfig { get; private set; }
    public string LastImagePath { get; private set; }

    // ── Configuração de comportamento ────────────────────────────────────────
    public string FakeDownloadUrl { get; set; } = "https://fake-storage.com/profile_images/avatar.png";
    public bool ShouldThrowOnUpload { get; set; } = false;
    public bool ShouldCallOnCompleted { get; set; } = true;
    public bool ShouldCallOnFailed { get; set; } = false;
    public string FakeErrorMessage { get; set; } = "Upload simulado com falha";

    // ── IImageUploadService ──────────────────────────────────────────────────
    public bool IsUploading { get; private set; }

    public async Task<string> UploadAsync(ImageUploadConfig config)
    {
        if (ShouldThrowOnUpload)
            throw new Exception($"[FakeImageUploadService] {FakeErrorMessage}");

        IsUploading = true;
        UploadCallCount++;
        LastConfig = config;
        LastImagePath = config.ImagePath;

        try
        {
            config.OnProgress?.Invoke("Upload simulado...");

            if (ShouldCallOnFailed)
            {
                config.OnFailed?.Invoke(FakeErrorMessage);
                return null;
            }

            if (ShouldCallOnCompleted && config.OnCompleted != null)
            {
                await config.OnCompleted.Invoke(FakeDownloadUrl);
            }

            return FakeDownloadUrl;
        }
        finally
        {
            IsUploading = false;
        }
    }

    // ── Helpers para testes ──────────────────────────────────────────────────
    public void Reset()
    {
        UploadCallCount = 0;
        LastConfig = null;
        LastImagePath = null;
        ShouldThrowOnUpload = false;
        ShouldCallOnCompleted = true;
        ShouldCallOnFailed = false;
    }
}
