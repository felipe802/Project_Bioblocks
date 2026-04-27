using UnityEngine;

public class BioBlocksSettings : MonoBehaviour
{
    private static BioBlocksSettings _instance;

    [Header("Debug Settings")]
    [SerializeField] private bool isDebugMode = false;
    [SerializeField] private bool useMockQuestions = false;

    public static BioBlocksSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<BioBlocksSettings>();
                if (_instance == null)
                {
                    Debug.LogError("[BioBlocksSettings] Não existe BioBlocksSettings na cena inicial. Adicione um GameObject com este componente na primeira cena (Boot/Menu).");
                }
            }
            return _instance;
        }
    }

#if DEBUG
    public const string VERSION = "3.0.0-dev";
    public const bool IS_DEBUG = true;
    public const string ENVIRONMENT = "Development";
#else
    public const string VERSION = "3.0.0";
    public const bool IS_DEBUG = false;
    public const string ENVIRONMENT = "Production";
#endif

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

#if RELEASE
        isDebugMode = false;
        useMockQuestions = false;
#endif

        InitializeSettings();
    }

    private void InitializeSettings()
    {
#if DEBUG
        Debug.Log($"[ProjectSettings] Initialized in {ENVIRONMENT} mode");
        Debug.Log($"[ProjectSettings] Version: {VERSION}");
        Debug.Log($"[ProjectSettings] Debug Mode: {isDebugMode}");
        Debug.Log($"[ProjectSettings] Mock Questions: {useMockQuestions}");
        Application.logMessageReceived += HandleLog;
#else
        Debug.unityLogger.logEnabled = false;
#endif
    }

#if DEBUG
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
        }
    }
#endif

    public bool IsDebugMode()
    {
#if RELEASE
        return false;
#elif DEBUG
        return isDebugMode;
#else
        return false;
#endif
    }

    public bool UseMockQuestions()
    {
#if RELEASE
        return false;
#elif DEBUG
        return isDebugMode && useMockQuestions;
#else
        return false;
#endif
    }

    public void ToggleDebugMode()
    {
#if DEBUG
        isDebugMode = !isDebugMode;
        Debug.Log($"[ProjectSettings] Debug Mode changed to: {isDebugMode}");
#endif
    }

    public void SetDebugMode(bool debug)
    {
#if DEBUG
        isDebugMode = debug;
        Debug.Log($"[ProjectSettings] Debug Mode set to: {isDebugMode}");
#endif
    }

    public void ToggleMockQuestions()
    {
#if DEBUG
        useMockQuestions = !useMockQuestions;
        Debug.Log($"[ProjectSettings] Mock Questions changed to: {useMockQuestions}");
#endif
    }

    public void SetMockQuestions(bool useMock)
    {
#if DEBUG
        useMockQuestions = useMock;
        Debug.Log($"[ProjectSettings] Mock Questions set to: {useMockQuestions}");
#endif
    }
}