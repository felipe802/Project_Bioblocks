using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class ProfileManager : MonoBehaviour
{
    [Header("UserData UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text emailText;
    [SerializeField] private TMP_Text createdTimeText;
    [SerializeField] private CanvasGroup userDataTable;

    [Header("Delete Account")]
    [SerializeField] private Button deleteAccountButton;
    [SerializeField] private TextMeshProUGUI deleteAccountButtonText;
    [SerializeField] private DeleteAccountPanel deleteAccountPanel;
    [SerializeField] private TextMeshProUGUI deleteAccountFeedbackText;

    [Header("Configurações de Overlay")]
    [SerializeField] private GameObject deleteAccountDarkOverlay;
    [SerializeField] private float overlayAlpha = 0.6f;

    [Header("ReAuthentication")]
    [SerializeField] private ReAuthenticationUI reAuthUI;

    [Header("Navegação e UI Global")]
    private INavigationService _navigation;
    private LoadingSpinnerComponent loadingSpinner;
    private HalfViewComponent _halfView;

    // -------------------------------------------------------
    // Dependências — obtidas do AppContext no Start()
    // -------------------------------------------------------
    private IFirestoreRepository _firestore;
    private IAuthRepository _auth;
    private IStatisticsProvider _statistics;

    private UserData currentUserData;
    private bool firestoreDeleted = false;

    // -------------------------------------------------------
    // Ciclo de vida
    // -------------------------------------------------------
    private void Start()
    {
        _firestore    = AppContext.Firestore;
        _auth         = AppContext.Auth;
        _statistics   = AppContext.Statistics;
        _navigation   = AppContext.Navigation;
        loadingSpinner = LoadingSpinnerComponent.Instance;

        if (deleteAccountPanel == null)
            Debug.LogError("DeleteAccountPanel não está atribuído no ProfileManager!");

        if (deleteAccountDarkOverlay != null)
            deleteAccountDarkOverlay.SetActive(false);

        _halfView = HalfViewRegistry.GetHalfViewForScene(SceneManager.GetActiveScene().name);
        ConfigureHalfView();

        InitializeAccountManager();
    }

    private void OnEnable()
    {
        UserDataStore.OnUserDataChanged += OnUserDataChanged;
        AnsweredQuestionsManager.OnAnsweredQuestionsUpdated += HandleAnsweredQuestionsUpdated;
    }

    private void OnDisable()
    {
        UserDataStore.OnUserDataChanged -= OnUserDataChanged;
        AnsweredQuestionsManager.OnAnsweredQuestionsUpdated -= HandleAnsweredQuestionsUpdated;
        if (this != null && gameObject != null && gameObject.scene.isLoaded)
        {
            GameObject darkOverlay = GameObject.Find("DarkOverlay");
            if (darkOverlay != null && darkOverlay.activeInHierarchy)
            {
                darkOverlay.SetActive(false);
                Debug.Log("DarkOverlay desativado via OnDisable");
            }
        }
    }

    // -------------------------------------------------------
    // Inicialização
    // -------------------------------------------------------
    private void InitializeAccountManager()
    {
        currentUserData = UserDataStore.CurrentUserData;
        Debug.Log($"CurrentUserData: {(currentUserData != null ? "Loaded" : "Null")}");

        if (currentUserData == null)
        {
            Debug.LogError("User data not loaded. Redirecting to Login.");
            return;
        }

        UpdateUI();

        _firestore.ListenToUserData(
            currentUserData.UserId,
            onScoreChanged: null,
            onWeekScoreChanged: null,
            onAnsweredQuestionsChanged: answeredQuestions =>
            {
                Debug.Log("ProfileManager: Recebeu atualização de questões respondidas via listener");
                if (_statistics.IsInitialized)
                    DisplayAnsweredQuestionsCount();
            }
        );

        if (_statistics.IsInitialized)
        {
            DisplayAnsweredQuestionsCount();
        }
        else
        {
            StartCoroutine(InitializeDatabaseStatistics());
        }
    }

    private IEnumerator InitializeDatabaseStatistics()
    {
        yield return null;
        var task = (_statistics as DatabaseStatisticsManager)?.Initialize();
        if (task == null) yield break;
        while (!task.IsCompleted) yield return null;
        if (task.IsFaulted)
            Debug.LogError($"[ProfileManager] Erro ao inicializar estatísticas: {task.Exception}");
        else
            DisplayAnsweredQuestionsCount();
    }

    // -------------------------------------------------------
    // UI
    // -------------------------------------------------------
    private void OnUserDataChanged(UserData userData)
    {
        currentUserData = userData;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currentUserData == null)
        {
            Debug.LogError("Tentando atualizar UI com userData null");
            return;
        }

        nameText.text = currentUserData.Name;
        if (scoreText != null)
            scoreText.text = $"{currentUserData.Score} pontos no total";
        emailText.text = currentUserData.Email;
        createdTimeText.text = $"Conta criada em {currentUserData.GetFormattedCreatedTime()}";
    }

    private void HandleAnsweredQuestionsUpdated(Dictionary<string, int> answeredCounts)
    {
        if (this == null) return;
        Debug.Log("ProfileManager: Recebeu atualização do AnsweredQuestionsManager");
        DisplayAnsweredQuestionsCount();
    }

    private void DisplayAnsweredQuestionsCount()
    {
        if (currentUserData == null || string.IsNullOrEmpty(currentUserData.UserId))
        {
            Debug.LogError("Usuário não encontrado ou ID inválido");
            return;
        }

        var userAnsweredQuestions = AnsweredQuestionsListStore.GetAnsweredQuestionsCountForUser(currentUserData.UserId);

        string[] allDatabases =
        {
            "AcidBaseBufferQuestionDatabase",
            "AminoacidQuestionDatabase",
            "BiochemistryIntroductionQuestionDatabase",
            "CarbohydratesQuestionDatabase",
            "EnzymeQuestionDatabase",
            "LipidsQuestionDatabase",
            "MembranesQuestionDatabase",
            "NucleicAcidsQuestionDatabase",
            "ProteinQuestionDatabase",
            "WaterQuestionDatabase"
        };

        foreach (string databankName in allDatabases)
        {
            int answeredCount = userAnsweredQuestions.ContainsKey(databankName) ? userAnsweredQuestions[databankName] : 0;
            int totalQuestions = QuestionBankStatistics.GetTotalQuestions(databankName);
            if (totalQuestions <= 0) totalQuestions = 50;

            string objectName = $"{databankName}CountText";
            GameObject textObject = GameObject.Find(objectName);
            if (textObject == null)
            {
                Debug.LogWarning($"Não foi possível encontrar o GameObject: {objectName}");
                continue;
            }

            TextMeshProUGUI tmpText = textObject.GetComponent<TextMeshProUGUI>();
            if (tmpText == null)
            {
                Debug.LogWarning($"TextMeshProUGUI não encontrado em: {objectName}");
                continue;
            }

            tmpText.text = $"{answeredCount}/{totalQuestions}";
        }
    }

    // -------------------------------------------------------
    // Navegação
    // -------------------------------------------------------
    public void Navigate(string sceneName)
    {
        
        if (_navigation != null)
            _navigation.NavigateTo(sceneName);
        else
        {
            Debug.LogWarning("[ProfileManager] NavigationService não disponível, usando SceneManager diretamente.");
            SceneManager.LoadScene(sceneName);
        }
    }

    // -------------------------------------------------------
    // Logout
    // -------------------------------------------------------
    public void LogoutButton()
    {
        StartCoroutine(LogoutAsync().AsCoroutine());
    }

    public async Task LogoutAsync()
    {
        try
        {
            UserDataStore.OnUserDataChanged -= OnUserDataChanged;
            AnsweredQuestionsManager.OnAnsweredQuestionsUpdated -= HandleAnsweredQuestionsUpdated;
            string currentUserId = UserDataStore.CurrentUserData?.UserId;

            // Sincroniza tudo que estiver pendente antes de limpar
            if (!string.IsNullOrEmpty(currentUserId) &&
                Application.internetReachability != NetworkReachability.NotReachable)
            {
                Debug.Log("[ProfileManager] Sincronizando dados pendentes antes do logout...");

                // Score, questões respondidas, dados gerais — via SyncService
                try
                {
                    await AppContext.UserDataSync.TrySyncPendingData(currentUserId);
                    Debug.Log("[ProfileManager] Dados sincronizados.");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ProfileManager] Falha ao sincronizar dados: {e.Message}");
                }
            }

            UserDataStore.CurrentUserData = null;
            AppContext.AnsweredQuestions?.ResetManager();

            if (!string.IsNullOrEmpty(currentUserId))
                AnsweredQuestionsListStore.ClearUserAnsweredQuestions(currentUserId);

            await _auth.LogoutAsync().ConfigureAwait(false);
            await Task.Yield();

            Debug.Log("Logout realizado com sucesso");
            Navigate("LoginView");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao realizar logout: {ex.Message}");
            throw;
        }
    }

    // -------------------------------------------------------
    // Delete Account
    // -------------------------------------------------------
    private void ConfigureHalfView()
    {
        if (_halfView == null) return;
        _halfView.Configure(
            "Opções da Conta",
            "Escolha uma das opções abaixo:",
            "Sair da Conta",
            () => LogoutButton(),
            "Deletar Conta",
            () =>
            {
                _halfView.HideMenu();
                StartCoroutine(DelayedStartDeleteAccount());
            }
        );
    }

    private IEnumerator DelayedStartDeleteAccount()
    {
        yield return new WaitForSeconds(0.1f);
        StartDeleteAccount();
    }

    public void StartDeleteAccount()
    {
        Debug.Log("StartDeleteAccount chamado");

        if (deleteAccountPanel == null)
        {
            Debug.LogError("DeleteAccountPanel é null!");
            return;
        }

        if (deleteAccountDarkOverlay != null)
        {
            deleteAccountDarkOverlay.SetActive(true);

            Image overlayImage = deleteAccountDarkOverlay.GetComponent<Image>();
            if (overlayImage != null)
            {
                Color color = overlayImage.color;
                color.a = overlayAlpha;
                overlayImage.color = color;
            }
        }
        else
        {
            Debug.LogError("DeleteAccountDarkOverlay é null!");
        }

        userDataTable.alpha = 0;
        userDataTable.interactable = false;
        userDataTable.blocksRaycasts = false;

        HalfViewRegistry.HideHalfViewForCurrentScene();
        deleteAccountPanel.ShowPanel();
        Debug.Log("DeleteAccountPanel exibido com sucesso");
    }

    public void CancelDeleteAccount()
    {
        if (deleteAccountPanel == null) return;

        if (deleteAccountDarkOverlay != null)
            deleteAccountDarkOverlay.SetActive(false);

        // Restaura os callbacks do HalfView, que são sobrescritos pelo
        // SetupButtonListeners() interno quando preventButtonReconfiguration
        // volta a false após HideMenu().
        ConfigureHalfView();

        GameObject halfViewOverlay = GameObject.Find("HalfViewDarkOverlay");
        if (halfViewOverlay != null)
            halfViewOverlay.SetActive(false);

        ReenableSceneInteractions();
        SetDeleteFeedback("");
        userDataTable.alpha = 1;
        userDataTable.interactable = true;
        userDataTable.blocksRaycasts = true;
        deleteAccountPanel.HidePanel();
    }

    public void DeleteAccountButton()
    {
        if (deleteAccountPanel != null && !deleteAccountPanel.IsReadyForInput)
        {
            Debug.Log("[DeleteAccountButton] Ignorado — painel ainda não pronto para input (touch bleed-through).");
            return;
        }
        StartCoroutine(DeleteAccountAsync().AsCoroutine());
    }

    public async Task DeleteAccountAsync(bool isRetry = false)
    {
        Debug.Log($"Starting DeleteAccountAsync... (isRetry: {isRetry})");

        if (currentUserData?.UserId == null)
        {
            Debug.LogError("The user is not connected.");
            return;
        }

        string userId    = currentUserData.UserId;
        string nickName  = currentUserData.NickName;

        try
        {
            if (deleteAccountButton != null) deleteAccountButton.interactable = false;
            SetDeleteFeedback("Verificando...");

            // ----------------------------------------------------------------
            // Fase 1 — Deleção de dados no Firestore
            //
            // IMPORTANTE: deve ocorrer ANTES do DeleteUser porque o
            // FirestoreRepository real exige usuário autenticado no Firebase.
            //
            // Cada operação usa seu próprio flag ou é idempotente — assim,
            // se a sessão estava expirada e as deleções falharam por permissão,
            // elas são repetidas após a reautenticação (isRetry=true) usando os
            // mesmos flags para evitar duplicatas quando já tiveram sucesso.
            // ----------------------------------------------------------------

            // 1a. Deletar documento principal do usuário (coleção Users)
            if (!firestoreDeleted)
            {
                try
                {
                    await _firestore.DeleteDocument("Users", userId);
                    Debug.Log("[DeleteAccount] Users: deletado com sucesso");
                    firestoreDeleted = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DeleteAccount] Erro ao deletar Users: {ex.Message}");
                }
            }

            // 1b. Deletar entrada na tabela de classificação (coleção Rankings)
            try
            {
                await _firestore.DeleteDocument("Rankings", userId);
                Debug.Log("[DeleteAccount] Rankings: deletado com sucesso");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DeleteAccount] Rankings (pode não existir): {ex.Message}");
            }

            // 1c. Deletar nickname na coleção Nicknames
            if (!string.IsNullOrEmpty(nickName))
            {
                try
                {
                    await _firestore.DeleteDocument("Nicknames", nickName);
                    Debug.Log("[DeleteAccount] Nicknames: deletado com sucesso");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[DeleteAccount] Nicknames: {ex.Message}");
                }
            }

            // 1d. Deletar o usuário de demais coleções onde ele possa existir
            //     (operações idempotentes — safe em retry)
            string[] additionalCollections =
            {
                "QuestionSceneBonus",
                "UserBonus",
                "UserFeedback",
                "UserLevelProgress",
                "UserRetries"
            };

            foreach (string collection in additionalCollections)
            {
                try
                {
                    await _firestore.DeleteDocument(collection, userId);
                    Debug.Log($"[DeleteAccount] {collection}: deletado com sucesso");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[DeleteAccount] {collection} (pode não existir): {ex.Message}");
                }
            }

            // ----------------------------------------------------------------
            // Fase 2 — Deletar o usuário do Firebase Authentication
            //
            // NÃO usar ConfigureAwait(false) aqui: quando
            // ReauthenticationRequiredException é lançada, a continuação
            // precisa estar na main thread para manipular UI (botões, panels).
            // ----------------------------------------------------------------
            bool reauthRequired = false;
            try
            {
                await _auth.DeleteUser(userId);
                Debug.Log("[DeleteAccount] Authentication: usuário deletado com sucesso");
            }
            catch (ReauthenticationRequiredException)
            {
                reauthRequired = true;
            }

            if (reauthRequired)
            {
                Debug.Log("[DeleteAccount] Reautenticação necessária");

                if (deleteAccountButton != null)
                    deleteAccountButton.interactable = true;
                SetDeleteFeedback("");

                try { deleteAccountPanel.HidePanel(); } catch { /* seguro */ }

                if (reAuthUI != null)
                {
                    Canvas reAuthCanvas = reAuthUI.GetComponent<Canvas>()
                                      ?? reAuthUI.gameObject.AddComponent<Canvas>();
                    reAuthCanvas.overrideSorting = true;
                    reAuthCanvas.sortingOrder    = 200;

                    if (reAuthUI.GetComponent<GraphicRaycaster>() == null)
                        reAuthUI.gameObject.AddComponent<GraphicRaycaster>();

                    reAuthUI.ShowReAuthPanel(currentUserData.Email, async () =>
                    {
                        Debug.Log("[DeleteAccount] Reautenticação bem-sucedida — repetindo fluxo completo...");
                        await DeleteAccountAsync(isRetry: true);
                    });
                }
                else
                {
                    Debug.LogError("[DeleteAccount] ReAuthUI não atribuído — não é possível solicitar reautenticação.");
                }

                return; // navegação ocorrerá na chamada com isRetry=true
            }

            // ----------------------------------------------------------------
            // Fase 3 — Limpeza de UI e estado local, depois navegar para Login
            // Cada passo de cleanup é isolado para que uma falha de UI não
            // impeça a navegação para LoginView.
            // ----------------------------------------------------------------
            SetDeleteFeedback("Até a próxima!");

            try { deleteAccountPanel.HidePanel(); }
            catch (Exception ex) { Debug.LogWarning($"[DeleteAccount] HidePanel: {ex.Message}"); }

            try { CleanupOverlays(); }
            catch (Exception ex) { Debug.LogWarning($"[DeleteAccount] CleanupOverlays: {ex.Message}"); }

            try { ReenableSceneInteractions(); }
            catch (Exception ex) { Debug.LogWarning($"[DeleteAccount] ReenableSceneInteractions: {ex.Message}"); }

            // Limpa dados locais
            UserDataStore.OnUserDataChanged -= OnUserDataChanged;
            UserDataStore.CurrentUserData    = null;
            AppContext.AnsweredQuestions?.ResetManager();
            AnsweredQuestionsListStore.ClearAll();

            Debug.Log("[DeleteAccount] === LIMPEZA COMPLETA FINALIZADA ===");

            loadingSpinner?.HideSpinner();
            Navigate("LoginView");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DeleteAccount] Erro inesperado: {ex.Message}");
            if (deleteAccountButton != null)
                deleteAccountButton.interactable = true;
            SetDeleteFeedback("Erro ao deletar. Tente novamente.");
            loadingSpinner?.HideSpinner();
        }
    }

    // -------------------------------------------------------
    // Helpers privados
    // -------------------------------------------------------

    /// <summary>
    /// Exibe mensagem de status abaixo dos botões do painel de deleção.
    /// Passa string vazia para limpar o texto.
    /// </summary>
    private void SetDeleteFeedback(string message)
    {
        if (deleteAccountFeedbackText != null)
            deleteAccountFeedbackText.text = message;
    }

    private void CleanupOverlays()
    {
        if (deleteAccountDarkOverlay != null)
        {
            Canvas overlayCanvas = deleteAccountDarkOverlay.GetComponent<Canvas>();
            if (overlayCanvas != null) Destroy(overlayCanvas);
            deleteAccountDarkOverlay.SetActive(false);
        }

        GameObject halfViewOverlay = GameObject.Find("HalfViewDarkOverlay");
        if (halfViewOverlay != null) halfViewOverlay.SetActive(false);

        string[] overlayNames = { "DarkOverlay", "DeleteAccountDarkOverlay", "HalfViewDarkOverlay", "Overlay", "BlockerPanel" };
        foreach (string overlayName in overlayNames)
        {
            GameObject overlay = GameObject.Find(overlayName);
            if (overlay != null && overlay.activeSelf)
            {
                Canvas canvas = overlay.GetComponent<Canvas>();
                if (canvas != null) canvas.enabled = false;
                overlay.SetActive(false);
            }
        }

        Canvas[] allCanvases = FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.sortingOrder >= 100 && canvas.gameObject.name.Contains("Dark"))
                canvas.gameObject.SetActive(false);
        }
    }

    private void ReenableSceneInteractions()
    {
        Selectable[] selectables = FindObjectsByType<Selectable>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
        foreach (Selectable selectable in selectables)
        {
            if (!ShouldStayDisabled(selectable.gameObject))
                selectable.interactable = true;
        }
        Debug.Log("Todas as interações da cena reabilitadas");
    }

    private bool ShouldStayDisabled(GameObject obj) => false;
}