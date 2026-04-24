using UnityEngine;

/// <summary>
/// Configuração dos níveis de jogador — 10 faixas de 10% cada,
/// com bônus de 1000 a 10000 pontos.
///
/// <para>
/// Regra fundamental: o nível NUNCA deve diminuir (monotonicidade).
/// O denominador usado no cálculo (<c>totalQuestions</c>) é o
/// <c>LevelSnapshotDenominator</c> do usuário, congelado no momento
/// do último level-up. Isso evita que um aumento/diminuição no total
/// de questões do app faça o jogador pular ou perder níveis sem
/// responder uma questão sequer.
/// </para>
/// </summary>
public static class PlayerLevelConfig
{
    public static readonly LevelThreshold[] LEVEL_THRESHOLDS = new LevelThreshold[]
    {
        new LevelThreshold(1,  0.00f, 0.10f, 1000),
        new LevelThreshold(2,  0.10f, 0.20f, 2000),
        new LevelThreshold(3,  0.20f, 0.30f, 3000),
        new LevelThreshold(4,  0.30f, 0.40f, 4000),
        new LevelThreshold(5,  0.40f, 0.50f, 5000),
        new LevelThreshold(6,  0.50f, 0.60f, 6000),
        new LevelThreshold(7,  0.60f, 0.70f, 7000),
        new LevelThreshold(8,  0.70f, 0.80f, 8000),
        new LevelThreshold(9,  0.80f, 0.90f, 9000),
        new LevelThreshold(10, 0.90f, 1.00f, 10000)
    };

    public static int MaxLevel => LEVEL_THRESHOLDS[LEVEL_THRESHOLDS.Length - 1].Level;

    [System.Serializable]
    public struct LevelThreshold
    {
        public int Level;
        public float MinPercentage;
        public float MaxPercentage;
        public int BonusPoints;

        public LevelThreshold(int level, float min, float max, int bonus)
        {
            Level = level;
            MinPercentage = min;
            MaxPercentage = max;
            BonusPoints = bonus;
        }

        public int GetRequiredQuestions(int totalQuestions)
        {
            return CeilExact(totalQuestions, MaxPercentage);
        }

        public int GetMinRequiredQuestions(int totalQuestions)
        {
            return CeilExact(totalQuestions, MinPercentage);
        }

        // Mathf.CeilToInt(total * pct) e mesmo (double)total * (double)pct sofrem
        // de off-by-one por imprecisão de float: ex. 100 * 0.60f = 60.0000023 →
        // Ceiling = 61. Um epsilon absoluto não resolve porque o drift escala com
        // o total (para total=1e6 o erro chega a ~1.5e-3, estourando epsilons
        // pequenos) e porque 0.10f drifta no sentido positivo (0.10000000149...),
        // o que faz 100 * 0.10f = 10.00000015 → Ceiling = 11.
        //
        // Solução robusta: converter a porcentagem para permille (milésimos) via
        // Round — isso colapsa a imprecisão de float para o inteiro mais próximo
        // (0.60f → 600, 0.10f → 100) — e fazer o resto em aritmética inteira pura.
        // ceil(a/b) = (a + b - 1) / b para inteiros positivos.
        private static int CeilExact(int totalQuestions, float percentage)
        {
            if (totalQuestions <= 0) return 0;
            if (percentage <= 0f)    return 0;

            int permille = (int)System.Math.Round((double)percentage * 1000.0);
            long numerator = (long)totalQuestions * (long)permille;
            return (int)((numerator + 999L) / 1000L);
        }
    }

    /// <summary>
    /// Calcula o nível bruto a partir de uma porcentagem de questões respondidas.
    ///
    /// <para>
    /// Importante: o <paramref name="totalQuestions"/> aqui é o
    /// <c>LevelSnapshotDenominator</c> do usuário — congelado no momento do
    /// último level-up — e NÃO o total atual do app. Isso previne o bug de
    /// fallthrough onde <c>percentage &gt;= 1.0</c> (quando o denominador
    /// encolhia inesperadamente) caía direto no <c>return 10</c>, pulando vários
    /// níveis de uma só vez.
    /// </para>
    ///
    /// <para>
    /// O resultado é clampado em <c>[0, 1]</c> antes de procurar a faixa — se
    /// por algum motivo a porcentagem exceder 1.0, o método retorna
    /// <see cref="MaxLevel"/> deterministicamente em vez de depender do
    /// comportamento de fallthrough do loop.
    /// </para>
    /// </summary>
    public static int CalculateLevel(int questionsAnswered, int totalQuestions)
    {
        if (totalQuestions <= 0)      return 1;
        if (questionsAnswered <= 0)   return 1;

        // Clamp em [0, 1] — defensivo contra denominadores inconsistentes.
        float percentage = Mathf.Clamp01((float)questionsAnswered / totalQuestions);

        // Se todas as questões foram respondidas, vai para o nível máximo.
        // Necessário porque a faixa do último nível é [0.90, 1.00) — exclusive
        // no limite superior, então 1.0f não casa no foreach abaixo.
        if (percentage >= 1.0f) return MaxLevel;

        foreach (var threshold in LEVEL_THRESHOLDS)
        {
            if (percentage >= threshold.MinPercentage && percentage < threshold.MaxPercentage)
                return threshold.Level;
        }

        // Teoricamente inatingível após o clamp; fallback defensivo para nível 1.
        return 1;
    }

    /// <summary>
    /// Retorna true se o jogador acabou de atingir o mínimo para subir
    /// exatamente para <c>currentLevel + 1</c>. Usado pelo
    /// <see cref="PlayerLevelService"/> para decidir se refresca o
    /// <c>LevelSnapshotDenominator</c> e concede bônus.
    /// </summary>
    public static bool CanLevelUp(int currentLevel, int questionsAnswered, int totalQuestions)
    {
        if (totalQuestions <= 0)       return false;
        if (currentLevel >= MaxLevel)  return false;

        int nextLevel      = currentLevel + 1;
        var nextThreshold  = GetThresholdForLevel(nextLevel);
        int required       = nextThreshold.GetMinRequiredQuestions(totalQuestions);

        return questionsAnswered >= required;
    }

    public static int GetBonusForLevel(int level)
    {
        foreach (var threshold in LEVEL_THRESHOLDS)
        {
            if (threshold.Level == level)
                return threshold.BonusPoints;
        }
        return 0;
    }

    public static LevelThreshold GetThresholdForLevel(int level)
    {
        foreach (var threshold in LEVEL_THRESHOLDS)
        {
            if (threshold.Level == level)
                return threshold;
        }
        return LEVEL_THRESHOLDS[0];
    }
}
