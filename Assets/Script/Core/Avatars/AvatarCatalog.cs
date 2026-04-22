// Assets/Script/Core/Avatars/AvatarCatalog.cs
//
// Fonte única da verdade dos avatares preset.
// Classe estática populada declarativamente — adicionar/remover avatar implica
// editar este arquivo + ter o PNG correspondente em Assets/Resources/Avatars/<Classe>/.
//
// Convenção:
//   Id físico do arquivo: Avatars/<Classe>/avatar_<classeId>_<NN>.png
//   Id lógico (persistido como preset:<Id>): avatar_<classeId>_<NN>
//   <Classe>     → PascalCase (nome da pasta, ex.: "DNA", "PlantCell")
//   <classeId>   → lowercase  (chave no catálogo, ex.: "dna", "plantcell")
//   <NN>         → zero-padded de 01 a 05 (1ª variante é a default)

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Catálogo estático de todos os avatares preset disponíveis no app.
/// Consumidores devem acessar avatares exclusivamente por esta API,
/// nunca por caminho hardcoded de Resources.
/// </summary>
public static class AvatarCatalog
{
    // ─────────────────────────────────────────────────────────────────────
    // Definições — ordem determinística para UI (classe, depois variante)
    // ─────────────────────────────────────────────────────────────────────
    private static readonly AvatarDefinition[] _all = new[]
    {
        // DNA
        Make("dna",          "DNA",          1, "DNA"),
        Make("dna",          "DNA",          2, "DNA"),
        Make("dna",          "DNA",          3, "DNA"),
        Make("dna",          "DNA",          4, "DNA"),
        Make("dna",          "DNA",          5, "DNA"),

        // Cell
        Make("cell",         "Cell",         1, "Célula"),
        Make("cell",         "Cell",         2, "Célula"),
        Make("cell",         "Cell",         3, "Célula"),
        Make("cell",         "Cell",         4, "Célula"),
        Make("cell",         "Cell",         5, "Célula"),

        // Bacteria
        Make("bacteria",     "Bacteria",     1, "Bactéria"),
        Make("bacteria",     "Bacteria",     2, "Bactéria"),
        Make("bacteria",     "Bacteria",     3, "Bactéria"),
        Make("bacteria",     "Bacteria",     4, "Bactéria"),
        Make("bacteria",     "Bacteria",     5, "Bactéria"),

        // Virus
        Make("virus",        "Virus",        1, "Vírus"),
        Make("virus",        "Virus",        2, "Vírus"),
        Make("virus",        "Virus",        3, "Vírus"),
        Make("virus",        "Virus",        4, "Vírus"),
        Make("virus",        "Virus",        5, "Vírus"),

        // Protein
        Make("protein",      "Protein",      1, "Proteína"),
        Make("protein",      "Protein",      2, "Proteína"),
        Make("protein",      "Protein",      3, "Proteína"),
        Make("protein",      "Protein",      4, "Proteína"),
        Make("protein",      "Protein",      5, "Proteína"),

        // Mitochondria
        Make("mitochondria", "Mitochondria", 1, "Mitocôndria"),
        Make("mitochondria", "Mitochondria", 2, "Mitocôndria"),
        Make("mitochondria", "Mitochondria", 3, "Mitocôndria"),
        Make("mitochondria", "Mitochondria", 4, "Mitocôndria"),
        Make("mitochondria", "Mitochondria", 5, "Mitocôndria"),

        // Ribosome
        Make("ribosome",     "Ribosome",     1, "Ribossomo"),
        Make("ribosome",     "Ribosome",     2, "Ribossomo"),
        Make("ribosome",     "Ribosome",     3, "Ribossomo"),
        Make("ribosome",     "Ribosome",     4, "Ribossomo"),
        Make("ribosome",     "Ribosome",     5, "Ribossomo"),

        // Sugar
        Make("sugar",        "Sugar",        1, "Açúcar"),
        Make("sugar",        "Sugar",        2, "Açúcar"),
        Make("sugar",        "Sugar",        3, "Açúcar"),
        Make("sugar",        "Sugar",        4, "Açúcar"),
        Make("sugar",        "Sugar",        5, "Açúcar"),

        // Neuron
        Make("neuron",       "Neuron",       1, "Neurônio"),
        Make("neuron",       "Neuron",       2, "Neurônio"),
        Make("neuron",       "Neuron",       3, "Neurônio"),
        Make("neuron",       "Neuron",       4, "Neurônio"),
        Make("neuron",       "Neuron",       5, "Neurônio"),

        // PlantCell
        Make("plantcell",    "PlantCell",    1, "Célula Vegetal"),
        Make("plantcell",    "PlantCell",    2, "Célula Vegetal"),
        Make("plantcell",    "PlantCell",    3, "Célula Vegetal"),
        Make("plantcell",    "PlantCell",    4, "Célula Vegetal"),
        Make("plantcell",    "PlantCell",    5, "Célula Vegetal"),
    };

