// ─────────────────────────────────────────────────────────────────────────────
// ADICIONE ESTES 3 MÉTODOS DENTRO DA CLASSE AvatarCatalogPanelController
// (logo após o método Show())
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Posição do painel quando totalmente visível.</summary>
public Vector2 VisiblePosition => _visibleAnchoredPosition;

/// <summary>True enquanto o painel está visível (ou abrindo).</summary>
public bool IsVisible => _isVisible;

/// <summary>
/// Fecha o painel via arraste — equivale ao CloseButton mas pode ser
/// chamado externamente pelo DragToClosePanel.
/// </summary>
public void Hide()
{
    OnCloseClicked();
}

/// <summary>
/// Cancela qualquer animação em andamento (chamado pelo DragToClosePanel
/// quando o usuário começa a arrastar, para não conflitar com a animação).
/// </summary>
public void CancelAnimation()
{
    if (_animCoroutine != null)
    {
        StopCoroutine(_animCoroutine);
        _animCoroutine = null;
    }
}
