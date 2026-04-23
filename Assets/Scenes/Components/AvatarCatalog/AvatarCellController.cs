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
/// </summary>
public class AvatarCellController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Image      avatarImage;
    [SerializeField] private Button     button;
    [SerializeField] private GameObject selectedIndicator;

    /// <summary>Id do avatar que esta célula representa (ex.: <c>avatar_dna_03</c>).</summary>
    public string AvatarId { get; private set; }

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
    }

    /// <summary>Liga/desliga o indicador visual de seleção.</summary>
    public void SetSelected(bool selected)
    {
        if (selectedIndicator != null)
            selectedIndicator.SetActive(selected);
    }

    private void HandleClick()
    {
        _onTapped?.Invoke(_definition);
    }
}
