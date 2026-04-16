using UnityEngine;
using TMPro;
using System.Collections;

public class FeedbackManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private float feedbackDuration = 2f;

    private Coroutine hideFeedbackCoroutine;

    public virtual void ShowFeedback(string message, bool isError = false)
    {
        if (feedbackText == null)
        {
            Debug.LogError("Feedback Text UI element not found!");
            return;
        }

        if (hideFeedbackCoroutine != null)
        {
            StopCoroutine(hideFeedbackCoroutine);
        }

        feedbackText.text = message;
        feedbackText.color = isError ? Color.red : Color.green;
        feedbackText.gameObject.SetActive(true);

        hideFeedbackCoroutine = StartCoroutine(HideFeedbackAfterDelay());
    }

    public virtual void HideFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
    }

    private IEnumerator HideFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDuration);
        HideFeedback();
    }
}