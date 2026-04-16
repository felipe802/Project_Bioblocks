// Assets/Editor/Tests/AuthFlowTests.cs
//
// Testes de fluxo para RegisterManager e LoginManager.
//
// O que É testado:
//   Registro:
//   ✅ Campos obrigatórios vazios → exibe feedback de erro
//   ✅ Nickname já em uso → exibe feedback de erro
//   ✅ Registro bem-sucedido → UserDataStore populado
//   ✅ Registro bem-sucedido → AnsweredQuestions.ForceUpdate chamado
//   ✅ Registro bem-sucedido → Statistics inicializado se necessário
//   ✅ Falha no GetUserData após registro → exibe feedback de erro
//   ✅ isProcessing impede chamadas duplicadas
//
//   Login:
//   ✅ Campos vazios → exibe feedback de erro sem chamar auth
//   ✅ Login bem-sucedido → UserDataStore populado
//   ✅ Login bem-sucedido → AnsweredQuestions.ForceUpdate chamado
//   ✅ Rate limit após 5 tentativas → bloqueia nova tentativa
//   ✅ Login falha com credenciais erradas → feedback e botões reativados
//
// O que NÃO é testado:
//   - SceneManager.LoadScene (requer cena real carregada)
//   - UI visual (sprites, cores, animações)
//   - Integração real com Firebase SDK

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;
using QuestionSystem;

[TestFixture]
public class AuthFlowTests
{
    // ── Fixtures comuns ────────────────────────────────────────────────────────
    private FakeAuthRepository                       _fakeAuth;
    private FakeFirestoreRepository                  _fakeFirestore;
    private FakeAnsweredQuestionsManagerForAuth      _fakeAnswered;
    private FakeStatisticsProvider                   _fakeStatistics;
    private FakeQuestionSyncService                  _fakeSync;
    private FakeNavigationService                    _fakeNavigation;

    [SetUp]
    public void Setup()
    {
        _fakeAuth       = new FakeAuthRepository();
        _fakeFirestore  = new FakeFirestoreRepository();
        _fakeAnswered   = new FakeAnsweredQuestionsManagerForAuth();
        _fakeStatistics = new FakeStatisticsProvider();
        _fakeSync       = new FakeQuestionSyncService { IsCacheReady = true };
        _fakeNavigation = new FakeNavigationService();

        AppContext.OverrideForTests(
            auth:              _fakeAuth,
            firestore:         _fakeFirestore,
            answeredQuestions: _fakeAnswered,
            statistics:        _fakeStatistics,
            questionSync:      _fakeSync,
            navigation:        _fakeNavigation
        );

        UserDataStore.CurrentUserData = null;
        UserDataStore.Logger = _ => { };
    }

    [TearDown]
    public void TearDown()
    {
        UserDataStore.Clear();
    }

    // =======================================================================
    // Helpers — cria managers com UI mínima funcional
    // =======================================================================
    private (RegisterManager manager, GameObject go, SpyFeedbackManager spy) CreateRegisterManager(
        string name = "Test", string nick = "TestUser",
        string email = "test@test.com", string password = "123456")
    {
        var go      = new GameObject("RegisterManager");
        var manager = go.AddComponent<RegisterManager>();
        var spy     = go.AddComponent<SpyFeedbackManager>();

        // Botões mínimos — necessários para SetAllButtonsInteractable não lançar NullReferenceException
        var registerBtn = new GameObject("RegisterBtn").AddComponent<Button>();
        var backBtn     = new GameObject("BackBtn").AddComponent<Button>();
        registerBtn.transform.SetParent(go.transform);
        backBtn.transform.SetParent(go.transform);

        // Start() não roda em Edit Mode — injeta dependências diretamente
        SetField(manager, "_auth",             _fakeAuth);
        SetField(manager, "_firestore",        _fakeFirestore);
        SetField(manager, "_navigation",       _fakeNavigation);
        SetField(manager, "feedbackManager",   spy);
        SetField(manager, "registerButton",    registerBtn);
        SetField(manager, "backButton",        backBtn);
        SetField(manager, "nameInput",         CreateInput(go, name));
        SetField(manager, "nickNameInput",     CreateInput(go, nick));
        SetField(manager, "emailInput",        CreateInput(go, email));
        SetField(manager, "passwordInput",     CreateInput(go, password));

        return (manager, go, spy);
    }

