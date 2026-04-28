using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class EnvironmentSetup : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => -1000; // muito cedo, antes do Firebase Resolver

    // === AJUSTE ESSAS CONSTANTES PARA OS SEUS VALORES REAIS ===
    private const string ProdBundleIdAndroid = "com.halbus.labedu"; 
    private const string ProdBundleIdiOS     = "com.halbus.labedu"; 
    private const string DevBundleIdAndroid  = "com.edutesc.bioblocks_dev";
    private const string DevBundleIdiOS      = "com.edutesc.bioblocks-dev";

    private const string ProdProductName = "BioBlocks";
    private const string DevProductName  = "BioBlocks Dev";
    // ==========================================================

    private const string FirebaseSourceRoot = "Firebase";
    private const string AndroidConfigFile  = "google-services.json";
    private const string iOSConfigFile      = "GoogleService-Info.plist";

    // Estado salvo entre Pre e Post para reverter PlayerSettings
    private static string _prevBundleIdAndroid;
    private static string _prevBundleIdiOS;
    private static string _prevProductName;

    // ---------- Menu manual ----------
    [MenuItem("BioBlocks/Apply Environment Config")]
    public static void ApplyEnvironmentConfigMenu()
    {
        var cfg = EnvironmentConfig.Load();
        if (cfg == null)
        {
            EditorUtility.DisplayDialog("Erro",
                "EnvironmentConfig não encontrado em Resources/.", "OK");
            return;
        }

        CopyFirebaseConfig(cfg.FirebaseEnvironment);
        // Não troca bundle id no menu — só no build (pra não sujar ProjectSettings.asset)
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Pronto",
            $"Ambiente aplicado:\n" +
            $"  Firebase: {cfg.FirebaseEnvironment}\n" +
            $"  Preview Mode: {cfg.QuestionPreviewMode}",
            "OK");
    }

    // ---------- Hooks de build ----------
    public void OnPreprocessBuild(BuildReport report)
    {
        var cfg = EnvironmentConfig.Load();
        if (cfg == null)
            throw new BuildFailedException(
                "[EnvironmentSetup] EnvironmentConfig não encontrado em Resources/. Build abortado.");

        Debug.Log($"[EnvironmentSetup] ▶ Pré-build: {cfg.FirebaseEnvironment} / PreviewMode={cfg.QuestionPreviewMode}");

        CopyFirebaseConfig(cfg.FirebaseEnvironment);
        ApplyBundleIdAndName(cfg.FirebaseEnvironment, savePrevious: true);
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        // Reverte bundle id / product name pros valores anteriores ao build,
        // pra ProjectSettings.asset não ficar sujo.
        if (_prevBundleIdAndroid != null)
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, _prevBundleIdAndroid);
        if (_prevBundleIdiOS != null)
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, _prevBundleIdiOS);
        if (_prevProductName != null)
            PlayerSettings.productName = _prevProductName;

        Debug.Log("[EnvironmentSetup] ◀ Pós-build: PlayerSettings restaurados.");
    }

    // ---------- Cópia dos arquivos Firebase ----------
    private static void CopyFirebaseConfig(FirebaseEnvironment env)
    {
        string envFolder = env.ToString(); // "Prod" ou "Dev"
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string sourceDir = Path.Combine(projectRoot, FirebaseSourceRoot, envFolder);

        if (!Directory.Exists(sourceDir))
            throw new BuildFailedException(
                $"[EnvironmentSetup] Pasta de Firebase não existe: {sourceDir}");

        CopyIfDifferent(
            Path.Combine(sourceDir, AndroidConfigFile),
            Path.Combine(Application.dataPath, AndroidConfigFile));

        CopyIfDifferent(
            Path.Combine(sourceDir, iOSConfigFile),
            Path.Combine(Application.dataPath, iOSConfigFile));
    }

    private static void CopyIfDifferent(string source, string dest)
    {
        if (!File.Exists(source))
            throw new BuildFailedException(
                $"[EnvironmentSetup] Arquivo fonte ausente: {source}");

        if (File.Exists(dest) && HashFile(source) == HashFile(dest))
        {
            Debug.Log($"[EnvironmentSetup]   = {Path.GetFileName(dest)} já está sincronizado.");
            return;
        }

        File.Copy(source, dest, overwrite: true);
        Debug.Log($"[EnvironmentSetup]   ✓ Copiado: {Path.GetFileName(dest)}");
    }

    private static string HashFile(string path)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        using var stream = File.OpenRead(path);
        return BitConverter.ToString(md5.ComputeHash(stream));
    }

    // ---------- Bundle ID / Product Name ----------
        private static void ApplyBundleIdAndName(FirebaseEnvironment env, bool savePrevious)
    {
        string targetAndroid = env == FirebaseEnvironment.Prod ? ProdBundleIdAndroid : DevBundleIdAndroid;
        string targetiOS     = env == FirebaseEnvironment.Prod ? ProdBundleIdiOS     : DevBundleIdiOS;
        string targetProduct = env == FirebaseEnvironment.Prod ? ProdProductName     : DevProductName;

        if (savePrevious)
        {
            _prevBundleIdAndroid = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            _prevBundleIdiOS     = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);
            _prevProductName     = PlayerSettings.productName;
        }

        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, targetAndroid);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS,     targetiOS);
        PlayerSettings.productName = targetProduct;

        Debug.Log($"[EnvironmentSetup]   ✓ Bundle Android → {targetAndroid}");
        Debug.Log($"[EnvironmentSetup]   ✓ Bundle iOS     → {targetiOS}");
        Debug.Log($"[EnvironmentSetup]   ✓ Product Name   → {targetProduct}");
    }
}