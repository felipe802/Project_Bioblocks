// Assets/Editor/Tests/AvatarCatalogTests.cs
//
// Testes estruturais do AvatarCatalog — pega erros de digitação, inconsistência
// de case, arquivos faltando em Resources, duplicações, e garante que a API
// pública se comporta como contratada.
//
// O que É testado:
//   ✅ Catálogo tem exatamente 50 avatares (10 classes × 5 variantes)
//   ✅ 10 classes distintas, cada uma com 5 variantes (1..5)
//   ✅ Exatamente 1 default por classe, e é a variante 1
//   ✅ Todos os ids são únicos
//   ✅ Todos os ids seguem o padrão avatar_<classId>_<NN>
//   ✅ Todos os ClassIds são lowercase
//   ✅ Todos os ResourcePaths resolvem via Resources.Load<Texture2D>
//   ✅ GetById retorna a definição correta
//   ✅ GetById com id inexistente retorna null (não lança)
//   ✅ GetByClass retorna as 5 variantes ordenadas
//   ✅ GetDefaultOfClass retorna a variante 1
//   ✅ Contains funciona para ids válidos e inválidos
//
// Observação: por acessar Resources, este teste precisa que os PNGs estejam
// nos caminhos esperados. Roda em Edit Mode.

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[TestFixture]
public class AvatarCatalogTests
{
    // =======================================================
    // Tamanho e estrutura
    // =======================================================

    [Test]
    public void Catalog_ContainsExactly50Avatars()
    {
        Assert.AreEqual(50, AvatarCatalog.All.Count,
            "Catálogo deve ter 50 avatares (10 classes × 5 variantes)");
    }

    [Test]
    public void Catalog_Has10DistinctClasses()
    {
        var classes = AvatarCatalog.All.Select(a => a.ClassId).Distinct().ToList();
        Assert.AreEqual(10, classes.Count, $"Esperado 10 classes, obtido {classes.Count}: {string.Join(",", classes)}");
    }

