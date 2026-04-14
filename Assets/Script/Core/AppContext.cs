using UnityEngine;
using System.Threading.Tasks;
using Firebase;
using System;

public class AppContext : MonoBehaviour
{
    private static AppContext _instance;
    public static event Action OnReady;

    // ── Serviços existentes ────────────────────────────────────────────────────
    public static IFirestoreRepository          Firestore         { get; private set; }
    public static IAuthRepository               Auth              { get; private set; }
    public static IStorageRepository            Storage           { get; private set; }
    public static IStatisticsProvider           Statistics        { get; private set; }
    public static INavigationService            Navigation        { get; private set; }
    public static ISceneDataService             SceneData         { get; private set; }
    public static ILiteDBManager                LocalDatabase     { get; private set; }
    public static IImageCacheService            ImageCache        { get; private set; }
    public static IImageUploadService           ImageUpload       { get; private set; }
    public static IAnsweredQuestionsManager     AnsweredQuestions { get; private set; }
    public static IPlayerLevelService           PlayerLevel       { get; private set; }
    public static IUserDataLocalRepository      UserDataLocal     { get; private set; }
    public static IUserDataSyncService          UserDataSync      { get; private set; }
    public static PendingUploadSyncService      PendingUploadSync { get; private set; }
    public static RankingSyncService            RankingSync       { get; private set; }
    public static ConnectivityMonitor           Connectivity      { get; private set; }

    // ── Novos serviços de questões ─────────────────────────────────────────────
    public static IFirestoreQuestionRepository  QuestionFirestore { get; private set; }
    public static IQuestionLocalRepository      QuestionLocal     { get; private set; }
    public static IQuestionSyncService          QuestionSync      { get; private set; }

    public static bool IsReady { get; private set; }

    private async void Awake()
    {
        var _ = MainThreadDispatcher.Instance;

        Debug.Log("[AppContext] Awake() chamado");
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        await InitializeServices();
    }

