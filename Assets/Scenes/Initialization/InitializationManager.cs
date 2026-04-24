using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class InitializationManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject retryPanel;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image progressBar;
    [SerializeField] private TMP_Text errorText;

    [Header("Configuration")]
    [SerializeField] private float minimumLoadingTime = 2.0f;

    [Header("Global Loading Spinner")]
    [SerializeField] private GameObject globalSpinnerPrefab;

    private IFirestoreRepository _firestore;
    private IAuthRepository _auth;
    private IUserDataSyncService _userDataSync;
    private IUserDataLocalRepository _userDataLocal;

    private LoadingSpinnerComponent globalSpinner;

    private void Awake()
    {
        InitializeGlobalSpinner();
    }

    private void Start()
    {
        SetupUI();
        StartInitialization();
    }

    private void InitializeGlobalSpinner()
    {
        try
        {
            globalSpinner = LoadingSpinnerComponent.Instance;

            if (globalSpinner == null && globalSpinnerPrefab != null)
            {
                Canvas mainCanvas = FindObjectOfType<Canvas>();
                GameObject spinnerObject = mainCanvas != null
                    ? Instantiate(globalSpinnerPrefab, mainCanvas.transform)
                    : Instantiate(globalSpinnerPrefab);

                spinnerObject.name = "GlobalLoadingSpinner";
                DontDestroyOnLoad(spinnerObject);
                globalSpinner = spinnerObject.GetComponent<LoadingSpinnerComponent>();
            }
            globalSpinner?.ShowSpinner();
        }
        catch (Exception e)
        {
            Debug.LogError($"[InitializationManager] Erro ao inicializar spinner: {e.Message}");
        }
    }

    private void SetupUI()
    {
        if (retryPanel != null) retryPanel.SetActive(false);
        if (progressBar != null) progressBar.fillAmount = 0f;
    }

    private async void StartInitialization()
    {
        float startTime = Time.time;
        Debug.Log("[InitManager] StartInitialization começou");

        try
        {
            if (!AppContext.IsReady)
            {
                Debug.Log("[InitManager] Aguardando AppContext...");
                UpdateStatus("Inicializando Firebase...");
                await WaitForAppContext();
                Debug.Log("[InitManager] AppContext pronto");
            }
            else
            {
                Debug.Log("[InitManager] AppContext já estava pronto");
            }

            _firestore     = AppContext.Firestore;
            _auth          = AppContext.Auth;
            _userDataSync  = AppContext.UserDataSync;
            _userDataLocal = AppContext.UserDataLocal;

            UpdateProgress(0.3f);
            UpdateStatus("Verificando autenticação...");
            Debug.Log("[InitManager] Verificando autenticação...");
            bool isAuthenticated = await CheckAuthentication();
            Debug.Log($"[InitManager] isAuthenticated={isAuthenticated}");
            UpdateProgress(0.5f);

            bool userDataLoaded = false;

            if (isAuthenticated)
            {
                UpdateStatus("Carregando dados do usuário...");
                Debug.Log("[InitManager] Carregando dados...");
                userDataLoaded = await LoadUserData();
                Debug.Log($"[InitManager] userDataLoaded={userDataLoaded}");
                UpdateProgress(0.7f);

                if (userDataLoaded)
                {
                    UpdateStatus("Carregando bancos de questões...");
                    await (AppContext.Statistics as DatabaseStatisticsManager)?.Initialize();
                    UpdateProgress(0.85f);

                    UpdateStatus("Configurando sistema de níveis...");
                    InitializePlayerLevelService();
                    UpdateProgress(0.9f);
                }
            }

            float elapsed = Time.time - startTime;
            if (elapsed < minimumLoadingTime)
                await Task.Delay(Mathf.RoundToInt((minimumLoadingTime - elapsed) * 1000));

            NavigateAfterInit(isAuthenticated && userDataLoaded);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[InitializationManager] Falha: {ex.GetType().Name}: {ex.Message}");
            Debug.LogError($"[InitializationManager] StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
                Debug.LogError($"[InitializationManager] InnerException: {ex.InnerException.Message}");

            try { globalSpinner?.HideSpinner(); } catch { }
            ShowError($"Falha na inicialização: {ex.Message}");
        }
    }

    private async Task WaitForAppContext()
    {
        float timeout = 15f;
        float elapsed = 0f;

        while (!AppContext.IsReady && elapsed < timeout)
        {
            await Task.Delay(100);
            elapsed += 0.1f;

            if (Mathf.RoundToInt(elapsed * 10) % 30 == 0)
                Debug.Log($"[InitManager] Aguardando AppContext... {elapsed:F1}s");
        }

        if (!AppContext.IsReady)
            throw new Exception("Sem conexão. Verifique sua internet e tente novamente.");
    }

    private async Task<bool> CheckAuthentication()
    {
        if (!_auth.HasLocalSession()) return false;

        try
        {
            await _auth.ReloadCurrentUserAsync();
            Debug.Log("[InitializationManager] Sessão validada com o servidor.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[InitializationManager] Validação online falhou, entrando offline: {e.Message}");
            return _auth.HasLocalSession();
        }
    }

    private async Task<bool> LoadUserData()
    {
        try
        {
            if (!_auth.HasLocalSession()) return false;

            string userId = _auth.CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return false;

            // TrySyncPendingData já resolve todos os cenários:
            //   - sem cache local    → busca Firestore → salva LiteDB
            //   - cache dirty        → envia ao Firestore → marca synced
            //   - cache stale        → busca Firestore → atualiza LiteDB
            //   - cache válido       → carrega LiteDB direto
            //   - Firestore offline  → usa LiteDB como fallback
            await _userDataSync.TrySyncPendingData(userId);

            if (UserDataStore.CurrentUserData == null)
            {
                Debug.LogError("[InitializationManager] UserData nulo após sync.");
                return false;
            }

            Debug.Log($"[InitializationManager] UserData pronto. " +
                      $"UserId: {UserDataStore.CurrentUserData.UserId}, " +
                      $"Level: {UserDataStore.CurrentUserData.PlayerLevel}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[InitializationManager] Erro ao carregar dados: {e.Message}");

            // Último recurso: tenta carregar direto do LiteDB
            string userId = _auth.CurrentUserId;
            if (!string.IsNullOrEmpty(userId))
            {
                var cached = _userDataLocal.GetUser(userId);
                if (cached != null)
                {
                    UserDataStore.CurrentUserData = cached;
                    Debug.LogWarning("[InitializationManager] UserData carregado do cache de emergência.");
                    return true;
                }
            }

            return false;
        }
    }

    private void NavigateAfterInit(bool authenticated)
    {
        try
        {
            string targetScene = authenticated ? "PathwayScene" : "LoginView";
            globalSpinner?.ShowSpinnerUntilSceneLoaded(targetScene);
            SceneManager.LoadScene(targetScene);
        }
        catch (Exception)
        {
            string targetScene = authenticated ? "PathwayScene" : "LoginView";
            SceneManager.LoadScene(targetScene);
        }
    }

    private void InitializePlayerLevelService()
    {
        if (AppContext.PlayerLevel == null)
        {
            Debug.LogError("[InitializationManager] PlayerLevelService não encontrado no AppContext.");
            return;
        }
        Debug.Log("[InitializationManager] PlayerLevelService pronto.");
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }

    private void UpdateProgress(float progress)
    {
        if (progressBar != null) progressBar.fillAmount = progress;
    }

    private void ShowError(string message)
    {
        // Esconde o spinner explicitamente aqui também
        try { globalSpinner?.HideSpinner(); } catch { }

        if (retryPanel != null)
        {
            retryPanel.SetActive(true);
            if (errorText != null) errorText.text = message;
        }
        else
        {
            // retryPanel não configurado — loga para identificar no Xcode
            Debug.LogError($"[InitManager] retryPanel é null. Mensagem de erro: {message}");
        }
    }
}