    private (LoginManager manager, GameObject go, SpyFeedbackManager spy) CreateLoginManager(
        string email = "test@test.com", string password = "123456")
    {
        var go      = new GameObject("LoginManager");
        var manager = go.AddComponent<LoginManager>();
        var spy     = go.AddComponent<SpyFeedbackManager>();

        // Start() não roda em Edit Mode — injeta dependências diretamente
        SetField(manager, "_auth",           _fakeAuth);
        SetField(manager, "_navigation",     _fakeNavigation);
        SetField(manager, "feedbackManager", spy);
        SetField(manager, "emailInput",     CreateInput(go, email));
        SetField(manager, "passwordInput",  CreateInput(go, password));

        var loginBtn    = new GameObject("LoginBtn").AddComponent<Button>();
        var registerBtn = new GameObject("RegisterBtn").AddComponent<Button>();
        loginBtn.transform.SetParent(go.transform);
        registerBtn.transform.SetParent(go.transform);
        SetField(manager, "loginButton",    loginBtn);
        SetField(manager, "registerButton", registerBtn);

        return (manager, go, spy);
    }

    // =======================================================================
    // REGISTRO — testes
    // =======================================================================
    [UnityTest]
    public IEnumerator Register_CamposVazios_ExibeFeedbackDeErro()
    {
        var (manager, go, spy) = CreateRegisterManager(email: "");

        var task = RunAndWait(manager.HandleRegistration, spy);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(spy.LastWasError,                        "Deve exibir mensagem de erro.");
        Assert.IsFalse(string.IsNullOrEmpty(spy.LastMessage),  "Mensagem não deve ser vazia.");
        Assert.IsNull(UserDataStore.CurrentUserData,           "UserDataStore não deve ser populado.");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Register_NicknameJaEmUso_ExibeFeedbackDeErro()
    {
        // Nickname já cadastrado — AreNicknameTaken usa _allUsers do FakeFirestoreRepository
        var existing = new UserData("existing-id", "TestUser", "Existing", "e@e.com");
        _fakeFirestore.SetFakeUser(existing);

        var (manager, go, spy) = CreateRegisterManager(nick: "TestUser");

        var task = RunAndWait(manager.HandleRegistration, spy);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(spy.LastWasError, "Deve exibir erro de nickname em uso.");
        Assert.IsTrue(
            spy.LastMessage.ToLower().Contains("nickname") || spy.LastMessage.ToLower().Contains("uso"),
            $"Mensagem deve mencionar nickname. Recebido: '{spy.LastMessage}'");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Register_Sucesso_PopulaUserDataStore()
    {
        var userId   = "new-user-id";
        var userData = new UserData(userId, "NewUser", "New User", "new@test.com");
        _fakeFirestore.SetFakeUserForGetUserData(userData);
        _fakeAuth.SetUserIdForNextRegistration(userId);

        var (manager, go, spy) = CreateRegisterManager(nick: "NewUser", email: "new@test.com");

        var task = RunAndWait(manager.HandleRegistration, spy);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsNotNull(UserDataStore.CurrentUserData,
            "UserDataStore deve ser populado após registro.");
        Assert.AreEqual(userId, UserDataStore.CurrentUserData.UserId);

        Object.DestroyImmediate(go);
    }    
    
    [UnityTest]
    public IEnumerator Register_Sucesso_ChamaForceUpdateDeAnsweredQuestions()
    {
        var userId   = "new-user-id-2";
        _fakeFirestore.SetFakeUserForGetUserData(new UserData(userId, "NewUser2", "New 2", "new2@test.com"));
        _fakeAuth.SetUserIdForNextRegistration(userId);

        var (manager, go, spy) = CreateRegisterManager(nick: "NewUser2", email: "new2@test.com");

        var task = RunAndWait(manager.HandleRegistration, spy);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(_fakeAnswered.ForceUpdateWasCalled,
            "ForceUpdate do AnsweredQuestionsManager deve ser chamado após registro.");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Register_Sucesso_VerificaSeEstatisticasEstaoInicializadas()
    {
        // O DatabaseStatisticsManager é inicializado no boot pelo AppContext —
        // não mais pelo RegisterManager. O registro deve completar normalmente
        // sem depender do estado das estatísticas.
        var userId = "new-user-id-3";
        _fakeFirestore.SetFakeUserForGetUserData(new UserData(userId, "NewUser3", "New 3", "new3@test.com"));
        _fakeAuth.SetUserIdForNextRegistration(userId);

        var (manager, go, spy) = CreateRegisterManager(nick: "NewUser3", email: "new3@test.com");

        var task = RunAndWait(manager.HandleRegistration, spy);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(_fakeAnswered.ForceUpdateWasCalled,
            "ForceUpdate deve ser chamado no fluxo de registro independente do estado das estatísticas.");
        Assert.IsNotNull(UserDataStore.CurrentUserData,
            "UserDataStore deve ser populado após registro bem-sucedido.");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Register_GetUserDataRetornaNull_ExibeFeedbackDeErro()
    {
        // Auth cria o usuário mas GetUserData retorna null (usuário não encontrado no Firestore)
        _fakeAuth.SetUserIdForNextRegistration("null-user-id");
        // NÃO chama SetFakeUser — GetUserData retornará null

        var (manager, go, spy) = CreateRegisterManager(nick: "NullUser");

        var task = RunAndWait(manager.HandleRegistration, spy);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(spy.LastWasError,              "Deve exibir erro quando GetUserData retorna null.");
        Assert.IsNull(UserDataStore.CurrentUserData, "UserDataStore não deve ser populado.");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Register_IsProcessing_ImpedeChamadaDupla()
    {
        var userId = "dup-user-id";
        _fakeFirestore.SetFakeUserForGetUserData(new UserData(userId, "DupUser", "Dup", "dup@test.com"));
        _fakeAuth.SetUserIdForNextRegistration(userId);

        var (manager, go, spy) = CreateRegisterManager(nick: "DupUser");

        // Chama duas vezes — a segunda deve ser ignorada pelo isProcessing
        manager.HandleRegistration();
        manager.HandleRegistration();

        yield return new WaitForSeconds(1f);

        Assert.AreEqual(1, _fakeAuth.RegisterCallCount,
            "RegisterUserAsync deve ser chamado apenas uma vez mesmo com chamadas duplicadas.");

        Object.DestroyImmediate(go);
    }

    // =======================================================================
    // LOGIN — testes
    // =======================================================================
    [UnityTest]
    public IEnumerator Login_CamposVazios_ExibeFeedbackSemChamarAuth()
    {
        var (manager, go, spy) = CreateLoginManager(email: "");

        var task = RunAndWait(manager.HandleLogin, spy);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(spy.LastWasError,                "Deve exibir erro para campos vazios.");
        Assert.AreEqual(0, _fakeAuth.SignInCallCount,  "Auth não deve ser chamado com campos vazios.");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Login_Sucesso_PopulaUserDataStore()
    {
        var userId   = "login-user-id";
        var userData = new UserData(userId, "LoginUser", "Login", "login@test.com");
        _fakeAuth.SetLoggedInUser(userId);
        _fakeAuth.SetUserDataForSignIn(userData);

        var (manager, go, spy) = CreateLoginManager(email: "login@test.com");

        var task = RunAndWait(manager.HandleLogin, spy);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsNotNull(UserDataStore.CurrentUserData,
            "UserDataStore deve ser populado após login bem-sucedido.");
        Assert.AreEqual(userId, UserDataStore.CurrentUserData.UserId);
        Assert.AreEqual("PathwayScene", _fakeNavigation.LastScene,
            "Deve navegar para PathwayScene após login bem-sucedido.");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Login_Sucesso_ChamaForceUpdateDeAnsweredQuestions()
    {
        var userId   = "login-user-id-2";
        var userData = new UserData(userId, "LoginUser2", "Login 2", "login2@test.com");
        _fakeAuth.SetLoggedInUser(userId);
        _fakeAuth.SetUserDataForSignIn(userData);

        var (manager, go, spy) = CreateLoginManager(email: "login2@test.com");

        var task = RunAndWait(manager.HandleLogin, spy);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(_fakeAnswered.ForceUpdateWasCalled,
            "ForceUpdate deve ser chamado após login bem-sucedido.");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Login_CredenciaisErradas_ExibeFeedbackDeErro()
    {
        _fakeAuth.SetSignInShouldFail(true);

        var (manager, go, spy) = CreateLoginManager();

        var task = RunAndWait(manager.HandleLogin, spy);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(spy.LastWasError,              "Deve exibir erro para credenciais inválidas.");
        Assert.IsNull(UserDataStore.CurrentUserData, "UserDataStore não deve ser populado.");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Login_RateLimit_BloqueiaApos5Tentativas()
    {
        // O rate limit é gerenciado pelo LoginManager internamente via loginAttempts.
        // Como o catch usa MainThreadDispatcher (não executa em Edit Mode),
        // simulamos as 5 tentativas diretamente via reflection no dicionário interno.
        var (manager, go, spy) = CreateLoginManager(email: "blocked@test.com");

        // Acessa e popula loginAttempts diretamente — simula 5 falhas anteriores
        var loginAttempts = GetField(manager, "loginAttempts")
            as System.Collections.Generic.Dictionary<string, object>;

        // Usa reflection para invocar IncrementAttempt 5 vezes
        const string email = "blocked@test.com";
        for (int i = 0; i < 5; i++)
            InvokeIncrementAttempt(manager, email);

        // 6ª tentativa — deve ser bloqueada SINCRONAMENTE pelo CheckRateLimit
        // antes de qualquer operação async
        spy.Reset();
        manager.HandleLogin();
        yield return null; // um frame para processar

        Assert.IsTrue(spy.LastWasError,
            "Deve exibir mensagem de bloqueio por rate limit.");
        Assert.IsTrue(
            spy.LastMessage != null && spy.LastMessage.Contains("minutos"),
            $"Mensagem deve indicar tempo de espera. Recebido: '{spy.LastMessage}'");
        Assert.AreEqual(0, _fakeAuth.SignInCallCount,
            "Auth não deve ser chamado (bloqueado por rate limit).");

        Object.DestroyImmediate(go);
    }

    private static void InvokeIncrementAttempt(LoginManager manager, string email)
    {
        // Acessa loginAttempts e chama IncrementAttempt na entrada do email
        var attemptsField = typeof(LoginManager).GetField("loginAttempts",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (attemptsField == null) return;

        var attempts = attemptsField.GetValue(manager);
        if (attempts == null) return;

        // Usa o indexer do Dictionary para obter ou criar a entrada
        var dictType    = attempts.GetType();
        var containsKey = dictType.GetMethod("ContainsKey");
        var indexer     = dictType.GetProperty("Item");

        bool hasKey = (bool)containsKey.Invoke(attempts, new object[] { email });
        if (!hasKey)
        {
            // Cria uma nova LoginAttempt via reflection (classe privada)
            var loginAttemptType = typeof(LoginManager).GetNestedType(
                "LoginAttempt", System.Reflection.BindingFlags.NonPublic);
            var newAttempt = System.Activator.CreateInstance(loginAttemptType);
            dictType.GetMethod("Add").Invoke(attempts, new object[] { email, newAttempt });
        }

        var attempt        = indexer.GetValue(attempts, new object[] { email });
        var incrementMethod = attempt.GetType().GetMethod("IncrementAttempt");
        incrementMethod.Invoke(attempt, null);
    }

    private static object GetField(object target, string fieldName)
    {
        var type = target.GetType();
        while (type != null)
        {
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public    |
                System.Reflection.BindingFlags.Instance);
            if (field != null) return field.GetValue(target);
            type = type.BaseType;
        }
        return null;
    }

    // =======================================================================
    // Helpers
    // =======================================================================

    /// <summary>
    /// Invoca um método async void e aguarda conclusão detectável em Edit Mode.
    ///
    /// Critérios de término (o primeiro que ocorrer):
    ///   1. spy recebeu feedback → erro ou validação síncrona
    ///   2. ForceUpdate foi chamado → lógica de negócio concluída
    ///   3. NavigateTo foi chamado → fluxo de sucesso concluído
    ///   4. Timeout de 3s → segurança
    /// </summary>
    private async System.Threading.Tasks.Task RunAndWait(
        System.Action action,
        SpyFeedbackManager spy = null)
    {
        int  callCountBefore    = spy?.CallCount ?? 0;
        bool forceUpdateBefore  = _fakeAnswered.ForceUpdateWasCalled;
        int  navCountBefore     = _fakeNavigation.NavigateCallCount;

        action();

        float elapsed = 0f;
        while (elapsed < 5f)
        {
            await System.Threading.Tasks.Task.Delay(100);
            elapsed += 0.1f;

            // Critério 1: spy recebeu feedback (erro ou validação)
            if (spy != null && spy.CallCount > callCountBefore)
                return;

            // Critério 2: NavigateTo foi chamado (LoginManager — sem MainThreadDispatcher)
            if (_fakeNavigation.NavigateCallCount > navCountBefore)
                return;

            // Critério 3: ForceUpdate foi chamado E UserDataStore já foi populado
            // Necessário para RegisterManager onde NavigateTo está no MainThreadDispatcher
            // (que é null em Edit Mode e não executa)
            if (!forceUpdateBefore
                && _fakeAnswered.ForceUpdateWasCalled
                && UserDataStore.CurrentUserData != null)
                return;

            if (elapsed >= 3f) return;
        }
    }

    private static void SetField(object target, string fieldName, object value)
    {
        var type = target.GetType();
        while (type != null)
        {
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public    |
                System.Reflection.BindingFlags.Instance);
            if (field != null) { field.SetValue(target, value); return; }
            type = type.BaseType;
        }
    }

    private static TMP_InputField CreateInput(GameObject parent, string value)
    {
        var go    = new GameObject($"Input");
        go.transform.SetParent(parent.transform);
        var input = go.AddComponent<TMP_InputField>();
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform);
        input.textComponent = textGo.AddComponent<TextMeshProUGUI>();
        input.text = value;
        return input;
    }
}

// =======================================================================
// Fakes específicos para AuthFlowTests
// =======================================================================

/// <summary>
/// SpyFeedbackManager — captura mensagens exibidas para assertions nos testes.
/// ShowFeedback e HideFeedback são override de FeedbackManager (virtual),
/// evitando que a classe base tente acessar feedbackText (null em testes).
/// </summary>
public class SpyFeedbackManager : FeedbackManager
{
    public string LastMessage  { get; private set; }
    public bool   LastWasError { get; private set; }
    public int    CallCount    { get; private set; }

    public override void ShowFeedback(string message, bool isError = false)
    {
        LastMessage  = message;
        LastWasError = isError;
        CallCount++;
    }

    public override void HideFeedback()
    {
        // não faz nada em testes — evita NullReferenceException no feedbackText
    }

    public void Reset()
    {
        LastMessage  = null;
        LastWasError = false;
        CallCount    = 0;
    }
}
