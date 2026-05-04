using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using QuestionSystem;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

[InitializeOnLoad]
public class DevelopmentFlagsValidator : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        var envCfg = EnvironmentConfig.Load();
        if (envCfg == null)
        {
            // EnvironmentSetup (callbackOrder=-1000) já lança BuildFailedException nesse caso,
            // mas garantimos a cobertura aqui também.
            throw new BuildFailedException(
                "[DevelopmentFlagsValidator] EnvironmentConfig não encontrado em Resources/. Build abortado.");
        }

        bool isDevelopmentBuild = EditorUserBuildSettings.development;
        bool isPreviewMode      = envCfg.QuestionPreviewMode;
        bool isProdFirebase     = envCfg.FirebaseEnvironment == FirebaseEnvironment.Prod;

        List<string> errors   = new List<string>();
        List<string> warnings = new List<string>();

        // ── 1. Validações do EnvironmentConfig ─────────────────────────────────
        ValidateEnvironmentConfig(errors, warnings, isDevelopmentBuild, isPreviewMode, isProdFirebase);

        // ── 2. Validações dos bancos de dados ──────────────────────────────────
        // Em Preview Mode o HardcodedQuestionSource ignora databaseInDevelopment por completo
        // (chama GetQuestions() diretamente, sem passar pelo QuestionFilterService).
        // Validar essa flag no contexto de Preview Mode geraria falsos positivos.
        if (!isPreviewMode)
            ValidateDatabases(errors, warnings, isDevelopmentBuild, isProdFirebase);
        else
            warnings.Add("⚠️  Preview Mode ativo — validação de databaseInDevelopment ignorada " +
                         "(a flag não tem efeito neste ambiente).");

        // ── 3. Log de contexto ─────────────────────────────────────────────────
        Debug.Log($"[DevelopmentFlagsValidator] Ambiente: Firebase={envCfg.FirebaseEnvironment} | " +
                  $"PreviewMode={isPreviewMode} | DevelopmentBuild={isDevelopmentBuild}");

        // ── 4. Resultado ───────────────────────────────────────────────────────
        if (errors.Count > 0)
        {
            string errorMessage = "BUILD BLOQUEADO! Configuração de ambiente inválida:\n\n" +
                                  string.Join("\n", errors);

            Debug.LogError("=================================================");
            Debug.LogError(errorMessage);
            Debug.LogError("Verifique o EnvironmentConfig em Assets/Resources/EnvironmentConfig.");
            Debug.LogError("=================================================");

            throw new BuildFailedException(errorMessage);
        }

        if (isDevelopmentBuild)
        {
            Debug.LogWarning("=================================================");
            Debug.LogWarning("DEVELOPMENT BUILD DETECTADO");
            Debug.LogWarning($"Firebase: {envCfg.FirebaseEnvironment} | Preview Mode: {isPreviewMode}");
            if (warnings.Count > 0)
                foreach (var warning in warnings)
                    Debug.LogWarning(warning);
            Debug.LogWarning("=================================================");
        }
        else
        {
            if (warnings.Count > 0)
            {
                Debug.LogWarning("=================================================");
                Debug.LogWarning("AVISOS DE BUILD DE RELEASE:");
                foreach (var warning in warnings)
                    Debug.LogWarning(warning);
                Debug.LogWarning("=================================================");
            }
            else
            {
                Debug.Log("=================================================");
                Debug.Log("✓ BUILD DE RELEASE: Validação passou");
                Debug.Log($"✓ Firebase: {envCfg.FirebaseEnvironment} | Preview Mode: {isPreviewMode}");
                Debug.Log("=================================================");
            }
        }
    }

    // ── Validações do EnvironmentConfig ────────────────────────────────────────

    private static void ValidateEnvironmentConfig(
        List<string> errors, List<string> warnings,
        bool isDevelopmentBuild, bool isPreviewMode, bool isProdFirebase)
    {
        // Preview Mode não pode ir para nenhum build de Release — ele bypassa Firebase e
        // autenticação inteiramente. Usuários receberiam um app sem dados reais.
        if (!isDevelopmentBuild && isPreviewMode)
            errors.Add("❌ questionPreviewMode=true em build de Release. " +
                       "Preview Mode bypassa Firebase e autenticação — jamais pode chegar a usuários.");

        // Dev Firebase não pode ir para um build de Release. Todo release deve apontar para Prod.
        if (!isDevelopmentBuild && !isProdFirebase)
            errors.Add("❌ FirebaseEnvironment=Dev em build de Release. " +
                       "Builds de Release devem sempre apontar para Prod Firebase.");
    }

    // ── Validações dos bancos de dados ─────────────────────────────────────────

    private static void ValidateDatabases(
        List<string> errors, List<string> warnings,
        bool isDevelopmentBuild, bool isProdFirebase)
    {
        var databases = Object.FindObjectsOfType<MonoBehaviour>().OfType<IQuestionDatabase>();
        if (!databases.Any())
            databases = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<IQuestionDatabase>();

        foreach (var database in databases)
        {
            string databaseName = database.GetDatabankName();

            if (database.IsDatabaseInDevelopment())
            {
                if (isProdFirebase)
                {
                    // Prod Firebase + databaseInDevelopment=true: nunca faz sentido em qualquer
                    // tipo de build. Usuários de Prod só veriam as questões marcadas como em edição.
                    errors.Add($"❌ Database '{databaseName}' com databaseInDevelopment=true apontando para " +
                               "Prod Firebase. Essa combinação não tem uso válido.");
                }
                else if (!isDevelopmentBuild)
                {
                    // Dev Firebase + Release Build + databaseInDevelopment=true: incomum mas
                    // intencional (ex: release de Dev para testar somente as questões em edição).
                    warnings.Add($"⚠️  Database '{databaseName}' com databaseInDevelopment=true em Release Build de Dev. " +
                                 "Apenas questões com questionInDevelopment=true serão visíveis.");
                }
                else
                {
                    // Dev Firebase + Development Build: fluxo esperado de desenvolvimento.
                    warnings.Add($"⚠️  Database '{databaseName}' em modo desenvolvimento " +
                                 "(OK — Dev Firebase + Development Build).");
                }
            }

            // Nota: GetQuestions() é usado diretamente para encontrar questões em desenvolvimento
            // em qualquer banco, independentemente do estado de databaseInDevelopment.
            // QuestionFilterService.FilterQuestions() excluiria essas questões quando
            // databaseInDevelopment=false, tornando o aviso inútil.
            List<Question> allQuestions = database.GetQuestions();
            var devQuestions = allQuestions.Where(q => q.questionInDevelopment).ToList();

            if (devQuestions.Any())
            {
                string questionNumbers = string.Join(", ", devQuestions.Select(q => q.questionNumber));
                warnings.Add($"⚠️  Database '{databaseName}' contém {devQuestions.Count} questão(ões) com " +
                             $"questionInDevelopment=true (serão filtradas em modo normal). Questões: {questionNumbers}");
            }
        }

        if (errors.Count == 0 && warnings.Count == 0)
            Debug.Log("[DevelopmentFlagsValidator] ✓ Nenhuma flag problemática nos bancos de dados.");
    }
}

