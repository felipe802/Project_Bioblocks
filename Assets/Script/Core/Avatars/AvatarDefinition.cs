// Assets/Script/Core/Avatars/AvatarDefinition.cs
//
// Metadados imutáveis de um avatar preset.
// Instâncias vivem em AvatarCatalog — não devem ser criadas em runtime.

/// <summary>
/// Representação imutável de um avatar preset.
/// Um avatar é identificado por <see cref="Id"/> (ex.: "avatar_dna_01"),
/// pertence a uma classe biológica (<see cref="ClassId"/>, ex.: "dna"),
/// tem uma variante numérica dentro da classe (<see cref="Variant"/>, 1..5),
/// e é carregável em runtime via <c>Resources.Load&lt;Texture2D&gt;(<see cref="ResourcePath"/>)</c>.
/// </summary>
public sealed class AvatarDefinition
{
    /// <summary>Identificador único e estável. Persistido como <c>preset:&lt;Id&gt;</c>.</summary>
    public string Id { get; }

    /// <summary>Classe biológica (ex.: "dna", "cell", "plantcell"). Lowercase.</summary>
    public string ClassId { get; }

    /// <summary>Variante dentro da classe (1 a 5 neste primeiro release).</summary>
    public int Variant { get; }

    /// <summary>Caminho relativo para <c>Resources.Load</c> (sem extensão).</summary>
    public string ResourcePath { get; }

    /// <summary>
    /// <c>true</c> se este é o avatar desbloqueado por padrão da classe.
    /// Na Etapa 1 existe exatamente 1 default por classe (variante 1).
    /// </summary>
    public bool IsDefault { get; }

    /// <summary>Nome amigável para UI (ex.: "DNA", "Célula Vegetal"). Usado em tooltips/headers.</summary>
    public string DisplayName { get; }

    public AvatarDefinition(
        string id,
        string classId,
        int variant,
        string resourcePath,
        bool isDefault,
        string displayName)
    {
        Id           = id;
        ClassId      = classId;
        Variant      = variant;
        ResourcePath = resourcePath;
        IsDefault    = isDefault;
        DisplayName  = displayName;
    }

    public override string ToString() => $"AvatarDefinition({Id}, class={ClassId}, var={Variant}, default={IsDefault})";
}
