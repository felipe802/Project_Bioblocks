using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public static class HalfViewRegistry
{
    private const string HALF_VIEW_PREFAB_PATH = "Prefabs/HalfViewComponent";
    public static event Action OnAnyHalfViewHidden;
    private static Dictionary<string, HalfViewComponent> sceneHalfViews = new Dictionary<string, HalfViewComponent>();

    public static void RegisterHalfView(string sceneName, HalfViewComponent component)
    {
        sceneHalfViews[sceneName] = component;
    }

    public static void UnregisterHalfView(string sceneName)
    {
        if (sceneHalfViews.ContainsKey(sceneName))
        {
            sceneHalfViews.Remove(sceneName);
        }
    }

    public static HalfViewComponent GetHalfViewForScene(string sceneName)
    {
        if (sceneHalfViews.TryGetValue(sceneName, out HalfViewComponent component))
        {
            return component;
        }
        return null;
    }

    public static void ShowHalfViewForCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        ShowHalfViewForScene(currentScene);
    }

    public static void ShowHalfViewForScene(string sceneName)
    {
        HalfViewComponent halfView = GetHalfViewForScene(sceneName);
        if (halfView != null)
        {
            halfView.ShowMenu();
        }
        else
        {
            Debug.LogWarning($"[HalfViewRegistry] Nenhum HalfView registrado para cena '{sceneName}'");
        }
    }

    public static void HideHalfViewForCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        HideHalfViewForScene(currentScene);
    }

    public static void HideHalfViewForScene(string sceneName)
    {
        HalfViewComponent halfView = GetHalfViewForScene(sceneName);
        if (halfView != null)
        {
            halfView.HideMenu();
            OnAnyHalfViewHidden?.Invoke();
        }
    }

    public static void ClearRegistry()
    {
        sceneHalfViews.Clear();
    }

    public static HalfViewComponent EnsureHalfViewInCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        HalfViewComponent existingComponent = GetHalfViewForScene(currentScene);
        if (existingComponent != null)
        {
            return existingComponent;
        }

        GameObject halfViewPrefab = Resources.Load<GameObject>(HALF_VIEW_PREFAB_PATH);
        if (halfViewPrefab == null)
        {
            Debug.LogError($"[HalfViewRegistry] Prefab não encontrado em: {HALF_VIEW_PREFAB_PATH}");
            return null;
        }

        GameObject halfViewInstance = GameObject.Instantiate(halfViewPrefab);
        halfViewInstance.name = "HalfView";

        Canvas mainCanvas = GetMainCanvas();
        if (mainCanvas != null)
        {
            halfViewInstance.transform.SetParent(mainCanvas.transform, false);
        }
        else
        {
            Debug.LogWarning("[HalfViewRegistry] Canvas principal não encontrado. O HalfView foi instanciado como um GameObject raiz.");
        }

        HalfViewComponent halfViewComponent = halfViewInstance.GetComponent<HalfViewComponent>();

        if (GetHalfViewForScene(currentScene) == null)
        {
            Debug.LogError("[HalfViewRegistry] Falha ao registrar o HalfView recém-criado.");
        }

        return halfViewComponent;
    }

    private static Canvas GetMainCanvas()
    {
        Canvas mainCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();

        if (mainCanvas == null)
        {
            mainCanvas = GameObject.Find("MainCanvas")?.GetComponent<Canvas>();
        }

        if (mainCanvas == null)
        {
            Canvas[] allCanvases = GameObject.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (allCanvases.Length > 0)
            {
                int highestOrder = -1;
                foreach (Canvas canvas in allCanvases)
                {
                    if (canvas.sortingOrder > highestOrder && canvas.renderMode != RenderMode.WorldSpace)
                    {
                        highestOrder = canvas.sortingOrder;
                        mainCanvas = canvas;
                    }
                }

                if (mainCanvas == null && allCanvases.Length > 0)
                {
                    mainCanvas = allCanvases[0];
                }
            }
        }

        if (mainCanvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        return mainCanvas;
    }

}
