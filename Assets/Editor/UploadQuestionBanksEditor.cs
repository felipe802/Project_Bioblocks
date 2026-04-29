using System;
using System.Collections.Generic;
using System.Text;
using QuestionSystem;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Script Editor para fazer upload direto dos bancos de dados para Firestore.
/// 
/// Uso:
/// 1. Copie FirebaseSecrets-template.json para Assets/Editor/FirebaseSecrets.json
/// 2. Edite FirebaseSecrets.json com suas secret keys (Dev e Prod)
/// 3. Adicione FirebaseSecrets.json ao .gitignore
/// 4. Adicione os 10 bancos em GetAllDatabases()
/// 5. Tools > Upload Question Banks to Firestore
/// 6. Cole a URL da Cloud Function
/// https://us-central1-microlearning-dev-79c0c.cloudfunctions.net/uploadQuestionBanks
/// 7. Click "Upload" — pronto!
/// 
/// Não precisa digitar secret key — é carregada do arquivo local.
/// </summary>

public class UploadQuestionBanksEditor : EditorWindow
{
    private string cloudFunctionUrl = "";
    private string secretKey = "";
    private bool isUploading = false;
    private string uploadStatus = "";
    private Vector2 scrollPosition;

    [MenuItem("Tools/Upload Question Banks to Firestore")]
    public static void ShowWindow()
    {
        var window = GetWindow<UploadQuestionBanksEditor>("Upload Question Banks");
        window.minSize = new Vector2(550, 450);
    }

    private void OnEnable()
    {
        LoadSecrets();
    }

