using UnityEngine;

/// <summary>
/// Qual projeto Firebase está ativo em runtime.
/// Prod  → MicroLearning (microlearning-33132)
/// Dev   → MicroLearning-Dev (microlearning-dev-5003c)
/// </summary>
public enum FirebaseEnvironment
{
    Prod,
    Dev
}

/// <summary>
/// Fonte de questões da QuestionScene.
/// Firestore → questões reais via QuestionSync (LiteDB cacheado de Firestore).
/// Fake      → questões locais editáveis em FakeQuestions (Resources).
/// </summary>
public enum QuestionSourceMode
{
    Firestore,
    Fake
}

/// <summary>
/// Configuração de ambiente do app, lida em runtime e em build time.
///
/// Asset esperado em Assets/Resources/EnvironmentConfig.asset.
/// Valores padrão por branch:
///   dev  → Dev + Fake
///   main → Prod + Firestore
///
/// IMPORTANTE: este asset está protegido por .gitattributes com
/// merge=ours, para que cada branch preserve seus próprios valores.
/// Cada colaborador precisa configurar o merge driver localmente uma
/// vez (ver README do projeto).
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
    [Tooltip("Qual projeto Firebase usar: Prod (microlearning-33132) ou Dev (microlearning-dev-5003c).")]
    [SerializeField] private FirebaseEnvironment firebaseEnvironment = FirebaseEnvironment.Dev;

    [Header("Questions")]
    [Tooltip("Fonte de questões: Firestore (real) ou Fake (FakeQuestions local, editável).")]
    [SerializeField] private QuestionSourceMode questionSource = QuestionSourceMode.Fake;

    public FirebaseEnvironment FirebaseEnvironment => firebaseEnvironment;
    public QuestionSourceMode QuestionSource => questionSource;

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
}