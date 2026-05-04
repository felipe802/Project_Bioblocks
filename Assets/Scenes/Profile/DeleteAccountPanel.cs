using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DeleteAccountPanel : MonoBehaviour
{
    /// <summary>
    /// Janela de tempo (segundos) após ShowPanel() durante a qual cliques
    /// no botão de confirmação são ignorados. Evita touch bleed-through:
    /// o mesmo toque que abriu o painel não aciona a deleção acidentalmente.
    /// A guarda fica no handler do botão (não no CanvasGroup.interactable),
    /// porque eventos na fila do EventSystem disparam assim que
    /// interactable volta a true.
    /// </summary>
    [SerializeField] private float confirmInputDelay = 0.5f;

    private CanvasGroup canvasGroup;
    private float _shownAt = -1f;

    /// <summary>
    /// True quando já passou tempo suficiente desde ShowPanel()
    /// para aceitar input do usuário com segurança.
    /// </summary>
    public bool IsReadyForInput
        => _shownAt > 0f && Time.realtimeSinceStartup - _shownAt >= confirmInputDelay;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()   => HidePanel();

    private void OnEnable()
    {
        if (canvasGroup != null)
        {
            Debug.Log($"[DeleteAccountPanel] OnEnable → HidePanel");
            HidePanel();
        }
    }

    public void ShowPanel()
    {
        if (canvasGroup == null)
        {
            Debug.LogError($"CanvasGroup é null em {gameObject.name}");
            return;
        }

        // Reabilita o GR próprio (se existir) para que os botões do painel
        // voltem a receber raycasts — foi desabilitado em HidePanel.
        GraphicRaycaster gr = GetComponent<GraphicRaycaster>();
        if (gr != null) gr.enabled = true;

        _shownAt = Time.realtimeSinceStartup;

        canvasGroup.alpha          = 1;
        canvasGroup.interactable   = true;
        canvasGroup.blocksRaycasts = true;

        Debug.Log($"[DeleteAccountPanel] ShowPanel — alpha={canvasGroup.alpha}, activeInHierarchy={gameObject.activeInHierarchy}");
    }

    public void HidePanel()
    {
        if (canvasGroup == null)
        {
            Debug.LogError($"CanvasGroup é null em {gameObject.name}");
            return;
        }

        // Desabilita o GR próprio para impedir que os botões do painel recebam
        // raycasts enquanto invisível. CanvasGroup.blocksRaycasts = false só
        // instrui o GR do Canvas PAI — o GR próprio do painel ignoraria essa flag.
        GraphicRaycaster gr = GetComponent<GraphicRaycaster>();
        if (gr != null) gr.enabled = false;

        Debug.Log($"[DeleteAccountPanel] HidePanel\n{new System.Diagnostics.StackTrace(true)}");

        _shownAt = -1f;
        canvasGroup.alpha          = 0;
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = false;
    }
}
