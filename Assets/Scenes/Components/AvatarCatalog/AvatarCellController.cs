using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla uma célula individual da grade do catálogo de avatares.
/// Vive num prefab (<c>AvatarCell.prefab</c>) instanciado pelo
/// <see cref="AvatarCatalogPanelController"/>.
///
/// Responsabilidades:
///   - Renderizar o sprite do avatar a partir de <see cref="AvatarDefinition.ResourcePath"/>.
///   - Capturar tap e propagar para o callback fornecido no Bind.
///   - Exibir estado visual "selecionado" via <see cref="selectedIndicator"/>.
///   - Exibir estado visual "bloqueado" via <see cref="lockOverlay"/>, desabilitando
///     clique quando o avatar não está disponível para seleção.
/// </summary>
public class AvatarCellController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Image      avatarImage;
    [SerializeField] private Button     button;
    [SerializeField] private GameObject selectedIndicator;

    [Tooltip("Overlay exibido quando o avatar está bloqueado (desfoque/escurecimento). " +
             "Deve ser um GameObject filho do AvatarCell — tipicamente uma Image semi-transparente " +
             "sobre o AvatarImage.")]
    [SerializeField] private GameObject lockOverlay;

    /// <summary>Id do avatar que esta célula representa (ex.: <c>avatar_dna_03</c>).</summary>
    public string AvatarId { get; private set; }

    /// <summary><c>true</c> quando a célula está bloqueada para seleção.</summary>
    public bool IsLocked { get; private set; }

    private AvatarDefinition         _definition;
    private Action<AvatarDefinition> _onTapped;

    /// <summary>
    /// Popula a célula a partir de uma definição do catálogo. Deve ser chamado
    /// uma vez após Instantiate, pelo controller da grade.
    /// </summary>
    public void Bind(AvatarDefinition def, Action<AvatarDefinition> onTapped)
    {
        if (def == null)
        {
            Debug.LogWarning("[AvatarCell] Bind chamado com AvatarDefinition null.");
            return;
        }

        _definition = def;
        AvatarId    = def.Id;
        _onTapped   = onTapped;

        if (avatarImage != null)
        {
            var sprite = Resources.Load<Sprite>(def.ResourcePath);
            if (sprite != null)
            {
                avatarImage.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"[AvatarCell] Sprite não encontrado em Resources: '{def.ResourcePath}'. " +
                                 "Verifique se o PNG está na pasta correta e marcado como Sprite (2D and UI).");
            }
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }

        SetSelected(false);
        SetLocked(false);
    }

    /// <summary>Liga/desliga o indicador visual de seleção.</summary>
    public void SetSelected(bool selected)
    {
        if (selectedIndicator != null)
            selectedIndicator.SetActive(selected);
    }

    /// <summary>
    /// Define o estado de bloqueio da célula.
    /// Quando <paramref name="locked"/> é <c>true</c>:
    ///   - O botão fica não-interativo (não dispara <c>onClick</c>).
    ///   - O <see cref="lockOverlay"/> é ativado (desfoque/escurecimento).
    ///   - O indicador de seleção é desligado (célula bloqueada não pode estar selecionada).
    /// </summary>
    public void SetLocked(bool locked)
    {
        IsLocked = locked;

        if (button != null)
            button.interactable = !locked;

        if (lockOverlay != null)
            lockOverlay.SetActive(locked);

        if (locked)
            SetSelected(false);
    }

    private void HandleClick()
    {
        // Guard extra: mesmo que o Button.interactable falhe por alguma razão,
        // não propagamos cliques em células bloqueadas.
        if (IsLocked) return;

        _onTapped?.Invoke(_definition);
    }
}
