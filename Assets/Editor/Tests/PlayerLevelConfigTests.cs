// Assets/Editor/Tests/PlayerLevelConfigTests.cs
// Testes unitários para PlayerLevelConfig e LevelThreshold.
//
// Classe estática pura — zero dependências, zero setup.
// Todos os testes rodam em Edit Mode.
//
// Thresholds configurados no projeto:
//   Nível 1:  0% –  10%  | bônus 1000
//   Nível 2: 10% –  20%  | bônus 2000
//   Nível 3: 20% –  30%  | bônus 3000
//   Nível 4: 30% –  40%  | bônus 4000
//   Nível 5: 40% –  50%  | bônus 5000
//   Nível 6: 50% –  60%  | bônus 6000
//   Nível 7: 60% –  70%  | bônus 7000
//   Nível 8: 70% –  80%  | bônus 8000
//   Nível 9: 80% –  90%  | bônus 9000
//   Nível 10: 90% – 100% | bônus 10000
//
// Para rodar: Window → General → Test Runner → EditMode → Run All

using NUnit.Framework;

[TestFixture]
public class PlayerLevelConfigTests
{
    // =======================================================
    // CalculateLevel — casos básicos de progressão
    // =======================================================

    [Test]
    public void CalculateLevel_TotalZero_RetornaUm()
    {
        // Proteção contra divisão por zero
        Assert.AreEqual(1, PlayerLevelConfig.CalculateLevel(0, 0));
    }

    [Test]
    public void CalculateLevel_TotalNegativo_RetornaUm()
    {
        Assert.AreEqual(1, PlayerLevelConfig.CalculateLevel(0, -10));
    }

    [Test]
    public void CalculateLevel_ZeroRespondidas_RetornaUm()
    {
        Assert.AreEqual(1, PlayerLevelConfig.CalculateLevel(0, 100));
    }

    [Test]
    public void CalculateLevel_5PorCento_RetornaUm()
    {
        // 5/100 = 5% → dentro do nível 1 (0%–10%)
        Assert.AreEqual(1, PlayerLevelConfig.CalculateLevel(5, 100));
    }

    [Test]
    public void CalculateLevel_ExatamenteFronteiraNivel2_RetornaDois()
    {
        // 10/100 = 10% → fronteira inferior do nível 2 (≥ 10%)
        Assert.AreEqual(2, PlayerLevelConfig.CalculateLevel(10, 100));
    }

    [Test]
    public void CalculateLevel_15PorCento_RetornaDois()
    {
        Assert.AreEqual(2, PlayerLevelConfig.CalculateLevel(15, 100));
    }

    [Test]
    public void CalculateLevel_Nivel5_40PorCento()
    {
        Assert.AreEqual(5, PlayerLevelConfig.CalculateLevel(40, 100));
    }

    [Test]
    public void CalculateLevel_Nivel9_85PorCento()
    {
        Assert.AreEqual(9, PlayerLevelConfig.CalculateLevel(85, 100));
    }

    [Test]
    public void CalculateLevel_ExatamenteFronteiraNivel10_RetornaDez()
    {
        // 90/100 = 90% → fronteira inferior do nível 10
        Assert.AreEqual(10, PlayerLevelConfig.CalculateLevel(90, 100));
    }

    [Test]
    public void CalculateLevel_100PorCento_RetornaDez()
    {
        // 100% respondidas → nível máximo (10)
        // O loop não encontra threshold que cubra 100% (MaxPercentage é exclusive),
        // então o método retorna 10 como fallback — comportamento esperado
        Assert.AreEqual(10, PlayerLevelConfig.CalculateLevel(100, 100));
    }

    [Test]
    public void CalculateLevel_AcimaDeTotal_RetornaDez()
    {
        // Mais respostas do que questões (edge case de migração)
        Assert.AreEqual(10, PlayerLevelConfig.CalculateLevel(150, 100));
    }

    // =======================================================
    // CalculateLevel — com diferentes totais de questões
    // =======================================================

    [Test]
    public void CalculateLevel_TotalDiferente_MesmaPercentagem_MesmoNivel()
    {
        // 20% de 50 == 20% de 200 → mesmo nível
        int nivel50  = PlayerLevelConfig.CalculateLevel(10, 50);
        int nivel200 = PlayerLevelConfig.CalculateLevel(40, 200);
        Assert.AreEqual(nivel50, nivel200,
            "O nível deve depender da percentagem, não do total absoluto");
    }

    // =======================================================
    // GetBonusForLevel
    // =======================================================

    [Test]
    public void GetBonusForLevel_Nivel1_Retorna1000()
    {
        Assert.AreEqual(1000, PlayerLevelConfig.GetBonusForLevel(1));
    }

    [Test]
    public void GetBonusForLevel_Nivel5_Retorna5000()
    {
        Assert.AreEqual(5000, PlayerLevelConfig.GetBonusForLevel(5));
    }

    [Test]
    public void GetBonusForLevel_Nivel10_Retorna10000()
    {
        Assert.AreEqual(10000, PlayerLevelConfig.GetBonusForLevel(10));
    }

    [Test]
    public void GetBonusForLevel_NivelInexistente_RetornaZero()
    {
        // Nível 11 não existe na configuração
        Assert.AreEqual(0, PlayerLevelConfig.GetBonusForLevel(11));
    }

