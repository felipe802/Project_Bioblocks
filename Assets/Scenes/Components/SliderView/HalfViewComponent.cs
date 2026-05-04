using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HalfViewComponent : MonoBehaviour
{
    [Header("Referências do Painel")]
    [SerializeField] private RectTransform menuPanel;
    [SerializeField] private Canvas menuCanvas;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button _primaryButton;
    [SerializeField] private TextMeshProUGUI _primaryButtonText;
    [SerializeField] private Button _secondaryButton;
    [SerializeField] private TextMeshProUGUI _secondaryButtonText;
    [SerializeField] private Button _closeButton;

    public Button PrimaryButton => _primaryButton;
    public TextMeshProUGUI PrimaryButtonText => _primaryButtonText;
    public Button SecondaryButton => _secondaryButton;
    public TextMeshProUGUI SecondaryButtonText => _secondaryButtonText;
    public Button CloseButton => _closeButton;

    [Header("Botão de Ativação")]
    [SerializeField] private Button triggerButton;

    [Header("Configurações de Animação")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Configurações de Overlay")]
    [SerializeField] private GameObject darkOverlay;
    [SerializeField] private float overlayAlpha = 0.6f;

    // Eventos
    public event Action OnPrimaryButtonClicked;
    public event Action OnSecondaryButtonClicked;
    public event Action OnCloseButtonClicked;
    public event Action OnHalfViewShown;
    public event Action OnHalfViewHidden;
    public event Action OnCancelled;

    // Variáveis privadas
    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;
    private bool isVisible = false;
    private Coroutine animationCoroutine;

    // Lista de elementos interativos a serem desabilitados
    private List<Selectable> interactableElements = new List<Selectable>();
    private List<bool> originalInteractableStates = new List<bool>();

    // Variáveis para auto-detecção da bottomBar
    private RectTransform bottomBar = null;
    private Canvas bottomBarCanvas = null;
    private int originalBottomBarSortingOrder = 0;

    private bool preventButtonReconfiguration = false;

    private void Awake()
    {
        EnsureCanvasSetup();
        string sceneName = SceneManager.GetActiveScene().name;
        HalfViewRegistry.RegisterHalfView(sceneName, this);
    }

    private void OnEnable()
    {
        SetupButtonListeners();
    }

    private void Start()
    {
        FindBottomBar();
        SetupPositions();

        if (menuPanel != null)
        {
            menuPanel.anchoredPosition = hiddenPosition;
        }

        SetupButtonListeners();
        SetupOverlay();
        CollectInteractableElements();

        if (darkOverlay != null)
        {
            darkOverlay.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        HalfViewRegistry.UnregisterHalfView(sceneName);
    }

    private void EnsureCanvasSetup()
    {
        if (menuCanvas == null)
        {
            menuCanvas = GetComponent<Canvas>();
            if (menuCanvas == null)
            {
                menuCanvas = gameObject.AddComponent<Canvas>();
            }
        }

        menuCanvas.overrideSorting = true;
        menuCanvas.sortingOrder = 100;

        GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = gameObject.AddComponent<GraphicRaycaster>();
            Debug.Log("[HalfViewComponent] GraphicRaycaster adicionado");
        }
    }

    private void FindBottomBar()
    {
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.gameObject.name.ToLower().Contains("bottom") ||
                canvas.gameObject.name.ToLower().Contains("bar"))
            {
                bottomBarCanvas = canvas;
                bottomBar = canvas.GetComponent<RectTransform>();
                originalBottomBarSortingOrder = canvas.sortingOrder;
                break;
            }
        }
    }

    private void SetupPositions()
    {
        if (menuPanel == null)
        {
            Debug.LogError("[HalfViewComponent] Menu Panel não atribuído!");
            return;
        }

        hiddenPosition = new Vector2(0, -menuPanel.rect.height);
        visiblePosition = new Vector2(0, 0);
    }

    private void SetupButtonListeners()
    {
        if (_primaryButton != null)
        {
            _primaryButton.onClick.RemoveAllListeners();
            _primaryButton.onClick.AddListener(() =>
            {
                OnPrimaryButtonClicked?.Invoke();
                HideMenu();
            });
            _primaryButton.interactable = true;

            if (_primaryButton.targetGraphic != null)
            {
                _primaryButton.targetGraphic.raycastTarget = true;
            }
            Debug.Log("[HalfViewComponent] Primary button configurado");
        }

        if (_secondaryButton != null)
        {
            _secondaryButton.onClick.RemoveAllListeners();
            _secondaryButton.onClick.AddListener(() =>
            {
                OnSecondaryButtonClicked?.Invoke();
                HideMenu();
            });
            _secondaryButton.interactable = true;

            if (_secondaryButton.targetGraphic != null)
            {
                _secondaryButton.targetGraphic.raycastTarget = true;
            }
            Debug.Log("[HalfViewComponent] Secondary button configurado");
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(() =>
            {
                OnCloseButtonClicked?.Invoke();
                HideMenu();
            });
            _closeButton.interactable = true;

            if (_closeButton.targetGraphic != null)
            {
                _closeButton.targetGraphic.raycastTarget = true;
            }
            Debug.Log("[HalfViewComponent] Close button configurado");
        }

        if (triggerButton != null)
        {
            triggerButton.onClick.RemoveListener(ToggleMenu);
            triggerButton.onClick.AddListener(ToggleMenu);
            Debug.Log("[HalfViewComponent] Trigger button configurado");
        }
    }

    private void SetupOverlay()
    {
        if (darkOverlay == null) return;

        darkOverlay.SetActive(false);

        Canvas overlayCanvas = darkOverlay.GetComponent<Canvas>();
        if (overlayCanvas == null)
        {
            overlayCanvas = darkOverlay.AddComponent<Canvas>();
        }
        overlayCanvas.overrideSorting = true;
        overlayCanvas.sortingOrder = 90;

        Image overlayImage = darkOverlay.GetComponent<Image>();
        if (overlayImage != null)
        {
            Color color = overlayImage.color;
            color.a = overlayAlpha;
            overlayImage.color = color;
        }

        Button overlayButton = darkOverlay.GetComponent<Button>();
        if (overlayButton == null)
        {
            overlayButton = darkOverlay.AddComponent<Button>();
            ColorBlock colors = overlayButton.colors;
            colors.normalColor = new Color(0, 0, 0, 0);
            colors.highlightedColor = new Color(0, 0, 0, 0);
            colors.pressedColor = new Color(0, 0, 0, 0);
            colors.selectedColor = new Color(0, 0, 0, 0);
            overlayButton.colors = colors;
        }
        overlayButton.onClick.RemoveAllListeners();
        overlayButton.onClick.AddListener(HideMenu);
        darkOverlay.transform.SetSiblingIndex(0);
    }

    private void CollectInteractableElements()
    {
        interactableElements.Clear();
        originalInteractableStates.Clear();

        Selectable[] selectables = FindObjectsByType<Selectable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Selectable selectable in selectables)
        {
            if (!IsPartOfHalfView(selectable.gameObject))
            {
                interactableElements.Add(selectable);
                originalInteractableStates.Add(selectable.interactable);
            }
        }
    }

    private bool IsPartOfHalfView(GameObject obj)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.gameObject == gameObject || (darkOverlay != null && current.gameObject == darkOverlay))
            {
                return true;
            }
            current = current.parent;
        }
        return false;
    }

    public void ToggleMenu()
    {
        if (isVisible)
            HideMenu();
        else
            ShowMenu();
    }

    public void ShowMenu()
    {
        if (isVisible) return;
        gameObject.SetActive(true);

        if (darkOverlay != null)
        {
            darkOverlay.SetActive(true);
            Image overlayImage = darkOverlay.GetComponent<Image>();
            if (overlayImage != null)
            {
                overlayImage.raycastTarget = true;
                Color color = overlayImage.color;
                color.a = overlayAlpha;
                overlayImage.color = color;
            }
        }
        else
        {
            Debug.LogError("DarkOverlay não está configurado no HalfView!");
        }

        CollectInteractableElements();
        DisableSceneInteraction();

        EnsureCanvasSetup();

        CanvasGroup panelGroup = menuPanel.GetComponent<CanvasGroup>();
        if (panelGroup != null)
        {
            panelGroup.interactable = true;
            panelGroup.blocksRaycasts = true;
            panelGroup.alpha = 1f;
        }

        if (darkOverlay != null)
        {
            darkOverlay.SetActive(true);
            darkOverlay.transform.SetSiblingIndex(0);
        }

        AdjustSortingOrderForVisibility(true);

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateMenu(hiddenPosition, visiblePosition));
        isVisible = true;

        if (!preventButtonReconfiguration)
        {
            SetupButtonListeners();
        }

        OnHalfViewShown?.Invoke();
    }

    public void HideMenu()
    {
        if (!isVisible) return;

        EnableSceneInteraction();
        OnCancelled?.Invoke();

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateMenuAndDeactivate(visiblePosition, hiddenPosition));
        isVisible = false;

        if (darkOverlay != null)
        {
            darkOverlay.SetActive(false);
        }

        OnHalfViewHidden?.Invoke();
    }

    private IEnumerator AnimateMenuAndDeactivate(Vector2 startPos, Vector2 endPos)
    {
        if (menuPanel == null) yield break;

        float elapsedTime = 0;
        menuPanel.anchoredPosition = startPos;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / animationDuration);
            float curveValue = animationCurve.Evaluate(normalizedTime);

            menuPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);

            yield return null;
        }

        menuPanel.anchoredPosition = endPos;
        gameObject.SetActive(false);
        animationCoroutine = null;
    }

    private void DisableSceneInteraction()
    {
        for (int i = 0; i < interactableElements.Count; i++)
        {
            if (interactableElements[i] != null)
            {
                if (!ShouldKeepInteractable(interactableElements[i].gameObject))
                {
                    originalInteractableStates[i] = interactableElements[i].interactable;
                    interactableElements[i].interactable = false;
                }
            }
        }
    }

    private bool ShouldKeepInteractable(GameObject obj)
    {
        return false;
    }

    private void EnableSceneInteraction()
    {
        try
        {
            for (int i = 0; i < interactableElements.Count; i++)
            {
                if (interactableElements[i] != null)
                {
                    interactableElements[i].interactable = originalInteractableStates[i];
                }
            }

            GameObject persistentBottomBar = GameObject.Find("PersistentBottomBar");
            if (persistentBottomBar != null)
            {
                Button[] navButtons = persistentBottomBar.GetComponentsInChildren<Button>(true);
                foreach (Button button in navButtons)
                {
                    button.interactable = true;
                }


                persistentBottomBar.SetActive(true);
                Canvas navCanvas = persistentBottomBar.GetComponentInParent<Canvas>();

                if (navCanvas != null)
                {
                    navCanvas.enabled = true;
                }

                CanvasGroup canvasGroup = persistentBottomBar.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.interactable = true;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao reativar interações: {e.Message}");
        }
    }

    private void AdjustSortingOrderForVisibility(bool show)
    {
        if (menuCanvas != null)
        {
            if (show)
            {
                menuCanvas.sortingOrder = 1000;

                if (bottomBarCanvas != null)
                {
                    bottomBarCanvas.sortingOrder = 88;
                }

                if (darkOverlay != null)
                {
                    Canvas overlayCanvas = darkOverlay.GetComponent<Canvas>();
                    if (overlayCanvas != null)
                    {
                        overlayCanvas.sortingOrder = 140;
                    }
                }
            }
            else
            {
                menuCanvas.sortingOrder = 0;

                if (bottomBarCanvas != null)
                {
                    bottomBarCanvas.sortingOrder = originalBottomBarSortingOrder;
                }
            }
        }
    }

    private IEnumerator AnimateMenu(Vector2 startPos, Vector2 endPos, bool hideWhenDone = false)
    {
        if (menuPanel == null) yield break;

        float elapsedTime = 0;
        menuPanel.anchoredPosition = startPos;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / animationDuration);
            float curveValue = animationCurve.Evaluate(normalizedTime);

            menuPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);

            yield return null;
        }

        menuPanel.anchoredPosition = endPos;

        animationCoroutine = null;
    }

    #region Configuração da HalfView

    public void SetTriggerButton(Button button)
    {
        if (triggerButton != null)
        {
            triggerButton.onClick.RemoveListener(ToggleMenu);
        }

        triggerButton = button;

        if (triggerButton != null)
        {
            triggerButton.onClick.AddListener(ToggleMenu);
            Debug.Log("[HalfViewComponent] Novo trigger button configurado");
        }
    }

    public void SetTitle(string title)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
    }

    public void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    public void SetPrimaryButton(string text, Action onClickAction, bool hideAfterClick = true)
    {
        if (_primaryButton != null && _primaryButtonText != null)
        {
            _primaryButtonText.text = text;
            _primaryButton.onClick.RemoveAllListeners();
            _primaryButton.onClick.AddListener(() =>
            {
                onClickAction?.Invoke();
                if (hideAfterClick)
                {
                    HideMenu();
                }
            });
            _primaryButton.gameObject.SetActive(true);
        }
    }

    public void SetSecondaryButton(string text, Action onClickAction, bool hideAfterClick = true)
    {
        if (_secondaryButton != null && _secondaryButtonText != null)
        {
            _secondaryButtonText.text = text;
            _secondaryButton.onClick.RemoveAllListeners();
            _secondaryButton.onClick.AddListener(() =>
            {
                onClickAction?.Invoke();
                if (hideAfterClick)
                {
                    HideMenu();
                }
            });
            _secondaryButton.gameObject.SetActive(true);
        }
    }

    public void HidePrimaryButton()
    {
        if (_primaryButton != null)
        {
            _primaryButton.gameObject.SetActive(false);
        }
    }

    public void HideSecondaryButton()
    {
        if (_secondaryButton != null)
        {
            _secondaryButton.gameObject.SetActive(false);
        }
    }

    public void Configure(string title, string message,
                     string primaryButtonText, Action primaryAction,
                     string secondaryButtonText = null, Action secondaryAction = null)
    {
        // Marcar que os botões já foram configurados
        preventButtonReconfiguration = true;

        SetTitle(title);
        SetMessage(message);

        if (!string.IsNullOrEmpty(primaryButtonText) && primaryAction != null)
        {
            SetPrimaryButton(primaryButtonText, primaryAction);
        }
        else
        {
            HidePrimaryButton();
        }

        if (!string.IsNullOrEmpty(secondaryButtonText) && secondaryAction != null)
        {
            SetSecondaryButton(secondaryButtonText, secondaryAction);
        }
        else
        {
            HideSecondaryButton();
        }
    }

    #endregion
}
