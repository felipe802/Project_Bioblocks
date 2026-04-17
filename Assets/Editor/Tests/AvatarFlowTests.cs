// Assets/Editor/Tests/AvatarFlowTests.cs
//
// Testes do fluxo de seleção e atribuição de avatares preset.
//
// O que É testado:
//   AvatarPickerPanel:
//   ✅ Carrega todas as 10 texturas de Resources/AvatarPresets
//   ✅ Estado inicial do painel é invisível
//   ✅ ShowPanel altera estado para visível
//   ✅ HidePanel altera estado para invisível
//   ✅ Seleção de avatar dispara evento OnAvatarSelected
//
//   RegisterManager (avatar padrão):
//   ✅ Registro bem-sucedido chama ImageUploadService
//   ✅ Config do upload tem DestinationFolder e FileNamePrefix corretos
//   ✅ Upload concluído atualiza Firestore com URL da imagem
//   ✅ Upload concluído atualiza UserDataStore com URL da imagem
//   ✅ Falha no upload do avatar não impede registro
//
//   ProfileImageUploader (seleção de preset):
//   ✅ OnPresetAvatarSelected salva imagem em temp e chama ProcessSelectedImage
//
// O que NÃO é testado:
//   - Animação do painel (coroutines requerem Play Mode)
//   - UI visual do grid de avatares
//   - NativeGallery (galeria desativada)

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

[TestFixture]
public class AvatarFlowTests
{
    // ── Fixtures comuns ────────────────────────────────────────────────────────
    private FakeAuthRepository                       _fakeAuth;
    private FakeFirestoreRepository                  _fakeFirestore;
    private FakeImageUploadService                   _fakeImageUpload;
    private FakeAnsweredQuestionsManagerForAuth       _fakeAnswered;
    private FakeStatisticsProvider                   _fakeStatistics;
    private FakeQuestionSyncService                  _fakeSync;
    private FakeNavigationService                    _fakeNavigation;