    private async Task InitializeServices()
    {
        IsReady = false;

        try
        {
            Debug.Log("[AppContext] Verificando dependências do Firebase...");

            var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
            var timeoutTask    = Task.Delay(10000);

            var completed = await Task.WhenAny(dependencyTask, timeoutTask);

            if (completed == timeoutTask)
                throw new Exception("Timeout ao verificar Firebase. Verifique sua conexão.");

            var dependencyStatus = await dependencyTask;
            if (dependencyStatus != DependencyStatus.Available)
                throw new Exception($"Firebase dependencies unavailable: {dependencyStatus}");

            Debug.Log("[AppContext] Firebase disponível.");

            // ── Obtenção dos componentes ───────────────────────────────────────
            var authRepo              = GetComponent<AuthenticationRepository>();
            var firestoreRepo         = GetComponent<FirestoreRepository>();
            var storageRepo           = GetComponent<StorageRepository>();
            var statsManager          = GetComponent<DatabaseStatisticsManager>();
            var navigationMgr         = GetComponent<NavigationManager>();
            var sceneDataMgr          = GetComponent<SceneDataManager>();
            var liteDBMgr             = GetComponent<LiteDBManager>();
            var imageCacheSvc         = GetComponent<ImageCacheService>();
            var imageUploadSvc        = GetComponent<ImageUploadService>();
            var answeredQuestionsMgr  = GetComponent<AnsweredQuestionsManager>();
            var playerLevelMgr        = GetComponent<PlayerLevelService>();
            var userDataLocalRepo     = GetComponent<UserDataLocalRepository>();
            var userDataSyncSvc       = GetComponent<UserDataSyncService>();
            var pendingUploadSync     = GetComponent<PendingUploadSyncService>();
            var rankingSyncSvc        = GetComponent<RankingSyncService>();
            var connectivityMonitor   = GetComponent<ConnectivityMonitor>();

            // ── Novos componentes de questões ──────────────────────────────────
            var questionFirestoreRepo = GetComponent<FirestoreQuestionRepository>();
            var questionLocalRepo     = GetComponent<QuestionLocalRepository>();
            var questionSyncSvc       = GetComponent<QuestionSyncService>();

            // ── Validações existentes ──────────────────────────────────────────
            if (authRepo            == null) throw new Exception("[AppContext] AuthenticationRepository não encontrado.");
            if (firestoreRepo       == null) throw new Exception("[AppContext] FirestoreRepository não encontrado.");
            if (liteDBMgr           == null) throw new Exception("[AppContext] LiteDBManager não encontrado.");
            if (imageCacheSvc       == null) throw new Exception("[AppContext] ImageCacheService não encontrado.");
            if (navigationMgr       == null) throw new Exception("[AppContext] NavigationManager não encontrado.");
            if (sceneDataMgr        == null) throw new Exception("[AppContext] SceneDataManager não encontrado.");
            if (statsManager        == null) throw new Exception("[AppContext] DatabaseStatisticsManager não encontrado.");
            if (storageRepo         == null) throw new Exception("[AppContext] StorageRepository não encontrado.");
            if (imageUploadSvc      == null) throw new Exception("[AppContext] ImageUploadService não encontrado.");
            if (answeredQuestionsMgr== null) throw new Exception("[AppContext] AnsweredQuestionsManager não encontrado.");
            if (playerLevelMgr      == null) throw new Exception("[AppContext] PlayerLevelService não encontrado.");
            if (userDataLocalRepo   == null) throw new Exception("[AppContext] UserDataLocalRepository não encontrado.");
            if (userDataSyncSvc     == null) throw new Exception("[AppContext] UserDataSyncService não encontrado.");
            if (pendingUploadSync   == null) throw new Exception("[AppContext] PendingUploadSyncService não encontrado.");
            if (rankingSyncSvc      == null) throw new Exception("[AppContext] RankingSyncService não encontrado.");
            if (connectivityMonitor == null) throw new Exception("[AppContext] ConnectivityMonitor não encontrado.");

            // ── Validações novas ───────────────────────────────────────────────
            if (questionFirestoreRepo == null) throw new Exception("[AppContext] FirestoreQuestionRepository não encontrado.");
            if (questionLocalRepo     == null) throw new Exception("[AppContext] QuestionLocalRepository não encontrado.");
            if (questionSyncSvc       == null) throw new Exception("[AppContext] QuestionSyncService não encontrado.");

            // ── 1. LiteDB ──────────────────────────────────────────────────────
            liteDBMgr.Initialize();

            // ── 2. Firebase ────────────────────────────────────────────────────
            await authRepo.InitializeAsync();
            firestoreRepo.Initialize();
            storageRepo.Initialize();

            // ── 3. Dependências cruzadas Firebase ─────────────────────────────
            authRepo.InjectDependencies(firestoreRepo);
            storageRepo.InjectDependencies(authRepo);
            imageUploadSvc.InjectDependencies(storageRepo);

            // ── 4. Dependências LiteDB (usuário) ───────────────────────────────
            userDataLocalRepo.InjectDependencies(liteDBMgr);
            userDataSyncSvc.InjectDependencies(userDataLocalRepo, firestoreRepo);
            imageCacheSvc.InjectDependencies(liteDBMgr);

            // ── 5. Dependências LiteDB (questões) ──────────────────────────────
            questionFirestoreRepo.Initialize();
            questionLocalRepo.InjectDependencies(liteDBMgr);
            questionSyncSvc.InjectDependencies(questionFirestoreRepo, questionLocalRepo);

            // ── 6. Navegação ───────────────────────────────────────────────────
            navigationMgr.InjectDependencies(sceneDataMgr);

            // ── 7. Estatísticas ────────────────────────────────────────────────
            await statsManager.Initialize();

            // ── 8. Sincronização de questões (antes de expor IsReady) ──────────
            Debug.Log("[AppContext] Sincronizando questões...");
            bool questionsReady = await questionSyncSvc.InitializeAsync();
            if (!questionsReady)
                Debug.LogWarning("[AppContext] Questões indisponíveis (sem internet e sem cache). " +
                                 "O app pode não funcionar corretamente sem conexão na primeira abertura.");

            // ── 9. Expõe serviços existentes ───────────────────────────────────
            Auth              = authRepo;
            Firestore         = firestoreRepo;
            Storage           = storageRepo;
            Statistics        = statsManager;
            Navigation        = navigationMgr;
            SceneData         = sceneDataMgr;
            LocalDatabase     = liteDBMgr;
            ImageCache        = imageCacheSvc;
            ImageUpload       = imageUploadSvc;
            AnsweredQuestions = answeredQuestionsMgr;
            PlayerLevel       = playerLevelMgr;
            UserDataLocal     = userDataLocalRepo;
            UserDataSync      = userDataSyncSvc;
            PendingUploadSync = pendingUploadSync;
            RankingSync       = rankingSyncSvc;
            Connectivity      = connectivityMonitor;

            // ── 10. Expõe novos serviços de questões ───────────────────────────
            QuestionFirestore = questionFirestoreRepo;
            QuestionLocal     = questionLocalRepo;
            QuestionSync      = questionSyncSvc;

            IsReady = true;
            OnReady?.Invoke();
            Debug.Log("[AppContext] Todos os serviços prontos.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AppContext] Falha na inicialização: {e.Message}");
            IsReady = false;
            throw;
        }
    }

    public static void OverrideForTests(
        IFirestoreRepository            firestore            = null,
        IAuthRepository                 auth                 = null,
        IStorageRepository              storage              = null,
        IStatisticsProvider             statistics           = null,
        INavigationService              navigation           = null,
        ISceneDataService               sceneData            = null,
        ILiteDBManager                  localDatabase        = null,
        IImageCacheService              imageCache           = null,
        IImageUploadService             imageUpload          = null,
        IAnsweredQuestionsManager       answeredQuestions    = null,
        IPlayerLevelService             playerLevel          = null,
        IUserDataLocalRepository        userDataLocal        = null,
        IUserDataSyncService            userDataSync         = null,
        IFirestoreQuestionRepository    questionFirestore    = null,
        IQuestionLocalRepository        questionLocal        = null,
        IQuestionSyncService            questionSync         = null)
    {
        if (firestore         != null) Firestore         = firestore;
        if (auth              != null) Auth              = auth;
        if (storage           != null) Storage           = storage;
        if (statistics        != null) Statistics        = statistics;
        if (navigation        != null) Navigation        = navigation;
        if (sceneData         != null) SceneData         = sceneData;
        if (localDatabase     != null) LocalDatabase     = localDatabase;
        if (imageCache        != null) ImageCache        = imageCache;
        if (imageUpload       != null) ImageUpload       = imageUpload;
        if (answeredQuestions != null) AnsweredQuestions = answeredQuestions;
        if (playerLevel       != null) PlayerLevel       = playerLevel;
        if (userDataLocal     != null) UserDataLocal     = userDataLocal;
        if (userDataSync      != null) UserDataSync      = userDataSync;
        if (questionFirestore != null) QuestionFirestore = questionFirestore;
        if (questionLocal     != null) QuestionLocal     = questionLocal;
        if (questionSync      != null) QuestionSync      = questionSync;
        IsReady = true;
    }
}
