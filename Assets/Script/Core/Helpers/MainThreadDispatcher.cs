using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher _instance;
    private readonly Queue<Action> _queue = new Queue<Action>();
    private static readonly object _lock = new object();

    public static MainThreadDispatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("MainThreadDispatcher");
                _instance = go.AddComponent<MainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public static void Enqueue(Action action)
    {
        if (action == null) return;

        if (_instance == null)
        {
            Debug.LogWarning("[MainThreadDispatcher] Instance null — ação descartada.");
            return;
        }

        lock (_lock)
            _instance._queue.Enqueue(action);
    }

    private void Update()
    {
        lock (_lock)
        {
            while (_queue.Count > 0)
                _queue.Dequeue()?.Invoke();
        }
    }
}