    // ─────────────────────────────────────────────────────────────────────
    // Índices pré-computados (inicializados no construtor estático)
    // ─────────────────────────────────────────────────────────────────────
    private static readonly Dictionary<string, AvatarDefinition>        _byId;
    private static readonly Dictionary<string, List<AvatarDefinition>>  _byClass;
    private static readonly AvatarDefinition[]                          _defaults;

    static AvatarCatalog()
    {
        _byId     = _all.ToDictionary(a => a.Id);
        _byClass  = _all.GroupBy(a => a.ClassId)
                        .ToDictionary(g => g.Key, g => g.OrderBy(a => a.Variant).ToList());
        _defaults = _all.Where(a => a.IsDefault).ToArray();
    }

    // ─────────────────────────────────────────────────────────────────────
    // API pública
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Todos os avatares em ordem determinística (classe, depois variante).</summary>
    public static IReadOnlyList<AvatarDefinition> All => _all;

    /// <summary>Avatares desbloqueados por padrão (um por classe nesta etapa).</summary>
    public static IReadOnlyList<AvatarDefinition> Defaults => _defaults;

    /// <summary>Lista de todas as classes biológicas (lowercase).</summary>
    public static IReadOnlyList<string> ClassIds => _byClass.Keys.ToList();

    /// <summary>Retorna a definição do avatar ou <c>null</c> se o id não existe.</summary>
    public static AvatarDefinition GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _byId.TryGetValue(id, out var def) ? def : null;
    }

    /// <summary>Retorna as variantes de uma classe (ordenadas por <c>Variant</c>).</summary>
    public static IReadOnlyList<AvatarDefinition> GetByClass(string classId)
    {
        if (string.IsNullOrEmpty(classId)) return System.Array.Empty<AvatarDefinition>();
        return _byClass.TryGetValue(classId, out var list)
            ? (IReadOnlyList<AvatarDefinition>)list
            : System.Array.Empty<AvatarDefinition>();
    }

    /// <summary><c>true</c> se o id existe no catálogo.</summary>
    public static bool Contains(string id) => !string.IsNullOrEmpty(id) && _byId.ContainsKey(id);

    /// <summary>Retorna o avatar default da classe ou <c>null</c> se a classe não existe.</summary>
    public static AvatarDefinition GetDefaultOfClass(string classId)
    {
        if (string.IsNullOrEmpty(classId)) return null;
        if (!_byClass.TryGetValue(classId, out var list)) return null;
        for (int i = 0; i < list.Count; i++)
            if (list[i].IsDefault) return list[i];
        return null;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Factory interna — centraliza a convenção de nomes em um único lugar
    // ─────────────────────────────────────────────────────────────────────
    private static AvatarDefinition Make(
        string classId,
        string folderName,
        int variant,
        string displayName)
    {
        string id           = $"avatar_{classId}_{variant:00}";
        string resourcePath = $"Avatars/{folderName}/{id}";
        bool   isDefault    = (variant == 1);

        return new AvatarDefinition(id, classId, variant, resourcePath, isDefault, displayName);
    }
}
