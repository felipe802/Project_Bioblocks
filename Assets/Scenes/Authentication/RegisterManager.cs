using Firebase;
using Firebase.Auth;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;

public class RegisterManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField nickNameInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button backButton;
    [SerializeField] private FeedbackManager feedbackManager;
    [SerializeField] private LoadingSpinnerComponent loadingSpinner;

    private IAuthRepository    _auth;
    private IFirestoreRepository _firestore;
    private INavigationService _navigation;

    private bool isProcessing = false;

    // Usado apenas em AssignRandomDefaultAvatar. System.Random em vez de UnityEngine.Random
    // porque o sorteio roda em background thread (após a cadeia de ConfigureAwait(false)
    // em HandleRegistration), e APIs da Unity — incluindo Random.Range — exigem main thread.
    private static readonly System.Random _avatarRng = new System.Random();

    private void Start()
    {
        _auth        = AppContext.Auth;
        _firestore   = AppContext.Firestore;
        _navigation  = AppContext.Navigation;

        nickNameInput.contentType    = TMP_InputField.ContentType.Standard;
        nickNameInput.characterLimit = 15;
        nickNameInput.onValueChanged.AddListener(ValidateNickname);
        registerButton.onClick.AddListener(HandleRegistration);
    }

    // -------------------------------------------------------
    // Registro
    // -------------------------------------------------------

    public async void HandleRegistration()
    {
        if (isProcessing) return;

        // Validação síncrona antes do try — garante feedback imediato na main thread
        // sem passar pelo catch/MainThreadDispatcher
        if (string.IsNullOrEmpty(nickNameInput.text) ||
            string.IsNullOrEmpty(nameInput.text)     ||
            string.IsNullOrEmpty(emailInput.text)    ||
            string.IsNullOrEmpty(passwordInput.text))
        {
            feedbackManager.ShowFeedback("Todos os campos são obrigatórios.", true);
            return;
        }

        isProcessing = true;
        SetAllButtonsInteractable(false);
        loadingSpinner?.ShowSpinner();

        try
        {
            bool nicknameExists = await _firestore.AreNicknameTaken(nickNameInput.text).ConfigureAwait(false);
            await Task.Yield();

            if (nicknameExists)
                throw new Exception("Este nickname já está em uso. Por favor, escolha outro.");

            Debug.Log("=== LIMPEZA ANTES DE REGISTRAR NOVO USUÁRIO ===");

            UserDataStore.CurrentUserData = null;
            Debug.Log("✓ UserDataStore limpo");
            AppContext.AnsweredQuestions?.ResetManager();
            AnsweredQuestionsListStore.ClearAll();
            Debug.Log("=== LIMPEZA CONCLUÍDA, INICIANDO REGISTRO ===");

            await _auth.RegisterUserAsync(
                nameInput.text,
                nickNameInput.text,
                emailInput.text,
                passwordInput.text
            ).ConfigureAwait(false);
            await Task.Yield();
            await Task.Delay(300);

            string userId = _auth.CurrentUserId;
            if (string.IsNullOrEmpty(userId))
                throw new Exception("Erro: usuário criado mas ID não encontrado.");

            var userData = await _firestore.GetUserData(userId).ConfigureAwait(false);
            await Task.Yield(); // retorna ao main thread

            if (userData == null)
                throw new Exception("Erro ao carregar dados do usuário recém-criado.");

            UserDataStore.CurrentUserData = userData;
            Debug.Log("[RegisterManager] UserData definido. Iniciando ForceUpdate...");
            await Task.Delay(300);
            await AppContext.AnsweredQuestions.ForceUpdate().ConfigureAwait(false);
            Debug.Log("[RegisterManager] ForceUpdate concluído.");

            // Atribui um avatar aleatório em background (não bloqueia navegação)
            AssignRandomDefaultAvatar(userData);

            Debug.Log("[RegisterManager] Enfileirando LoadScene na main thread...");

            // Task.Yield() não garante retorno à main thread quando ConfigureAwait(false)
            // foi usado anteriormente na cadeia. MainThreadDispatcher.Enqueue garante.
            MainThreadDispatcher.Enqueue(() =>
            {
                Debug.Log("[RegisterManager] Carregando PathwayScene na main thread...");
                loadingSpinner?.ShowSpinnerUntilSceneLoaded("PathwayScene");
                _navigation.NavigateTo("PathwayScene");
            });
        }
        catch (FirebaseException e)
        {
            string errorMessage = GetFirebaseAuthErrorMessage(e);
            Debug.LogWarning($"[RegisterManager] {errorMessage}");
            feedbackManager.ShowFeedback(errorMessage, true);
            loadingSpinner?.HideSpinner();
            SetAllButtonsInteractable(true);
            isProcessing = false;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[RegisterManager] {e.Message}");
            feedbackManager.ShowFeedback(e.Message, true);
            loadingSpinner?.HideSpinner();
            SetAllButtonsInteractable(true);
            isProcessing = false;
        }
    }

    // -------------------------------------------------------
    // Avatar padrão aleatório
    // -------------------------------------------------------

    /// <summary>
    /// Sorteia um avatar preset default (um por classe biológica) e associa ao usuário.
    /// Fluxo 100% por referência: persiste apenas a string "preset:&lt;id&gt;" no Firestore
    /// e no UserData local. O PNG físico já está bundled em Resources — nenhum upload
    /// para Storage, nenhum arquivo temporário em disco.
    /// Roda em background — não bloqueia a navegação para PathwayScene.
    /// </summary>
    private async void AssignRandomDefaultAvatar(UserData userData)
    {
        try
        {
            var defaults = AvatarCatalog.Defaults;
            if (defaults.Count == 0)
            {
                Debug.LogWarning("[RegisterManager] AvatarCatalog.Defaults vazio — avatar padrão não atribuído.");
                return;
            }

            var chosen    = defaults[_avatarRng.Next(defaults.Count)];
            string presetUrl = $"preset:{chosen.Id}";
            Debug.Log($"[RegisterManager] Avatar padrão sorteado: {chosen.Id} ({chosen.DisplayName})");

            // Persiste no Firestore — string curta, sem Storage envolvido.
            // Falha aqui não impede atualização local; o usuário vê o avatar correto
            // e o PendingUploadSyncService (ou o próximo login) reconciliará.
            try
            {
                await _firestore.UpdateUserProfileImageUrl(userData.UserId, presetUrl).ConfigureAwait(false);
                Debug.Log("[RegisterManager] Firestore atualizado com preset:id");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RegisterManager] Firestore update falhou: {e.Message}");
            }

            // Volta à main thread para mexer em stores e notificar UI (TopBar, etc).
            MainThreadDispatcher.Enqueue(() =>
            {
                userData.ProfileImageUrl      = presetUrl;
                UserDataStore.CurrentUserData = userData;
                AppContext.UserDataLocal?.UpdateUser(userData);
                UserAvatarSyncHelper.NotifyAvatarChanged(presetUrl);
                Debug.Log("[RegisterManager] Avatar padrão aplicado com sucesso");
            });
        }
        catch (Exception e)
        {
            // Falha no avatar padrão não deve impedir o uso do app
            Debug.LogWarning($"[RegisterManager] Erro ao atribuir avatar padrão: {e.Message}");
        }
    }

    // -------------------------------------------------------
    // Navegação
    // -------------------------------------------------------

    public void SceneLoader()
    {
        if (isProcessing) return;

        isProcessing = true;
        SetAllButtonsInteractable(false);
        loadingSpinner?.ShowSpinnerUntilSceneLoaded("LoginView");
        _navigation.NavigateTo("LoginView");
    }

    // -------------------------------------------------------
    // Validação
    // -------------------------------------------------------

    private void ValidateNickname(string value)
    {
        if (value.Length < 3)
            feedbackManager.ShowFeedback("Nickname deve possuir mais de 3 caracteres.", true);
        else
            feedbackManager.HideFeedback();
    }

    // -------------------------------------------------------
    // UI helpers
    // -------------------------------------------------------

    private void SetAllButtonsInteractable(bool interactable)
    {
        registerButton.interactable = interactable;
        if (backButton != null) backButton.interactable = interactable;
        nameInput.interactable     = interactable;
        nickNameInput.interactable = interactable;
        emailInput.interactable    = interactable;
        passwordInput.interactable = interactable;
    }

    // -------------------------------------------------------
    // Tradução de erros Firebase
    // Isolado aqui — se o SDK mudar, só este método é afetado
    // -------------------------------------------------------

    private string GetFirebaseAuthErrorMessage(FirebaseException e)
    {
        var errorCode = (int)e.ErrorCode;
        return errorCode switch
        {
            (int)AuthError.EmailAlreadyInUse => "Email já registrado.",
            (int)AuthError.WeakPassword      => "Senha muito fraca.",
            _                                => e.Message
        };
    }
}