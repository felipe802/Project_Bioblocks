// Assets/Editor/Tests/QuestionBonusManagerTests.cs
// Testes unitários para QuestionBonusManager — superfície testável sem Firestore.
//
// QuestionBonusManager tem dois blocos distintos:
//
//   (A) TESTÁVEL sem Firebase:
//       - IsBonusActive()             → lê isBonusActive (campo privado via estado)
//       - GetCurrentScoreMultiplier() → retorna 1 sem bonus, ou combinedMultiplier
//       - ApplyBonusToScore()         → aplica multiplicador ao score base
//       Esses métodos têm fallback local quando UserHeaderManager é null.
//
//   (B) NÃO testável agora (depende de FirebaseFirestore diretamente):
//       - CheckForActiveBonuses / ActivateBonus / CheckIfUserHasActiveBonus
//       QuestionSceneBonusManager chama FirebaseFirestore.DefaultInstance no construtor
//       sem interface — impossível mockar sem refatoração.
//
//   (C) NÃO testável agora (usa reflection sobre QuestionManager):
//       - CheckAnswer — acessa campo privado "currentSession" via BindingFlags
//
// Para rodar: Window → General → Test Runner → EditMode → Run All

using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class QuestionBonusManagerTests
{
    private GameObject           _managerGO;
    private QuestionBonusManager _bonusManager;

    [SetUp]
    public void Setup()
    {
        _managerGO    = new GameObject("QuestionBonusManager");
        _bonusManager = _managerGO.AddComponent<QuestionBonusManager>();

        // Não chamamos Start() — evita FindFirstObjectByType e Firebase
        // Os métodos testados aqui usam apenas estado local (isBonusActive,
        // combinedMultiplier) e o fallback sem UserHeaderManager.
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_managerGO);
    }

    // -------------------------------------------------------
    // Helper: seta campo privado via reflection
    // -------------------------------------------------------

    private void SetPrivateField(string fieldName, object value)
    {
        var field = typeof(QuestionBonusManager)
            .GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

        field?.SetValue(_bonusManager, value);
    }

    // =======================================================
    // IsBonusActive
    // =======================================================

    [Test]
    public void IsBonusActive_EstadoInicial_RetornaFalse()
    {
        // Sem Start(), sem UserHeaderManager — estado padrão = false
        Assert.IsFalse(_bonusManager.IsBonusActive());
    }

    [Test]
    public void IsBonusActive_CampoPrivadoTrue_RetornaTrue()
    {
        SetPrivateField("isBonusActive", true);
        Assert.IsTrue(_bonusManager.IsBonusActive());
    }

    [Test]
    public void IsBonusActive_CampoPrivadoFalse_RetornaFalse()
    {
        SetPrivateField("isBonusActive", false);
        Assert.IsFalse(_bonusManager.IsBonusActive());
    }

    // =======================================================
    // GetCurrentScoreMultiplier
    // =======================================================

    [Test]
    public void GetCurrentScoreMultiplier_SemBonus_RetornaUm()
    {
        // Sem bonus ativo, multiplicador padrão = 1
        Assert.AreEqual(1, _bonusManager.GetCurrentScoreMultiplier());
    }

    [Test]
    public void GetCurrentScoreMultiplier_ComBonusAtivo_RetornaCombinedMultiplier()
    {
        SetPrivateField("isBonusActive",      true);
        SetPrivateField("combinedMultiplier", 2);

        Assert.AreEqual(2, _bonusManager.GetCurrentScoreMultiplier());
    }

    [Test]
    public void GetCurrentScoreMultiplier_BonusInativo_RetornaUm()
    {
        SetPrivateField("isBonusActive",      false);
        SetPrivateField("combinedMultiplier", 3); // multiplicador configurado mas inativo

        Assert.AreEqual(1, _bonusManager.GetCurrentScoreMultiplier(),
            "Mesmo com combinedMultiplier > 1, se o bonus não está ativo deve retornar 1");
    }

    // =======================================================
    // ApplyBonusToScore
    // =======================================================

    [Test]
    public void ApplyBonusToScore_SemBonus_RetornaScoreOriginal()
    {
        int result = _bonusManager.ApplyBonusToScore(10);
        Assert.AreEqual(10, result);
    }

    [Test]
    public void ApplyBonusToScore_ComBonusMultiplicadorDois_DobradoScore()
    {
        SetPrivateField("isBonusActive",      true);
        SetPrivateField("combinedMultiplier", 2);

        int result = _bonusManager.ApplyBonusToScore(10);

        Assert.AreEqual(20, result,
            "Com multiplicador 2x e bonus ativo, score deve ser dobrado");
    }

    [Test]
    public void ApplyBonusToScore_ComBonusMultiplicadorTres_TriplicadoScore()
    {
        SetPrivateField("isBonusActive",      true);
        SetPrivateField("combinedMultiplier", 3);

        int result = _bonusManager.ApplyBonusToScore(5);

        Assert.AreEqual(15, result);
    }

    [Test]
    public void ApplyBonusToScore_ScoreZero_RetornaZero()
    {
        SetPrivateField("isBonusActive",      true);
        SetPrivateField("combinedMultiplier", 5);

        int result = _bonusManager.ApplyBonusToScore(0);

        Assert.AreEqual(0, result,
            "Score 0 com qualquer multiplicador deve continuar 0");
    }

    [Test]
    public void ApplyBonusToScore_ScoreNegativo_NaoAplicaBonus()
    {
        SetPrivateField("isBonusActive",      true);
        SetPrivateField("combinedMultiplier", 2);

        // baseScore negativo (penalidade) — bônus não deve ser aplicado
        int result = _bonusManager.ApplyBonusToScore(-5);

        Assert.AreEqual(-5, result,
            "Penalidade (score negativo) não deve ser afetada pelo bônus");
    }

    // =======================================================
    // Consistência entre IsBonusActive e GetCurrentScoreMultiplier
    // =======================================================

    [Test]
    public void QuandoBonusAtivo_MultiplicadorDeveSerMaiorQueUm()
    {
        SetPrivateField("isBonusActive",      true);
        SetPrivateField("combinedMultiplier", 2);

        Assert.IsTrue(_bonusManager.IsBonusActive());
        Assert.Greater(_bonusManager.GetCurrentScoreMultiplier(), 1);
    }

    [Test]
    public void QuandoBonusInativo_MultiplicadorDeveSerUm()
    {
        SetPrivateField("isBonusActive", false);

        Assert.IsFalse(_bonusManager.IsBonusActive());
        Assert.AreEqual(1, _bonusManager.GetCurrentScoreMultiplier());
    }
}