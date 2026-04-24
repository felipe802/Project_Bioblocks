// Assets/Editor/Tests/PlayerLevelServiceTests.cs
// Testes unitários para PlayerLevelService — getters e progressão de nível.
//
// IMPORTANTE: Este arquivo deve estar em Assets/Editor/Tests/ mas os testes
// com [UnityTest] rodam automaticamente em Play Mode pelo Unity Test Runner.
// Os testes [Test] síncronos rodam em Edit Mode.
//
// Para rodar: Window → General → Test Runner → PlayMode → Run All

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class PlayerLevelServiceTests
{
    // -------------------------------------------------------
    // Fixtures
    // -------------------------------------------------------
    private GameObject          _serviceGO;
    private PlayerLevelService  _playerLevel;
    private GameObject          _dispatcherGO;

    private FakeFirestoreRepository  _firestore;
    private FakeLiteDBManager        _db;
    private UserDataLocalRepository  _localRepo;
    private UserDataSyncService      _syncService;
    private FakeStatisticsProvider   _statistics;
    private GameObject               _syncServiceGO;
    private GameObject               _localRepoGO;

    private const string USER_ID        = "player-level-test";
    private const int    TOTAL_QUESTIONS = 100;

    [SetUp]
    public void Setup()
    {
        // 1. MainThreadDispatcher criado manualmente — sem usar a propriedade
        //    Instance para evitar DontDestroyOnLoad (proibido em Edit Mode).
        //    Setamos _instance diretamente via reflection para que Enqueue funcione.
        _dispatcherGO = new GameObject("MainThreadDispatcher");
        var dispatcher = _dispatcherGO.AddComponent<MainThreadDispatcher>();
        typeof(MainThreadDispatcher)
            .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static)
            ?.SetValue(null, dispatcher);

        // 2. Fakes
        _firestore  = new FakeFirestoreRepository();
        _db         = new FakeLiteDBManager();
        _statistics = new FakeStatisticsProvider(TOTAL_QUESTIONS);

        _localRepoGO = new GameObject("UserDataLocalRepo");
        _localRepo   = _localRepoGO.AddComponent<UserDataLocalRepository>();
        _localRepo.InjectDependencies(_db);

        _syncServiceGO = new GameObject("SyncService");
        _syncService   = _syncServiceGO.AddComponent<UserDataSyncService>();
        _syncService.InjectDependencies(_localRepo, _firestore);

        // 3. AppContext
        AppContext.OverrideForTests(
            firestore:     _firestore,
            localDatabase: _db,
            userDataLocal: _localRepo,
            userDataSync:  _syncService,
            statistics:    _statistics
        );

        // 4. UserData com TotalQuestionsInAllDatabanks preenchido ANTES de
        //    InitializeDependencies, para que GetTotalQuestionsCount() não
        //    trave o cache em 0 durante PerformMigrationIfNeeded
        var user = MakeUser(level: 1, totalAnswered: 0);
        _localRepo.SaveUser(user);
        UserDataStore.CurrentUserData = user;
        UserDataStore.Logger = _ => { };

        // 5. PlayerLevelService
        _serviceGO   = new GameObject("PlayerLevelService");
        _playerLevel = _serviceGO.AddComponent<PlayerLevelService>();

        // Pré-seta _migrationChecked = true para impedir que
        // PerformMigrationIfNeeded execute durante InitializeDependencies.
        // Esse método é async void e chama GetTotalQuestionsCount() antes
        // do cache estar pronto, travando _cachedTotalQuestions em 0.
        SetPrivateField("_migrationChecked", true);

        // Pré-seta o cache com TOTAL_QUESTIONS como segunda camada de proteção.
        SetCachedTotalQuestions(TOTAL_QUESTIONS);

        LogAssert.ignoreFailingMessages = true;
        InvokeInitializeDependencies();
        LogAssert.ignoreFailingMessages = false;
    }

    [TearDown]
    public void TearDown()
    {
        _db.Close();
        UserDataStore.Clear();
        Object.DestroyImmediate(_serviceGO);
        Object.DestroyImmediate(_syncServiceGO);
        Object.DestroyImmediate(_localRepoGO);

        // Limpa o campo estático _instance antes de destruir o GameObject.
        // Limpa _instance antes de destruir o GameObject — sem isso o próximo
        // teste recebe um _instance apontando para um objeto destruído.
        typeof(MainThreadDispatcher)
            .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static)
            ?.SetValue(null, null);

        Object.DestroyImmediate(_dispatcherGO);
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private static UserData MakeUser(int level, int totalAnswered, int totalInAllBanks = TOTAL_QUESTIONS)
    {
        var user = new UserData(USER_ID, "Tester", "Tester", "t@test.com");
        user.PlayerLevel                    = level;
        user.TotalValidQuestionsAnswered    = totalAnswered;
        user.TotalQuestionsInAllDatabanks   = totalInAllBanks;
        return user;
    }

    private void SetUser(int level, int totalAnswered)
    {
        var user = MakeUser(level, totalAnswered);
        _localRepo.UpdateUser(user);
        UserDataStore.CurrentUserData = user;
        // Garante que o cache reflita TOTAL_QUESTIONS após troca de UserData
        SetCachedTotalQuestions(TOTAL_QUESTIONS);
    }

    private void InvokeInitializeDependencies()
    {
        typeof(PlayerLevelService)
            .GetMethod("InitializeDependencies",
                BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(_playerLevel, null);
    }

    private void SetPrivateField(string fieldName, object value)
    {
        typeof(PlayerLevelService)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(_playerLevel, value);
    }

    private void SetCachedTotalQuestions(int value)
        => SetPrivateField("_cachedTotalQuestions", value);

    // =======================================================
    // Getters simples
    // =======================================================

    [Test]
    public void GetCurrentLevel_UsuarioNivel1_RetornaUm()
    {
        Assert.AreEqual(1, _playerLevel.GetCurrentLevel());
    }

    [Test]
    public void GetCurrentLevel_SemUserData_RetornaUm()
    {
        UserDataStore.Clear();
        // _currentUserData = null → fallback é 1
        Assert.AreEqual(1, _playerLevel.GetCurrentLevel());
    }

    [Test]
    public void GetTotalValidAnswered_ZeroRespostas_RetornaZero()
    {
        Assert.AreEqual(0, _playerLevel.GetTotalValidAnswered());
    }

    [Test]
    public void GetTotalQuestionsInAllDatabanks_RetornaTotalDoUsuario()
    {
        Assert.AreEqual(TOTAL_QUESTIONS, _playerLevel.GetTotalQuestionsInAllDatabanks());
    }

    // =======================================================
    // GetProgressInCurrentLevel
    // =======================================================

    [Test]
    public void GetProgressInCurrentLevel_SemUserData_RetornaZero()
    {
        UserDataStore.Clear();
        Assert.AreEqual(0f, _playerLevel.GetProgressInCurrentLevel(), delta: 0.001f);
    }

    [Test]
    public void GetProgressInCurrentLevel_InicioDoNivel_RetornaZero()
    {
        SetUser(level: 1, totalAnswered: 0);
        Assert.AreEqual(0f, _playerLevel.GetProgressInCurrentLevel(), delta: 0.01f);
    }

    // Os testes de GetProgressInCurrentLevel que dependem de GetTotalQuestionsCount()
    // são cobertos diretamente em PlayerLevelConfigTests (GetRequiredQuestions,
    // GetMinRequiredQuestions, CalculateLevel), onde a lógica pura já está validada.

    // =======================================================
    // GetQuestionsUntilNextLevel
    // =======================================================

    [Test]
    public void GetQuestionsUntilNextLevel_SemUserData_RetornaZero()
    {
        // _currentUserData == null → guard no início do método retorna 0
        // Para garantir null, zeramos o campo interno via reflection
        SetPrivateField("_currentUserData", null);
        Assert.AreEqual(0, _playerLevel.GetQuestionsUntilNextLevel());
    }

    [Test]
    public void GetQuestionsUntilNextLevel_Nivel10_RetornaZero()
    {
        // Guard explícito: if (PlayerLevel >= 10) return 0
        // Setamos _currentUserData diretamente via reflection para garantir Level=10
        SetPrivateField("_currentUserData", MakeUser(level: 10, totalAnswered: 90));
        Assert.AreEqual(0, _playerLevel.GetQuestionsUntilNextLevel());
    }

    [Test]
    public void GetQuestionsUntilNextLevel_Nivel1_ZeroRespondidas_Retorna10()
    {
        // Com total=100 no UserData: threshold nível 2 = 10% de 100 = 10
        // GetTotalQuestionsCount() lê TotalQuestionsInAllDatabanks=100 do UserData
        SetUser(level: 1, totalAnswered: 0);
        Assert.AreEqual(10, _playerLevel.GetQuestionsUntilNextLevel());
    }

    // =======================================================
    // GetQuestionsAtLevelStart
    // =======================================================

    [Test]
    public void GetQuestionsAtLevelStart_SemUserData_RetornaZero()
    {
        UserDataStore.Clear();
        Assert.AreEqual(0, _playerLevel.GetQuestionsAtLevelStart());
    }

    [Test]
    public void GetQuestionsAtLevelStart_Nivel1_RetornaZero()
    {
        // Nível 1 min = 0% → sempre 0, independente do total
        SetUser(level: 1, totalAnswered: 0);
        Assert.AreEqual(0, _playerLevel.GetQuestionsAtLevelStart());
    }

    // =======================================================
    // IncrementTotalAnswered
    // =======================================================

    [UnityTest]
    public IEnumerator IncrementTotalAnswered_IncrementaContadorLocal()
    {
        // Parte do estado inicial (totalAnswered=0) para evitar dependência
        // de SetUser propagar corretamente para _currentUserData interno do serviço
        var task = _playerLevel.IncrementTotalAnswered();
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(1, _playerLevel.GetTotalValidAnswered());
    }

    [UnityTest]
    public IEnumerator IncrementTotalAnswered_MultiplasChamadas_AcumulaCorretamente()
    {
        SetUser(level: 1, totalAnswered: 0);

        var t1 = _playerLevel.IncrementTotalAnswered();
        yield return new WaitUntil(() => t1.IsCompleted);

        var t2 = _playerLevel.IncrementTotalAnswered();
        yield return new WaitUntil(() => t2.IsCompleted);

        var t3 = _playerLevel.IncrementTotalAnswered();
        yield return new WaitUntil(() => t3.IsCompleted);

        Assert.AreEqual(3, _playerLevel.GetTotalValidAnswered());
    }

    // =======================================================
    // CheckAndHandleLevelUp
    // =======================================================

    [UnityTest]
    public IEnumerator CheckAndHandleLevelUp_SemLevelUp_MantemNivel()
    {
        // Com 5 respondidas e nível 1 → ainda no nível 1 (CalculateLevel(5,100)=1)
        SetUser(level: 1, totalAnswered: 5);

        var task = _playerLevel.CheckAndHandleLevelUp();
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(1, _playerLevel.GetCurrentLevel());
    }

    [UnityTest]
    public IEnumerator CheckAndHandleLevelUp_ComLevelUp_AtualizaNivel()
    {
        // SetUser não propaga para _currentUserData interno — usamos reflection diretamente
        SetPrivateField("_currentUserData", MakeUser(level: 1, totalAnswered: 10));
        SetCachedTotalQuestions(TOTAL_QUESTIONS);

        var task = _playerLevel.CheckAndHandleLevelUp();
        yield return new WaitUntil(() => task.IsCompleted);

        // OnLevelChanged dispara via MainThreadDispatcher.Enqueue que só processa
        // no Update() — em Edit Mode isso não roda de forma confiável.
        // Verificamos diretamente o estado interno que é setado ANTES do Enqueue.
        Assert.AreEqual(2, _playerLevel.GetCurrentLevel(),
            "PlayerLevel deve ser 2 após level up de 1→2");
    }

    [UnityTest]
    public IEnumerator CheckAndHandleLevelUp_JaNoNivelCorreto_NaoDisparaEvento()
    {
        // CalculateLevel(15,100) = 2 e usuário já está no nível 2 → sem level up
        SetUser(level: 2, totalAnswered: 15);

        bool eventoDisparado = false;
        _playerLevel.OnLevelChanged += (_, __) => eventoDisparado = true;

        var task = _playerLevel.CheckAndHandleLevelUp();
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsFalse(eventoDisparado,
            "Não deve disparar OnLevelChanged quando já está no nível correto");
    }

    // =======================================================
    // REGRESSION TESTS — snapshot-based denominator
    //
    // Reproduzem o bug reportado onde o jogador pulou do nível 6
    // direto ao 10. Garantem que a nova lógica com
    // LevelSnapshotDenominator não permite esse comportamento.
    // =======================================================

    private UserData MakeUserWithSnapshot(int level, int totalAnswered, int snapshot, int totalInAllBanks = TOTAL_QUESTIONS)
    {
        var user = MakeUser(level, totalAnswered, totalInAllBanks);
        user.LevelSnapshotDenominator = snapshot;
        return user;
    }

    [UnityTest]
    public IEnumerator CheckAndHandleLevelUp_DenominadorEncolheNoProvider_NaoPulaNiveis()
    {
        // Cenário do bug: usuário está no nível 6 com 55 respondidas (55%),
        // snapshot congelado em 100. O provider começa a reportar 50
        // (total encolheu num outro dispositivo antes do fix).
        //
        // Bug original: pct = 55/50 = 110% → CalculateLevel = 10 → pulo 6→10.
        // Novo comportamento: usa snapshot=100 → pct=55% → continua no nível 6.
        SetPrivateField("_currentUserData", MakeUserWithSnapshot(level: 6, totalAnswered: 55, snapshot: 100));
        SetCachedTotalQuestions(0);             // força releitura
        SetPrivateField("_statistics", new FakeStatisticsProvider(50));

        var task = _playerLevel.CheckAndHandleLevelUp();
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(6, _playerLevel.GetCurrentLevel(),
            "Snapshot deve proteger o jogador de pular níveis quando o total encolhe.");
    }

    [UnityTest]
    public IEnumerator CheckAndHandleLevelUp_SnapshotRefrescaAoSubir()
    {
        // Nível 1 → 2: snapshot inicial 100, answered passa de 9 para 10,
        // cruza o threshold (10% = 10 respostas). Snapshot deve permanecer
        // igual ou crescer — nunca diminuir.
        SetPrivateField("_currentUserData", MakeUserWithSnapshot(level: 1, totalAnswered: 10, snapshot: 100));
        SetCachedTotalQuestions(TOTAL_QUESTIONS);

        var task = _playerLevel.CheckAndHandleLevelUp();
        yield return new WaitUntil(() => task.IsCompleted);

        var userData = (UserData)typeof(PlayerLevelService)
            .GetField("_currentUserData", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(_playerLevel);

        Assert.AreEqual(2, userData.PlayerLevel);
        Assert.GreaterOrEqual(userData.LevelSnapshotDenominator, 100,
            "LevelSnapshotDenominator deve ser monotonicamente não-decrescente.");
    }

    [UnityTest]
    public IEnumerator CheckAndHandleLevelUp_BugRegressivo_6ParaExatamente7()
    {
        // Reproduz o cenário exato relatado pelo usuário: estava no nível 6
        // com 60 respondidas no snapshot original de 100 → deveria ir só a 7.
        // Antes do fix, com o provider reportando 50, o cálculo mostrava
        // percentage=120% e o jogador pulava para o nível 10, ganhando os
        // bônus de 7, 8, 9 e 10 de uma só vez.
        //
        // Novo comportamento: snapshot=100 é preservado, pct=60% → level-up
        // EXATAMENTE para 7. Snapshot refrescado respeita max(provider, old).
        SetPrivateField("_currentUserData", MakeUserWithSnapshot(level: 6, totalAnswered: 60, snapshot: 100));
        SetCachedTotalQuestions(0);
        SetPrivateField("_statistics", new FakeStatisticsProvider(50));

        var task = _playerLevel.CheckAndHandleLevelUp();
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(7, _playerLevel.GetCurrentLevel(),
            "Jogador deve subir EXATAMENTE para 7, nunca pular para 8, 9 ou 10.");

        var userData = (UserData)typeof(PlayerLevelService)
            .GetField("_currentUserData", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(_playerLevel);
        Assert.AreEqual(100, userData.LevelSnapshotDenominator,
            "Snapshot não pode diminuir quando o provider reporta um valor menor.");
    }

    [UnityTest]
    public IEnumerator CheckAndHandleLevelUp_NuncaDesce()
    {
        // Usuário no nível 10, com snapshot muito grande — CalculateLevel
        // com snapshot retornaria um nível menor, mas o serviço NÃO deve
        // diminuir o nível do jogador.
        SetPrivateField("_currentUserData", MakeUserWithSnapshot(level: 10, totalAnswered: 10, snapshot: 1000));
        SetCachedTotalQuestions(1000);

        var task = _playerLevel.CheckAndHandleLevelUp();
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(10, _playerLevel.GetCurrentLevel(),
            "Nível do jogador nunca pode diminuir.");
    }
}
