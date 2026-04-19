using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using QuestionSystem;

/// <summary>
/// Testes unitários para a camada LiteDB do BioBlocks.
///
/// Cobre:
///   - UserDataLocalRepository  (CRUD, dirty/synced, score, answered questions, SavedAt)
///   - UserDataSyncService      (TrySyncPendingData, MergeWithFirestore via SavedAt)
///   - PendingUploadDB          (upsert, delete, findAll)
///   - RankingDB                (FromDomain, ToDomain, ordenação)
///   - UserDataDB               (FromDomain, ToDomain, mapeamento de campos)
///   - UserDataStore            (mutations, Clear)
///   - QuestionDB               (FromDomain, ToDomain, campos novos do Firestore)
///   - QuestionLocalRepository  (SaveQuestions, GetByDatabankName, ClearAll, cache timestamp)
///
/// Padrão: Arrange / Act / Assert
/// Sem dependências de Unity — usa FakeLiteDBManager e FakeFirestoreRepository.
///
/// Para rodar: Window → General → Test Runner → EditMode → Run All
/// </summary>
/// 
[TestFixture]
public class LiteDBTests
{
    // -------------------------------------------------------
    // Fixtures compartilhadas
    // -------------------------------------------------------

    private FakeLiteDBManager           _db;
    private UserDataLocalRepository     _repo;
    private FakeFirestoreRepository     _firestore;
    private UserDataSyncService         _syncService;
    private GameObject                  _syncServiceGO;
    private QuestionLocalRepository     _questionRepo;
    private GameObject                  _questionRepoGO;

    [SetUp]
    public void Setup()
    {
        _db        = new FakeLiteDBManager();
        _repo      = new UserDataLocalRepository();
        _repo.InjectDependencies(_db);

        _firestore = new FakeFirestoreRepository();

        // UserDataSyncService é MonoBehaviour — precisa de AddComponent
        _syncServiceGO = new GameObject("SyncService");
        _syncService   = _syncServiceGO.AddComponent<UserDataSyncService>();
        _syncService.InjectDependencies(_repo, _firestore);

        _questionRepoGO = new GameObject("QuestionLocalRepository");
        _questionRepo   = _questionRepoGO.AddComponent<QuestionLocalRepository>();
        _questionRepo.InjectDependencies(_db);

        UserDataStore.Clear();
        UserDataStore.Logger = _ => { };
    }

    [TearDown]
    public void TearDown()
    {
        _db.Close();
        UserDataStore.Clear();
        if (_syncServiceGO != null)
            UnityEngine.Object.DestroyImmediate(_syncServiceGO);
        if (_questionRepoGO != null)
            UnityEngine.Object.DestroyImmediate(_questionRepoGO);
    }

    // ═══════════════════════════════════════════════════════
    // UserDataLocalRepository — CRUD básico
    // ═══════════════════════════════════════════════════════
    [Test]
    public void SaveUser_WhenUserDoesNotExist_InsertsSuccessfully()
    {
        var user = MakeUser("u1", score: 100);

        _repo.SaveUser(user);

        Assert.IsTrue(_repo.HasUser("u1"));
        Assert.AreEqual(100, _repo.GetUser("u1").Score);
    }

    [Test]
    public void GetUser_WhenUserDoesNotExist_ReturnsNull()
    {
        Assert.IsNull(_repo.GetUser("nonexistent"));
    }

    [Test]
    public void UpdateUser_WhenUserExists_UpdatesFields()
    {
        _repo.SaveUser(MakeUser("u1", score: 100, nickName: "Old"));

        _repo.UpdateUser(MakeUser("u1", score: 200, nickName: "New"));

        var loaded = _repo.GetUser("u1");
        Assert.AreEqual(200, loaded.Score);
        Assert.AreEqual("New", loaded.NickName);
    }

    [Test]
    public void UpdateUser_WhenUserDoesNotExist_InsertsUser()
    {
        _repo.UpdateUser(MakeUser("u_new", score: 50));

        Assert.IsTrue(_repo.HasUser("u_new"));
    }

    [Test]
    public void DeleteUser_WhenUserExists_RemovesFromDB()
    {
        _repo.SaveUser(MakeUser("u1"));
        _repo.DeleteUser("u1");
        Assert.IsFalse(_repo.HasUser("u1"));
    }

    [Test]
    public void HasUser_WhenUserExists_ReturnsTrue()
    {
        _repo.SaveUser(MakeUser("u1"));
        Assert.IsTrue(_repo.HasUser("u1"));
    }

    [Test]
    public void HasUser_WhenUserDoesNotExist_ReturnsFalse()
    {
        Assert.IsFalse(_repo.HasUser("ghost"));
    }

    // ═══════════════════════════════════════════════════════
    // UserDataLocalRepository — Dirty / Synced
    // ═══════════════════════════════════════════════════════

    [Test]
    public void SaveUser_InitiallyNotDirty()
    {
        _repo.SaveUser(MakeUser("u1"));
        Assert.IsFalse(_repo.IsDirty("u1"));
    }

    [Test]
    public void MarkAsDirty_SetsDirtyFlagTrue()
    {
        _repo.SaveUser(MakeUser("u1"));
        _repo.MarkAsDirty("u1");
        Assert.IsTrue(_repo.IsDirty("u1"));
    }

