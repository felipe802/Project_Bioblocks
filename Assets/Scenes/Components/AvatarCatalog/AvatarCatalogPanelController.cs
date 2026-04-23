using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla o painel de catálogo de avatares (tela cheia, slide-up).
///
/// Fluxo:
///   - Show() é chamado por <c>UserHeaderManager.OpenAvatarCatalog()</c> (tap no avatar do top bar).
///   - A grade é construída uma única vez a partir de <see cref="AvatarCatalog.All"/>.
///   - Tap numa célula → delega ao <see cref="IAvatarSelectionService.PreviewSelection"/>,
///     que atualiza UserDataStore e o top bar reage sozinho via OnUserDataChanged.
///   - CloseButton → anima slide-down e dispara
///     <see cref="IAvatarSelectionService.CommitSessionAsync"/>.
///
/// O GameObject permanece ativo durante todo o lifecycle da cena; visibilidade é
/// controlada pela posição off-screen do <see cref="panelRect"/>.
/// </summary>
public class AvatarCatalogPanelController : MonoBehaviour
{
    [Header("Painel deslizante")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private Button        closeButton;

    [Header("Grade")]
    [SerializeField] private Transform            gridContainer;
    [SerializeField] private AvatarCellController cellPrefab;

    [Header("Posições (anchoredPosition)")]
    [Tooltip("Posição do painel quando oculto. A posição visível é capturada do RectTransform " +
             "configurado no Editor — este script nunca altera tamanho, anchors ou pivot do painel.")]
    [SerializeField] private Vector2 hiddenAnchoredPosition = new Vector2(0f, -2000f);

    [Header("Animação")]
    [SerializeField] private float          animationDuration = 0.3f;
    [SerializeField] private AnimationCurve animationCurve    = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private const string PRESET_PREFIX = "preset:";

    private Vector2   _visibleAnchoredPosition;
    private bool      _isVisible;
    private bool      _gridBuilt;
    private Coroutine _animCoroutine;

    private readonly List<AvatarCellController> _cells = new List<AvatarCellController>();

    private void Awake()
    {
        if (panelRect == null)
        {
            Debug.LogError("[AvatarCatalogPanel] panelRect não atribuído no Inspector.");
            return;
        }

        // A posição configurada no Editor é a posição visível. Script não interfere no layout.
        _visibleAnchoredPosition = panelRect.anchoredPosition;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        panelRect.anchoredPosition = hiddenAnchoredPosition;
    }

    public void Show()
    {
        if (_isVisible) return;

        if (AppContext.AvatarSelection == null)
        {
            Debug.LogError("[AvatarCatalogPanel] AppContext.AvatarSelection não disponível. Abortando Show.");
            return;
        }

        // Lazy: a grade é construída apenas na primeira abertura.
        if (!_gridBuilt) BuildGrid();

        AppContext.AvatarSelection.BeginSession();
        RefreshHighlightFromCurrentUser();

        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(AnimateTo(_visibleAnchoredPosition));

        _isVisible = true;
    }

    private void OnCloseClicked()
    {
        if (!_isVisible) return;
        _isVisible = false;

        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(AnimateTo(hiddenAnchoredPosition));

        // Fire-and-forget. Erros são logados pelo próprio service.
        if (AppContext.AvatarSelection != null)
        {
            _ = AppContext.AvatarSelection.CommitSessionAsync();
        }
    }

    private IEnumerator AnimateTo(Vector2 target)
    {
        if (panelRect == null) yield break;

        var start   = panelRect.anchoredPosition;
        var elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / animationDuration);
            panelRect.anchoredPosition = Vector2.Lerp(start, target, animationCurve.Evaluate(t));
            yield return null;
        }

        panelRect.anchoredPosition = target;
        _animCoroutine = null;
    }

    private void BuildGrid()
    {
        if (cellPrefab == null || gridContainer == null)
        {
            Debug.LogError("[AvatarCatalogPanel] cellPrefab ou gridContainer não atribuído no Inspector.");
            return;
        }

        _cells.Clear();
        foreach (var def in AvatarCatalog.All)
        {
            var cell = Instantiate(cellPrefab, gridContainer);
            cell.Bind(def, OnCellTapped);
            _cells.Add(cell);
        }

        _gridBuilt = true;
        Debug.Log($"[AvatarCatalogPanel] Grade construída com {_cells.Count} células.");
    }

    private void OnCellTapped(AvatarDefinition def)
    {
        if (AppContext.AvatarSelection == null || def == null) return;

        AppContext.AvatarSelection.PreviewSelection(def.Id);
        HighlightSelection(def.Id);
    }

    private void RefreshHighlightFromCurrentUser()
    {
        var url = UserDataStore.CurrentUserData?.ProfileImageUrl ?? string.Empty;
        string currentId = url.StartsWith(PRESET_PREFIX) ? url.Substring(PRESET_PREFIX.Length) : null;
        HighlightSelection(currentId);
    }

    private void HighlightSelection(string selectedId)
    {
        for (int i = 0; i < _cells.Count; i++)
        {
            var cell = _cells[i];
            if (cell != null) cell.SetSelected(cell.AvatarId == selectedId);
        }
    }
}
