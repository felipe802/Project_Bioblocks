using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Anexe este script ao objeto Header do painel.
/// Ele captura os eventos de arraste e repassa para o DragToClosePanel,
/// que fica num objeto sempre ativo fora do painel.
/// </summary>
public class DragEventForwarder : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("Referência ao DragToClosePanel que está no objeto controlador (sempre ativo)")]
    [SerializeField] private DragToClosePanel controller;

    public void OnBeginDrag(PointerEventData eventData) => controller.OnBeginDrag(eventData);
    public void OnDrag(PointerEventData eventData)      => controller.OnDrag(eventData);
    public void OnEndDrag(PointerEventData eventData)   => controller.OnEndDrag(eventData);
}
