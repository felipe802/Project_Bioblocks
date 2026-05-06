using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Transition Overlay")]
    [SerializeField] private CanvasGroup overlayCanvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.4f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isTransitioning;

    public event Action OnTransitionMidpoint;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        overlayCanvasGroup.alpha = 0f;
        overlayCanvasGroup.blocksRaycasts = false;
    }

    public async Task TransitionToScene(string sceneName)
    {
        if (isTransitioning) return;

        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(TransitionCoroutine(sceneName, tcs));
        await tcs.Task;
    }

    private IEnumerator TransitionCoroutine(string sceneName, TaskCompletionSource<bool> tcs)
    {
        isTransitioning = true;
        overlayCanvasGroup.blocksRaycasts = true;

        // Fade IN (tela escurece)
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            overlayCanvasGroup.alpha = transitionCurve.Evaluate(elapsed / transitionDuration);
            yield return null;
        }
        overlayCanvasGroup.alpha = 1f;

        OnTransitionMidpoint?.Invoke();
        SceneManager.LoadScene(sceneName);

        yield return null; // espera um frame para a cena carregar

        // Fade OUT (tela clareia)
        elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            overlayCanvasGroup.alpha = 1f - transitionCurve.Evaluate(elapsed / transitionDuration);
            yield return null;
        }

        overlayCanvasGroup.alpha = 0f;
        overlayCanvasGroup.blocksRaycasts = false;
        isTransitioning = false;
        tcs.SetResult(true);
    }
}