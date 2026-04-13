using UnityEngine;

public class ConnectivityMonitor : MonoBehaviour
{
    public bool IsOnline => Application.internetReachability != NetworkReachability.NotReachable;

    private NetworkReachability _lastReachability;

    public delegate void ConnectivityChangedHandler(bool isOnline);
    public event ConnectivityChangedHandler OnConnectivityChanged;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _lastReachability = Application.internetReachability;
        Debug.Log($"[ConnectivityMonitor] Iniciado. Online={IsOnline}");
    }

    private void Update()
    {
        var current = Application.internetReachability;
        if (current == _lastReachability) return;

        _lastReachability = current;
        bool isOnline = current != NetworkReachability.NotReachable;

        Debug.Log($"[ConnectivityMonitor] Conectividade mudou. Online={isOnline}");
        OnConnectivityChanged?.Invoke(isOnline);
    }
}