using System;
using System.Collections.Generic;

/// <summary>
/// Estatísticas canônicas dos bancos de questões, mantidas no Firestore em
/// Config/QuestionStats pelo Cloud Function <c>syncQuestionStats</c>.
///
/// Fonte única de verdade para o total de questões do app (cliente NUNCA escreve).
/// Usado pelo <see cref="PlayerLevelService"/> como denominador de referência
/// quando o jogador sobe de nível e re-sincroniza o snapshot.
/// </summary>
[System.Serializable]
public class QuestionStats
{
    /// <summary>Total de questões somando todos os bancos. Valor canônico.</summary>
    public int TotalQuestions { get; set; }

    /// <summary>Mapeamento databankName → quantidade de questões naquele banco.</summary>
    public Dictionary<string, int> PerBank { get; set; } = new Dictionary<string, int>();

    /// <summary>Versão incremental — usada para invalidar caches.</summary>
    public long Version { get; set; }

    /// <summary>Timestamp da última atualização feita pelo Cloud Function.</summary>
    public DateTime UpdatedAt { get; set; }
}
