using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;

public class FirestoreRepository : MonoBehaviour
{
    private static FirestoreRepository _instance;
    private FirebaseFirestore db;
    private bool isInitialized;
    public bool IsInitialized => isInitialized;

    public static FirestoreRepository Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<FirestoreRepository>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("FirestoreRepository");
                    _instance = go.AddComponent<FirestoreRepository>();
                }
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

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

    public async Task<UserData> GetUserData(string userId)
    {
        try
        {
            DocumentSnapshot snapshot = await db.Collection("Users").Document(userId).GetSnapshotAsync();

            if (snapshot.Exists)
            {
                Dictionary<string, object> userData = snapshot.ToDictionary();
                Debug.Log($"Dados brutos do Firestore: {string.Join(", ", userData.Select(kv => $"{kv.Key}: {kv.Value}"))}");

                Timestamp createdTime = Timestamp.FromDateTime(DateTime.UtcNow);
                if (userData["CreatedTime"] is Timestamp timestamp)
                {
                    createdTime = timestamp;
                }

                UserData user = new UserData
                {
                    UserId = userData["UserId"].ToString(),
                    NickName = userData["NickName"].ToString(),
                    Name = userData["Name"].ToString(),
                    Email = userData["Email"].ToString(),
                    Score = Convert.ToInt32(userData["Score"]),
                    WeekScore = userData.ContainsKey("WeekScore") ? Convert.ToInt32(userData["WeekScore"]) : 0,
                    QuestionTypeProgress = userData.ContainsKey("QuestionTypeProgress") 
                        ? Convert.ToInt32(userData["QuestionTypeProgress"]) 
                        : (userData.ContainsKey("Progress") ? Convert.ToInt32(userData["Progress"]) : 0),
                    ProfileImageUrl = userData["ProfileImageUrl"]?.ToString() ?? "",
                    CreatedTime = createdTime,
                    IsUserRegistered = Convert.ToBoolean(userData["IsUserRegistered"]),
                    PlayerLevel = userData.ContainsKey("PlayerLevel") ? Convert.ToInt32(userData["PlayerLevel"]) : 1,
                    TotalValidQuestionsAnswered = userData.ContainsKey("TotalValidQuestionsAnswered") ? Convert.ToInt32(userData["TotalValidQuestionsAnswered"]) : 0,
                    TotalQuestionsInAllDatabanks = userData.ContainsKey("TotalQuestionsInAllDatabanks") ? Convert.ToInt32(userData["TotalQuestionsInAllDatabanks"]) : 0
                };

                if (userData.ContainsKey("AnsweredQuestions"))
                {
                    user.AnsweredQuestions = new Dictionary<string, List<int>>();
                    var answeredQuestions = userData["AnsweredQuestions"] as Dictionary<string, object>;
                    if (answeredQuestions != null)
                    {
                        foreach (var kvp in answeredQuestions)
                        {
                            if (kvp.Value is IEnumerable<object> list)
                            {
                                user.AnsweredQuestions[kvp.Key] = list.Select(x => Convert.ToInt32(x)).ToList();
                            }
                        }
                    }
                }

                if (userData.ContainsKey("ResetDatabankFlags"))
                {
                    user.ResetDatabankFlags = new Dictionary<string, bool>();
                    var resetFlags = userData["ResetDatabankFlags"] as Dictionary<string, object>;
                    if (resetFlags != null)
                    {
                        foreach (var kvp in resetFlags)
                        {
                            user.ResetDatabankFlags[kvp.Key] = Convert.ToBoolean(kvp.Value);
                        }
                    }
                }

                Debug.Log($"UserData carregado - NickName: {user.NickName}, Email: {user.Email}, Score: {user.Score}, WeekScore: {user.WeekScore}");
                return user;
            }
            else
            {
                Debug.LogError($"Documento do usuário {userId} não encontrado");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao buscar dados do usuário: {ex.Message}");
            throw;
        }
    }

    private UserData ConvertFromFirestore(Dictionary<string, object> data)
    {
        UserData userData = new UserData();
        userData.UserId = (string)data["UserId"];
        userData.NickName = (string)data["NickName"];
        userData.Name = (string)data["Name"];
        userData.Email = (string)data["Email"];
        userData.ProfileImageUrl = (string)data["ProfileImageUrl"];
        userData.Score = Convert.ToInt32(data["Score"]);

        // Adicionar WeekScore com verificação de existência
        userData.WeekScore = data.ContainsKey("WeekScore") ? Convert.ToInt32(data["WeekScore"]) : 0;

        userData.QuestionTypeProgress = data.ContainsKey("QuestionTypeProgress") 
    ? Convert.ToInt32(data["QuestionTypeProgress"]) 
    : (data.ContainsKey("Progress") ? Convert.ToInt32(data["Progress"]) : 0);


        userData.PlayerLevel = data.ContainsKey("PlayerLevel") ? Convert.ToInt32(data["PlayerLevel"]) : 1;
        userData.TotalValidQuestionsAnswered = data.ContainsKey("TotalValidQuestionsAnswered") ? Convert.ToInt32(data["TotalValidQuestionsAnswered"]) : 0;
        userData.TotalQuestionsInAllDatabanks = data.ContainsKey("TotalQuestionsInAllDatabanks") ? Convert.ToInt32(data["TotalQuestionsInAllDatabanks"]) : 0;

        if (data.ContainsKey("ResetDatabankFlags") && data["ResetDatabankFlags"] is Dictionary<string, object> resetFlagsData)
        {
            userData.ResetDatabankFlags = new Dictionary<string, bool>();
            foreach (var kvp in resetFlagsData)
            {
                userData.ResetDatabankFlags[kvp.Key] = Convert.ToBoolean(kvp.Value);
            }
        }

        userData.IsUserRegistered = Convert.ToBoolean(data["IsUserRegistered"]);

        if (data["CreatedTime"] is Timestamp timestamp)
        {
            userData.CreatedTime = timestamp;
        }

        userData.AnsweredQuestions = new Dictionary<string, List<int>>();
        if (data.ContainsKey("AnsweredQuestions") && data["AnsweredQuestions"] is Dictionary<string, object> answeredQuestionsData)
        {
            foreach (var kvp in answeredQuestionsData)
            {
                string databankName = kvp.Key;
                if (kvp.Value is List<object> questionsList)
                {
                    userData.AnsweredQuestions[databankName] = questionsList.Select(x => Convert.ToInt32(x)).ToList();
                }
            }
        }

        return userData;
    }

    public async Task CreateUserDocument(UserData userData)
    {
        try
        {
            if (!isInitialized) throw new System.Exception("Firestore não inicializado");

            if (string.IsNullOrEmpty(userData.UserId))
                throw new ArgumentException("UserId não pode ser vazio");

            var requiredFields = new Dictionary<string, object>
        {
            { "UserId", userData.UserId },
            { "NickName", userData.NickName },
            { "Name", userData.Name },
            { "Email", userData.Email },
            { "Score", userData.Score },
            { "WeekScore", userData.WeekScore }, // Adicionar WeekScore
            { "QuestionTypeProgress", userData.QuestionTypeProgress },
            { "IsUserRegistered", userData.IsUserRegistered },
            { "CreatedTime", userData.CreatedTime }
        };

            DocumentReference docRef = db.Collection("Users").Document(userData.UserId);
            await docRef.SetAsync(requiredFields);
            Debug.Log($"Documento do usuário criado com sucesso: {userData.UserId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao criar documento do usuário: {e.Message}");
            throw;
        }
    }

    public async Task UpdateUserScore(string userId, int newScore, int questionNumber, string databankName, bool isCorrect)
    {
        try
        {
            if (!isInitialized) throw new System.Exception("Firestore não inicializado");

            UserDataStore.UpdateScore(newScore);

            DocumentReference docRef = db.Collection("Users").Document(userId);

            await db.RunTransactionAsync(async transaction =>
            {
                DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(docRef);
                Dictionary<string, object> updates = new Dictionary<string, object> { { "Score", newScore } };

                if (snapshot.Exists)
                {
                    Dictionary<string, List<int>> answeredQuestions = snapshot.GetValue<Dictionary<string, List<int>>>("AnsweredQuestions");

                    if (answeredQuestions == null)
                    {
                        answeredQuestions = new Dictionary<string, List<int>>();
                    }

                    if (isCorrect)
                    {
                        if (!answeredQuestions.ContainsKey(databankName))
                        {
                            answeredQuestions[databankName] = new List<int>();
                        }
                        answeredQuestions[databankName].Add(questionNumber);
                    }

                    updates["AnsweredQuestions"] = answeredQuestions;
                }
                else
                {
                    Debug.LogError("User document not found during score update!");
                    return;
                }

                transaction.Update(docRef, updates);
            });

            Debug.Log($"Score atualizado para {newScore} e questionNumber {questionNumber} adicionado em {databankName} se a resposta foi correta");
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao atualizar score: {e.Message}");
            throw;
        }
    }

    public async Task UpdateUserData(UserData userData)
    {
        try
        {
            if (!isInitialized) throw new System.Exception("Firestore não inicializado");

            if (string.IsNullOrEmpty(userData.UserId))
                throw new ArgumentException("UserId não pode ser vazio");

            DocumentReference docRef = db.Collection("Users").Document(userData.UserId);

            // Usar o método ToDictionary para garantir consistência nos campos
            Dictionary<string, object> userDataDict = userData.ToDictionary();

            await docRef.UpdateAsync(userDataDict);
            Debug.Log($"Dados do usuário {userData.UserId} atualizados com sucesso");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao atualizar dados do usuário: {e.Message}");
            throw;
        }
    }

    public void ListenToUserData(string userId, Action<int> onScoreChanged = null, Action<int> onWeekScoreChanged = null, Action<Dictionary<string, List<int>>> onAnsweredQuestionsChanged = null)
    {
        db.Collection("Users").Document(userId)
        .Listen(snapshot =>
        {
            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();

                // Processa alterações no Score
                if (onScoreChanged != null && data.ContainsKey("Score"))
                {
                    int newScore = Convert.ToInt32(data["Score"]);
                    UserDataStore.UpdateScore(newScore);
                    onScoreChanged.Invoke(newScore);
                    Debug.Log($"Score atualizado do Firestore: {newScore}");
                }

                // Processa alterações no WeekScore
                if (onWeekScoreChanged != null && data.ContainsKey("WeekScore"))
                {
                    int newWeekScore = Convert.ToInt32(data["WeekScore"]);
                    if (UserDataStore.CurrentUserData != null)
                    {
                        UserDataStore.UpdateWeekScore(newWeekScore);
                    }
                    onWeekScoreChanged.Invoke(newWeekScore);
                    Debug.Log($"WeekScore atualizado do Firestore: {newWeekScore}");
                }

                // Processa alterações em AnsweredQuestions
                if (onAnsweredQuestionsChanged != null && data.ContainsKey("AnsweredQuestions"))
                {
                    try
                    {
                        Dictionary<string, List<int>> answeredQuestions = new Dictionary<string, List<int>>();
                        var answeredQuestionsData = data["AnsweredQuestions"] as Dictionary<string, object>;

                        if (answeredQuestionsData != null)
                        {
                            foreach (var kvp in answeredQuestionsData)
                            {
                                string databankName = kvp.Key;
                                var questionsList = kvp.Value as IEnumerable<object>;

                                if (questionsList != null)
                                {
                                    answeredQuestions[databankName] = questionsList
                                        .Select(q => Convert.ToInt32(q))
                                        .ToList();

                                    // Atualiza o AnsweredQuestionsListStore
                                    AnsweredQuestionsListStore.UpdateAnsweredQuestionsCount(
                                        userId,
                                        databankName,
                                        answeredQuestions[databankName].Count
                                    );

                                    Debug.Log($"Questões respondidas atualizadas para {databankName}: {answeredQuestions[databankName].Count}");
                                }
                            }

                            // Notifica o callback
                            onAnsweredQuestionsChanged.Invoke(answeredQuestions);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Erro ao processar AnsweredQuestions do Firestore: {ex.Message}");
                    }
                }
            }
        });
    }

    public async Task UpdateUserProgress(string userId, int progress)
    {
        try
        {
            if (!isInitialized) throw new System.Exception("Firestore não inicializado");

            DocumentReference docRef = db.Collection("Users").Document(userId);
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "Progress", progress }
            };

            await docRef.UpdateAsync(updates);
            Debug.Log($"Progresso do usuário atualizado para {progress}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao atualizar progresso do usuário: {e.Message}");
            throw;
        }
    }

    public async Task ResetAnsweredQuestions(string userId, string databankName)
    {
        try
        {
            if (!isInitialized) throw new System.Exception("Firestore não inicializado");

            DocumentReference docRef = db.Collection("Users").Document(userId);

            await db.RunTransactionAsync(async transaction =>
            {
                DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(docRef);

                if (snapshot.Exists)
                {
                    Dictionary<string, List<int>> answeredQuestions = snapshot.GetValue<Dictionary<string, List<int>>>("AnsweredQuestions");

                    if (answeredQuestions != null && answeredQuestions.ContainsKey(databankName))
                    {
                        answeredQuestions.Remove(databankName);
                        transaction.Update(docRef, "AnsweredQuestions", answeredQuestions);
                        Debug.Log($"AnsweredQuestions para {databankName} removidas para o usuário {userId}");
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

    public async Task DeleteDocument(string collection, string documentId)
    {
        try
        {
            if (!isInitialized) throw new System.Exception("Firestore não inicializado");

            var user = AuthenticationRepository.Instance.Auth.CurrentUser;
            if (user == null) throw new System.Exception("Usuário não está autenticado");

            string token = await user.TokenAsync(true);
            Debug.Log($"Token atualizado antes da deleção: {!string.IsNullOrEmpty(token)}");

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
                catch (System.Exception e) when (i < maxRetries - 1)
                {
                    Debug.LogWarning($"Tentativa {i + 1} falhou: {e.Message}. Tentando novamente...");
                    await Task.Delay(1000);
                    token = await user.TokenAsync(true);
                }
            }

            throw new System.Exception($"Falha ao deletar documento após {maxRetries} tentativas");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao deletar documento: {e.Message}");
            throw;
        }
    }

    public async Task<bool> AreNicknameTaken(string nickName)
    {
        QuerySnapshot snapshotNickname = await db.Collection("Users").WhereEqualTo("NickName", nickName).GetSnapshotAsync();
        return snapshotNickname.Documents.Count() > 0;
    }

    public async Task UpdateUserProfileImageUrl(string userId, string imageUrl)
    {
        try
        {
            DocumentReference userRef = db.Collection("Users").Document(userId);
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "ProfileImageUrl", imageUrl }
            };

            await userRef.UpdateAsync(updates);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao atualizar URL da imagem de perfil: {ex.Message}");
            throw;
        }
    }

    public async Task<List<UserData>> GetAllUsersData()
    {
        try
        {
            if (!isInitialized)
            {
                throw new System.Exception("Firestore não inicializado");
            }

            QuerySnapshot querySnapshot = await db.Collection("Users").GetSnapshotAsync();
            List<UserData> users = new List<UserData>();

            foreach (DocumentSnapshot doc in querySnapshot.Documents)
            {
                Dictionary<string, object> userData = doc.ToDictionary();
                users.Add(ConvertFromFirestore(userData));
            }

            return users;
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao buscar todos os usuários: {e.Message}");
            throw;
        }
    }

    public async Task UpdateUserWeekScore(string userId, int additionalScore)
    {
        try
        {
            if (!isInitialized) throw new System.Exception("Firestore não inicializado");

            DocumentReference docRef = db.Collection("Users").Document(userId);

            await db.RunTransactionAsync(async transaction =>
            {
                DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(docRef);

                if (snapshot.Exists)
                {
                    int currentWeekScore = 0;

                    // Verificar se WeekScore já existe
                    if (snapshot.ContainsField("WeekScore"))
                    {
                        currentWeekScore = Convert.ToInt32(snapshot.GetValue<object>("WeekScore"));
                    }

                    int newWeekScore = currentWeekScore + additionalScore;

                    transaction.Update(docRef, "WeekScore", newWeekScore);

                    // Atualizar localmente se for o usuário atual
                    if (UserDataStore.CurrentUserData != null && UserDataStore.CurrentUserData.UserId == userId)
                    {
                        // Use o método UpdateWeekScore em vez de invocar o evento diretamente
                        UserDataStore.UpdateWeekScore(newWeekScore);
                    }

                    Debug.Log($"WeekScore incrementado em {additionalScore}. Novo WeekScore: {newWeekScore}");
                }
                else
                {
                    Debug.LogError("User document not found during week score update!");
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao atualizar score semanal: {e.Message}");
            throw;
        }
    }

    public async Task UpdateUserScores(string userId, int additionalScore, int questionNumber, string databankName, bool isCorrect)
    {
        try
        {
            if (!isInitialized) throw new System.Exception("Firestore não inicializado");

            // Buscar dados atuais do usuário
            DocumentReference docRef = db.Collection("Users").Document(userId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                // Obter scores atuais
                int currentScore = Convert.ToInt32(snapshot.GetValue<object>("Score"));

                // Verificar se WeekScore existe e obter seu valor, ou usar 0 como padrão
                int currentWeekScore = 0;
                if (snapshot.ContainsField("WeekScore")) // Usando ContainsField em vez de Contains
                {
                    currentWeekScore = Convert.ToInt32(snapshot.GetValue<object>("WeekScore"));
                }

                // Calcular novos scores
                int newScore = currentScore + additionalScore;
                int newWeekScore = currentWeekScore + additionalScore;

                // Preparar atualizações
                Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "Score", newScore },
                { "WeekScore", newWeekScore }
            };

                // Atualizar AnsweredQuestions se necessário
                if (isCorrect && !string.IsNullOrEmpty(databankName) && questionNumber > 0)
                {
                    Dictionary<string, List<int>> answeredQuestions = snapshot.GetValue<Dictionary<string, List<int>>>("AnsweredQuestions");

                    if (answeredQuestions == null)
                    {
                        answeredQuestions = new Dictionary<string, List<int>>();
                    }

                    if (!answeredQuestions.ContainsKey(databankName))
                    {
                        answeredQuestions[databankName] = new List<int>();
                    }

                    if (!answeredQuestions[databankName].Contains(questionNumber))
                    {
                        answeredQuestions[databankName].Add(questionNumber);
                        updates["AnsweredQuestions"] = answeredQuestions;
                    }
                }

                // Aplicar atualizações
                await docRef.UpdateAsync(updates);

                // Atualizar localmente
                if (UserDataStore.CurrentUserData != null && UserDataStore.CurrentUserData.UserId == userId)
                {
                    UserDataStore.CurrentUserData.Score = newScore;
                    UserDataStore.CurrentUserData.WeekScore = newWeekScore;
                    UserDataStore.UpdateScore(newScore); // Isso dispara o evento OnUserDataChanged
                }

                Debug.Log($"Scores atualizados - Score: {newScore}, WeekScore: {newWeekScore}");
            }
            else
            {
                Debug.LogError("Usuário não encontrado para atualização de score!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao atualizar scores: {e.Message}");
            throw;
        }
    }

    public async Task ResetAllWeeklyScores()
    {
        try
        {
            if (!isInitialized) throw new System.Exception("Firestore não inicializado");

            // Obter todos os usuários
            QuerySnapshot querySnapshot = await db.Collection("Users").GetSnapshotAsync();

            // Verificar se há documentos
            if (querySnapshot.Documents.Count() == 0)
            {
                Debug.Log("Nenhum usuário encontrado para resetar scores semanais");
                return;
            }

            // Usar batch para atualizar múltiplos documentos de uma vez
            WriteBatch batch = db.StartBatch();
            int userCount = 0;

            foreach (DocumentSnapshot doc in querySnapshot.Documents)
            {
                batch.Update(doc.Reference, "WeekScore", 0);
                userCount++;

                // Firebase tem um limite de 500 operações por batch
                if (userCount >= 450)
                {
                    await batch.CommitAsync();
                    Debug.Log($"Lote de {userCount} usuários atualizado");
                    batch = db.StartBatch();
                    userCount = 0;
                }
            }

            // Commit do batch final se houver operações pendentes
            if (userCount > 0)
            {
                await batch.CommitAsync();
                Debug.Log($"Lote final de {userCount} usuários atualizado");
            }

            // Atualizar localmente se houver um usuário atual
            if (UserDataStore.CurrentUserData != null)
            {
                UserDataStore.CurrentUserData.WeekScore = 0;
                UserDataStore.UpdateScore(UserDataStore.CurrentUserData.Score); // Dispara evento
            }

            Debug.Log("Todos os scores semanais foram resetados com sucesso");
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao resetar scores semanais: {e.Message}");
            throw;
        }
    }

    public async Task EnsureWeekScoreField()
    {
        try
        {
            if (!isInitialized) throw new System.Exception("Firestore não inicializado");

            // Obter todos os usuários
            QuerySnapshot querySnapshot = await db.Collection("Users").GetSnapshotAsync();

            // Usar batch para atualizar múltiplos documentos de uma vez
            WriteBatch batch = db.StartBatch();
            int userCount = 0;

            foreach (DocumentSnapshot doc in querySnapshot.Documents)
            {
                // Verifica se o documento não tem o campo WeekScore
                if (!doc.ContainsField("WeekScore")) // Usando ContainsField em vez de Contains
                {
                    batch.Update(doc.Reference, "WeekScore", 0);
                    userCount++;

                    // Firebase tem um limite de 500 operações por batch
                    if (userCount >= 450)
                    {
                        await batch.CommitAsync();
                        Debug.Log($"Lote de {userCount} usuários atualizado com WeekScore");
                        batch = db.StartBatch();
                        userCount = 0;
                    }
                }
            }

            // Commit do batch final se houver operações pendentes
            if (userCount > 0)
            {
                await batch.CommitAsync();
                Debug.Log($"Lote final de {userCount} usuários atualizado com WeekScore");
            }

            Debug.Log("Verificação de WeekScore concluída");
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao verificar campo WeekScore: {e.Message}");
            throw;
        }
    }

    public async Task UpdateUserField(string userId, string fieldName, object value)
    {
        try
        {
            if (!isInitialized) throw new System.Exception("Firestore não inicializado");

            DocumentReference docRef = db.Collection("Users").Document(userId);
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { fieldName, value }
            };

            await docRef.UpdateAsync(updates);
            Debug.Log($"{fieldName} atualizado para {value}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao atualizar {fieldName}: {e.Message}");
            throw;
        }
    }

}


