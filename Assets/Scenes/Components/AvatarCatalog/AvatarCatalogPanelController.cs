using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla a lógica específica do catálogo de avatares.
/// Toda a lógica de visibilidade e animação foi delegada ao SlidePanel.
///
/// SETUP NO INSPECTOR:
///   1. Slide Panel    → objeto AvatarCatalogPanel (que tem o componente SlidePanel)
///   2. Grid Container → o Transform "Content" dentro do Scroll View
///   3. Cell Prefab    → o prefab AvatarCell
///   4. Close Button   → opcional; configure onClick → SlidePanel.Hide()
///
/// EVENTOS DO SlidePanel (conecte no Inspector do SlidePanel):
///   OnBeforeShow → AvatarCatalogPanelController.OnPanelWillShow()
///   OnBeforeHide → AvatarCatalogPanelController.OnPanelWillHide()
/// </summary>
public class AvatarCatalogPanelController : MonoBehaviour
{
    [Header("Painel")]
    [Tooltip("Objeto que tem o componente SlidePanel")]
    [SerializeField] private SlidePanel slidePanel;

    [Header("Grade")]
    [SerializeField] private Transform            gridContainer;
    [SerializeField] private AvatarCellController cellPrefab;

    [Header("Opcional")]
    [SerializeField] private Button closeButton;

    // ── Estado interno ─────────────────────────────────────────────────────────

    private const string PRESET_PREFIX = "preset:";

    private bool                        _gridBuilt;
    private readonly List<AvatarCellController> _cells = new List<AvatarCellController>();

    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(() => slidePanel.Hide());
    }

    // ── API pública ────────────────────────────────────────────────────────────

    /// <summary>
    /// Abre o catálogo. Conecte ao onClick do botão de avatar no top bar.
    /// </summary>
    public void Show() => slidePanel.Show();

    // ── Callbacks do SlidePanel (conecte nos eventos do Inspector) ─────────────

    /// <summary>
    /// Chamado pelo SlidePanel.OnBeforeShow — prepara o conteúdo antes da animação começar.
    /// Conecte em: SlidePanel → OnBeforeShow → AvatarCatalogPanelController.OnPanelWillShow
    /// </summary>
    public void OnPanelWillShow()
    {
        if (AppContext.AvatarSelection == null)
        {
            Debug.LogError("[AvatarCatalogPanel] AppContext.AvatarSelection não disponível.");
            return;
        }

        // Lazy: grade construída apenas na primeira abertura
        if (!_gridBuilt) BuildGrid();

        AppContext.AvatarSelection.BeginSession();
        RefreshHighlightFromCurrentUser();
    }

    /// <summary>
    /// Chamado pelo SlidePanel.OnBeforeHide — persiste a seleção antes de fechar.
    /// Conecte em: SlidePanel → OnBeforeHide → AvatarCatalogPanelController.OnPanelWillHide
    /// </summary>
    public void OnPanelWillHide()
    {
        if (AppContext.AvatarSelection != null)
            _ = AppContext.AvatarSelection.CommitSessionAsync();
    }

    // ── Grade ──────────────────────────────────────────────────────────────────

    private void BuildGrid()
    {
        if (cellPrefab == null || gridContainer == null)
        {
            Debug.LogError("[AvatarCatalogPanel] cellPrefab ou gridContainer não atribuído.");
            return;
        }

        _cells.Clear();

        // Ordenação para exibição:
        //   1) Variante (1 primeiro → todos os "-01" desbloqueados vêm juntos no início)
        //   2) ClassId (ordem alfabética estável dentro de cada bloco de variante)
        // Isso agrupa visualmente as células habilitadas, seguidas das bloqueadas.
        var ordered = AvatarCatalog.All
            .OrderBy(d => d.Variant)
            .ThenBy(d => d.ClassId);

        foreach (var def in ordered)
        {
            var cell = Instantiate(cellPrefab, gridContainer);
            cell.Bind(def, OnCellTapped);

            // Nesta etapa, apenas os avatares "-01" (primeira variante de cada classe)
            // estão desbloqueados para seleção. Os demais ficam com overlay ativado e
            // botão não-interativo.
            cell.SetLocked(!IsUnlocked(def));

            _cells.Add(cell);
        }

        _gridBuilt = true;
        Debug.Log($"[AvatarCatalogPanel] Grade construída com {_cells.Count} células.");
    }

    /// <summary>
    /// Regra de disponibilidade: o avatar está desbloqueado se for a variante 01
    /// da sua classe (ex.: <c>avatar_dna_01</c>, <c>avatar_cell_01</c>, ...).
    /// Equivalente a <c>def.IsDefault</c> no estado atual do catálogo, mas usamos
    /// o sufixo do Id para refletir literalmente a regra "termina em -01".
    /// </summary>
    private static bool IsUnlocked(AvatarDefinition def)
    {
        return def != null && def.Id != null && def.Id.EndsWith("_01");
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
        var currentId = url.StartsWith(PRESET_PREFIX)
            ? url.Substring(PRESET_PREFIX.Length)
            : null;
        HighlightSelection(currentId);
    }

    private void HighlightSelection(string selectedId)
    {
        for (int i = 0; i < _cells.Count; i++)
        {
            var cell = _cells[i];
            if (cell == null) continue;

            // Células bloqueadas nunca aparecem como selecionadas,
            // mesmo que o usuário tivesse um avatar não-01 salvo antes desta regra.
            bool isSelected = !cell.IsLocked && cell.AvatarId == selectedId;
            cell.SetSelected(isSelected);
        }
    }
}