#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// 밝고 귀여운 한낮 TD 분위기: 스카이박스, 원경, PP, 조명, 스포너, 적 비주얼 통일.
/// </summary>
public static class GameVisualThemeSetup
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string VolumeProfilePath = "Assets/Settings/GameplayVisualProfile.asset";
    private const string SkyboxPath = "Assets/3D/Next_Log/Fantasy_Map/Map/Texture/Materials/Sky_Box_01.mat";
    private const string SpawnerFxPrefab =
        "Assets/3D/Next_Log/Fantasy_Map/Map/Effect/Prefab/FX_Smoke_01.prefab";

    [MenuItem("TowerDefense/Apply Bright Cute Visual Theme")]
    public static void ApplyFromMenu()
    {
        ApplyAll();
        Debug.Log("밝은 한낮 TD 비주얼 테마 적용 완료.");
    }

    public static void ApplyAll()
    {
        VolumeProfile profile = CreateOrUpdateVolumeProfile();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        RemoveDuplicateVolumes();
        ApplySkyboxAndAmbient();
        ApplyLighting();
        ApplyPostProcessingVolume(profile);
        ApplyCameraSettings();
        SetupBackdrop(new Vector3(6f, 0f, -3f));
        GameplayVisualLayerSetup.ApplyLayerSplit();
        CleanupGroundScatter();
        FixSpawnerVisual();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    private static VolumeProfile CreateOrUpdateVolumeProfile()
    {
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, VolumeProfilePath);
        }

        SetVolumeOverride(profile, out Tonemapping tonemapping);
        tonemapping.active = true;
        tonemapping.mode.Override(TonemappingMode.ACES);

        SetVolumeOverride(profile, out ColorAdjustments color);
        color.active = true;
        color.postExposure.Override(0.12f);
        color.contrast.Override(8f);
        color.saturation.Override(12f);
        color.colorFilter.Override(new Color(1f, 0.99f, 0.96f, 1f));

        SetVolumeOverride(profile, out Bloom bloom);
        bloom.active = true;
        bloom.threshold.Override(1.05f);
        bloom.intensity.Override(0.1f);
        bloom.scatter.Override(0.45f);

        SetVolumeOverride(profile, out Vignette vignette);
        vignette.active = true;
        vignette.intensity.Override(0.08f);
        vignette.smoothness.Override(0.35f);

        SetVolumeOverride(profile, out LiftGammaGain liftGammaGain);
        liftGammaGain.active = true;
        liftGammaGain.lift.Override(new Vector4(1f, 1.02f, 1.05f, 0f));
        liftGammaGain.gamma.Override(new Vector4(1f, 1f, 1f, 0f));
        liftGammaGain.gain.Override(new Vector4(1.02f, 1.02f, 1f, 0f));

        EditorUtility.SetDirty(profile);
        return profile;
    }

    private static void SetVolumeOverride<T>(VolumeProfile profile, out T component) where T : VolumeComponent
    {
        if (!profile.TryGet(out component))
            component = profile.Add<T>(true);
    }

    private static void ApplySkyboxAndAmbient()
    {
        Material skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxPath);
        if (skybox != null)
        {
            RenderSettings.skybox = skybox;
            if (skybox.HasProperty("_Exposure"))
                skybox.SetFloat("_Exposure", 1.05f);
            // 현실적 로우폴리 하늘: 더 진하고 자연스러운 파란색
            if (skybox.HasProperty("_Tint"))
                skybox.SetColor("_Tint", new Color(0.28f, 0.50f, 0.80f, 1f));
        }

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.78f, 0.88f, 1f);      // 하늘
        RenderSettings.ambientEquatorColor = new Color(0.75f, 0.88f, 0.66f); // 지평선
        RenderSettings.ambientGroundColor = new Color(0.55f, 0.48f, 0.40f);  // 땅
        RenderSettings.ambientIntensity = 1.45f;

        // 거리감을 주는 대기 안개 (로우폴리 하늘섬 느낌 유지)
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.70f, 0.84f, 0.98f);
        RenderSettings.fogStartDistance = 36f;
        RenderSettings.fogEndDistance = 95f;
    }

    private static void ApplyLighting()
    {
        Light sun = Object.FindFirstObjectByType<Light>();
        if (sun == null || sun.type != LightType.Directional)
            return;

        sun.transform.rotation = Quaternion.Euler(52f, -28f, 0f);
        sun.color = new Color(1f, 0.98f, 0.9f);
        sun.intensity = 1.75f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.45f;

        RenderSettings.sun = sun;
    }

    private static void ApplyPostProcessingVolume(VolumeProfile profile)
    {
        GameObject volumeObject = GameObject.Find("Global Volume");
        if (volumeObject == null)
            volumeObject = new GameObject("Global Volume");

        Volume volume = volumeObject.GetComponent<Volume>();
        if (volume == null)
            volume = volumeObject.AddComponent<Volume>();

        volume.isGlobal = true;
        volume.sharedProfile = profile;
        volume.weight = 1f;
        volume.priority = 0f;
    }

    private static void RemoveDuplicateVolumes()
    {
        Volume[] volumes = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
        foreach (Volume volume in volumes)
        {
            if (volume == null)
                continue;

            if (volume.gameObject.name == "Global Volume")
                continue;

            Object.DestroyImmediate(volume.gameObject);
        }
    }

    private static void ApplyCameraSettings()
    {
        Camera camera = Camera.main;
        if (camera == null)
            return;

        UniversalAdditionalCameraData cameraData = camera.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null)
            cameraData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();

        cameraData.renderPostProcessing = false;
        cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
        cameraData.antialiasingQuality = AntialiasingQuality.Low;
        camera.orthographic = true;
        camera.orthographicSize = GameplayViewSettings.OrthographicSize;
        // 현실적 로우폴리 하늘 배경색
        camera.backgroundColor = new Color(0.36f, 0.58f, 0.88f);
    }

    private static void SetupBackdrop(Vector3 arenaCenter)
    {
        ForestBackdropSetup.Apply(arenaCenter);
    }

    private static void PlaceRingPrefabs(
        Transform parent,
        IReadOnlyList<string> prefabPaths,
        int count,
        float minRadius,
        float maxRadius,
        float y,
        float minScale,
        float maxScale)
    {
        if (prefabPaths == null || prefabPaths.Count == 0)
            return;

        for (int i = 0; i < count; i++)
        {
            string path = prefabPaths[i % prefabPaths.Count];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                continue;

            float angle = (360f / count) * i + Random.Range(-8f, 8f);
            float radius = Random.Range(minRadius, maxRadius);
            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                y + Random.Range(-0.5f, 0.5f),
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.localPosition = offset;
            instance.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            float scale = Random.Range(minScale, maxScale);
            instance.transform.localScale = Vector3.one * scale;
        }
    }

    private static void CleanupGroundScatter()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
            return;

        foreach (Transform child in ground.transform)
        {
            if (child.name.StartsWith("Cube_Rock_2"))
                child.gameObject.SetActive(false);
        }
    }

    private static void FixSpawnerVisual()
    {
        GameObject spawner = GameObject.Find("Spawner");
        if (spawner == null)
            return;

        MeshRenderer renderer = spawner.GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.enabled = false;

        MeshFilter filter = spawner.GetComponent<MeshFilter>();
        if (filter != null)
            Object.DestroyImmediate(filter);

        Transform existingEgg = spawner.transform.Find("EggNest");
        if (existingEgg != null)
            Object.DestroyImmediate(existingEgg.gameObject);

        Transform existingPortal = spawner.transform.Find("PortalVisual");
        if (existingPortal != null)
        {
            SpawnerVisualSetup.Apply();
            return;
        }

        spawner.transform.localScale = Vector3.one;

        GameObject fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpawnerFxPrefab);
        if (fxPrefab == null)
            return;

        GameObject portalFx = (GameObject)PrefabUtility.InstantiatePrefab(fxPrefab, spawner.transform);
        portalFx.name = "PortalVisual";
        portalFx.transform.localPosition = new Vector3(0f, 0.35f, 0.15f);
        portalFx.transform.localRotation = Quaternion.identity;
        portalFx.transform.localScale = Vector3.one * 0.85f;
        SpawnerVisualSetup.Apply();
    }

}
#endif
