using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;

public class FirestoreRepository : MonoBehaviour, IFirestoreRepository
{
    private FirebaseFirestore db;
    private bool isInitialized;
    public bool IsInitialized => isInitialized;
    private ListenerRegistration _userDataListener;
    private ListenerRegistration _answeredQuestionsListener;
    public bool IsListening => _userDataListener != null;

    // -------------------------------------------------------
    // Inicialização
    // -------------------------------------------------------

    public void Initialize()
    {
        if (isInitialized) return;

        try
        {
            db = FirebaseFirestore.DefaultInstance;
            isInitialized = true;
            Debug.Log("Firestore initialized successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Firestore initialization failed: {e.Message}");
            throw;
        }
    }

    // -------------------------------------------------------
    // Leitura
    // -------------------------------------------------------

    public async Task<UserData> GetUserData(string userId)
    {
        try
        {
            DocumentSnapshot snapshot = await db.Collection("Users").Document(userId).GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Debug.LogError($"Documento do usuário {userId} não encontrado");
                return null;
            }

            UserData user = ConvertFromFirestore(snapshot.ToDictionary());
            Debug.Log($"[FirestoreRepository] UserData carregado - NickName: {user.NickName}, Score: {user.Score}");
            return user;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao buscar dados do usuário: {ex.Message}");
            throw;
        }
    }

    public async Task<List<UserData>> GetAllUsersData()
    {
        try
        {
            if (!isInitialized) throw new Exception("Firestore não inicializado");

            QuerySnapshot querySnapshot = await db.Collection("Users").GetSnapshotAsync();
            List<UserData> users = new List<UserData>();

            foreach (DocumentSnapshot doc in querySnapshot.Documents)
                users.Add(ConvertFromFirestore(doc.ToDictionary()));

            return users;
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao buscar todos os usuários: {e.Message}");
            throw;
        }
    }

    public async Task<bool> AreNicknameTaken(string nickName)
    {
        DocumentSnapshot snapshot = await db.Collection("Nicknames")
            .Document(nickName.ToLower())
            .GetSnapshotAsync();
        return snapshot.Exists;
    }

    // -------------------------------------------------------
    // Criação
    // -------------------------------------------------------

    public async Task CreateUserDocument(UserData userData)
    {
        try
        {
            if (!isInitialized) throw new Exception("Firestore não inicializado");
            if (string.IsNullOrEmpty(userData.UserId))
                throw new ArgumentException("UserId não pode ser vazio");

            var requiredFields = new Dictionary<string, object>
            {
                { "UserId",              userData.UserId },
                { "NickName",            userData.NickName },
                { "Name",                userData.Name },
                { "Email",               userData.Email },
                { "Score",               userData.Score },
                { "WeekScore",           userData.WeekScore },
                { "QuestionTypeProgress",userData.QuestionTypeProgress },
                { "IsUserRegistered",    userData.IsUserRegistered },
                { "CreatedTime",         Timestamp.FromDateTime(
                    DateTime.SpecifyKind(userData.CreatedTime, DateTimeKind.Utc)) },
                { "ProfileImageUrl",     userData.ProfileImageUrl },
                { "AnsweredQuestions",   new Dictionary<string, object>() }
            };

            DocumentReference docRef = db.Collection("Users").Document(userData.UserId);
            await docRef.SetAsync(requiredFields);

            await db.Collection("Nicknames")
                .Document(userData.NickName.ToLower())
                .SetAsync(new Dictionary<string, object> { { "userId", userData.UserId } });

            Debug.Log($"Documento do usuário criado com sucesso: {userData.UserId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao criar documento do usuário: {e.Message}");
            throw;
        }
    }

    // -------------------------------------------------------
    // Atualização de score e questões respondidas
    // -------------------------------------------------------

    /// <summary>
    /// Versão principal chamada pelo UserDataSyncService (background).
    /// Usa os valores já calculados no UserDataStore — sem transaction,
    /// sem read-modify-write, sem risco de deadlock no Android.
    /// FieldValue.ArrayUnion garante atomicidade no servidor para AnsweredQuestions.
    /// </summary>
    public async Task UpdateUserScores(
        string userId,
        int additionalScore,
        int questionNumber,
        string databankName,
        bool isCorrect,
        UserData capturedUserData)
    {
        if (!isInitialized) throw new Exception("Firestore não inicializado");

        var current = UserDataStore.CurrentUserData;
        if (current == null || current.UserId != userId)
        {
            Debug.LogError("[FirestoreRepository] CurrentUserData inválido em UpdateUserScores.");
            return;
        }

        if (capturedUserData == null || capturedUserData.UserId != userId)
        {
            Debug.LogError("[FirestoreRepository] capturedUserData inválido em UpdateUserScores.");
            return;
        }

        var updates = new Dictionary<string, object>
        {
            { "Score",     current.Score },
            { "WeekScore", current.WeekScore },
            { "SavedAt", FieldValue.ServerTimestamp }
        };

        if (isCorrect && !string.IsNullOrEmpty(databankName) && questionNumber > 0)
        {
            // ArrayUnion é atômico no servidor — não sobrescreve outros bancos,
            // não precisa ler o documento antes de escrever
            updates[$"AnsweredQuestions.{databankName}"] = FieldValue.ArrayUnion(questionNumber);
        }

        var docRef = db.Collection("Users").Document(userId);
        await docRef.UpdateAsync(updates).ConfigureAwait(false);

        Debug.Log($"[FirestoreRepository] UpdateUserScores concluído. Score={current.Score}");
    }

    /// <summary>
    /// Versão legada — mantida para compatibilidade com código que ainda a chama.
    /// Substituída pelo fluxo principal do UserDataSyncService → UpdateUserScores.
    /// NÃO adicionar novos usos desta versão.
    /// </summary>
    public async Task UpdateUserScore(string userId, int newScore,
        int questionNumber, string databankName, bool isCorrect)
    {
        if (!isInitialized) throw new Exception("Firestore não inicializado");

        var updates = new Dictionary<string, object> { { "Score", newScore } };

        if (isCorrect && !string.IsNullOrEmpty(databankName) && questionNumber > 0)
            updates[$"AnsweredQuestions.{databankName}"] = FieldValue.ArrayUnion(questionNumber);

        var docRef = db.Collection("Users").Document(userId);
        await docRef.UpdateAsync(updates).ConfigureAwait(false);

        Debug.Log($"[FirestoreRepository] UpdateUserScore concluído. Score={newScore}");
    }

    /// <summary>
    /// Atualiza apenas o WeekScore. Usa valor já calculado do UserDataStore —
    /// sem transaction, sem deadlock.
    /// </summary>
    public async Task UpdateUserWeekScore(string userId, int additionalScore)
    {
        if (!isInitialized) throw new Exception("Firestore não inicializado");

        var current = UserDataStore.CurrentUserData;
        if (current == null || current.UserId != userId) return;

        var docRef = db.Collection("Users").Document(userId);
        await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "WeekScore", current.WeekScore },
            { "SavedAt", FieldValue.ServerTimestamp }
        }).ConfigureAwait(false);

        Debug.Log($"[FirestoreRepository] WeekScore atualizado: {current.WeekScore}");
    }

    // -------------------------------------------------------
    // Atualização de dados gerais
    // -------------------------------------------------------
    public async Task UpdateUserData(UserData userData)
    {
        try
        {
            if (!isInitialized) throw new Exception("Firestore não inicializado");
            if (string.IsNullOrEmpty(userData.UserId))
                throw new ArgumentException("UserId não pode ser vazio");

            DocumentReference docRef = db.Collection("Users").Document(userData.UserId);
            await docRef.UpdateAsync(ConvertToFirestore(userData));
            Debug.Log($"Dados do usuário {userData.UserId} atualizados com sucesso");
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao atualizar dados do usuário: {e.Message}");
            throw;
        }
    }

    public async Task UpdateUserProgress(string userId, int progress)
    {
        try
        {
            if (!isInitialized) throw new Exception("Firestore não inicializado");

            DocumentReference docRef = db.Collection("Users").Document(userId);
            await docRef.UpdateAsync(new Dictionary<string, object> { { "Progress", progress } });
            Debug.Log($"Progresso do usuário atualizado para {progress}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao atualizar progresso do usuário: {e.Message}");
            throw;
        }
    }

    public async Task UpdateUserProfileImageUrl(string userId, string imageUrl)
    {
        try
        {
            DocumentReference userRef = db.Collection("Users").Document(userId);
            await userRef.UpdateAsync(new Dictionary<string, object> { 
                { "ProfileImageUrl", imageUrl },
                { "SavedAt", FieldValue.ServerTimestamp } 
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao atualizar URL da imagem de perfil: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateUserField(string userId, string fieldName, object value)
    {
        try
        {
            if (!isInitialized) throw new Exception("Firestore não inicializado");

            DocumentReference docRef = db.Collection("Users").Document(userId);
            await docRef.UpdateAsync(new Dictionary<string, object> { { fieldName, value } });
            Debug.Log($"{fieldName} atualizado para {value}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao atualizar {fieldName}: {e.Message}");
            throw;
        }
    }

    // -------------------------------------------------------
    // Reset de questões respondidas
    // -------------------------------------------------------
    public async Task ResetAnsweredQuestions(string userId, string databankName)
    {
        try
        {
            if (!isInitialized) throw new Exception("Firestore não inicializado");

            DocumentReference docRef = db.Collection("Users").Document(userId);

            // RunTransactionAsync é aceitável aqui: ResetAnsweredQuestions é uma
            // operação administrativa rara, não chamada no fluxo de resposta de questões
            await db.RunTransactionAsync(async transaction =>
            {
                DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(docRef);

                if (snapshot.Exists)
                {
                    Dictionary<string, List<int>> answeredQuestions =
                        snapshot.GetValue<Dictionary<string, List<int>>>("AnsweredQuestions");

                    if (answeredQuestions != null && answeredQuestions.ContainsKey(databankName))
                    {
                        answeredQuestions.Remove(databankName);
                        transaction.Update(docRef, "AnsweredQuestions", answeredQuestions);
                    }
                    else
                    {
                        Debug.LogWarning($"AnsweredQuestions para {databankName} não encontrada para o usuário {userId}");
                    }
                }
                else
                {
                    Debug.LogError($"Usuário {userId} não encontrado!");
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao remover AnsweredQuestions: {e.Message}");
            throw;
        }
    }

    // -------------------------------------------------------
    // Listeners
    // -------------------------------------------------------
    public void ListenToUserData(
        string userId,
        Action<int> onScoreChanged = null,
        Action<int> onWeekScoreChanged = null,
        Action<Dictionary<string, List<int>>> onAnsweredQuestionsChanged = null)
    {
        _userDataListener?.Stop();
        _userDataListener = db.Collection("Users").Document(userId)
        .Listen(snapshot =>
        {
            // Proteção contra snapshots inválidos por erro de rede
            try
            {
                if (snapshot == null || !snapshot.Exists) return;

                Dictionary<string, object> data;
                try
                {
                    data = snapshot.ToDictionary();
                }
                catch (Exception)
                {
                    // Snapshot inválido — conexão perdida, Firebase vai reconectar
                    return;
                }

                UserData currentUserData = UserDataStore.CurrentUserData;

                // Score
                if (data.ContainsKey("Score"))
                {
                    int incomingScore = Convert.ToInt32(data["Score"]);
                    if (currentUserData == null || incomingScore >= currentUserData.Score)
                    {
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            UserDataStore.UpdateScore(incomingScore);
                            onScoreChanged?.Invoke(incomingScore);
                        });
                    }
                }

                // WeekScore
                if (data.ContainsKey("WeekScore"))
                {
                    int incomingWeekScore = Convert.ToInt32(data["WeekScore"]);
                    if (currentUserData == null || incomingWeekScore >= currentUserData.WeekScore)
                    {
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            if (UserDataStore.CurrentUserData != null)
                                UserDataStore.UpdateWeekScore(incomingWeekScore);
                            onWeekScoreChanged?.Invoke(incomingWeekScore);
                        });
                    }
                }

                // TotalValidQuestionsAnswered
                if (data.ContainsKey("TotalValidQuestionsAnswered") && currentUserData != null)
                {
                    int total = Convert.ToInt32(data["TotalValidQuestionsAnswered"]);
                    if (currentUserData.TotalValidQuestionsAnswered != total)
                    {
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            var local = UserDataStore.CurrentUserData;
                            if (local != null)
                            {
                                local.TotalValidQuestionsAnswered = total;
                                UserDataStore.UpdateTotalValidQuestionsAnswered(total);
                            }
                        });
                    }
                }

                // PlayerLevel
                if (data.ContainsKey("PlayerLevel") && currentUserData != null)
                {
                    int level = Convert.ToInt32(data["PlayerLevel"]);
                    if (currentUserData.PlayerLevel != level)
                    {
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            var local = UserDataStore.CurrentUserData;
                            if (local != null)
                            {
                                local.PlayerLevel = level;
                                UserDataStore.UpdatePlayerLevel(level);
                            }
                        });
                    }
                }

                //listener to profileImage
                if (data.ContainsKey("ProfileImageUrl") && currentUserData != null)
                {
                    string incomingUrl = data["ProfileImageUrl"] as string ?? "";
                    
                    // Ignora se a URL incoming é vazia ou igual à atual
                    if (string.IsNullOrEmpty(incomingUrl)) return;
                    if (currentUserData.ProfileImageUrl == incomingUrl) return;
                    
                    // Ignora se há upload pendente — o path local deve ser preservado
                    var pendingUpload = AppContext.LocalDatabase?.PendingUploads
                                                .FindById(currentUserData.UserId);
                    if (pendingUpload != null)
                    {
                        Debug.Log("[FirestoreRepository] ProfileImageUrl ignorado — upload pendente.");
                        return;
                    }

                    // Ignora se a URL incoming é um path local (não começa com http)
                    if (!incomingUrl.StartsWith("http"))
                    {
                        Debug.Log("[FirestoreRepository] ProfileImageUrl ignorado — path local.");
                        return;
                    }

                    var capturedUrl = incomingUrl;
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        var local = UserDataStore.CurrentUserData;
                        if (local == null) return;
                        
                        // Verifica novamente se não há upload pendente no main thread
                        var pending = AppContext.LocalDatabase?.PendingUploads.FindById(local.UserId);
                        if (pending != null) return;
                        
                        local.ProfileImageUrl = capturedUrl;
                        UserDataStore.CurrentUserData = local;
                        UserAvatarSyncHelper.NotifyAvatarChanged(capturedUrl);
                        Debug.Log($"[FirestoreRepository] ProfileImageUrl atualizado via listener: {capturedUrl}");
                    });
                }
                                                                                                                                                                                                                                                                                                                                
                // AnsweredQuestions
                if (onAnsweredQuestionsChanged != null && data.ContainsKey("AnsweredQuestions"))
                {
                    try
                    {
                        var answeredQuestionsData = data["AnsweredQuestions"] 
                            as Dictionary<string, object>;
                        if (answeredQuestionsData == null) return;

                        var answeredQuestions = new Dictionary<string, List<int>>();
                        foreach (var kvp in answeredQuestionsData)
                        {
                            var questionsList = kvp.Value as IEnumerable<object>;
                            if (questionsList != null)
                            {
                                answeredQuestions[kvp.Key] = questionsList
                                    .Select(q => Convert.ToInt32(q))
                                    .ToList();
                            }
                        }

                        var captured = answeredQuestions;
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            var local = UserDataStore.CurrentUserData;
                            if (local != null)
                                local.AnsweredQuestions = captured;

                            foreach (var kvp in captured)
                            {
                                AnsweredQuestionsListStore.UpdateAnsweredQuestionsCount(
                                    userId, kvp.Key, kvp.Value.Count);
                            }

                            onAnsweredQuestionsChanged.Invoke(captured);
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[FirestoreRepository] Erro ao processar AnsweredQuestions: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Captura qualquer exceção do callback para evitar crash em thread do pool
                Debug.LogWarning($"[FirestoreRepository] Erro no listener (provável perda de conexão): {ex.Message}");
            }
        });
    }

    public IDisposable ListenToScore(string userId,
        Action<int> onScoreChanged,
        Action<int> onWeekScoreChanged)
    {
        _userDataListener?.Stop();
        _userDataListener = db.Collection("Users").Document(userId)
            .Listen(snapshot =>
            {
                if (!snapshot.Exists) return;
                var data = snapshot.ToDictionary();

                if (data.ContainsKey("Score"))
                {
                    int newScore = Convert.ToInt32(data["Score"]);
                    UserDataStore.UpdateScore(newScore);
                    onScoreChanged?.Invoke(newScore);
                }

                if (data.ContainsKey("WeekScore"))
                {
                    int newWeekScore = Convert.ToInt32(data["WeekScore"]);
                    UserDataStore.UpdateWeekScore(newWeekScore);
                    onWeekScoreChanged?.Invoke(newWeekScore);
                }
            });
        return null;
    }

    public IDisposable ListenToAnsweredQuestions(string userId,
        Action<Dictionary<string, List<int>>> onChanged)
    {
        _answeredQuestionsListener?.Stop();
        _answeredQuestionsListener = db.Collection("Users").Document(userId)
            .Listen(snapshot =>
            {
                if (!snapshot.Exists) return;
                var data = snapshot.ToDictionary();

                if (!data.ContainsKey("AnsweredQuestions")) return;

                try
                {
                    var answeredQuestions = new Dictionary<string, List<int>>();
                    var raw = data["AnsweredQuestions"] as Dictionary<string, object>;
                    if (raw == null) return;

                    foreach (var kvp in raw)
                    {
                        if (kvp.Value is IEnumerable<object> list)
                            answeredQuestions[kvp.Key] = list
                                .Select(q => Convert.ToInt32(q))
                                .ToList();
                    }

                    onChanged?.Invoke(answeredQuestions);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[FirestoreRepository] Erro no listener AnsweredQuestions: {ex.Message}");
                }
            });
        return null;
    }

    public void StopListening()
    {
        if (_userDataListener != null)
        {
            _userDataListener.Stop();
            _userDataListener = null;
            Debug.Log("[FirestoreRepository] Listener parado.");
        }
    }

    public void ResumeListening(string userId,
        Action<int> onScoreChanged = null,
        Action<int> onWeekScoreChanged = null,
        Action<Dictionary<string, List<int>>> onAnsweredQuestionsChanged = null)
    {
        ListenToUserData(userId, onScoreChanged, onWeekScoreChanged, onAnsweredQuestionsChanged);
        Debug.Log("[FirestoreRepository] Listener retomado.");
    }

    // -------------------------------------------------------
    // Deleção
    // -------------------------------------------------------

    public async Task DeleteDocument(string collection, string documentId)
    {
        try
        {
            if (!isInitialized) throw new Exception("Firestore não inicializado");

            var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
            if (user == null) throw new Exception("Usuário não está autenticado");

            string token = await user.TokenAsync(true);

            DocumentReference docRef = db.Collection(collection).Document(documentId);

            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await docRef.DeleteAsync();
                    Debug.Log($"Documento {documentId} deletado com sucesso da coleção {collection}");
                    return;
                }
                catch (Exception e) when (i < maxRetries - 1)
                {
                    Debug.LogWarning($"Tentativa {i + 1} falhou: {e.Message}. Tentando novamente...");
                    await Task.Delay(1000);
                    token = await user.TokenAsync(true);
                }
            }

            throw new Exception($"Falha ao deletar documento após {maxRetries} tentativas");
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao deletar documento: {e.Message}");
            throw;
        }
    }

    // -------------------------------------------------------
    // Manutenção / admin
    // -------------------------------------------------------

    public async Task EnsureWeekScoreField()
    {
        try
        {
            if (!isInitialized) throw new Exception("Firestore não inicializado");

            QuerySnapshot querySnapshot = await db.Collection("Users").GetSnapshotAsync();
            WriteBatch batch = db.StartBatch();
            int userCount = 0;

            foreach (DocumentSnapshot doc in querySnapshot.Documents)
            {
                if (!doc.ContainsField("WeekScore"))
                {
                    batch.Update(doc.Reference, "WeekScore", 0);
                    userCount++;

                    if (userCount >= 450)
                    {
                        await batch.CommitAsync();
                        batch = db.StartBatch();
                        userCount = 0;
                    }
                }
            }

            if (userCount > 0)
                await batch.CommitAsync();

            Debug.Log("Verificação de WeekScore concluída");
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao verificar campo WeekScore: {e.Message}");
            throw;
        }
    }

    // -------------------------------------------------------
    // Ciclo de vida
    // -------------------------------------------------------

    private void OnDestroy()
    {
        _userDataListener?.Stop();
        _answeredQuestionsListener?.Stop();
    }

    // -------------------------------------------------------
    // Conversão de dados
    // -------------------------------------------------------

    private UserData ConvertFromFirestore(Dictionary<string, object> data)
    {
        UserData userData = new UserData();

        userData.UserId      = data.ContainsKey("UserId")   ? (string)data["UserId"]   : "";
        userData.NickName    = data.ContainsKey("NickName")  ? (string)data["NickName"] : "";
        userData.Name        = data.ContainsKey("Name")      ? (string)data["Name"]     : "";
        userData.Email       = data.ContainsKey("Email")     ? (string)data["Email"]    : "";
        userData.ProfileImageUrl = data.ContainsKey("ProfileImageUrl")
            ? (string)data["ProfileImageUrl"] ?? ""
            : "";
        userData.Score     = data.ContainsKey("Score")     ? Convert.ToInt32(data["Score"])     : 0;
        userData.WeekScore = data.ContainsKey("WeekScore") ? Convert.ToInt32(data["WeekScore"]) : 0;
        userData.QuestionTypeProgress = data.ContainsKey("QuestionTypeProgress")
            ? Convert.ToInt32(data["QuestionTypeProgress"])
            : (data.ContainsKey("Progress") ? Convert.ToInt32(data["Progress"]) : 0);
        userData.PlayerLevel = data.ContainsKey("PlayerLevel")
            ? Convert.ToInt32(data["PlayerLevel"]) : 1;
        userData.TotalValidQuestionsAnswered = data.ContainsKey("TotalValidQuestionsAnswered")
            ? Convert.ToInt32(data["TotalValidQuestionsAnswered"]) : 0;
        userData.TotalQuestionsInAllDatabanks = data.ContainsKey("TotalQuestionsInAllDatabanks")
            ? Convert.ToInt32(data["TotalQuestionsInAllDatabanks"]) : 0;
        userData.IsUserRegistered = data.ContainsKey("IsUserRegistered")
            ? Convert.ToBoolean(data["IsUserRegistered"]) : false;
        userData.SavedAt = data.ContainsKey("SavedAt") && data["SavedAt"] is Timestamp savedAt 
            ? savedAt.ToDateTime() : DateTime.MinValue; 

        if (data.ContainsKey("CreatedTime") && data["CreatedTime"] is Timestamp timestamp)
            userData.CreatedTime = timestamp.ToDateTime();
        else
            userData.CreatedTime = DateTime.UtcNow;

        if (data.ContainsKey("ResetDatabankFlags") &&
            data["ResetDatabankFlags"] is Dictionary<string, object> resetFlagsData)
        {
            userData.ResetDatabankFlags = new Dictionary<string, bool>();
            foreach (var kvp in resetFlagsData)
                userData.ResetDatabankFlags[kvp.Key] = Convert.ToBoolean(kvp.Value);
        }

        userData.AnsweredQuestions = new Dictionary<string, List<int>>();
        if (data.ContainsKey("AnsweredQuestions") &&
            data["AnsweredQuestions"] is Dictionary<string, object> answeredQuestionsData)
        {
            foreach (var kvp in answeredQuestionsData)
            {
                if (kvp.Value is IEnumerable<object> list)
                    userData.AnsweredQuestions[kvp.Key] = list.Select(x => Convert.ToInt32(x)).ToList();
            }
        }

        return userData;
    }

    private Dictionary<string, object> ConvertToFirestore(UserData userData)
    {
        return new Dictionary<string, object>
        {
            { "UserId",                       userData.UserId },
            { "NickName",                     userData.NickName },
            { "Name",                         userData.Name },
            { "Email",                        userData.Email },
            { "ProfileImageUrl",              userData.ProfileImageUrl ?? "" },
            { "Score",                        userData.Score },
            { "WeekScore",                    userData.WeekScore },
            { "QuestionTypeProgress",         userData.QuestionTypeProgress },
            { "IsUserRegistered",             userData.IsUserRegistered },
            { "SavedAt",                      FieldValue.ServerTimestamp },
            { "PlayerLevel",                  userData.PlayerLevel },
            { "TotalValidQuestionsAnswered",  userData.TotalValidQuestionsAnswered },
            { "TotalQuestionsInAllDatabanks", userData.TotalQuestionsInAllDatabanks },
            { "AnsweredQuestions",            userData.AnsweredQuestions
                                                ?? new Dictionary<string, List<int>>() },
            { "ResetDatabankFlags",           userData.ResetDatabankFlags
                                                ?? new Dictionary<string, bool>() },
            { "CreatedTime",                  Timestamp.FromDateTime(
                DateTime.SpecifyKind(userData.CreatedTime, DateTimeKind.Utc)) }
        };
    }
}