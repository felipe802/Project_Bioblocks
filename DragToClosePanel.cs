using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Componente genérico e reutilizável para fechar um SlidePanel via arraste.
///
/// Anexe ao objeto que será a área de toque para arrastar (ex: Header do painel).
/// Não precisa de DragEventForwarder — este script detecta os eventos diretamente.
///
/// SETUP NO INSPECTOR:
///   1. Slide Panel  → arraste o objeto que tem o componente SlidePanel
///   2. Panel Rect   → arraste o RectTransform do "Panel" (o mesmo referenciado no SlidePanel)
/// </summary>
public class DragToClosePanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Referências")]
    [Tooltip("Objeto que contém o componente SlidePanel (pai sempre ativo)")]
    [SerializeField] private SlidePanel slidePanel;

    [Tooltip("RectTransform do painel animado (mesmo Panel referenciado no SlidePanel)")]
    [SerializeField] private RectTransform panelRect;

    [Header("Configurações")]
    [Tooltip("Distância mínima de arraste para baixo (px) para fechar o painel")]
    [SerializeField] private float closeThreshold = 100f;

    [Tooltip("Resistência ao arraste: 1 = sem resistência, 0.4 = bastante resistência")]
    [Range(0.1f, 1f)]
    [SerializeField] private float dragResistance = 0.6f;

    [Tooltip("Duração do snap de volta quando não atinge o threshold (s)")]
    [SerializeField] private float snapBackDuration = 0.2f;

    // ── Estado interno ─────────────────────────────────────────────────────────

    private Vector2   _openPosition;
    private bool      _isDragging;
    private Coroutine _snapAnim;

    // ──────────────────────────────────────────────────────────────────────────

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Só permite drag quando o painel está totalmente visível (não animando)
        if (slidePanel == null || !slidePanel.IsVisible || slidePanel.IsAnimating) return;

        _isDragging   = true;
        _openPosition = slidePanel.VisiblePosition;

        // Para a animação do SlidePanel para não conflitar com o drag
        slidePanel.CancelAnimation();
        StopSnap();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;

        Vector2 pos = panelRect.anchoredPosition;
        pos.y += eventData.delta.y * dragResistance;

        // Impede arrastar para cima (acima da posição totalmente aberta)
        if (pos.y > _openPosition.y)
            pos.y = _openPosition.y;

        panelRect.anchoredPosition = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;

        float draggedDistance = _openPosition.y - panelRect.anchoredPosition.y;

        if (draggedDistance >= closeThreshold)
        {
            // SlidePanel anima do ponto atual até off-screen e chama SetActive(false)
            slidePanel.Hide();
        }
        else
        {
            // Snap de volta à posição aberta com efeito elástico
            _snapAnim = StartCoroutine(SnapBack());
        }
    }

    // ── Snap back ─────────────────────────────────────────────────────────────

    private void StopSnap()
    {
        if (_snapAnim != null) { StopCoroutine(_snapAnim); _snapAnim = null; }
    }

    private IEnumerator SnapBack()
    {
        Vector2 start   = panelRect.anchoredPosition;
        float   elapsed = 0f;

        while (elapsed < snapBackDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / snapBackDuration);
            panelRect.anchoredPosition = Vector2.Lerp(start, _openPosition, EaseOutBack(t));
            yield return null;
        }

        panelRect.anchoredPosition = _openPosition;
        _snapAnim = null;
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
