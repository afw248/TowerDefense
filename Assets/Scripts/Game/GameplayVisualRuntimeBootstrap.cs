using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Ensures the gameplay scene starts with a bright, neutral render setup.
/// </summary>
public static class GameplayVisualRuntimeBootstrap
{
    private static readonly Color AmbientSky = new(0.78f, 0.88f, 1f, 1f);
    private static readonly Color AmbientEquator = new(0.75f, 0.88f, 0.66f, 1f);
    private static readonly Color AmbientGround = new(0.55f, 0.48f, 0.40f, 1f);
    private static readonly Color FogColor = new(0.70f, 0.84f, 0.98f, 1f);
    private static readonly Color CameraBackground = new(0.46f, 0.68f, 0.95f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        ApplyToActiveScene();
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToActiveScene();
    }

    private static void ApplyToActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.name == "TitleScene")
            return;

        ApplyRenderSettings();
        ApplyCameraSettings();
        ApplyKeyLight();
        DisableGlobalVolumes();
    }

    private static void ApplyRenderSettings()
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = AmbientSky;
        RenderSettings.ambientEquatorColor = AmbientEquator;
        RenderSettings.ambientGroundColor = AmbientGround;
        RenderSettings.ambientIntensity = 1.45f;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = FogColor;
        RenderSettings.fogStartDistance = 36f;
        RenderSettings.fogEndDistance = 95f;
    }

    private static void ApplyCameraSettings()
    {
        Camera camera = Camera.main;
        if (camera == null)
            return;

        camera.backgroundColor = CameraBackground;

        UniversalAdditionalCameraData cameraData = camera.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData != null)
            cameraData.renderPostProcessing = false;
    }

    private static void ApplyKeyLight()
    {
        Light sun = RenderSettings.sun;
        if (sun == null || sun.type != LightType.Directional)
            sun = Object.FindFirstObjectByType<Light>();

        if (sun == null || sun.type != LightType.Directional)
            return;

        sun.enabled = true;
        sun.color = new Color(1f, 0.98f, 0.9f, 1f);
        sun.intensity = 1.75f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.45f;
        sun.transform.rotation = Quaternion.Euler(52f, -28f, 0f);
        RenderSettings.sun = sun;
    }

    private static void DisableGlobalVolumes()
    {
        foreach (Volume volume in Object.FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (volume == null)
                continue;

            volume.weight = 0f;
            volume.enabled = false;
        }
    }
}
