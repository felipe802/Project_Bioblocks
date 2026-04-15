// DIAGNÓSTICO TEMPORÁRIO — remover após uso
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class QuestionCacheDiagnostic
{
    [MenuItem("BioBlocks/Diagnóstico/Listar DatabankNames no LiteDB")]
    public static void ListDatabankNamesInCache()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Diagnóstico] Entre em Play Mode primeiro e então rode o diagnóstico.");
            return;
        }

        if (AppContext.LocalDatabase == null)
        {
            Debug.LogError("[Diagnóstico] AppContext.LocalDatabase é null — AppContext ainda não inicializou.");
            return;
        }

        var col = AppContext.LocalDatabase.Questions;
        var total = col.Count();

        if (total == 0)
        {
            Debug.LogWarning("[Diagnóstico] Cache vazio — nenhuma questão encontrada no LiteDB.");
            return;
        }

        var groups = col.FindAll()
                        .GroupBy(q => q.QuestionDatabankName)
                        .OrderBy(g => g.Key)
                        .ToList();

        Debug.Log($"[Diagnóstico] Total de questões no cache: {total}");
        Debug.Log($"[Diagnóstico] {groups.Count} banco(s) encontrado(s):");
        foreach (var g in groups)
            Debug.Log($"  → \"{g.Key}\" : {g.Count()} questões");
    }

    [MenuItem("BioBlocks/Diagnóstico/Deletar Cache LiteDB")]
    public static void DeleteCache()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("[Diagnóstico] Saia do Play Mode antes de deletar o cache.");
            return;
        }

        // Deleta o banco principal E o arquivo de log do LiteDB
        string[] files = {
            System.IO.Path.Combine(Application.persistentDataPath, "app_cache.db"),
            System.IO.Path.Combine(Application.persistentDataPath, "app_cache-log.db")
        };

        bool deletedAny = false;
        foreach (var path in files)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                Debug.Log($"[Diagnóstico] Deletado: {path}");
                deletedAny = true;
            }
        }

        if (!deletedAny)
            Debug.LogWarning($"[Diagnóstico] Nenhum arquivo de cache encontrado em: {Application.persistentDataPath}");
    }
}
#endif