    [Test]
    public void Catalog_EachClassHas5Variants()
    {
        var byClass = AvatarCatalog.All.GroupBy(a => a.ClassId);
        foreach (var group in byClass)
        {
            var variants = group.Select(a => a.Variant).OrderBy(v => v).ToList();
            Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }, variants.ToArray(),
                $"Classe '{group.Key}' deve ter exatamente as variantes 1..5");
        }
    }

    // =======================================================
    // Defaults
    // =======================================================

    [Test]
    public void Catalog_Has10Defaults()
    {
        Assert.AreEqual(10, AvatarCatalog.Defaults.Count,
            "Deve haver exatamente 1 default por classe");
    }

    [Test]
    public void Catalog_EachDefaultIsVariant1()
    {
        foreach (var def in AvatarCatalog.Defaults)
        {
            Assert.AreEqual(1, def.Variant,
                $"Default da classe '{def.ClassId}' deve ser variante 1, mas é {def.Variant}");
        }
    }

    [Test]
    public void Catalog_EachClassHasExactlyOneDefault()
    {
        var defaultsByClass = AvatarCatalog.All
            .Where(a => a.IsDefault)
            .GroupBy(a => a.ClassId);

        foreach (var group in defaultsByClass)
        {
            Assert.AreEqual(1, group.Count(),
                $"Classe '{group.Key}' deve ter exatamente 1 default");
        }
    }

    // =======================================================
    // Integridade dos IDs
    // =======================================================

    [Test]
    public void Catalog_AllIdsAreUnique()
    {
        var ids = AvatarCatalog.All.Select(a => a.Id).ToList();
        Assert.AreEqual(ids.Count, ids.Distinct().Count(), "Existem ids duplicados no catálogo");
    }

    [Test]
    public void Catalog_AllIdsMatchConvention()
    {
        // avatar_<classId lowercase>_<NN zero-padded>
        var pattern = new Regex(@"^avatar_[a-z]+_0[1-9]$");
        foreach (var def in AvatarCatalog.All)
        {
            Assert.IsTrue(pattern.IsMatch(def.Id),
                $"Id '{def.Id}' não segue o padrão avatar_<classe>_<NN>");
        }
    }

    [Test]
    public void Catalog_AllClassIdsAreLowercase()
    {
        foreach (var def in AvatarCatalog.All)
        {
            Assert.AreEqual(def.ClassId.ToLowerInvariant(), def.ClassId,
                $"ClassId '{def.ClassId}' deve ser lowercase");
        }
    }

    [Test]
    public void Catalog_IdContainsClassIdAndVariant()
    {
        foreach (var def in AvatarCatalog.All)
        {
            string expectedId = $"avatar_{def.ClassId}_{def.Variant:00}";
            Assert.AreEqual(expectedId, def.Id,
                $"Id '{def.Id}' inconsistente com classId='{def.ClassId}' e variant={def.Variant}");
        }
    }

    // =======================================================
    // Resolução física (Resources.Load)
    // Este é o teste que pega arquivos faltando ou path errado.
    // =======================================================

    [Test]
    public void Catalog_AllResourcePathsResolveToTexture()
    {
        var missing = new List<string>();
        foreach (var def in AvatarCatalog.All)
        {
            var tex = Resources.Load<Texture2D>(def.ResourcePath);
            if (tex == null)
                missing.Add($"{def.Id} → {def.ResourcePath}");
        }

        Assert.IsEmpty(missing,
            $"{missing.Count} avatar(es) não foram encontrados em Resources:\n  " +
            string.Join("\n  ", missing));
    }

    // =======================================================
    // API pública
    // =======================================================

    [Test]
    public void GetById_KnownId_ReturnsDefinition()
    {
        var def = AvatarCatalog.GetById("avatar_dna_01");
        Assert.IsNotNull(def);
        Assert.AreEqual("dna", def.ClassId);
        Assert.AreEqual(1, def.Variant);
        Assert.IsTrue(def.IsDefault);
    }

    [Test]
    public void GetById_UnknownId_ReturnsNull()
    {
        Assert.IsNull(AvatarCatalog.GetById("avatar_unknown_99"));
    }

    [Test]
    public void GetById_NullOrEmpty_ReturnsNull()
    {
        Assert.IsNull(AvatarCatalog.GetById(null));
        Assert.IsNull(AvatarCatalog.GetById(""));
    }

    [Test]
    public void GetByClass_KnownClass_Returns5VariantsSorted()
    {
        var dnaAvatars = AvatarCatalog.GetByClass("dna");
        Assert.AreEqual(5, dnaAvatars.Count);

        for (int i = 0; i < 5; i++)
            Assert.AreEqual(i + 1, dnaAvatars[i].Variant, "Deve estar ordenado por variante");
    }

    [Test]
    public void GetByClass_UnknownClass_ReturnsEmpty()
    {
        var result = AvatarCatalog.GetByClass("unknown");
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [Test]
    public void GetByClass_NullOrEmpty_ReturnsEmpty()
    {
        Assert.IsEmpty(AvatarCatalog.GetByClass(null));
        Assert.IsEmpty(AvatarCatalog.GetByClass(""));
    }

    [Test]
    public void GetDefaultOfClass_KnownClass_ReturnsVariant1()
    {
        foreach (var classId in AvatarCatalog.ClassIds)
        {
            var def = AvatarCatalog.GetDefaultOfClass(classId);
            Assert.IsNotNull(def, $"Default da classe '{classId}' não encontrado");
            Assert.AreEqual(1, def.Variant);
            Assert.IsTrue(def.IsDefault);
        }
    }

    [Test]
    public void GetDefaultOfClass_UnknownClass_ReturnsNull()
    {
        Assert.IsNull(AvatarCatalog.GetDefaultOfClass("unknown"));
    }

    [Test]
    public void Contains_KnownId_ReturnsTrue()
    {
        Assert.IsTrue(AvatarCatalog.Contains("avatar_dna_01"));
    }

    [Test]
    public void Contains_UnknownId_ReturnsFalse()
    {
        Assert.IsFalse(AvatarCatalog.Contains("avatar_unknown_99"));
        Assert.IsFalse(AvatarCatalog.Contains(null));
        Assert.IsFalse(AvatarCatalog.Contains(""));
    }

    [Test]
    public void ClassIds_Has10Entries()
    {
        Assert.AreEqual(10, AvatarCatalog.ClassIds.Count);
    }
}