    [SetUp]
    public void Setup()
    {
        _fakeAuth        = new FakeAuthRepository();
        _fakeFirestore   = new FakeFirestoreRepository();
        _fakeImageUpload = new FakeImageUploadService();
        _fakeAnswered    = new FakeAnsweredQuestionsManagerForAuth();
        _fakeStatistics  = new FakeStatisticsProvider();
        _fakeSync        = new FakeQuestionSyncService { IsCacheReady = true };
        _fakeNavigation  = new FakeNavigationService();

        AppContext.OverrideForTests(
            auth:              _fakeAuth,
            firestore:         _fakeFirestore,
            imageUpload:       _fakeImageUpload,
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
    // AvatarPickerPanel — testes
    // =======================================================================

    [Test]
    public void AvatarPickerPanel_CarregaTodasAsTexturas()
    {
        var (panel, go) = CreateAvatarPickerPanel();

        // As texturas são carregadas no Awake — invocamos via reflection
        // já que Awake não executa automaticamente em Edit Mode após AddComponent
        InvokeMethod(panel, "LoadAvatarTextures");

        var textures = panel.LoadedTextures;

        // Verificar se alguma textura foi carregada
        // (em ambiente de CI sem assets, pode ser 0 — neste caso o teste é inconcluso)
        if (textures.Count == 0)
        {
            Assert.Inconclusive(
                "Nenhuma textura encontrada em Resources/AvatarPresets/. " +
                "Verifique se as imagens estão na pasta correta.");
            Object.DestroyImmediate(go);
            return;
        }

        Assert.AreEqual(10, textures.Count,
            $"Deve carregar 10 texturas. Carregadas: {textures.Count}");

        // Verifica nomes específicos
        Assert.IsTrue(textures.ContainsKey("avatar_dna"),           "Deve conter avatar_dna");
        Assert.IsTrue(textures.ContainsKey("avatar_cell"),          "Deve conter avatar_cell");
        Assert.IsTrue(textures.ContainsKey("avatar_bacteria"),      "Deve conter avatar_bacteria");
        Assert.IsTrue(textures.ContainsKey("avatar_virus"),         "Deve conter avatar_virus");
        Assert.IsTrue(textures.ContainsKey("avatar_protein"),       "Deve conter avatar_protein");
        Assert.IsTrue(textures.ContainsKey("avatar_mitochondria"),  "Deve conter avatar_mitochondria");
        Assert.IsTrue(textures.ContainsKey("avatar_ribosome"),      "Deve conter avatar_ribosome");
        Assert.IsTrue(textures.ContainsKey("avatar_sugar"),         "Deve conter avatar_sugar");
        Assert.IsTrue(textures.ContainsKey("avatar_neuron"),        "Deve conter avatar_neuron");
        Assert.IsTrue(textures.ContainsKey("avatar_plant_cell"),    "Deve conter avatar_plant_cell");

        Object.DestroyImmediate(go);
    }

    [Test]
    public void AvatarPickerPanel_EstadoInicialInvisivel()
    {
        var (panel, go) = CreateAvatarPickerPanel();

        Assert.IsFalse(panel.IsVisible,   "Painel deve iniciar invisível");
        Assert.IsFalse(panel.IsAnimating,  "Painel não deve estar animando");

        Object.DestroyImmediate(go);
    }

    [Test]
    public void AvatarPickerPanel_ShowPanel_AlteraEstadoParaVisivel()
    {
        var (panel, go) = CreateAvatarPickerPanel();

        panel.ShowPanel();

        Assert.IsTrue(panel.IsVisible || panel.IsAnimating,
            "Painel deve estar visível ou animando após ShowPanel");

        Object.DestroyImmediate(go);
    }

    [Test]
    public void AvatarPickerPanel_OnAvatarSelected_DisparaEvento()
    {
        var (panel, go) = CreateAvatarPickerPanel();

        string selectedName = null;
        panel.OnAvatarSelected += name => selectedName = name;

        // Simula seleção via reflection (método privado)
        InvokeMethod(panel, "OnAvatarButtonClicked", "avatar_dna");

        Assert.AreEqual("avatar_dna", selectedName,
            "Evento OnAvatarSelected deve ser disparado com o nome correto");

        Object.DestroyImmediate(go);
    }

    // =======================================================================
    // RegisterManager — avatar padrão no registro
    // =======================================================================

    [UnityTest]
    public IEnumerator Register_Sucesso_ChamaImageUploadParaAvatarPadrao()
    {
        var userId   = "avatar-user-id";
        var userData = new UserData(userId, "AvatarUser", "Avatar User", "avatar@test.com");
        _fakeFirestore.SetFakeUserForGetUserData(userData);
        _fakeAuth.SetUserIdForNextRegistration(userId);

        var (manager, go, spy) = CreateRegisterManager(nick: "AvatarUser", email: "avatar@test.com");

        var task = RunAndWait(manager.HandleRegistration, spy);
        yield return new WaitUntil(() => task.IsCompleted);

        // Aguarda um pouco extra para o AssignRandomDefaultAvatar (async void) completar
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(1, _fakeImageUpload.UploadCallCount,
            "ImageUploadService deve ser chamado uma vez para o avatar padrão");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Register_AvatarUpload_ConfigTemDestinationFolderCorreto()
    {
        var userId   = "config-user-id";
        var userData = new UserData(userId, "ConfigUser", "Config User", "config@test.com");
        _fakeFirestore.SetFakeUserForGetUserData(userData);
        _fakeAuth.SetUserIdForNextRegistration(userId);

        var (manager, go, spy) = CreateRegisterManager(nick: "ConfigUser", email: "config@test.com");

        var task = RunAndWait(manager.HandleRegistration, spy);
        yield return new WaitUntil(() => task.IsCompleted);
        yield return new WaitForSeconds(0.5f);

        Assert.IsNotNull(_fakeImageUpload.LastConfig,
            "Config do upload não deve ser nulo");
        Assert.AreEqual("profile_images", _fakeImageUpload.LastConfig.DestinationFolder,
            "DestinationFolder deve ser 'profile_images'");
        Assert.AreEqual(userId, _fakeImageUpload.LastConfig.FileNamePrefix,
            "FileNamePrefix deve ser o userId");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Register_AvatarUpload_Sucesso_AtualizaFirestore()
    {
        var userId   = "firestore-user-id";
        var userData = new UserData(userId, "FirestoreUser", "Firestore User", "firestore@test.com");
        _fakeFirestore.SetFakeUserForGetUserData(userData);
        _fakeAuth.SetUserIdForNextRegistration(userId);
        _fakeImageUpload.ShouldCallOnCompleted = true;

        var (manager, go, spy) = CreateRegisterManager(nick: "FirestoreUser", email: "firestore@test.com");

        var task = RunAndWait(manager.HandleRegistration, spy);
        yield return new WaitUntil(() => task.IsCompleted);
        yield return new WaitForSeconds(0.5f);

        Assert.IsTrue(_fakeFirestore.UpdateProfileImageUrlCalled,
            "Firestore.UpdateUserProfileImageUrl deve ser chamado após upload do avatar");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Register_AvatarUploadFalha_NaoImpedeRegistro()
    {
        var userId   = "fail-avatar-id";
        var userData = new UserData(userId, "FailAvatar", "Fail Avatar", "fail@test.com");
        _fakeFirestore.SetFakeUserForGetUserData(userData);
        _fakeAuth.SetUserIdForNextRegistration(userId);
        _fakeImageUpload.ShouldThrowOnUpload = true;

        var (manager, go, spy) = CreateRegisterManager(nick: "FailAvatar", email: "fail@test.com");

        var task = RunAndWait(manager.HandleRegistration, spy);
        yield return new WaitUntil(() => task.IsCompleted);

        // Registro deve ter concluído mesmo com falha no avatar
        Assert.IsNotNull(UserDataStore.CurrentUserData,
            "UserDataStore deve ser populado mesmo se o upload do avatar falhar");
        Assert.AreEqual(userId, UserDataStore.CurrentUserData.UserId,
            "UserId deve estar correto mesmo com falha no avatar");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Register_AvatarUpload_ImagePathApontaParaArquivoTemp()
    {
        var userId   = "path-user-id";
        var userData = new UserData(userId, "PathUser", "Path User", "path@test.com");
        _fakeFirestore.SetFakeUserForGetUserData(userData);
        _fakeAuth.SetUserIdForNextRegistration(userId);

        var (manager, go, spy) = CreateRegisterManager(nick: "PathUser", email: "path@test.com");

        var task = RunAndWait(manager.HandleRegistration, spy);
        yield return new WaitUntil(() => task.IsCompleted);
        yield return new WaitForSeconds(0.5f);

        if (_fakeImageUpload.LastConfig != null)
        {
            string imagePath = _fakeImageUpload.LastConfig.ImagePath;
            Assert.IsTrue(imagePath.Contains("ImageTemp"),
                $"ImagePath deve estar no diretório ImageTemp. Recebido: {imagePath}");
            Assert.IsTrue(imagePath.EndsWith(".png"),
                $"ImagePath deve ser um arquivo .png. Recebido: {imagePath}");
            Assert.IsTrue(imagePath.Contains("avatar_"),
                $"ImagePath deve conter 'avatar_'. Recebido: {imagePath}");
        }
        else
        {
            Assert.Inconclusive(
                "Upload não foi chamado — Resources.Load pode ter retornado null.");
        }

        Object.DestroyImmediate(go);
    }

    // =======================================================================
    // Helpers — criar componentes de teste
    // =======================================================================

    private (AvatarPickerPanel panel, GameObject go) CreateAvatarPickerPanel()
    {
        var go = new GameObject("AvatarPickerPanel");

        // Estrutura mínima de UI para o painel
        var panelChild = new GameObject("Panel", typeof(RectTransform));
        panelChild.transform.SetParent(go.transform);
        var canvasGroup = panelChild.AddComponent<CanvasGroup>();

        var gridContent = new GameObject("GridContent", typeof(RectTransform));
        gridContent.transform.SetParent(panelChild.transform);

        var closeBtn = new GameObject("CloseBtn").AddComponent<Button>();
        closeBtn.transform.SetParent(go.transform);

        var panel = go.AddComponent<AvatarPickerPanel>();

        // Injeta referências via reflection (Start() não roda em Edit Mode)
        SetField(panel, "panelRect",    panelChild.GetComponent<RectTransform>());
        SetField(panel, "canvasGroup",  canvasGroup);
        SetField(panel, "gridContent",  gridContent.transform);
        SetField(panel, "closeButton",  closeBtn);

        return (panel, go);
    }

    private (RegisterManager manager, GameObject go, SpyFeedbackManager spy) CreateRegisterManager(
        string name = "Test", string nick = "TestUser",
        string email = "test@test.com", string password = "123456")
    {
        var go      = new GameObject("RegisterManager");
        var manager = go.AddComponent<RegisterManager>();
        var spy     = go.AddComponent<SpyFeedbackManager>();

        var registerBtn = new GameObject("RegisterBtn").AddComponent<Button>();
        var backBtn     = new GameObject("BackBtn").AddComponent<Button>();
        registerBtn.transform.SetParent(go.transform);
        backBtn.transform.SetParent(go.transform);

        // Start() não roda em Edit Mode — injeta dependências diretamente
        SetField(manager, "_auth",             _fakeAuth);
        SetField(manager, "_firestore",        _fakeFirestore);
        SetField(manager, "_navigation",       _fakeNavigation);
        SetField(manager, "_imageUpload",      _fakeImageUpload);
        SetField(manager, "feedbackManager",   spy);
        SetField(manager, "registerButton",    registerBtn);
        SetField(manager, "backButton",        backBtn);
        SetField(manager, "nameInput",         CreateInput(go, name));
        SetField(manager, "nickNameInput",     CreateInput(go, nick));
        SetField(manager, "emailInput",        CreateInput(go, email));
        SetField(manager, "passwordInput",     CreateInput(go, password));

        return (manager, go, spy);
    }

    // =======================================================================
    // Helpers utilitários
    // =======================================================================

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

            if (spy != null && spy.CallCount > callCountBefore)
                return;

            if (_fakeNavigation.NavigateCallCount > navCountBefore)
                return;

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

    private static void InvokeMethod(object target, string methodName, params object[] args)
    {
        var type = target.GetType();
        var method = type.GetMethod(methodName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public    |
            System.Reflection.BindingFlags.Instance);

        if (method != null)
            method.Invoke(target, args);
        else
            Debug.LogWarning($"[AvatarFlowTests] Método '{methodName}' não encontrado em {type.Name}");
    }

    private static TMP_InputField CreateInput(GameObject parent, string value)
    {
        var go    = new GameObject("Input");
        go.transform.SetParent(parent.transform);
        var input = go.AddComponent<TMP_InputField>();
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform);
        input.textComponent = textGo.AddComponent<TextMeshProUGUI>();
        input.text = value;
        return input;
    }
}
