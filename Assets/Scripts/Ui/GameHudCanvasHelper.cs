using UnityEngine;
using UnityEngine.UI;

public static class GameHudCanvasHelper
{
    private static readonly string[] HudCanvasNames =
    {
        "GameHudCanvas",
        "PlayerUiCanvas",
        "WaveUiCanvas",
    };

    public static void EnsureCanvasScales()
    {
        foreach (string canvasName in HudCanvasNames)
        {
            GameObject canvas = FindCanvas(canvasName);
            if (canvas == null)
                continue;

            RectTransform rect = canvas.GetComponent<RectTransform>();
            if (rect == null)
                continue;

            float scale = canvasName == "PlayerUiCanvas"
                ? GameplayViewSettings.PlayerUiScale
                : GameplayViewSettings.HudUniformScale;

            rect.localScale = Vector3.one * scale;
        }
    }

    public static CanvasGroup EnsureCanvasGroup(string canvasName)
    {
        GameObject canvas = FindCanvas(canvasName);
        if (canvas == null)
            return null;

        if (!canvas.TryGetComponent(out CanvasGroup group))
            group = canvas.AddComponent<CanvasGroup>();

        return group;
    }

    public static GameObject FindCanvas(string canvasName)
    {
        GameObject canvas = GameObject.Find(canvasName);
        if (canvas != null)
            return canvas;

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Canvas candidate in canvases)
        {
            if (candidate != null && candidate.name == canvasName)
                return candidate.gameObject;
        }

        return null;
    }
}
