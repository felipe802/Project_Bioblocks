// Assets/Editor/Tests/QuestionSetManagerTests.cs
//
// Para rodar: Window → General → Test Runner → EditMode → Run All
//
// QuestionSetManager é uma classe estática com estado global (campo privado estático).
// O [TearDown] restaura o valor padrão após cada teste para garantir isolamento.
// Não remova essa chamada — sem ela, a ordem de execução dos testes pode causar falhas.

using NUnit.Framework;
using QuestionSystem;

[TestFixture]
public class QuestionSetManagerTests
{
    // -------------------------------------------------------
    // Isolamento — restaura o estado padrão antes e após cada teste
    // -------------------------------------------------------
    [SetUp]
    public void SetUp()
    {
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.biochem);
    }

    [TearDown]
    public void TearDown()
    {
        // Restaura o valor padrão definido na classe (QuestionSet.biochem)
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.biochem);
    }

    // =======================================================
    // Estado inicial
    // =======================================================
    [Test]
    public void GetCurrentQuestionSet_EstadoInicial_RetornaBiochem()
    {
        // O campo privado estático é inicializado como QuestionSet.biochem
        // Este teste verifica que o padrão não foi alterado inadvertidamente
        Assert.AreEqual(QuestionSet.biochem, QuestionSetManager.GetCurrentQuestionSet());
    }

    // =======================================================
    // Round-trip: Set → Get para cada valor do enum
    // =======================================================
    [Test]
    public void SetAndGet_AcidsBase_RoundTrip()
    {
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.acidsBase);
        Assert.AreEqual(QuestionSet.acidsBase, QuestionSetManager.GetCurrentQuestionSet());
    }

    [Test]
    public void SetAndGet_Aminoacids_RoundTrip()
    {
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.aminoacids);
        Assert.AreEqual(QuestionSet.aminoacids, QuestionSetManager.GetCurrentQuestionSet());
    }

    [Test]
    public void SetAndGet_Biochem_RoundTrip()
    {
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.biochem);
        Assert.AreEqual(QuestionSet.biochem, QuestionSetManager.GetCurrentQuestionSet());
    }

    [Test]
    public void SetAndGet_Carbohydrates_RoundTrip()
    {
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.carbohydrates);
        Assert.AreEqual(QuestionSet.carbohydrates, QuestionSetManager.GetCurrentQuestionSet());
    }

    [Test]
    public void SetAndGet_Enzymes_RoundTrip()
    {
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.enzymes);
        Assert.AreEqual(QuestionSet.enzymes, QuestionSetManager.GetCurrentQuestionSet());
    }

    [Test]
    public void SetAndGet_Lipids_RoundTrip()
    {
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.lipids);
        Assert.AreEqual(QuestionSet.lipids, QuestionSetManager.GetCurrentQuestionSet());
    }

    [Test]
    public void SetAndGet_Membranes_RoundTrip()
    {
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.membranes);
        Assert.AreEqual(QuestionSet.membranes, QuestionSetManager.GetCurrentQuestionSet());
    }

    [Test]
    public void SetAndGet_NucleicAcids_RoundTrip()
    {
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.nucleicAcids);
        Assert.AreEqual(QuestionSet.nucleicAcids, QuestionSetManager.GetCurrentQuestionSet());
    }

    [Test]
    public void SetAndGet_Proteins_RoundTrip()
    {
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.proteins);
        Assert.AreEqual(QuestionSet.proteins, QuestionSetManager.GetCurrentQuestionSet());
    }

    [Test]
    public void SetAndGet_Water_RoundTrip()
    {
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.water);
        Assert.AreEqual(QuestionSet.water, QuestionSetManager.GetCurrentQuestionSet());
    }

    // =======================================================
    // Troca de valor
    // =======================================================
    [Test]
    public void Set_TrocaDeValor_GetRefleteUltimoSet()
    {
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.lipids);
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.water);

        Assert.AreEqual(QuestionSet.water, QuestionSetManager.GetCurrentQuestionSet(),
            "O segundo Set deve sobrescrever o primeiro");
    }

    [Test]
    public void Set_MesmoValorDuasVezes_NaoAlteraResultado()
    {
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.enzymes);
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.enzymes);

        Assert.AreEqual(QuestionSet.enzymes, QuestionSetManager.GetCurrentQuestionSet());
    }

    [Test]
    public void Set_VoltaParaPadrao_RetornaBiochem()
    {
        // Simula navegação de cena: vai para outro banco e volta ao padrão
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.proteins);
        QuestionSetManager.SetCurrentQuestionSet(QuestionSet.biochem);

        Assert.AreEqual(QuestionSet.biochem, QuestionSetManager.GetCurrentQuestionSet());
    }
}