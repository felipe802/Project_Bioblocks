/// <summary>
/// Fake compartilhado do IStatisticsProvider para testes unitários.
///
/// Usado por:
///   - PlayerLevelServiceTests: construtor com totalQuestions fixo
///   - AuthFlowTests: IsInitialized configurável + rastreamento de acesso
///
/// Como usar (PlayerLevelServiceTests):
///   var stats = new FakeStatisticsProvider(680);
///
/// Como usar (AuthFlowTests):
///   var stats = new FakeStatisticsProvider();
///   stats.IsInitialized = false;
///   // após ação:
///   Assert.IsTrue(stats.IsInitializedWasChecked);
/// </summary>
public class FakeStatisticsProvider : IStatisticsProvider
{
    private readonly int _totalQuestions;
    private bool _isInitialized;

    /// <summary>Indica se a propriedade IsInitialized foi acessada.</summary>
    public bool IsInitializedWasChecked { get; private set; }

    /// <summary>
    /// Construtor para PlayerLevelServiceTests — total fixo, sempre inicializado.
    /// </summary>
    public FakeStatisticsProvider(int totalQuestions = 680)
    {
        _totalQuestions = totalQuestions;
        _isInitialized  = true;
    }

    public bool IsInitialized
    {
        get
        {
            IsInitializedWasChecked = true;
            return _isInitialized;
        }
        set => _isInitialized = value;
    }

    public int GetTotalQuestionsCount() => _totalQuestions;
}