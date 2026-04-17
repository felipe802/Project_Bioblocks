using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Painel lateral (slide-in da direita) para seleção de avatares preset.
/// Carrega texturas de Resources/AvatarPresets/ e exibe em grid.
/// Dispara OnAvatarSelected(resourceName) quando o usuário escolhe um avatar.
/// </summary>
public class AvatarPickerPanel : MonoBehaviour
{
    // -------------------------------------------------------
    // Configuração no Inspector
    // -------------------------------------------------------
    [Header("Panel")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Content")]
    [SerializeField] private Transform gridContent;

    [Header("Close")]
    [SerializeField] private Button closeButton;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Avatar Button Template")]
    [Tooltip("Tamanho de cada botão de avatar no grid")]
    [SerializeField] private Vector2 avatarButtonSize = new Vector2(100, 100);
    [SerializeField] private Color selectedBorderColor = new Color(0f, 0.78f, 1f, 1f); // cyan do app

    // -------------------------------------------------------
    // Estado interno
    // -------------------------------------------------------
    private readonly string[] avatarResourceNames =
    {
        "avatar_dna",
        "avatar_cell",
        "avatar_bacteria",
        "avatar_virus",
        "avatar_protein",
        "avatar_mitochondria",
        "avatar_ribosome",
        "avatar_sugar",
        "avatar_neuron",
        "avatar_plant_cell"
    };

    private Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();
    private List<GameObject> avatarButtons = new List<GameObject>();
    private bool isVisible = false;
    private bool isAnimating = false;
    private Coroutine currentAnimation;

    /// <summary>
    /// Callback disparado quando o usuário seleciona um avatar.
    /// O parâmetro é o nome do recurso (ex: "avatar_dna").
    /// </summary>
    public event Action<string> OnAvatarSelected;

    /// <summary>
    /// Callback disparado quando o painel fecha (após animação de hide).
    /// O ProfileImageUploader usa este evento para disparar o upload para Firebase.
    /// </summary>
    public event Action OnPanelClosed;

    // -------------------------------------------------------
    // Ciclo de vida
    // -------------------------------------------------------
    private void Awake()
    {
        LoadAvatarTextures();
        SetPanelState(false, immediate: true);
    }

    private void Start()
    {
        CreateAvatarButtons();

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HidePanel);
        }
    }

    // -------------------------------------------------------
    // Carregar texturas de Resources
    // -------------------------------------------------------
    private void LoadAvatarTextures()
    {
        loadedTextures.Clear();

        foreach (string resourceName in avatarResourceNames)
        {
            Texture2D texture = Resources.Load<Texture2D>($"AvatarPresets/{resourceName}");

            if (texture != null)
            {
                loadedTextures[resourceName] = texture;
            }
            else
            {
                Debug.LogWarning($"[AvatarPickerPanel] Textura não encontrada: AvatarPresets/{resourceName}");
            }
        }

        Debug.Log($"[AvatarPickerPanel] {loadedTextures.Count}/{avatarResourceNames.Length} texturas carregadas");
    }

    // -------------------------------------------------------
    // Criar botões de avatar no grid
    // -------------------------------------------------------
    private void CreateAvatarButtons()
    {
        if (gridContent == null)
        {
            Debug.LogError("[AvatarPickerPanel] gridContent não atribuído no Inspector");
            return;
        }

        // Limpa botões existentes (caso recrie)
        foreach (GameObject btn in avatarButtons)
        {
            if (btn != null) Destroy(btn);
        }
        avatarButtons.Clear();

        foreach (string resourceName in avatarResourceNames)
        {
            if (!loadedTextures.ContainsKey(resourceName)) continue;

            GameObject buttonObj = CreateAvatarButton(resourceName, loadedTextures[resourceName]);
            avatarButtons.Add(buttonObj);
        }
    }

    private GameObject CreateAvatarButton(string resourceName, Texture2D texture)
    {
        // Container do botão
        GameObject buttonObj = new GameObject($"AvatarBtn_{resourceName}", typeof(RectTransform));
        buttonObj.transform.SetParent(gridContent, false);

        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = avatarButtonSize;

        // Background / borda (Image no objeto pai)
        Image borderImage = buttonObj.AddComponent<Image>();
        borderImage.color = Color.clear;

        // Botão
        Button button = buttonObj.AddComponent<Button>();

        // Imagem do avatar (filho)
        GameObject imageObj = new GameObject("AvatarImage", typeof(RectTransform));
        imageObj.transform.SetParent(buttonObj.transform, false);

        RectTransform imgRect = imageObj.GetComponent<RectTransform>();
        imgRect.anchorMin = Vector2.zero;
        imgRect.anchorMax = Vector2.one;
        imgRect.sizeDelta = new Vector2(-8, -8); // padding interno
        imgRect.anchoredPosition = Vector2.zero;

        RawImage rawImage = imageObj.AddComponent<RawImage>();
        rawImage.texture = texture;
        rawImage.color = Color.white;

        // Máscara circular (opcional — cria no pai do RawImage)
        Mask mask = buttonObj.AddComponent<Mask>();
        mask.showMaskGraphic = true;
        borderImage.sprite = CreateCircleSprite(128);
        borderImage.type = Image.Type.Simple;
        borderImage.color = new Color(0.9f, 0.9f, 0.9f, 1f); // fundo cinza claro

        // Callback do clique
        string captured = resourceName;
        button.onClick.AddListener(() => OnAvatarButtonClicked(captured));

        return buttonObj;
    }

    private Sprite CreateCircleSprite(int resolution)
    {
        Texture2D texture = new Texture2D(resolution, resolution);
        float radius = resolution / 2f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                Color color = distance < radius ? Color.white : Color.clear;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return Sprite.Create(
            texture,
            new Rect(0, 0, resolution, resolution),
            Vector2.one * 0.5f,
            100f,
            0,
            SpriteMeshType.Tight
        );
    }

    // -------------------------------------------------------
    // Seleção de avatar
    // -------------------------------------------------------
    private void OnAvatarButtonClicked(string resourceName)
    {
        if (isAnimating) return;

        Debug.Log($"[AvatarPickerPanel] Avatar selecionado: {resourceName}");

        // Destaque visual no avatar selecionado
        HighlightSelectedAvatar(resourceName);

        // Dispara o callback (preview instantâneo — sem fechar o painel)
        // O usuário pode continuar experimentando outros avatares.
        // O painel fecha apenas quando o usuário clica no CloseButton.
        OnAvatarSelected?.Invoke(resourceName);
    }

    private void HighlightSelectedAvatar(string selectedName)
    {
        foreach (GameObject btnObj in avatarButtons)
        {
            if (btnObj == null) continue;

            Image borderImage = btnObj.GetComponent<Image>();
            if (borderImage == null) continue;

            bool isSelected = btnObj.name == $"AvatarBtn_{selectedName}";
            borderImage.color = isSelected
                ? selectedBorderColor
                : new Color(0.9f, 0.9f, 0.9f, 1f);
        }
    }

    // -------------------------------------------------------
    // Show / Hide com animação
    // -------------------------------------------------------
    public void ShowPanel()
    {
        if (isVisible || isAnimating) return;

        gameObject.SetActive(true);
        isVisible = true;

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimatePanel(show: true));
    }

    public void HidePanel()
    {
        if (!isVisible || isAnimating) return;

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimatePanel(show: false));
    }

    private IEnumerator AnimatePanel(bool show)
    {
        isAnimating = true;

        float panelWidth = panelRect != null ? panelRect.rect.width : 300f;

        // Slide da direita: hidden = fora da tela (x = +panelWidth), shown = posição final (x = 0)
        Vector2 hiddenPos = new Vector2(panelWidth, 0);
        Vector2 shownPos  = Vector2.zero;

        Vector2 startPos = show ? hiddenPos : shownPos;
        Vector2 endPos   = show ? shownPos  : hiddenPos;

        if (panelRect != null)
            panelRect.anchoredPosition = startPos;

        // Ativa interatividade ao mostrar
        if (show)
            SetCanvasGroupInteractable(true);

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float curveValue = animationCurve.Evaluate(t);

            if (panelRect != null)
                panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);

            // Fade do CanvasGroup em paralelo
            if (canvasGroup != null)
                canvasGroup.alpha = show ? curveValue : (1f - curveValue);

            yield return null;
        }

        // Garante posição final exata
        if (panelRect != null)
            panelRect.anchoredPosition = endPos;

        if (!show)
        {
            isVisible = false;
            SetCanvasGroupInteractable(false);
            gameObject.SetActive(false);

            // Notifica que o painel fechou (upload para Firebase é disparado aqui)
            OnPanelClosed?.Invoke();
        }

        isAnimating = false;
        currentAnimation = null;
    }

    // -------------------------------------------------------
    // Estado do painel
    // -------------------------------------------------------
    private void SetPanelState(bool visible, bool immediate = false)
    {
        if (immediate)
        {
            isVisible = visible;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }

            if (panelRect != null)
            {
                float panelWidth = panelRect.rect.width > 0 ? panelRect.rect.width : 300f;
                panelRect.anchoredPosition = visible ? Vector2.zero : new Vector2(panelWidth, 0);
            }

            gameObject.SetActive(visible);
        }
    }

    private void SetCanvasGroupInteractable(bool interactable)
    {
        if (canvasGroup == null) return;

        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
        canvasGroup.alpha = interactable ? 1f : 0f;
    }

    // -------------------------------------------------------
    // Acesso público
    // -------------------------------------------------------
    public bool IsVisible => isVisible;
    public bool IsAnimating => isAnimating;
    public IReadOnlyDictionary<string, Texture2D> LoadedTextures => loadedTextures;

    // -------------------------------------------------------
    // Cleanup
    // -------------------------------------------------------
    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();

        foreach (GameObject btn in avatarButtons)
        {
            if (btn != null)
            {
                Button button = btn.GetComponent<Button>();
                if (button != null) button.onClick.RemoveAllListeners();
            }
        }
    }
}
