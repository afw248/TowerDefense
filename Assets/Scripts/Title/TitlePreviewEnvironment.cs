using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public static class TitlePreviewEnvironment
{
    private static readonly string[] HiddenRootNames =
    {
        "GameHudCanvas",
        "WaveUiCanvas",
        "PlayerUiCanvas",
    };

    public static void Configure(Scene scene)
    {
        if (!scene.IsValid())
            return;

        foreach (string rootName in HiddenRootNames)
        {
            GameObject root = FindByName(scene, rootName);
            if (root != null)
                root.SetActive(false);
        }

        DisableAll<WaveManager>(scene);
        DisableAll<GameInputRouter>(scene);
        DisableAll<GameHudController>(scene);
        DisableAll<GameOverController>(scene);
        DisableAll<SpawnManager>(scene);
        DisableAll<TowerMergeController>(scene);

        foreach (EventSystem eventSystem in FindAllInScene<EventSystem>(scene))
            eventSystem.enabled = false;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Camera camera in root.GetComponentsInChildren<Camera>(true))
            {
                if (!camera.CompareTag("MainCamera"))
                    continue;

                camera.enabled = false;

                if (camera.TryGetComponent(out AudioListener listener))
                    listener.enabled = false;
            }
        }
    }

    public static void SetGameSystemsActive(bool active)
    {
        foreach (string rootName in HiddenRootNames)
        {
            GameObject root = GameHudCanvasHelper.FindCanvas(rootName);
            if (root != null)
                root.SetActive(active);
        }

        if (active)
            GameHudCanvasHelper.EnsureCanvasScales();

        SetBehaviourEnabled<WaveManager>(active);
        SetBehaviourEnabled<GameInputRouter>(active);
        SetBehaviourEnabled<GameHudController>(active);
        SetBehaviourEnabled<GameOverController>(active);
        SetBehaviourEnabled<SpawnManager>(active);
        SetBehaviourEnabled<TowerMergeController>(active);
        SetBehaviourEnabled<TileManager>(active);

        if (!active)
            return;

        EnsureWaveUiVisible();
        GameHudLayoutBootstrap.ApplyFinalPresentation();
        GameHudLayoutBootstrap bootstrap = Object.FindFirstObjectByType<GameHudLayoutBootstrap>(FindObjectsInactive.Include);
        bootstrap?.EnsureReady();
    }

    private static void EnsureWaveUiVisible()
    {
        GameObject waveCanvas = GameHudCanvasHelper.FindCanvas("WaveUiCanvas");
        if (waveCanvas != null)
            waveCanvas.SetActive(true);

        foreach (WaveUi waveUi in Object.FindObjectsByType<WaveUi>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            waveUi.gameObject.SetActive(true);
            waveUi.RepairWaveTextLayout();
        }

        foreach (PopUpWave popup in Object.FindObjectsByType<PopUpWave>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            popup.gameObject.SetActive(true);
    }

    private static void SetBehaviourEnabled<T>(bool enabled) where T : Behaviour
    {
        foreach (T behaviour in Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            behaviour.enabled = enabled;
    }

    private static GameObject FindByName(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == objectName)
                return root;

            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform transform in transforms)
            {
                if (transform.name == objectName)
                    return transform.gameObject;
            }
        }

        return null;
    }

    private static void DisableAll<T>(Scene scene) where T : Behaviour
    {
        foreach (T behaviour in FindAllInScene<T>(scene))
            behaviour.enabled = false;
    }

    private static List<T> FindAllInScene<T>(Scene scene) where T : Component
    {
        List<T> results = new();
        foreach (GameObject root in scene.GetRootGameObjects())
            results.AddRange(root.GetComponentsInChildren<T>(true));

        return results;
    }
}
