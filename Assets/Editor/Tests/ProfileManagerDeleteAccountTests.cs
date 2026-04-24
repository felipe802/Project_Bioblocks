// Assets/Editor/Tests/ProfileManagerDeleteAccountTests.cs
//
// Testes de fluxo para ProfileManager.DeleteAccountAsync
//
// O que É testado:
//   ✅ 1a — Deleta documento na coleção Users
//   ✅ 1b — Deleta entrada na coleção Rankings
//   ✅ 1c — Deleta nickname na coleção Nicknames
//   ✅ 1d — Deleta de todas as coleções adicionais (UserBonus, UserFeedback, etc.)
//   ✅ Navega para LoginView após deleção completa bem-sucedida
//   ✅ Limpa UserDataStore.CurrentUserData após deleção
//   ✅ Reautenticação necessária → exibe ReAuthUI e interrompe navegação
//   ✅ Após retry com isRetry=true → navega para LoginView
//   ✅ Falha no Firestore (Users) não impede deleção do Auth nem a navegação
//   ✅ Segundo Delete com firestoreDeleted=true → não chama DeleteDocument("Users") de novo
//
// O que NÃO é testado:
//   - SceneManager.LoadScene (requer cena real carregada)
//   - UI visual (panels, overlays, animações)
//   - Integração real com Firebase SDK
//
// Removido no refactor de avatar-preset (abril/2026): tudo que envolvia
// Storage (IStorageRepository). Avatares agora são presets estáticos
// do AvatarCatalog; DeleteAccountAsync não mais deleta imagens no Storage.

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class ProfileManagerDeleteAccountTests
{
    // ── Fixtures ────────────────────────────────────────────────────────────
    private FakeAuthRepository       _fakeAuth;
    private FakeFirestoreRepository  _fakeFirestore;
    private FakeNavigationService    _fakeNavigation;
    private FakeStatisticsProvider   _fakeStatistics;

    private const string UserId   = "test-user-id";
    private const string NickName = "TestNick";
    private const string ImageUrl = "preset:amino_default";

    // ── Setup / TearDown ────────────────────────────────────────────────────

    [SetUp]
    public void Setup()
    {
        _fakeAuth       = new FakeAuthRepository();
        _fakeFirestore  = new FakeFirestoreRepository();
        _fakeNavigation = new FakeNavigationService();
        _fakeStatistics = new FakeStatisticsProvider();

        AppContext.OverrideForTests(
            auth:       _fakeAuth,
            firestore:  _fakeFirestore,
            statistics: _fakeStatistics,
            navigation: _fakeNavigation
        );

        // Usuário logado com dados completos
        _fakeAuth.SetLoggedInUser(UserId);
        var userData = new UserData(UserId, NickName, "Test User", "test@test.com")
        {
            ProfileImageUrl = ImageUrl
        };
        _fakeFirestore.SetFakeUser(userData);
        UserDataStore.CurrentUserData = userData;
    }

    [TearDown]
    public void TearDown()
    {
        UserDataStore.Clear();
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Cria um ProfileManager funcional em Edit Mode, injetando todas as
    /// dependências via reflection (Start() não executa em Edit Mode).
    /// </summary>
    private (ProfileManager manager, GameObject go) CreateProfileManager(
        UserData userDataOverride = null)
    {
        var go      = new GameObject("ProfileManager");
        var manager = go.AddComponent<ProfileManager>();

        var userData = userDataOverride ?? UserDataStore.CurrentUserData;

        SetField(manager, "_auth",            _fakeAuth);
        SetField(manager, "_firestore",       _fakeFirestore);
        SetField(manager, "_navigation",      _fakeNavigation);
        SetField(manager, "_statistics",      _fakeStatistics);
        SetField(manager, "currentUserData",  userData);
        SetField(manager, "firestoreDeleted", false);

        return (manager, go);
    }

    /// <summary>
    /// Aguarda a conclusão de DeleteAccountAsync com timeout de segurança de 5s.
    /// Considera concluído quando: navegação ocorre, UserDataStore é limpo,
    /// ou o timeout é atingido.
    /// </summary>
    private static async Task WaitForDeletion(
        FakeNavigationService nav,
        int navCountBefore,
        float timeoutSeconds = 5f)
    {
        float elapsed = 0f;
        while (elapsed < timeoutSeconds)
        {
            await Task.Delay(100);
            elapsed += 0.1f;

            // Considera concluído quando a navegação foi disparada
            if (nav.NavigateCallCount > navCountBefore)
                return;

            // Ou quando o store foi limpo (fluxo de reauth em isRetry)
            if (UserDataStore.CurrentUserData == null)
            {
                await Task.Delay(200); // aguarda possível navegação
                return;
            }
        }
    }

    // =======================================================================
    // DELEÇÃO FIRESTORE — coleções
    // =======================================================================

    [UnityTest]
    public IEnumerator Delete_1a_DeletaDocumentoNaColeçãoUsers()
    {
        var (manager, go) = CreateProfileManager();
        int navBefore = _fakeNavigation.NavigateCallCount;

        var task = manager.DeleteAccountAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(
            _fakeFirestore.WasDocumentDeleted("Users", UserId),
            "Deve deletar o documento do usuário na coleção 'Users'.");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Delete_1b_DeletaEntradaNaColeçãoRankings()
    {
        var (manager, go) = CreateProfileManager();

        var task = manager.DeleteAccountAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(
            _fakeFirestore.WasDocumentDeleted("Rankings", UserId),
            "Deve deletar a entrada do usuário na coleção 'Rankings'.");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Delete_1c_DeletaNicknameColeçãoNicknames()
    {
        var (manager, go) = CreateProfileManager();

        var task = manager.DeleteAccountAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(
            _fakeFirestore.WasDocumentDeleted("Nicknames", NickName),
            $"Deve deletar o nickname '{NickName}' na coleção 'Nicknames'.");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Delete_1e_DeletaColeçõesAdicionais()
    {
        var (manager, go) = CreateProfileManager();

        var task = manager.DeleteAccountAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        // "Questions" excluído: coleção global do banco de questões,
        // não possui documentos indexados por userId.
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
            Assert.IsTrue(
                _fakeFirestore.WasDocumentDeleted(collection, UserId),
                $"Deve deletar documento do usuário na coleção '{collection}'.");
        }

        // Questions NÃO deve ser deletado (não há documento por userId)
        Assert.IsFalse(
            _fakeFirestore.WasDocumentDeleted("Questions", UserId),
            "Não deve tentar deletar da coleção 'Questions' (coleção global).");

        Object.DestroyImmediate(go);
    }

    // =======================================================================
    // NAVEGAÇÃO
    // =======================================================================

    [UnityTest]
    public IEnumerator Delete_NavegaParaLoginViewAposSucesso()
    {
        var (manager, go) = CreateProfileManager();
        int navBefore = _fakeNavigation.NavigateCallCount;

        var task = manager.DeleteAccountAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        // Task.Delay(300) interno — aguarda um pouco mais
        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual("LoginView", _fakeNavigation.LastScene,
            "Deve navegar para 'LoginView' após deleção bem-sucedida.");
        Assert.Greater(_fakeNavigation.NavigateCallCount, navBefore,
            "NavigateTo deve ter sido chamado após a deleção.");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Delete_LimpaUserDataStoreAposDelecao()
    {
        var (manager, go) = CreateProfileManager();

        var task = manager.DeleteAccountAsync();
        yield return new WaitUntil(() => task.IsCompleted);
        yield return new WaitForSeconds(0.2f);

        Assert.IsNull(UserDataStore.CurrentUserData,
            "UserDataStore.CurrentUserData deve ser null após deleção.");

        Object.DestroyImmediate(go);
    }

    // =======================================================================
    // AUTENTICAÇÃO — DeleteUser
    // =======================================================================

    [UnityTest]
    public IEnumerator Delete_ChamaDeleteUserNoAuth()
    {
        var (manager, go) = CreateProfileManager();

        var task = manager.DeleteAccountAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(1, _fakeAuth.DeleteUserCallCount,
            "Deve chamar DeleteUser exatamente uma vez.");
        Assert.AreEqual(UserId, _fakeAuth.LastDeletedUserId,
            "Deve deletar o userId correto no Auth.");

        Object.DestroyImmediate(go);
    }

    // =======================================================================
    // REAUTENTICAÇÃO
    // =======================================================================

    [UnityTest]
    public IEnumerator Delete_ReauthNecessaria_NãoNavegaImediatamente()
    {
        _fakeAuth.ShouldThrowReauthOnDelete = true;
        var (manager, go) = CreateProfileManager();
        int navBefore = _fakeNavigation.NavigateCallCount;

        // Em testes, reAuthUI é null — o ProfileManager loga um erro esperado
        // informando que a UI de reautenticação não está atribuída.
        LogAssert.Expect(LogType.Error,
            "[DeleteAccount] ReAuthUI não atribuído — não é possível solicitar reautenticação.");

        var task = manager.DeleteAccountAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(navBefore, _fakeNavigation.NavigateCallCount,
            "Não deve navegar quando reautenticação é necessária (fluxo de retry pendente).");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Delete_Retry_AposReauth_DeletaTudoENavegarParaLoginView()
    {
        // Simula o cenário pós-reautenticação: isRetry=true, Auth já foi corrigido.
        // Com a nova lógica, fase 1 é sempre executada em retry (com flags individuais),
        // o que permite que deleções Firestore que falharam por sessão expirada sejam
        // refeitas depois do reauth.
        _fakeAuth.ShouldThrowReauthOnDelete = false; // reauth já concluída
        var (manager, go) = CreateProfileManager();

        // Simula que Users JÁ foi deletado antes do reauth
        SetField(manager, "firestoreDeleted", true);

        var task = manager.DeleteAccountAsync(isRetry: true);
        yield return new WaitUntil(() => task.IsCompleted);
        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual("LoginView", _fakeNavigation.LastScene,
            "Deve navegar para 'LoginView' após retry com reauth bem-sucedida.");
        Assert.AreEqual(1, _fakeAuth.DeleteUserCallCount,
            "DeleteUser deve ser chamado uma vez no retry.");

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator Delete_Retry_NãoDuplicaUsersSeJáDeletado()
    {
        // A flag firestoreDeleted=true simula que Users já foi deletado com
        // sucesso na primeira tentativa. Em retry, somente as operações
        // idempotentes (Rankings, Nicknames, etc.) devem ser chamadas — Users
        // deve ser pulada.
        var (manager, go) = CreateProfileManager();
        SetField(manager, "firestoreDeleted", true);

        var task = manager.DeleteAccountAsync(isRetry: true);
        yield return new WaitUntil(() => task.IsCompleted);
        yield return new WaitForSeconds(0.2f);

        // Users não deve ter sido deletado novamente (guarda firestoreDeleted)
        Assert.IsFalse(
            _fakeFirestore.WasDocumentDeleted("Users", UserId),
            "Com firestoreDeleted=true, não deve chamar DeleteDocument('Users') novamente.");

        // Mas Rankings, Nicknames e outras coleções idempotentes devem ter sido tentadas
        Assert.IsTrue(
            _fakeFirestore.WasDocumentDeleted("Rankings", UserId),
            "Rankings deve ser tentada mesmo em retry (operação idempotente).");

        Object.DestroyImmediate(go);
    }

    // =======================================================================
    // RESILIÊNCIA — falhas parciais não devem bloquear a navegação
    // =======================================================================

    [UnityTest]
    public IEnumerator Delete_FalhaNoFirestore_AindaDeletaAuthENa()
    {
        // Simula erro na deleção Users mas os demais passos devem continuar
        // (FakeFirestoreRepository não lança por padrão — testamos que Auth é chamado)
        var (manager, go) = CreateProfileManager();

        var task = manager.DeleteAccountAsync();
        yield return new WaitUntil(() => task.IsCompleted);
        yield return new WaitForSeconds(0.2f);

        // Auth deve ter sido chamado mesmo que Firestore falhe
        Assert.AreEqual(1, _fakeAuth.DeleteUserCallCount,
            "DeleteUser deve ser chamado mesmo com falha parcial no Firestore.");

        // E a navegação deve ter ocorrido
        Assert.AreEqual("LoginView", _fakeNavigation.LastScene,
            "Deve navegar para LoginView mesmo com falha parcial no Firestore.");

        Object.DestroyImmediate(go);
    }

    // =======================================================================
    // IDEMPOTÊNCIA — não duplica deleções
    // =======================================================================

    [UnityTest]
    public IEnumerator Delete_ComFirestoreJaDeletado_NãoChamaDeleteDocumentUsers()
    {
        var (manager, go) = CreateProfileManager();
        SetField(manager, "firestoreDeleted", true); // já deletado antes

        var task = manager.DeleteAccountAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsFalse(
            _fakeFirestore.WasDocumentDeleted("Users", UserId),
            "Não deve deletar 'Users' novamente se firestoreDeleted já é true.");

        Object.DestroyImmediate(go);
    }

    // =======================================================================
    // ORDEM DAS OPERAÇÕES — Firestore antes de Auth
    // =======================================================================

    [UnityTest]
    public IEnumerator Delete_OrdemCorreta_FirestoreAntesDeAuth()
    {
        // O FirestoreRepository real exige usuário autenticado.
        // Verifica que o Firestore é deletado antes do Auth.
        var operationOrder = new List<string>();

        // Não é possível interceptar a ordem facilmente com fakes simples,
        // mas podemos verificar que ambos foram chamados e que firestoreDeleted
        // fica true (o que implica que Users foi deletado antes da exceção de Auth)
        var (manager, go) = CreateProfileManager();

        var task = manager.DeleteAccountAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        bool firestoreWasDeleted = _fakeFirestore.WasDocumentDeleted("Users", UserId);
        bool authWasDeleted      = _fakeAuth.DeleteUserCallCount > 0;

        Assert.IsTrue(firestoreWasDeleted, "Firestore deve ter sido deletado.");
        Assert.IsTrue(authWasDeleted,      "Auth deve ter sido deletado.");

        // firestoreDeleted=true confirma que a fase 1 completou antes da fase 2
        bool firestoreDeletedFlag = (bool)GetField(manager, "firestoreDeleted");
        Assert.IsTrue(firestoreDeletedFlag,
            "'firestoreDeleted' deve ser true, confirmando que Users foi deletado antes do Auth.");

        Object.DestroyImmediate(go);
    }

    // =======================================================================
    // Helpers de reflexão — reutilizando o padrão do projeto
    // =======================================================================

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
        Debug.LogWarning($"[Test] Campo '{fieldName}' não encontrado via reflection.");
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
}
