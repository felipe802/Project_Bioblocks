using System.Collections.Generic;

/// <summary>
/// FakeAnsweredQuestionsManager para AuthFlowTests — rastreia chamadas a ForceUpdate.
/// </summary>
public class FakeAnsweredQuestionsManagerForAuth : IAnsweredQuestionsManager
{
    public bool ForceUpdateWasCalled { get; private set; }
    public bool IsManagerInitialized => true;

    public System.Threading.Tasks.Task ForceUpdate()
    {
        ForceUpdateWasCalled = true;
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task<List<string>> FetchUserAnsweredQuestionsInTargetDatabase(string t)
        => System.Threading.Tasks.Task.FromResult(new List<string>());

    public System.Threading.Tasks.Task MarkQuestionAsAnswered(string db, int number)
        => System.Threading.Tasks.Task.CompletedTask;

    public System.Threading.Tasks.Task<bool> HasRemainingQuestions(string db, List<string> list)
        => System.Threading.Tasks.Task.FromResult(true);

    public void ResetManager() { }
}

