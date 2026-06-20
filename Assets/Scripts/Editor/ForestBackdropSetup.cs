#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Fantasy_Map 샘플(Fantasy_01) 숲 Area를 플레이 필드 아래 원경으로 배치합니다.
/// </summary>
public static class ForestBackdropSetup
{
    private const string TargetScenePath = "Assets/Scenes/SampleScene.unity";
    private const string FantasyScenePath = "Assets/3D/Next_Log/Fantasy_Map/Fantasy_01.unity";
    private const string BackgroundRootName = "Visual_Background";

    private static readonly string[] ForestAreaNames =
    {
        "Area_01",
        "Area_02",
        "Area_03",
        "Area_04",
        "Area_05",
    };

    private static readonly Vector3 DefaultArenaCenter = new(6f, 0f, -3f);
    private const float BackgroundFloorY = -10f;
    private const float BackdropScale = 3.5f;
    private const float BackdropCenterOffsetZ = -32f;

    [MenuItem("TowerDefense/Fix Backdrop And Spawner Scale")]
    public static void FixBackdropAndSpawnerFromMenu()
    {
        Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
        Apply(DefaultArenaCenter);
        GameplayVisualLayerSetup.ApplyLayerSplit();
        SpawnerVisualSetup.Apply();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("배경 확대 + 스포너 축소 적용 완료.");
    }

    [MenuItem("TowerDefense/Apply Fantasy Forest Backdrop")]
    public static void ApplyFromMenu()
    {
        Apply(DefaultArenaCenter);
        GameplayVisualLayerSetup.ApplyLayerSplit();
        Debug.Log("Fantasy_01 숲 원경(Area_03 + Area_04) 배경 적용 완료.");
    }

    public static void Apply(Vector3 arenaCenter)
    {
        Scene targetScene = EditorSceneManager.GetActiveScene();
        if (targetScene.path != TargetScenePath)
            targetScene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);

        RemoveExistingBackdrop(targetScene);

        GameObject backgroundRoot = GameObject.Find(BackgroundRootName);
        if (backgroundRoot == null)
        {
            backgroundRoot = new GameObject(BackgroundRootName);
            SceneManager.MoveGameObjectToScene(backgroundRoot, targetScene);
        }

        GameObject backdropRoot = new GameObject("Backdrop");
        backdropRoot.transform.SetParent(backgroundRoot.transform, false);

        Scene fantasyScene = EditorSceneManager.OpenScene(FantasyScenePath, OpenSceneMode.Additive);

        int copied = 0;
        foreach (string areaName in ForestAreaNames)
        {
            GameObject source = FindRootInScene(fantasyScene, areaName);
            if (source == null)
            {
                Debug.LogWarning($"Fantasy 샘플에서 {areaName} 을 찾지 못했습니다.");
                continue;
            }

            GameObject clone = Object.Instantiate(source);
            clone.name = areaName;
            SceneManager.MoveGameObjectToScene(clone, targetScene);
            clone.transform.SetParent(backdropRoot.transform, true);
            copied++;
        }

        EditorSceneManager.CloseScene(fantasyScene, true);

        if (copied == 0)
        {
            Object.DestroyImmediate(backdropRoot);
            Debug.LogError("숲 Area 복사에 실패했습니다.");
            return;
        }

        backdropRoot.transform.localScale = Vector3.one * BackdropScale;
        PositionForestBackdrop(backdropRoot.transform, arenaCenter);
        SanitizeBackdrop(backdropRoot);

        EditorSceneManager.MarkSceneDirty(targetScene);
    }

    public static void RepositionExisting(Vector3 arenaCenter)
    {
        Transform backdrop = GameObject.Find("Backdrop")?.transform;
        if (backdrop == null)
            return;

        backdrop.localScale = Vector3.one * BackdropScale;
        PositionForestBackdrop(backdrop, arenaCenter);
        SanitizeBackdrop(backdrop.gameObject);
    }

    private static void RemoveExistingBackdrop(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "Backdrop")
                Object.DestroyImmediate(root);
        }
    }

    private static GameObject FindRootInScene(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == name)
                return root;
        }

        return null;
    }

    private static void PositionForestBackdrop(Transform backdropRoot, Vector3 arenaCenter)
    {
        if (!TryGetRendererBounds(backdropRoot, out Bounds forestBounds))
            return;

        Vector3 targetCenter = arenaCenter + new Vector3(0f, BackgroundFloorY, BackdropCenterOffsetZ);
        Vector3 delta = targetCenter - forestBounds.center;
        delta.y = BackgroundFloorY - forestBounds.min.y;
        backdropRoot.position += delta;
    }

    private static void SanitizeBackdrop(GameObject backdropRoot)
    {
        foreach (Collider collider in backdropRoot.GetComponentsInChildren<Collider>(true))
            collider.enabled = false;

        foreach (Camera camera in backdropRoot.GetComponentsInChildren<Camera>(true))
            camera.enabled = false;

        foreach (Light light in backdropRoot.GetComponentsInChildren<Light>(true))
            light.enabled = false;

        foreach (AudioListener listener in backdropRoot.GetComponentsInChildren<AudioListener>(true))
            Object.DestroyImmediate(listener);

        DisableBackdropParticles(backdropRoot);
    }

    private static void DisableBackdropParticles(GameObject backdropRoot)
    {
        foreach (ParticleSystem particle in backdropRoot.GetComponentsInChildren<ParticleSystem>(true))
            particle.gameObject.SetActive(false);
    }

    [MenuItem("TowerDefense/Disable Backdrop Smoke Particles")]
    public static void DisableBackdropSmokeFromMenu()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.path != TargetScenePath)
            scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);

        GameObject backgroundRoot = GameObject.Find(BackgroundRootName);
        if (backgroundRoot == null)
        {
            Debug.LogWarning("Visual_Background 를 찾지 못했습니다.");
            return;
        }

        DisableBackdropParticles(backgroundRoot);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("원경 연기/먼지 파티클을 비활성화했습니다.");
    }

    private static bool TryGetRendererBounds(Transform root, out Bounds bounds)
    {
        bounds = default;
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return false;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds.size.sqrMagnitude > 0.0001f;
    }
}
#endif
