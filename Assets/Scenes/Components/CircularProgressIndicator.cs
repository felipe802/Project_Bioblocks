using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CircularProgressIndicator : MonoBehaviour
{
    [Header("Configurações de Visual")]
    [SerializeField] private Image backgroundRing; // MaskCircle (anel de fundo)
    [SerializeField] private Image fillImage; // CircleFill (anel que preenche)
    [SerializeField] private TMP_Text percentageText;

    [Header("Animação")]
    [SerializeField] private float fillAnimationDuration = 0.5f;

    [Header("Detecção Automática")]
    [SerializeField] private string databaseNameSuffix = "PorcentageText";
    [SerializeField] private bool autoSetupImages = true;

    private float targetFillAmount = 0f;
    private float currentFillAmount = 0f;
    private float fillVelocity = 0f;
    private string databaseName;

    private void Awake()
    {
        // Auto-detectar o banco de dados pelo nome do texto
        if (percentageText == null)
        {
            percentageText = GetComponentInChildren<TMP_Text>();
        }

        if (percentageText != null)
        {
            string objName = percentageText.gameObject.name;
            if (objName.EndsWith(databaseNameSuffix))
            {
                databaseName = objName.Substring(0, objName.Length - databaseNameSuffix.Length);
                Debug.Log($"CircularProgressIndicator detectou banco: {databaseName}");
            }
        }

        if (autoSetupImages)
        {
            AutoSetupImages();
        }

        SetupFillImage();
    }

    private void AutoSetupImages()
    {
        Image[] images = GetComponentsInChildren<Image>(true);

        foreach (Image img in images)
        {
            string objName = img.gameObject.name.ToLower();

            if (objName.Contains("mask") && backgroundRing == null)
            {
                backgroundRing = img;
                Debug.Log($"Auto-detectado background ring: {img.name}");
            }
            else if (objName.Contains("fill") && fillImage == null)
            {
                fillImage = img;
                Debug.Log($"Auto-detectado fill image: {img.name}");
            }

            else if (img.sprite != null && backgroundRing == null && fillImage == null)
            {
                string spriteName = img.sprite.name.ToLower();
                if (spriteName.Contains("mask") && backgroundRing == null)
                {
                    backgroundRing = img;
                    Debug.Log($"Auto-detectado background ring por sprite: {img.name}");
                }
                else if (spriteName.Contains("fill") && fillImage == null)
                {
                    fillImage = img;
                    Debug.Log($"Auto-detectado fill image por sprite: {img.name}");
                }
            }
        }
    }

    private void SetupFillImage()
    {
        if (fillImage == null)
        {
            Debug.LogError($"Fill Image não foi atribuída para {gameObject.name}!");
            return;
        }

        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Radial360;
        fillImage.fillOrigin = (int)Image.Origin360.Top;
        fillImage.fillClockwise = true;
        fillImage.fillAmount = 0f;
        fillImage.enabled = true;

        if (backgroundRing != null)
        {
            backgroundRing.type = Image.Type.Simple;
            backgroundRing.enabled = true;
            fillImage.transform.SetAsLastSibling();
        }
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            CleanupMaskComponents();
        }
#endif
    }

#if UNITY_EDITOR

    private void CleanupMaskComponents()
    {
        // Remove Mask do fillImage se existir
        Mask fillMask = fillImage.GetComponent<Mask>();
        if (fillMask != null)
        {
            Debug.Log($"Removendo Mask de {fillImage.name} - não é necessário!");
            DestroyImmediate(fillMask);
        }

        // Remove Mask do background se existir
        if (backgroundRing != null)
        {
            Mask bgMask = backgroundRing.GetComponent<Mask>();
            if (bgMask != null)
            {
                Debug.Log($"Removendo Mask de {backgroundRing.name} - não é necessário!");
                DestroyImmediate(bgMask);
            }
        }
    }
#endif

    private void Start()
    {
        AnsweredQuestionsManager.OnAnsweredQuestionsUpdated += HandleAnsweredQuestionsUpdated;
        UpdateVisuals(0f);
    }

    private void OnDestroy()
    {
        AnsweredQuestionsManager.OnAnsweredQuestionsUpdated -= HandleAnsweredQuestionsUpdated;
    }

    private void Update()
    {
        if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.001f)
        {
            currentFillAmount = Mathf.SmoothDamp(
                currentFillAmount,
                targetFillAmount,
                ref fillVelocity,
                fillAnimationDuration);

            if (Mathf.Abs(currentFillAmount - targetFillAmount) < 0.01f)
            {
                currentFillAmount = targetFillAmount;
            }

            UpdateVisuals(currentFillAmount);
        }
    }

    private void HandleAnsweredQuestionsUpdated(Dictionary<string, int> answeredCounts)
    {
        if (string.IsNullOrEmpty(databaseName) || !answeredCounts.ContainsKey(databaseName))
            return;

        int count = answeredCounts[databaseName];
        int totalQuestions = QuestionBankStatistics.GetTotalQuestions(databaseName);

        int percentage = totalQuestions > 0 ? Mathf.Min((count * 100) / totalQuestions, 100) : 0;

        if (percentageText != null)
            percentageText.text = $"{percentage}%";

        targetFillAmount = percentage / 100f;

        Debug.Log($"CircularProgress {databaseName}: {percentage}% ({count}/{totalQuestions})");
    }

    public void SetProgress(int percentage)
    {
        percentage = Mathf.Clamp(percentage, 0, 100);

        if (percentageText != null)
        {
            percentageText.text = $"{percentage}%";
        }

        targetFillAmount = percentage / 100f;

        Debug.Log($"SetProgress chamado para {gameObject.name}: {percentage}% - FillAmount: {targetFillAmount}");
    }

    private void UpdateVisuals(float fillAmount)
    {
        if (fillImage == null) return;
        fillImage.fillAmount = fillAmount;
    }

    private void OnValidate()
    {
        if (fillImage != null && !Application.isPlaying)
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Radial360;
            fillImage.fillOrigin = (int)Image.Origin360.Top;
            fillImage.fillClockwise = true;
        }

        if (backgroundRing != null && !Application.isPlaying)
        {
            backgroundRing.type = Image.Type.Simple;
        }
    }
}