    [Test]
    public void GetBonusForLevel_NivelZero_RetornaZero()
    {
        Assert.AreEqual(0, PlayerLevelConfig.GetBonusForLevel(0));
    }

    [Test]
    public void GetBonusForLevel_CresceCadaNivel()
    {
        // O bônus deve crescer a cada nível — garante que a tabela não foi invertida
        for (int level = 1; level < 10; level++)
        {
            int bonusAtual   = PlayerLevelConfig.GetBonusForLevel(level);
            int bonusProximo = PlayerLevelConfig.GetBonusForLevel(level + 1);
            Assert.Greater(bonusProximo, bonusAtual,
                $"Bônus do nível {level + 1} deve ser maior que o do nível {level}");
        }
    }

    // =======================================================
    // GetThresholdForLevel
    // =======================================================

    [Test]
    public void GetThresholdForLevel_Nivel1_MinZero_Max10PorCento()
    {
        var t = PlayerLevelConfig.GetThresholdForLevel(1);
        Assert.AreEqual(1,     t.Level);
        Assert.AreEqual(0.00f, t.MinPercentage, delta: 0.001f);
        Assert.AreEqual(0.10f, t.MaxPercentage, delta: 0.001f);
    }

    [Test]
    public void GetThresholdForLevel_Nivel10_Min90_Max100PorCento()
    {
        var t = PlayerLevelConfig.GetThresholdForLevel(10);
        Assert.AreEqual(10,    t.Level);
        Assert.AreEqual(0.90f, t.MinPercentage, delta: 0.001f);
        Assert.AreEqual(1.00f, t.MaxPercentage, delta: 0.001f);
    }

    [Test]
    public void GetThresholdForLevel_NivelInexistente_RetornaFallback()
    {
        // Nível inexistente retorna o primeiro threshold (nível 1) como fallback
        var t = PlayerLevelConfig.GetThresholdForLevel(99);
        Assert.AreEqual(1, t.Level);
    }

    [Test]
    public void GetThresholdForLevel_TodosNiveis_ThresholdsSaoContiguos()
    {
        // Garante que não há lacunas entre os thresholds
        for (int level = 1; level < 10; level++)
        {
            var atual   = PlayerLevelConfig.GetThresholdForLevel(level);
            var proximo = PlayerLevelConfig.GetThresholdForLevel(level + 1);
            Assert.AreEqual(atual.MaxPercentage, proximo.MinPercentage, delta: 0.001f,
                $"Threshold do nível {level} (max={atual.MaxPercentage}) deve ser " +
                $"contíguo ao nível {level + 1} (min={proximo.MinPercentage})");
        }
    }

    // =======================================================
    // LevelThreshold.GetRequiredQuestions
    // =======================================================

    [Test]
    public void GetRequiredQuestions_Nivel1_10PorCentoDeTotal()
    {
        var t = PlayerLevelConfig.GetThresholdForLevel(1);
        // 10% de 100 = 10
        Assert.AreEqual(10, t.GetRequiredQuestions(100));
    }

    [Test]
    public void GetRequiredQuestions_Nivel5_50PorCentoDeTotal()
    {
        var t = PlayerLevelConfig.GetThresholdForLevel(5);
        // 50% de 200 = 100
        Assert.AreEqual(100, t.GetRequiredQuestions(200));
    }

    [Test]
    public void GetRequiredQuestions_UsaCeil_ArredondaParaCima()
    {
        var t = PlayerLevelConfig.GetThresholdForLevel(1);
        // 10% de 15 = 1.5 → Ceil = 2
        Assert.AreEqual(2, t.GetRequiredQuestions(15));
    }

    // =======================================================
    // LevelThreshold.GetMinRequiredQuestions
    // =======================================================

    [Test]
    public void GetMinRequiredQuestions_Nivel1_ZeroQuestoes()
    {
        var t = PlayerLevelConfig.GetThresholdForLevel(1);
        // 0% de qualquer total = 0
        Assert.AreEqual(0, t.GetMinRequiredQuestions(100));
    }

    [Test]
    public void GetMinRequiredQuestions_Nivel2_10PorCentoDeTotal()
    {
        var t = PlayerLevelConfig.GetThresholdForLevel(2);
        // min = 10% de 100 = 10
        Assert.AreEqual(10, t.GetMinRequiredQuestions(100));
    }

    [Test]
    public void GetMinRequiredQuestions_Nivel10_90PorCentoDeTotal()
    {
        var t = PlayerLevelConfig.GetThresholdForLevel(10);
        // 90% de 100 = 90
        Assert.AreEqual(90, t.GetMinRequiredQuestions(100));
    }

    // =======================================================
    // Consistência geral da tabela
    // =======================================================

    [Test]
    public void LEVEL_THRESHOLDS_TemExatamente10Niveis()
    {
        Assert.AreEqual(10, PlayerLevelConfig.LEVEL_THRESHOLDS.Length);
    }

    [Test]
    public void LEVEL_THRESHOLDS_NiveisOrdenados()
    {
        for (int i = 0; i < PlayerLevelConfig.LEVEL_THRESHOLDS.Length; i++)
            Assert.AreEqual(i + 1, PlayerLevelConfig.LEVEL_THRESHOLDS[i].Level,
                $"Threshold[{i}] deve ser o nível {i + 1}");
    }
}