    private void LoadSecrets()
    {
        string secretsPath = "Assets/Editor/FirebaseSecrets.json";
        if (System.IO.File.Exists(secretsPath))
        {
            try
            {
                string json = System.IO.File.ReadAllText(secretsPath);
                var secrets = JsonUtility.FromJson<SecretsData>(json);
                
                // Usa Dev por padrão
                if (!string.IsNullOrEmpty(secrets.devSecretKey) && 
                    secrets.devSecretKey != "COLOQUE_AQUI_A_SECRET_KEY_DEV")
                {
                    secretKey = secrets.devSecretKey;
                    uploadStatus = "✓ Secret key Dev carregada do arquivo FirebaseSecrets.json\n";
                }
                else
                {
                    uploadStatus = "⚠️  FirebaseSecrets.json não foi preenchido corretamente.\n";
                    uploadStatus += "Edite o arquivo e adicione sua secret key Dev.\n";
                }
            }
            catch (System.Exception ex)
            {
                uploadStatus = $"❌ Erro ao carregar FirebaseSecrets.json: {ex.Message}\n";
            }
        }
        else
        {
            uploadStatus = "❌ Arquivo não encontrado: Assets/Editor/FirebaseSecrets.json\n\n";
            uploadStatus += "📋 Instruções:\n";
            uploadStatus += "1. Copie FirebaseSecrets-template.json\n";
            uploadStatus += "2. Renomeie para FirebaseSecrets.json\n";
            uploadStatus += "3. Coloque em Assets/Editor/\n";
            uploadStatus += "4. Edite com suas secret keys\n";
            uploadStatus += "5. Adicione ao .gitignore\n";
            uploadStatus += "6. Reabra esta janela\n";
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Upload Question Banks to Firestore", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Status da secret key
        if (string.IsNullOrEmpty(secretKey))
        {
            EditorGUILayout.HelpBox(
                "❌ Secret key não carregada!\n\nCopie FirebaseSecrets-template.json para Assets/Editor/FirebaseSecrets.json e edite com suas chaves.",
                MessageType.Error
            );
            
            if (GUILayout.Button("Tentar carregar novamente", GUILayout.Height(30)))
            {
                LoadSecrets();
            }
            
            return;
        }
        else
        {
            EditorGUILayout.HelpBox(
                "✓ Secret key Dev carregada com sucesso",
                MessageType.Info
            );
        }

        GUILayout.Space(10);

        // Cloud Function URL
        GUILayout.Label("Cloud Function URL:", EditorStyles.label);
        cloudFunctionUrl = EditorGUILayout.TextField(cloudFunctionUrl);
        EditorGUILayout.HelpBox(
            "Ex: https://region-projectid.cloudfunctions.net/uploadQuestionBanks\n\n" +
            "Para descobrir a URL, vá a Firebase Console > Functions > uploadQuestionBanks > Trigger",
            MessageType.Info
        );
        GUILayout.Space(15);

        // Ambiente (Dev/Prod)
        EditorGUILayout.LabelField("Ambiente:", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Você está usando a secret key de DEV.\n\n" +
            "Para enviar para PROD, edite FirebaseSecrets.json e mude a linha 61 do script de\n" +
            "  secretKey = secrets.devSecretKey;\n" +
            "para\n" +
            "  secretKey = secrets.prodSecretKey;",
            MessageType.Warning
        );
        GUILayout.Space(15);

        // Upload Button
        GUI.enabled = !isUploading && !string.IsNullOrEmpty(cloudFunctionUrl) && !string.IsNullOrEmpty(secretKey);
        if (GUILayout.Button("🚀 Upload Question Banks", GUILayout.Height(50)))
        {
            EditorApplication.delayCall += () => UploadDatabases();
        }
        GUI.enabled = true;

        GUILayout.Space(20);

        // Status
        if (!string.IsNullOrEmpty(uploadStatus))
        {
            GUILayout.Label("Status:", EditorStyles.boldLabel);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            GUILayout.TextArea(uploadStatus, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
        }

        if (isUploading)
        {
            GUILayout.Label("⏳ Uploading...", EditorStyles.miniLabel);
        }
    }

    private void UploadDatabases()
    {
        isUploading = true;
        uploadStatus = "Coletando bancos de dados...\n";

        try
        {
            // Coleta todos os bancos
            var databases = GetAllDatabases();
            uploadStatus += $"✓ {databases.Count} bancos encontrados\n";

            // Constrói payload
            var payload = new QuestionBanksPayload
            {
                questionBanks = new List<QuestionBankData>()
            };

            int totalQuestions = 0;
            foreach (var db in databases)
            {
                var bankName = db.GetDatabankName();
                var questions = db.GetQuestions();
                totalQuestions += questions.Count;

                var bankData = new QuestionBankData
                {
                    bankName = bankName,
                    questions = new List<QuestionData>()
                };

                foreach (var q in questions)
                {
                    var questionData = new QuestionData
                    {
                        globalId = string.IsNullOrEmpty(q.globalId)
                            ? $"{q.topic}_{q.questionNumber:D3}"
                            : q.globalId,
                        questionDatabankName = q.questionDatabankName,
                        questionNumber = q.questionNumber,
                        questionText = q.questionText,
                        answers = q.answers,
                        correctIndex = q.correctIndex,
                        isImageQuestion = q.isImageQuestion,
                        isImageAnswer = q.isImageAnswer,
                        questionImagePath = q.questionImagePath,
                        questionLevel = q.questionLevel,
                        topic = q.topic,
                        subtopic = q.subtopic,
                        displayName = q.displayName,
                        bloomLevel = q.bloomLevel ?? "unclassified",
                        conceptTags = q.conceptTags ?? new List<string>(),
                        prerequisites = q.prerequisites ?? new List<string>(),
                        questionInDevelopment = q.questionInDevelopment,
                        questionHint = q.questionHint != null ? new QuestionHintData
                        {
                            imagePath = q.questionHint.imagePath ?? "",
                            link = q.questionHint.link ?? "",
                            text = q.questionHint.text ?? "",
                            videoUrl = q.questionHint.videoUrl ?? ""
                        } : new QuestionHintData()
                    };

                    bankData.questions.Add(questionData);
                }

                payload.questionBanks.Add(bankData);
                uploadStatus += $"  • {bankName}: {bankData.questions.Count} questões\n";
            }

            uploadStatus += $"\n✓ Total: {totalQuestions} questões\n";
            uploadStatus += "Enviando para Firestore...\n";

            // Serializa e envia
            string json = JsonUtility.ToJson(payload);
            SendToCloudFunction(json);
        }
        catch (Exception ex)
        {
            uploadStatus += $"\n❌ ERRO: {ex.Message}\n{ex.StackTrace}";
            isUploading = false;
        }
    }

    private void SendToCloudFunction(string jsonPayload)
    {
        // Constrói URL com secret key
        string urlWithKey = $"{cloudFunctionUrl}?key={UnityWebRequest.EscapeURL(secretKey)}";

        // Cria request
        var www = new UnityWebRequest(urlWithKey, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        // Envia e aguarda resposta (blocking)
        var operation = www.SendWebRequest();

        int timeout = 0;
        while (!operation.isDone && timeout < 300) // 5 minutos timeout
        {
            System.Threading.Thread.Sleep(100);
            timeout++;
        }

        if (www.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string responseText = www.downloadHandler.text;
                uploadStatus += $"\n✅ SUCESSO!\n\n";
                uploadStatus += "Resposta da Cloud Function:\n";
                uploadStatus += responseText;
            }
            catch (Exception ex)
            {
                uploadStatus += $"\n⚠️  Resposta recebida mas erro ao parsear:\n{ex.Message}\n";
                uploadStatus += www.downloadHandler.text;
            }
        }
        else
        {
            uploadStatus += $"\n❌ ERRO na requisição:\n";
            uploadStatus += $"Status Code: {www.responseCode}\n";
            uploadStatus += $"Error: {www.error}\n";
            uploadStatus += $"Response: {www.downloadHandler.text}\n";
        }

        www.Dispose();
        isUploading = false;
    }

    /// <summary>
    /// ⚠️ ADICIONE AQUI OS 10 BANCOS DE DADOS
    /// </summary>
    private List<IQuestionDatabase> GetAllDatabases()
    {
        var databases = new List<IQuestionDatabase>
        {
            // Substitua pela lista completa dos 10 bancos:
            new AcidBaseBufferQuestionDatabase(),
            new AminoacidQuestionDatabase(),
            new BiochemistryIntroductionQuestionDatabase(),
            new CarbohydratesQuestionDatabase(),
            new EnzymeQuestionDatabase(),
            new LipidsQuestionDatabase(),
            new MembranesQuestionDatabase(),
            new NucleicAcidsQuestionDatabase(),
            new ProteinQuestionDatabase(),
            new WaterQuestionDatabase(),
        };

        return databases;
    }

    // ──────────────────────────────────────────────────────────────
    // Classes serializáveis
    // ──────────────────────────────────────────────────────────────

    [System.Serializable]
    public class QuestionBanksPayload
    {
        public List<QuestionBankData> questionBanks;
    }

    [System.Serializable]
    public class QuestionBankData
    {
        public string bankName;
        public List<QuestionData> questions;
    }

    [System.Serializable]
    public class QuestionData
    {
        public string globalId;
        public string questionDatabankName;
        public int questionNumber;
        public string questionText;
        public string[] answers;
        public int correctIndex;
        public bool isImageQuestion;
        public bool isImageAnswer;
        public string questionImagePath;
        public int questionLevel;
        public string topic;
        public string subtopic;
        public string displayName;
        public string bloomLevel;
        public List<string> conceptTags;
        public List<string> prerequisites;
        public bool questionInDevelopment;
        public QuestionHintData questionHint;
    }

    [System.Serializable]
    public class QuestionHintData
    {
        public string imagePath;
        public string link;
        public string text;
        public string videoUrl;
    }

    [System.Serializable]
    public class SecretsData
    {
        public string devSecretKey;
        public string prodSecretKey;
    }
}

#endif