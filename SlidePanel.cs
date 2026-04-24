using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Componente genérico e reutilizável para painéis modais com animação de slide.
///
/// ARQUITETURA:
///   Este componente deve ficar num GameObject SEMPRE ATIVO (pai ou irmão do painel),
///   para que as coroutines de animação funcionem mesmo quando o painel está fechado.
///
/// SETUP RECOMENDADO:
///   AvatarCatalogPanel (ativo, tem SlidePanel + controller específico)
///   └── Panel           (inativo no início, gerenciado por este script)
///       ├── Header      (tem DragToClosePanel)
///       └── Scroll View
///
/// SETUP NO INSPECTOR:
///   1. Panel Object    → o GameObject "Panel" (filho, que será ativado/desativado)
///   2. Panel Rect      → o RectTransform do mesmo "Panel"
///   3. Eventos         → conecte OnBeforeShow / OnBeforeHide ao seu controller específico
/// </summary>
public class SlidePanel : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("GameObject do painel que será ativado/desativado (SetActive)")]
    [SerializeField] private GameObject panelObject;

    [Tooltip("RectTransform do painel (mesmo objeto que Panel Object)")]
    [SerializeField] private RectTransform panelRect;

    [Header("Animação")]
    [Tooltip("Distância de slide fora da tela — deve ser maior que a altura do painel")]
    [SerializeField] private float slideDistance = 1400f;

    [Tooltip("Duração da animação de abertura (s)")]
    [SerializeField] private float showDuration = 0.35f;

    [Tooltip("Duração da animação de fechamento (s)")]
    [SerializeField] private float hideDuration = 0.28f;

    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve hideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Eventos")]
    [Tooltip("Disparado antes do painel aparecer — use para inicializar dados")]
    [SerializeField] private UnityEvent onBeforeShow;

    [Tooltip("Disparado após a animação de abertura terminar")]
    [SerializeField] private UnityEvent onAfterShow;

    [Tooltip("Disparado antes de fechar — use para persistir dados (ex: CommitSessionAsync)")]
    [SerializeField] private UnityEvent onBeforeHide;

    [Tooltip("Disparado após o painel ser desativado (SetActive false)")]
    [SerializeField] private UnityEvent onAfterHide;

    // ── API pública ────────────────────────────────────────────────────────────

    /// <summary>True quando o painel está totalmente visível (animação concluída).</summary>
    public bool IsVisible { get; private set; }

    /// <summary>True enquanto uma animação de abertura ou fechamento está em andamento.</summary>
    public bool IsAnimating => _anim != null;

    /// <summary>Posição anchorada do painel no estado aberto (configurada no Editor).</summary>
    public Vector2 VisiblePosition => _visiblePosition;

    // ── Estado interno ─────────────────────────────────────────────────────────

    private Vector2   _visiblePosition;
    private Coroutine _anim;

    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (panelRect == null || panelObject == null)
        {
            Debug.LogError($"[SlidePanel] '{name}': panelObject ou panelRect não atribuído no Inspector.");
            return;
        }

        // Captura a posição visível a partir do layout definido no Editor
        _visiblePosition = panelRect.anchoredPosition;

        // Garante que o painel começa fechado
        panelObject.SetActive(false);
        IsVisible = false;
    }

    /// <summary>
    /// Abre o painel com animação de slide-up.
    /// Seguro chamar do botão, de outro script, ou de qualquer lugar.
    /// </summary>
    public void Show()
    {
        if (panelObject.activeSelf && IsVisible) return;

        onBeforeShow?.Invoke();

        // Posiciona fora da tela antes de ativar
        panelRect.anchoredPosition = _visiblePosition + Vector2.down * slideDistance;
        panelObject.SetActive(true);

        StopAnim();
        _anim = StartCoroutine(AnimateTo(_visiblePosition, showDuration, showCurve, () =>
        {
            IsVisible = true;
            onAfterShow?.Invoke();
        }));
    }

    /// <summary>
    /// Fecha o painel com animação de slide-down e desativa via SetActive(false).
    /// Pode ser chamado a partir de qualquer posição (inclusive durante arraste).
    /// </summary>
    public void Hide()
    {
        if (!panelObject.activeSelf) return;

        IsVisible = false;
        onBeforeHide?.Invoke();

        // Alvo é sempre off-screen abaixo, independente da posição atual do drag
        Vector2 target = new Vector2(_visiblePosition.x, _visiblePosition.y - slideDistance);

        StopAnim();
        _anim = StartCoroutine(AnimateTo(target, hideDuration, hideCurve, () =>
        {
            // Reseta posição para a próxima abertura antes de desativar
            panelRect.anchoredPosition = _visiblePosition;
            panelObject.SetActive(false);
            onAfterHide?.Invoke();
        }));
    }

    /// <summary>
    /// Interrompe a animação atual sem alterar o estado do painel.
    /// Chamado pelo DragToClosePanel ao iniciar um arraste.
    /// </summary>
    public void CancelAnimation()
    {
        StopAnim();
    }

    // ── Animação ───────────────────────────────────────────────────────────────

    private void StopAnim()
    {
        if (_anim != null) { StopCoroutine(_anim); _anim = null; }
    }

    private IEnumerator AnimateTo(
        Vector2 target,
        float duration,
        AnimationCurve curve,
        System.Action onComplete = null)
    {
        Vector2 start   = panelRect.anchoredPosition;
        float   elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            panelRect.anchoredPosition = Vector2.Lerp(start, target, curve.Evaluate(t));
            yield return null;
        }

        panelRect.anchoredPosition = target;
        _anim = null;
        onComplete?.Invoke();
    }
}