public class DevelopmentFlagsRuntimeValidator
{
    static DevelopmentFlagsRuntimeValidator()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
            ValidateOnPlayMode();
    }

    private static void ValidateOnPlayMode()
    {
        var envCfg = EnvironmentConfig.Load();

        bool isPreviewMode  = envCfg?.QuestionPreviewMode ?? false;
        bool isProdFirebase = envCfg?.FirebaseEnvironment == FirebaseEnvironment.Prod;

        Debug.Log($"[DevelopmentFlagsValidator] Play Mode — Firebase={envCfg?.FirebaseEnvironment} | " +
                  $"PreviewMode={isPreviewMode}");

        // Em Preview Mode databaseInDevelopment não tem efeito, não faz sentido validar.
        if (isPreviewMode)
        {
            Debug.Log("[DevelopmentFlagsValidator] Preview Mode ativo — validação de bancos ignorada.");
            return;
        }

        var databases = Object.FindObjectsOfType<MonoBehaviour>().OfType<IQuestionDatabase>();

        foreach (var database in databases)
        {
            string databaseName = database.GetDatabankName();

            if (database.IsDatabaseInDevelopment())
            {
                if (isProdFirebase)
                {
                    Debug.LogError($"=================================================");
                    Debug.LogError($"❌ Database '{databaseName}' com databaseInDevelopment=true " +
                                   "apontando para Prod Firebase!");
                    Debug.LogError($"=================================================");
                }
                else
                {
                    Debug.LogWarning($"=================================================");
                    Debug.LogWarning($"⚠️  MODO DESENVOLVIMENTO ATIVO");
                    Debug.LogWarning($"Database: {databaseName} | Firebase: Dev | Saves: DESABILITADOS");
                    Debug.LogWarning($"=================================================");
                }
            }

            // Usa GetQuestions() diretamente para detectar questões dev em qualquer banco.
            List<Question> allQuestions = database.GetQuestions();
            var devQuestions = allQuestions.Where(q => q.questionInDevelopment).ToList();
            if (devQuestions.Any())
                Debug.LogWarning($"[DevelopmentFlagsValidator] '{databaseName}' tem {devQuestions.Count} " +
                                 "questão(ões) com questionInDevelopment=true.");
        }
    }
}
#endif