    [Test]
    public void MarkAsSynced_ClearsDirtyFlag()
    {
        _repo.SaveUser(MakeUser("u1"));
        _repo.MarkAsDirty("u1");
        _repo.MarkAsSynced("u1");
        Assert.IsFalse(_repo.IsDirty("u1"));
    }

    [Test]
    public void MarkAsSynced_UpdatesLastSyncedAt()
    {
        _repo.SaveUser(MakeUser("u1"));
        var before = DateTime.Now.AddSeconds(-1);

        _repo.MarkAsSynced("u1");
        var lastSync = _repo.GetLastSyncedAt("u1");

         Assert.GreaterOrEqual(_repo.GetLastSyncedAt("u1").ToUniversalTime(), before);
    }

    [Test]
    public void GetLastSyncedAt_WhenNeverSynced_ReturnsMinValue()
    {
        _repo.SaveUser(MakeUser("u1"));
        // SaveUser não chama MarkAsSynced
        Assert.AreEqual(DateTime.MinValue, _repo.GetLastSyncedAt("u1"));
    }

    [Test]
    public void UpdateUser_PreservesDirtyFlag()
    {
        _repo.SaveUser(MakeUser("u1", score: 100));
        _repo.MarkAsDirty("u1");

        _repo.UpdateUser(MakeUser("u1", score: 200)); // não deve limpar dirty

        Assert.IsTrue(_repo.IsDirty("u1"));
    }

    [Test]
    public void UpdateUser_PreservesLastSyncedAt()
    {
        _repo.SaveUser(MakeUser("u1"));
        _repo.MarkAsSynced("u1");

        _repo.UpdateUser(MakeUser("u1", score: 999));

        // Verifica que LastSyncedAt foi preservado — não voltou para MinValue
        Assert.AreNotEqual(DateTime.MinValue, _repo.GetLastSyncedAt("u1"));
    }

    // ═══════════════════════════════════════════════════════
    // UserDataLocalRepository — Score
    // ═══════════════════════════════════════════════════════
    [Test]
    public void UpdateScore_UpdatesScoreAndWeekScore()
    {
        _repo.SaveUser(MakeUser("u1", score: 100, weekScore: 50));

        _repo.UpdateScore("u1", newScore: 150, newWeekScore: 75);

        var loaded = _repo.GetUser("u1");
        Assert.AreEqual(150, loaded.Score);
        Assert.AreEqual(75,  loaded.WeekScore);
    }

    [Test]
    public void UpdateScore_MarksDirty()
    {
        _repo.SaveUser(MakeUser("u1", score: 100));

        _repo.UpdateScore("u1", 200, 100);

        Assert.IsTrue(_repo.IsDirty("u1"));
    }

    [Test]
    public void UpdateScore_UpdatesSavedAt()
    {
        _repo.SaveUser(MakeUser("u1", score: 100));
        var before = _repo.GetUser("u1").SavedAt;

        System.Threading.Thread.Sleep(10);
        _repo.UpdateScore("u1", 200, 100);

        Assert.GreaterOrEqual(_repo.GetUser("u1").SavedAt, before);
    }

