using UnityEngine;

/// <summary>
/// Qual projeto Firebase está ativo em runtime.
/// Prod  → MicroLearning (microlearning-33132)
/// Dev   → MicroLearning-Dev (microlearning-dev-79c0c)
/// </summary>
public enum FirebaseEnvironment
{
    Prod,
    Dev
}

/// <summary>
/// Configuração de ambiente do app, lida em runtime e em build time.
///
/// Asset esperado em Assets/Resources/EnvironmentConfig.asset.
/// Valores padrão commitados (ambas as branches):
///   firebaseEnvironment → Dev
///   questionPreviewMode → false
///
/// Apenas o admin altera firebaseEnvironment para Prod localmente,
/// sem commitar, ao gerar builds de produção.
/// </summary>
/// 
[CreateAssetMenu(
    fileName = "EnvironmentConfig",
    menuName = "BioBlocks/Environment Config",
    order = 0
)]
public class EnvironmentConfig : ScriptableObject
{
    [Header("Firebase")]
    [Tooltip("Qual projeto Firebase usar: Prod (microlearning-33132) ou Dev (microlearning-dev-79c0c).")]
    [SerializeField] private FirebaseEnvironment firebaseEnvironment = FirebaseEnvironment.Dev;

    [Header("Preview Mode")]
    [Tooltip("Ativa o modo de preview para criadores de conteúdo: bypassa TODA inicialização Firebase e carrega questões diretamente dos arquivos C# hardcoded. Não requer internet nem autenticação.")]
    [SerializeField] private bool questionPreviewMode = false;

    public FirebaseEnvironment FirebaseEnvironment => firebaseEnvironment;

    private static EnvironmentConfig _cached;

    /// <summary>
    /// Carrega o asset de Resources. Cacheia em static para evitar
    /// múltiplos Resources.Load.
    /// </summary>
    public static EnvironmentConfig Load()
    {
        if (_cached == null)
        {
            _cached = Resources.Load<EnvironmentConfig>("EnvironmentConfig");
            if (_cached == null)
            {
                Debug.LogError(
                    "[EnvironmentConfig] Asset não encontrado em Resources/EnvironmentConfig. " +
                    "Crie via menu Assets > Create > BioBlocks > Environment Config."
                );
            }
        }
        return _cached;
    }

    // ---------- Override para testes ----------
    private static bool? _previewModeOverride;

    public static void OverridePreviewModeForTests(bool value)
        => _previewModeOverride = value;

    public static void ClearTestOverride()
        => _previewModeOverride = null;

    public bool QuestionPreviewMode => _previewModeOverride ?? questionPreviewMode;
}