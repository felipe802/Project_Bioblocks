using UnityEngine;

public class LogcatTest : MonoBehaviour
{
    private void Start()
    {
        Debug.LogWarning("##### LOGCAT TEST - QuestionScene carregou #####");
    }

    private void Update()
    {
        if (Input.touchCount > 0)
            Debug.LogWarning("##### TOQUE DETECTADO #####");
    }
}