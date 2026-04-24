using System;
using System.Collections.Generic;

/// <summary>
/// Fake do INavigationService para testes unitários.
/// Captura chamadas a NavigateTo em vez de chamar SceneManager.LoadScene.
///
/// Como usar:
///   var fakeNav = new FakeNavigationService();
///   AppContext.OverrideForTests(navigation: fakeNav);
///   // após ação:
///   Assert.AreEqual("PathwayScene", fakeNav.LastScene);
/// </summary>
public class FakeNavigationService : INavigationService
{
    public string       LastScene         { get; private set; }
    public int          NavigateCallCount { get; private set; }
    public List<string> NavigationHistory { get; } = new List<string>();

    public event Action<string> OnSceneChanged;
    public event Action<string> OnNavigationComplete;

    public void NavigateTo(string sceneName, Dictionary<string, object> sceneData = null)
    {
        LastScene = sceneName;
        NavigateCallCount++;
        NavigationHistory.Add(sceneName);

        // Dispara eventos como o NavigationManager real faria
        OnSceneChanged?.Invoke(sceneName);
        OnNavigationComplete?.Invoke(sceneName);

        UnityEngine.Debug.Log($"[FakeNavigationService] NavigateTo: {sceneName}");
    }

    public void OnNavigationButtonClicked(string buttonName)
        => NavigateTo(buttonName);

    public void AddButtonSceneMapping(string buttonName, string sceneName) { }

    public void Reset()
    {
        LastScene         = null;
        NavigateCallCount = 0;
        NavigationHistory.Clear();
    }
}