    [Test]
    public void UpdateScore_WhenUserDoesNotExist_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _repo.UpdateScore("nonexistent", 100, 50));
    }

    // ═══════════════════════════════════════════════════════
    // UserDataLocalRepository — AnsweredQuestions
    // ═══════════════════════════════════════════════════════

    [Test]
    public void AddAnsweredQuestion_AddsToCorrectDatabank()
    {
        _repo.SaveUser(MakeUser("u1"));

        _repo.AddAnsweredQuestion("u1", "AminoacidDB", 42);

        var loaded = _repo.GetUser("u1");
        Assert.IsTrue(loaded.AnsweredQuestions.ContainsKey("AminoacidDB"));
        Assert.Contains(42, loaded.AnsweredQuestions["AminoacidDB"]);
    }

    [Test]
    public void AddAnsweredQuestion_DoesNotAddDuplicate()
    {
        _repo.SaveUser(MakeUser("u1"));
        _repo.AddAnsweredQuestion("u1", "AminoacidDB", 42);
        _repo.AddAnsweredQuestion("u1", "AminoacidDB", 42); // duplicata

        var loaded = _repo.GetUser("u1");
        Assert.AreEqual(1, loaded.AnsweredQuestions["AminoacidDB"].Count);
    }

    [Test]
    public void AddAnsweredQuestion_MarksDirty()
    {
        _repo.SaveUser(MakeUser("u1"));
        _repo.AddAnsweredQuestion("u1", "AminoacidDB", 1);
        Assert.IsTrue(_repo.IsDirty("u1"));
    }

    [Test]
    public void AddAnsweredQuestion_MultipleDatabanks_StoredSeparately()
    {
        _repo.SaveUser(MakeUser("u1"));
        _repo.AddAnsweredQuestion("u1", "AminoacidDB", 1);
        _repo.AddAnsweredQuestion("u1", "LipidsDB", 5);

        var loaded = _repo.GetUser("u1");
        Assert.AreEqual(1, loaded.AnsweredQuestions["AminoacidDB"].Count);
        Assert.AreEqual(1, loaded.AnsweredQuestions["LipidsDB"].Count);
    }

    // ═══════════════════════════════════════════════════════
    // UserDataLocalRepository — SavedAt
    // ═══════════════════════════════════════════════════════

    [Test]
    public void SaveUser_SetsSavedAtToNow()
    {
        var before = DateTime.Now.AddSeconds(-1); // ← DateTime.Now, não UtcNow
        _repo.SaveUser(MakeUser("u1"));
        Assert.GreaterOrEqual(_repo.GetUser("u1").SavedAt, before);
    }

    [Test]
    public void UpdateUser_UpdatesSavedAt()
    {
        _repo.SaveUser(MakeUser("u1"));
        var firstSavedAt = _repo.GetUser("u1").SavedAt;

        System.Threading.Thread.Sleep(10);
        _repo.UpdateUser(MakeUser("u1", score: 200));

        Assert.GreaterOrEqual(_repo.GetUser("u1").SavedAt, firstSavedAt);
    }

    // ═══════════════════════════════════════════════════════
    // PendingUploadDB
    // ═══════════════════════════════════════════════════════

    [Test]
    public void PendingUploads_UpsertAndFindById_Works()
    {
        var pending = new PendingUploadDB
        {
            Id          = "u1",
            UserId      = "u1",
            LocalPath   = "/tmp/image.jpg",
            OldImageUrl = "https://old.url",
            CreatedAt   = DateTime.Now
        };

        _db.PendingUploads.Upsert(pending);

        var loaded = _db.PendingUploads.FindById("u1");
        Assert.IsNotNull(loaded);
        Assert.AreEqual("/tmp/image.jpg", loaded.LocalPath);
        Assert.AreEqual("https://old.url", loaded.OldImageUrl);
    }

    [Test]
    public void PendingUploads_Delete_RemovesEntry()
    {
        _db.PendingUploads.Upsert(new PendingUploadDB { Id = "u1", UserId = "u1" });

        _db.PendingUploads.Delete("u1");

        Assert.IsNull(_db.PendingUploads.FindById("u1"));
    }

    [Test]
    public void PendingUploads_Upsert_OverwritesExistingEntry()
    {
        _db.PendingUploads.Upsert(new PendingUploadDB { Id = "u1", UserId = "u1", LocalPath = "/tmp/old.jpg" });
        _db.PendingUploads.Upsert(new PendingUploadDB { Id = "u1", UserId = "u1", LocalPath = "/tmp/new.jpg" });

        Assert.AreEqual("/tmp/new.jpg", _db.PendingUploads.FindById("u1").LocalPath);
    }

    [Test]
    public void PendingUploads_FindAll_ReturnsAllEntries()
    {
        _db.PendingUploads.Upsert(new PendingUploadDB { Id = "u1", UserId = "u1" });
        _db.PendingUploads.Upsert(new PendingUploadDB { Id = "u2", UserId = "u2" });

        var all = _db.PendingUploads.FindAll().ToList();

        Assert.AreEqual(2, all.Count);
    }

    [Test]
    public void PendingUploads_FindById_WhenNotExists_ReturnsNull()
    {
        Assert.IsNull(_db.PendingUploads.FindById("nonexistent"));
    }

    // ═══════════════════════════════════════════════════════
    // RankingDB — conversão e cache
    // ═══════════════════════════════════════════════════════

    [Test]
    public void RankingDB_FromDomain_MapsAllFields()
    {
        var ranking = new Ranking("Alice", 1000, 200, "https://img.url");

        var db = RankingDB.FromDomain(ranking);

        Assert.AreEqual("Alice",          db.NickName);
        Assert.AreEqual(1000,             db.Score);
        Assert.AreEqual(200,              db.WeekScore);
        Assert.AreEqual("https://img.url", db.ProfileImageUrl);
    }

    [Test]
    public void RankingDB_ToDomain_MapsAllFields()
    {
        var db = new RankingDB
        {
            NickName        = "Bob",
            Score           = 500,
            WeekScore       = 100,
            ProfileImageUrl = "https://img.url"
        };

        var domain = db.ToDomain();
        Assert.AreEqual("Bob",            domain.userName);
        Assert.AreEqual(500,              domain.userScore);
        Assert.AreEqual(100,              domain.userWeekScore);
        Assert.AreEqual("https://img.url", domain.profileImageUrl);
    }

    [Test]
    public void RankingDB_FromDomain_NullProfileImageUrl_UsesEmptyString()
    {
        var ranking = new Ranking("Alice", 100, 50, null);
        var db = RankingDB.FromDomain(ranking);
        Assert.AreEqual("", db.ProfileImageUrl);
    }

    [Test]
    public void Rankings_UpsertAndFindById_Works()
    {
        var ranking = RankingDB.FromDomain(new Ranking("Alice", 1000, 200, ""));

        _db.Rankings.Upsert(ranking);

        var loaded = _db.Rankings.FindById("u1");
        Assert.IsNotNull(loaded);
        Assert.AreEqual("Alice", loaded.NickName);
        Assert.AreEqual(1000, loaded.Score);
    }

    [Test]
    public void Rankings_DeleteAll_RemovesAllEntries()
    {
        _db.Rankings.Upsert(RankingDB.FromDomain(new Ranking("A", 100, 10, "")));
        _db.Rankings.Upsert(RankingDB.FromDomain(new Ranking("B", 200, 20, "")));

        _db.Rankings.DeleteAll();

        Assert.AreEqual(0, _db.Rankings.Count());
    }

    // [Test]
    // public void Rankings_OrderByScore_ReturnsCorrectOrder()
    // {
    //     _db.Rankings.Upsert(RankingDB.FromDomain(new Ranking("A", 100, 10, "")));
    //     _db.Rankings.Upsert(RankingDB.FromDomain(new Ranking("B", 500, 50, "")));
    //     _db.Rankings.Upsert(RankingDB.FromDomain(new Ranking("C", 300, 30, "")));

    //     var ordered = _db.Rankings.FindAll()
    //                      .OrderByDescending(r => r.Score)
    //                      .ToList();

    //     Assert.AreEqual(ordered[0].WeekScore); // 500
    //     Assert.AreEqual(ordered[1].WeekScore); // 300
    //     Assert.AreEqual(ordered[2].WeekScore); // 100
    // }

    // ═══════════════════════════════════════════════════════
    // UserDataDB — conversão domain ↔ DB
    // ═══════════════════════════════════════════════════════

    [Test]
    public void UserDataDB_FromDomain_MapsAllFields()
    {
        var domain = MakeUser("u1", score: 100, weekScore: 50, nickName: "Alice");
        domain.ProfileImageUrl = "https://img.url";
        domain.PlayerLevel     = 3;
        domain.TotalValidQuestionsAnswered = 42;
        domain.AnsweredQuestions["DB1"]    = new List<int> { 1, 2, 3 };

        var db = UserDataDB.FromDomain(domain);

        Assert.AreEqual("u1",             db.UserId);
        Assert.AreEqual("Alice",          db.NickName);
        Assert.AreEqual(100,              db.Score);
        Assert.AreEqual(50,               db.WeekScore);
        Assert.AreEqual(3,                db.PlayerLevel);
        Assert.AreEqual(42,               db.TotalValidQuestionsAnswered);
        Assert.AreEqual("https://img.url", db.ProfileImageUrl);
        Assert.AreEqual(3,                db.AnsweredQuestions["DB1"].Count);
    }

    [Test]
    public void UserDataDB_ToDomain_MapsAllFields()
    {
        var db = new UserDataDB
        {
            UserId      = "u1",
            NickName    = "Bob",
            Score       = 200,
            WeekScore   = 80,
            PlayerLevel = 5,
            ProfileImageUrl = "https://img.url",
            TotalValidQuestionsAnswered = 10,
            AnsweredQuestions = new Dictionary<string, List<int>> { ["DB1"] = new List<int> { 1, 2 } }
        };

        var domain = db.ToDomain();

        Assert.AreEqual("u1",  domain.UserId);
        Assert.AreEqual("Bob", domain.NickName);
        Assert.AreEqual(200,   domain.Score);
        Assert.AreEqual(80,    domain.WeekScore);
        Assert.AreEqual(5,     domain.PlayerLevel);
        Assert.AreEqual(10,    domain.TotalValidQuestionsAnswered);
        Assert.AreEqual(2,     domain.AnsweredQuestions["DB1"].Count);
    }

    [Test]
    public void UserDataDB_FromDomain_SetsIsDirtyFalse()
    {
        var db = UserDataDB.FromDomain(MakeUser("u1"));
        Assert.IsFalse(db.IsDirty);
    }

    [Test]
    public void UserDataDB_FromDomain_PreservesSavedAt()
    {
        var domain = MakeUser("u1");
        domain.SavedAt = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        var db = UserDataDB.FromDomain(domain);

        Assert.AreEqual(domain.SavedAt, db.SavedAt);
    }

    [Test]
    public void UserDataDB_ToDomain_PreservesSavedAt()
    {
        var savedAt = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var db = new UserDataDB { UserId = "u1", SavedAt = savedAt };

        var domain = db.ToDomain();

        Assert.AreEqual(savedAt, domain.SavedAt);
    }

    // ═══════════════════════════════════════════════════════
    // UserDataSyncService — TrySyncPendingData
    // ═══════════════════════════════════════════════════════
    [UnityTest]
    public IEnumerator TrySyncPendingData_WhenNoLocalUser_FetchesFromFirestore()
    {
        var remoteUser = MakeUser("u1", score: 500);
        _firestore.SetFakeUser(remoteUser);

        var task = _syncService.TrySyncPendingData("u1");
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsNotNull(UserDataStore.CurrentUserData);
        Assert.AreEqual(500, UserDataStore.CurrentUserData.Score);
        Assert.IsTrue(_repo.HasUser("u1")); // deve ter salvo no LiteDB
    }

    [UnityTest]
    public IEnumerator  TrySyncPendingData_WhenCacheDirty_SendsToFirestore()
    {
        var localUser = MakeUser("u1", score: 300);
        _repo.SaveUser(localUser);
        _repo.MarkAsDirty("u1");

        var task = _syncService.TrySyncPendingData("u1");
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsFalse(_repo.IsDirty("u1")); // deve ter marcado synced
    }

    [UnityTest]
    public IEnumerator TrySyncPendingData_WhenCacheValidAndClean_LoadsFromLiteDB_WithoutCallingFirestore()
    {
        var localUser = MakeUser("u1", score: 200);
        _repo.SaveUser(localUser);
        _repo.MarkAsSynced("u1"); // cache recente e limpo

        var task = _syncService.TrySyncPendingData("u1");
        yield return new WaitUntil(() => task.IsCompleted);

        // Cache válido — não deve ter buscado do Firestore
        // (FakeFirestoreRepository não tem o usuário, então se tivesse buscado,
        //  CurrentUserData seria null)
        Assert.AreEqual(200, UserDataStore.CurrentUserData?.Score);
    }

    [UnityTest]
    public IEnumerator TrySyncPendingData_WhenFirestoreHasNoUser_UsesLiteDBFallback()
    {
        var localUser = MakeUser("u1", score: 150);
        _repo.SaveUser(localUser);
        // IsDirty = false mas cache stale (GetLastSyncedAt = MinValue)
        // Firestore não tem o usuário → fallback para LiteDB

        var task = _syncService.TrySyncPendingData("u1");
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(150, UserDataStore.CurrentUserData?.Score);
    }

    // ═══════════════════════════════════════════════════════
    // UserDataSyncService — MergeWithFirestore (SavedAt)
    // ═══════════════════════════════════════════════════════
    [UnityTest]
    public IEnumerator MergeWithFirestore_WhenLocalMoreRecent_UsesLocalData()
    {
        var localUser = MakeUser("u1", score: 300);
        localUser.SavedAt = new DateTime(2026, 4, 13, 12, 0, 0, DateTimeKind.Utc); // mais recente
        _repo.SaveUser(localUser);

        var remoteUser = MakeUser("u1", score: 100);
        remoteUser.SavedAt = new DateTime(2026, 4, 13, 10, 0, 0, DateTimeKind.Utc); // mais antigo
        _firestore.SetFakeUser(remoteUser);

        var task = _syncService.TrySyncPendingData("u1");
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(300, UserDataStore.CurrentUserData?.Score);
    }

    [UnityTest]
    public IEnumerator MergeWithFirestore_WhenRemoteMoreRecent_UsesRemoteData()
    {
        // Local mais antigo
        var localUser = MakeUser("u1", score: 100);
        localUser.SavedAt = DateTime.Now.AddMinutes(-10);
        _repo.SaveUser(localUser);

        // Remoto mais recente
        var remoteUser = MakeUser("u1", score: 500);
        remoteUser.SavedAt = DateTime.Now;
        _firestore.SetFakeUser(remoteUser);

        var task = _syncService.TrySyncPendingData("u1");
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(500, UserDataStore.CurrentUserData?.Score);
    }

    [UnityTest]
    public IEnumerator MergeWithFirestore_WhenRemoteHasNoSavedAt_LocalWins()
    {
        // Simula usuário antigo sem SavedAt no Firestore (DateTime.MinValue)
        var localUser = MakeUser("u1", score: 200);
        localUser.SavedAt = DateTime.Now;
        _repo.SaveUser(localUser);

        var remoteUser = MakeUser("u1", score: 100);
        remoteUser.SavedAt = DateTime.MinValue; // sem SavedAt
        _firestore.SetFakeUser(remoteUser);

        var task = _syncService.TrySyncPendingData("u1");
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(200, UserDataStore.CurrentUserData?.Score);
    }

    [UnityTest]
    public IEnumerator MergeWithFirestore_WhenLocalMoreRecent_MarksDirtyForSync()
    {
        // Local mais recente — deve marcar dirty para sincronizar ao Firestore depois
        var localUser = MakeUser("u1", score: 300);
        localUser.SavedAt = DateTime.Now.AddMinutes(5); // definitivamente mais recente
        _repo.SaveUser(localUser);

        var remoteUser = MakeUser("u1", score: 100);
        remoteUser.SavedAt = DateTime.MinValue;
        _firestore.SetFakeUser(remoteUser);

        var task = _syncService.TrySyncPendingData("u1");
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.IsTrue(_repo.IsDirty("u1"));
    }

    // ═══════════════════════════════════════════════════════
    // UserDataSyncService — UpdateUserScores
    // ═══════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator UpdateUserScores_UpdatesScoreInLiteDB()
    {
        var user = MakeUser("u1", score: 100, weekScore: 50);
        _repo.SaveUser(user);
        UserDataStore.CurrentUserData = user;

        var task = _syncService.UpdateUserScores("u1", additionalScore: 5,
            questionNumber: 10, databankName: "AminoacidDB", isCorrect: true);
            yield return new WaitUntil(() => task.IsCompleted);

        var loaded = _repo.GetUser("u1");
        Assert.AreEqual(105, loaded.Score);
        Assert.AreEqual(55,  loaded.WeekScore);
    }

    [UnityTest]
    public IEnumerator UpdateUserScores_UpdatesUserDataStore()
    {
        var user = MakeUser("u1", score: 100, weekScore: 50);
        _repo.SaveUser(user);
        UserDataStore.CurrentUserData = user;

        var task = _syncService.UpdateUserScores("u1", 5, 10, "AminoacidDB", true);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.AreEqual(105, UserDataStore.CurrentUserData.Score);
        Assert.AreEqual(55,  UserDataStore.CurrentUserData.WeekScore);
    }

    [UnityTest]
    public IEnumerator UpdateUserScores_WhenCorrect_AddsAnsweredQuestion()
    {
        var user = MakeUser("u1", score: 100);
        _repo.SaveUser(user);
        UserDataStore.CurrentUserData = user;

        var task = _syncService.UpdateUserScores("u1", 5, 42, "AminoacidDB", isCorrect: true);
        yield return new WaitUntil(() => task.IsCompleted);

        var loaded = _repo.GetUser("u1");
        Assert.IsTrue(loaded.AnsweredQuestions.ContainsKey("AminoacidDB"));
        Assert.Contains(42, loaded.AnsweredQuestions["AminoacidDB"]);
    }

    [UnityTest]
    public IEnumerator UpdateUserScores_WhenIncorrect_DoesNotAddAnsweredQuestion()
    {
        var user = MakeUser("u1", score: 100);
        _repo.SaveUser(user);
        UserDataStore.CurrentUserData = user;

        var task = _syncService.UpdateUserScores("u1", -2, 42, "AminoacidDB", isCorrect: false);
        yield return new WaitUntil(() => task.IsCompleted);

        var loaded = _repo.GetUser("u1");
        Assert.IsFalse(loaded.AnsweredQuestions.ContainsKey("AminoacidDB"));
    }

    [UnityTest]
    public IEnumerator UpdateUserScores_ScoreCannotGoBelowZero()
    {
        var user = MakeUser("u1", score: 1);
        _repo.SaveUser(user);
        UserDataStore.CurrentUserData = user;

        var task = _syncService.UpdateUserScores("u1", -10, 0, "", isCorrect: false);
        yield return new WaitUntil(() => task.IsCompleted);

        Assert.GreaterOrEqual(_repo.GetUser("u1").Score, 0);
        Assert.GreaterOrEqual(UserDataStore.CurrentUserData.Score, 0);
    }

    [Test]
    [Ignore("UpdateUserScores usa Task.Yield() que trava em EditMode")]
    public void UpdateUserScores_WhenUserNotInLiteDB_DoesNotThrow()
    {
        UserDataStore.CurrentUserData = MakeUser("ghost", score: 100);
        Assert.DoesNotThrowAsync(() =>
            _syncService.UpdateUserScores("ghost", 5, 1, "DB", true));
    }

    // ═══════════════════════════════════════════════════════
    // UserDataStore — mutations
    // ═══════════════════════════════════════════════════════

    [Test]
    public void UserDataStore_SetCurrentUserData_PersistsInMemory()
    {
        var user = MakeUser("u1", score: 100);
        UserDataStore.CurrentUserData = user;
        Assert.AreEqual(100, UserDataStore.CurrentUserData.Score);
    }

    [Test]
    public void UserDataStore_UpdateScore_UpdatesValueInMemory()
    {
        UserDataStore.CurrentUserData = MakeUser("u1", score: 100);
        UserDataStore.UpdateScore(200);
        Assert.AreEqual(200, UserDataStore.CurrentUserData.Score);
    }

    [Test]
    public void UserDataStore_UpdateWeekScore_UpdatesValueInMemory()
    {
        UserDataStore.CurrentUserData = MakeUser("u1", weekScore: 50);
        UserDataStore.UpdateWeekScore(150);
        Assert.AreEqual(150, UserDataStore.CurrentUserData.WeekScore);
    }

    [Test]
    public void UserDataStore_AddScore_IncrementsBothScores()
    {
        UserDataStore.CurrentUserData = MakeUser("u1", score: 100, weekScore: 50);
        UserDataStore.AddScore(10);
        Assert.AreEqual(110, UserDataStore.CurrentUserData.Score);
        Assert.AreEqual(60,  UserDataStore.CurrentUserData.WeekScore);
    }

    [Test]
    public void UserDataStore_UpdatePlayerLevel_UpdatesValueInMemory()
    {
        UserDataStore.CurrentUserData = MakeUser("u1");
        UserDataStore.UpdatePlayerLevel(5);
        Assert.AreEqual(5, UserDataStore.CurrentUserData.PlayerLevel);
    }

    [Test]
    public void UserDataStore_UpdateTotalValidQuestionsAnswered_UpdatesValueInMemory()
    {
        UserDataStore.CurrentUserData = MakeUser("u1");
        UserDataStore.UpdateTotalValidQuestionsAnswered(42);
        Assert.AreEqual(42, UserDataStore.CurrentUserData.TotalValidQuestionsAnswered);
    }

    [Test]
    public void UserDataStore_Clear_SetsCurrentUserDataToNull()
    {
        UserDataStore.CurrentUserData = MakeUser("u1");
        UserDataStore.Clear();
        Assert.IsNull(UserDataStore.CurrentUserData);
    }

    [Test]
    public void UserDataStore_UpdateScore_WhenNoCurrentUser_DoesNotThrow()
    {
        UserDataStore.Clear();
        Assert.DoesNotThrow(() => UserDataStore.UpdateScore(100));
    }

    [Test]
    public void UserDataStore_MarkDatabankAsReset_StoresFlag()
    {
        UserDataStore.CurrentUserData = MakeUser("u1");
        UserDataStore.MarkDatabankAsReset("AminoacidDB", true);
        Assert.IsTrue(UserDataStore.IsDatabankReset("AminoacidDB"));
    }

    [Test]
    public void UserDataStore_IsDatabankReset_WhenNotSet_ReturnsFalse()
    {
        UserDataStore.CurrentUserData = MakeUser("u1");
        Assert.IsFalse(UserDataStore.IsDatabankReset("AminoacidDB"));
    }

    // ═══════════════════════════════════════════════════════
    // QuestionDB — conversão domain ↔ DB
    // ═══════════════════════════════════════════════════════

    [Test]
    public void QuestionDB_FromDomain_MapsOldFields()
    {
        var q = QuestionTestHelpers.MakeQuestion(number: 5, level: 2,
            databankName: "MembranesQuestionDatabase");

        var db = QuestionDB.FromDomain(q);

        Assert.AreEqual("MembranesQuestionDatabase", db.QuestionDatabankName);
        Assert.AreEqual(5,   db.QuestionNumber);
        Assert.AreEqual(2,   db.QuestionLevel);
        Assert.AreEqual("Questão 5", db.QuestionText);
        Assert.AreEqual(4,   db.Answers.Length);
        Assert.AreEqual(0,   db.CorrectIndex);
        Assert.IsFalse(db.QuestionInDevelopment);
    }

    [Test]
    public void QuestionDB_FromDomain_MapsNewFirestoreFields()
    {
        var q = QuestionTestHelpers.MakeFullQuestion(number: 1,
            databankName: "WaterQuestionDataBase",
            topic: "water",
            bloomLevel: "understand");

        var db = QuestionDB.FromDomain(q);

        Assert.AreEqual("WaterQuestionDataBase_001", db.GlobalId);
        Assert.AreEqual("water",      db.Topic);
        Assert.AreEqual("understand", db.BloomLevel);
        Assert.AreEqual("subtopico-1", db.Subtopic);
        Assert.AreEqual(2,            db.ConceptTags.Count);
    }

    [Test]
    public void QuestionDB_FromDomain_MapsQuestionHint()
    {
        var q = QuestionTestHelpers.MakeQuestion(1, databankName: "TestDB");
        q.questionHint = new QuestionHint { text = "Dica aqui", videoUrl = "https://video.url" };

        var db = QuestionDB.FromDomain(q);

        Assert.AreEqual("Dica aqui",          db.HintText);
        Assert.AreEqual("https://video.url",  db.HintVideoUrl);
        Assert.AreEqual("",                   db.HintImagePath);
        Assert.AreEqual("",                   db.HintLink);
    }

    [Test]
    public void QuestionDB_ToDomain_MapsOldFields()
    {
        var db = QuestionDB.FromDomain(
            QuestionTestHelpers.MakeQuestion(number: 3, level: 1,
                databankName: "EnzymeQuestionDataBase"));

        var domain = db.ToDomain();

        Assert.AreEqual("EnzymeQuestionDataBase", domain.questionDatabankName);
        Assert.AreEqual(3, domain.questionNumber);
        Assert.AreEqual(1, domain.questionLevel);
        Assert.AreEqual(4, domain.answers.Length);
    }

    [Test]
    public void QuestionDB_ToDomain_MapsNewFirestoreFields()
    {
        var db = QuestionDB.FromDomain(
            QuestionTestHelpers.MakeFullQuestion(1, topic: "lipids", bloomLevel: "apply",
                databankName: "LipidsQuestionDataBase"));

        var domain = db.ToDomain();

        Assert.AreEqual("LipidsQuestionDataBase_001", domain.globalId);
        Assert.AreEqual("lipids", domain.topic);
        Assert.AreEqual("apply",  domain.bloomLevel);
        Assert.IsNotNull(domain.conceptTags);
        Assert.IsNotNull(domain.prerequisites);
    }

    [Test]
    public void QuestionDB_ToDomain_ReconstroisQuestionHint()
    {
        var q = QuestionTestHelpers.MakeQuestion(1, databankName: "TestDB");
        q.questionHint = new QuestionHint { text = "Dica", link = "https://ref.url" };

        var domain = QuestionDB.FromDomain(q).ToDomain();

        Assert.IsNotNull(domain.questionHint);
        Assert.AreEqual("Dica",             domain.questionHint.text);
        Assert.AreEqual("https://ref.url",  domain.questionHint.link);
        Assert.IsTrue(domain.questionHint.HasAnyHint);
    }

    [Test]
    public void QuestionDB_FromDomain_GlobalId_GeneradoAutomaticamente_QuandoVazio()
    {
        var q = QuestionTestHelpers.MakeQuestion(7, databankName: "ProteinQuestionDataBase");
        q.globalId = null; // sem globalId

        var db = QuestionDB.FromDomain(q);

        Assert.AreEqual("ProteinQuestionDataBase_007", db.GlobalId);
    }

    [Test]
    public void QuestionDB_FromDomain_SetsCachedAtToNow()
    {
        var before = DateTime.Now.AddSeconds(-1);
        var db = QuestionDB.FromDomain(QuestionTestHelpers.MakeQuestion(1));
        Assert.GreaterOrEqual(db.CachedAt, before);
    }

    // ═══════════════════════════════════════════════════════
    // QuestionLocalRepository — integração com FakeLiteDBManager
    // ═══════════════════════════════════════════════════════

    [Test]
    public void QuestionLocalRepository_SaveAndGet_CicloCompleto()
    {
        var questions = QuestionTestHelpers.MakeQuestions(
            nivel1: 4, nivel2: 2, databankName: "MembranesQuestionDatabase");

        _questionRepo.SaveQuestions(questions);
        var result = _questionRepo.GetQuestionsByDatabankName("MembranesQuestionDatabase");

        Assert.AreEqual(6, result.Count);
        Assert.IsTrue(result.All(q => q.questionDatabankName == "MembranesQuestionDatabase"));
    }

    [Test]
    public void QuestionLocalRepository_GetByDatabankName_NaoRetornaOutrosBancos()
    {
        var acidsQuestions = QuestionTestHelpers.MakeQuestions(3,
            databankName: "AcidBaseBufferQuestionDatabase");
        var waterQuestions = QuestionTestHelpers.MakeQuestions(5,
            databankName: "WaterQuestionDataBase");

        _questionRepo.SaveQuestions(acidsQuestions);
        _questionRepo.SaveQuestions(waterQuestions);

        var result = _questionRepo.GetQuestionsByDatabankName("WaterQuestionDataBase");

        Assert.AreEqual(5, result.Count);
        Assert.IsTrue(result.All(q => q.questionDatabankName == "WaterQuestionDataBase"));
    }

    [Test]
    public void QuestionLocalRepository_GetAllQuestions_RetornaTodosOsBancos()
    {
        _questionRepo.SaveQuestions(QuestionTestHelpers.MakeQuestions(3, databankName: "BankA"));
        _questionRepo.SaveQuestions(QuestionTestHelpers.MakeQuestions(4, databankName: "BankB"));

        var all = _questionRepo.GetAllQuestions();

        Assert.AreEqual(7, all.Count);
    }

    [Test]
    public void QuestionLocalRepository_HasAnyQuestions_FalseQuandoVazio()
    {
        Assert.IsFalse(_questionRepo.HasAnyQuestions());
    }

    [Test]
    public void QuestionLocalRepository_HasAnyQuestions_TrueAposSalvar()
    {
        _questionRepo.SaveQuestions(QuestionTestHelpers.MakeQuestions(1, databankName: "TestDB"));
        Assert.IsTrue(_questionRepo.HasAnyQuestions());
    }

    [Test]
    public void QuestionLocalRepository_GetLatestCacheTimestamp_MinValueQuandoVazio()
    {
        Assert.AreEqual(DateTime.MinValue, _questionRepo.GetLatestCacheTimestamp());
    }

    [Test]
    public void QuestionLocalRepository_GetLatestCacheTimestamp_RetornaDataRecente()
    {
        var before = DateTime.Now.AddSeconds(-1);
        _questionRepo.SaveQuestions(QuestionTestHelpers.MakeQuestions(3, databankName: "TestDB"));

        Assert.GreaterOrEqual(_questionRepo.GetLatestCacheTimestamp(), before);
    }

    [Test]
    public void QuestionLocalRepository_ClearAll_RemoveTodasAsQuestoes()
    {
        _questionRepo.SaveQuestions(QuestionTestHelpers.MakeQuestions(5, databankName: "TestDB"));

        _questionRepo.ClearAll();

        Assert.IsFalse(_questionRepo.HasAnyQuestions());
        Assert.AreEqual(0, _questionRepo.GetAllQuestions().Count);
        Assert.AreEqual(DateTime.MinValue, _questionRepo.GetLatestCacheTimestamp());
    }

    [Test]
    public void QuestionLocalRepository_SaveQuestions_Upsert_NaoDuplicaQuestoes()
    {
        var questions = QuestionTestHelpers.MakeQuestions(3, databankName: "TestDB");
        _questionRepo.SaveQuestions(questions);
        _questionRepo.SaveQuestions(questions); // mesmas questões, segundo save

        Assert.AreEqual(3, _questionRepo.GetAllQuestions().Count);
    }

    [Test]
    public void QuestionLocalRepository_NovoCamposFirestore_SobrevivemAoCicloSaveGet()
    {
        var q = QuestionTestHelpers.MakeFullQuestion(number: 1,
            databankName: "TestDB", topic: "enzymes", bloomLevel: "analyze");

        _questionRepo.SaveQuestions(new List<Question> { q });
        var result = _questionRepo.GetQuestionsByDatabankName("TestDB");

        Assert.AreEqual(1, result.Count);
        var saved = result[0];
        Assert.AreEqual("TestDB_001",  saved.globalId);
        Assert.AreEqual("enzymes",     saved.topic);
        Assert.AreEqual("analyze",     saved.bloomLevel);
        Assert.AreEqual("subtopico-1", saved.subtopic);
        Assert.IsNotNull(saved.conceptTags);
        Assert.AreEqual(2, saved.conceptTags.Count);
        Assert.IsNotNull(saved.questionHint);
        Assert.AreEqual("Dica da questão 1", saved.questionHint.text);
    }

    // ═══════════════════════════════════════════════════════
    // Helper
    // ═══════════════════════════════════════════════════════

    private static UserData MakeUser(
        string userId    = "user1",
        int    score     = 0,
        int    weekScore = 0,
        string nickName  = "TestUser") => new UserData
    {
        UserId      = userId,
        NickName    = nickName,
        Name        = "Test Name",
        Email       = "test@test.com",
        Score       = score,
        WeekScore   = weekScore,
        PlayerLevel = 1,
        CreatedTime = DateTime.Now,
        AnsweredQuestions  = new Dictionary<string, List<int>>(),
        ResetDatabankFlags = new Dictionary<string, bool>()
    };
}
