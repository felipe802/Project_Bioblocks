using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Gerencia a seleção e cópia do google-services.json correto baseado em EnvironmentConfig.FirebaseEnvironment.
/// Executa automaticamente ao entrar no editor e antes de builds.
/// </summary>
[InitializeOnLoad]
public class FirebaseEnvironmentSetup : IPreprocessBuildWithReport
{
    private const string FirebaseSourceDevPath    = "Firebase/Dev/google-services.json";
    private const string FirebaseSourceProdPath   = "Firebase/Prod/google-services.json";
    private const string FirebaseTargetPath       = "Assets/google-services.json";

    private const string DevProjectId   = "microlearning-dev-79c0c";
    private const string ProdProjectId  = "microlearning-33132";

    public int callbackOrder => -1001; // Antes do EnvironmentSetup (-1000)

    // ── Inicialização automática no editor ──
    static FirebaseEnvironmentSetup()
    {
        EditorApplication.delayCall += () => SetupFirebaseConfiguration(isAutomatic: true);
    }

    // ── Hook de pré-build ──
    public void OnPreprocessBuild(BuildReport report)
    {
        SetupFirebaseConfiguration(isAutomatic: false);
    }

    // ── Método principal ──
    private static void SetupFirebaseConfiguration(bool isAutomatic)
    {
        var cfg = EnvironmentConfig.Load();
        if (cfg == null)
        {
            Debug.LogError(
                "[FirebaseEnvironmentSetup] EnvironmentConfig não encontrado. " +
                "Crie via Assets > Create > BioBlocks > Environment Config.");
            return;
        }

        string environment = cfg.FirebaseEnvironment.ToString();
        string sourceDir = Path.Combine(Application.dataPath, "..", 
            cfg.FirebaseEnvironment == FirebaseEnvironment.Prod ? FirebaseSourceProdPath : FirebaseSourceDevPath);
        string targetPath = Path.Combine(Application.dataPath, "google-services.json");
        string expectedProjectId = cfg.FirebaseEnvironment == FirebaseEnvironment.Prod ? ProdProjectId : DevProjectId;

        try
        {
            // 1. Validar que o arquivo de origem existe
            if (!File.Exists(sourceDir))
            {
                Debug.LogError(
                    $"[FirebaseEnvironmentSetup] Arquivo não encontrado: {sourceDir}\n" +
                    $"Certifique-se de que o arquivo google-services.json está em Firebase/{environment}/");
                return;
            }

            // 2. Copiar arquivo
            File.Copy(sourceDir, targetPath, overwrite: true);
            Debug.Log($"[FirebaseEnvironmentSetup] ✓ google-services.json copiado de Firebase/{environment}/");

            // 3. Validar conteúdo (verificar project_id)
            string content = File.ReadAllText(targetPath);
            if (!content.Contains(expectedProjectId))
            {
                Debug.LogWarning(
                    $"[FirebaseEnvironmentSetup] ⚠ AVISO: google-services.json copiado não contém o project_id esperado.\n" +
                    $"Esperado: {expectedProjectId}\n" +
                    $"Este arquivo pode estar desatualizado. Verifique Firebase/{environment}/google-services.json");
            }

            // 4. Marcar para reimportação
            AssetDatabase.ImportAsset("Assets/google-services.json", ImportAssetOptions.ForceUpdate);
            Debug.Log($"[FirebaseEnvironmentSetup] ✓ Assets reimportados.");

            // 5. Log final
            if (!isAutomatic)
            {
                Debug.Log(
                    $"[FirebaseEnvironmentSetup] ▶ Pré-build configurado:\n" +
                    $"  Ambiente: {environment}\n" +
                    $"  Project ID: {expectedProjectId}\n" +
                    $"  Arquivo: {targetPath}");
            }
            else
            {
                Debug.Log(
                    $"[FirebaseEnvironmentSetup] ✓ Firebase {environment} configurado " +
                    $"({expectedProjectId})");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"[FirebaseEnvironmentSetup] Erro ao configurar Firebase {environment}: {ex.Message}\n" +
                $"StackTrace: {ex.StackTrace}");
        }
    }

    // ── Menu manual para validação ──
    [MenuItem("BioBlocks/Validate Firebase Setup")]
    public static void ValidateFirebaseSetupMenu()
    {
        var cfg = EnvironmentConfig.Load();
        if (cfg == null)
        {
            EditorUtility.DisplayDialog("Erro", "EnvironmentConfig não encontrado.", "OK");
            return;
        }

        string environment = cfg.FirebaseEnvironment.ToString();
        string expectedProjectId = cfg.FirebaseEnvironment == FirebaseEnvironment.Prod ? ProdProjectId : DevProjectId;
        string targetPath = Path.Combine(Application.dataPath, "google-services.json");

        if (!File.Exists(targetPath))
        {
            EditorUtility.DisplayDialog("Erro", 
                $"google-services.json não encontrado em Assets/", "OK");
            return;
        }

        string content = File.ReadAllText(targetPath);
        bool hasCorrectProjectId = content.Contains(expectedProjectId);

        string message = $"Ambiente: {environment}\n" +
                        $"Project ID Esperado: {expectedProjectId}\n" +
                        $"Status: {(hasCorrectProjectId ? "✓ OK" : "✗ ERRO")}\n\n" +
                        $"Se o status for ERRO, verifique Firebase/{environment}/google-services.json";

        EditorUtility.DisplayDialog("Firebase Setup Validation", message, "OK");
    }
}