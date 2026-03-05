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
        bool isDevelopmentBuild = EditorUserBuildSettings.development;
        
        List<string> errors = new List<string>();
        List<string> warnings = new List<string>();

        ValidateDatabases(errors, warnings, isDevelopmentBuild);

        if (isDevelopmentBuild)
        {
            Debug.LogWarning("=================================================");
            Debug.LogWarning("DEVELOPMENT BUILD DETECTADO");
            Debug.LogWarning("Flags de desenvolvimento são PERMITIDAS");
            Debug.LogWarning("=================================================");
            
            if (warnings.Count > 0)
            {
                foreach (var warning in warnings)
                {
                    Debug.LogWarning(warning);
                }
            }
            
            return;
        }

        if (errors.Count > 0)
        {
            string errorMessage = "BUILD DE RELEASE BLOQUEADO! Flags de desenvolvimento detectadas:\n\n" + 
                                 string.Join("\n", errors);
            
            Debug.LogError("=================================================");
            Debug.LogError(errorMessage);
            Debug.LogError("Desmarque 'Development Build' OU remova as flags de desenvolvimento");
            Debug.LogError("=================================================");
            
            throw new BuildFailedException(errorMessage);
        }

        if (warnings.Count > 0)
        {
            Debug.LogWarning("=================================================");
            Debug.LogWarning("AVISOS DE BUILD DE RELEASE:");
            foreach (var warning in warnings)
            {
                Debug.LogWarning(warning);
            }
            Debug.LogWarning("=================================================");
        }
        else
        {
            Debug.Log("=================================================");
            Debug.Log("✓ BUILD DE RELEASE: Validação passou");
            Debug.Log("✓ Nenhuma flag de desenvolvimento detectada");
            Debug.Log("=================================================");
        }
    }

    private void ValidateDatabases(List<string> errors, List<string> warnings, bool isDevelopmentBuild)
    {
        var databases = Object.FindObjectsOfType<MonoBehaviour>().OfType<IQuestionDatabase>();

        if (!databases.Any())
        {
            databases = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<IQuestionDatabase>();
        }

        foreach (var database in databases)
        {
            string databaseName = database.GetDatabankName();

            if (database.IsDatabaseInDevelopment())
            {
                if (isDevelopmentBuild)
                {
                    warnings.Add($"⚠️  Database '{databaseName}' está com flag 'databaseInDevelopment = true' (OK para Development Build)");
                }
                else
                {
                    errors.Add($"❌ Database '{databaseName}' está com flag 'databaseInDevelopment = true'");
                }
            }

            List<Question> allQuestions = QuestionFilterService.FilterQuestions(database);
            var devQuestions = allQuestions.Where(q => q.questionInDevelopment).ToList();

            if (devQuestions.Any())
            {
                string questionNumbers = string.Join(", ", devQuestions.Select(q => q.questionNumber));
                warnings.Add($"⚠️  Database '{databaseName}' contém {devQuestions.Count} questão(ões) com flag 'questionInDevelopment = true' (Questões: {questionNumbers})");
            }
        }

        if (errors.Count == 0 && warnings.Count == 0)
        {
            Debug.Log("[DevelopmentFlagsValidator] ✓ Nenhuma flag de desenvolvimento detectada. Build pode prosseguir.");
        }
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
        {
            ValidateOnPlayMode();
        }
    }

    private static void ValidateOnPlayMode()
    {
        var databases = Object.FindObjectsOfType<MonoBehaviour>().OfType<IQuestionDatabase>();

        bool hasDevDatabases = false;
        bool hasDevQuestions = false;

        foreach (var database in databases)
        {
            string databaseName = database.GetDatabankName();

            if (database.IsDatabaseInDevelopment())
            {
                hasDevDatabases = true;
                Debug.LogWarning($"=================================================");
                Debug.LogWarning($"⚠️  MODO DESENVOLVIMENTO ATIVO");
                Debug.LogWarning($"Database: {databaseName}");
                Debug.LogWarning($"Firebase: DESABILITADO");
                Debug.LogWarning($"=================================================");
            }

            List<Question> allQuestions = QuestionFilterService.FilterQuestions(database);
            var devQuestions = allQuestions.Where(q => q.questionInDevelopment).ToList();

            if (devQuestions.Any())
            {
                hasDevQuestions = true;
            }
        }

        if (hasDevDatabases || hasDevQuestions)
        {
            Debug.LogWarning($"[DevelopmentFlagsValidator] Aplicação rodando com flags de desenvolvimento ativas");
        }
    }
}
#endif