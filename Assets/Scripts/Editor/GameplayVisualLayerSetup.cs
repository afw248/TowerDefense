#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 플레이 타일/적 경로는 위, 숲·원경 배경은 아래 레이어로 분리합니다. 타일 좌표는 건드리지 않습니다.
/// </summary>
public static class GameplayVisualLayerSetup
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string BackgroundRootName = "Visual_Background";

    private static readonly Vector3 GroundPosition = new(6f, 3f, 6f);
    private static readonly Vector3 EnemyPosition = new(0f, -1.5f, 0f);
    private static readonly Vector3 TowerGroundPosition = new(6f, 1.5f, -3f);

    private const int BackgroundRenderQueue = 1800;
    private const int GameplayRenderQueue = 2100;

    [MenuItem("TowerDefense/Restore Tiles And Split Visual Layers")]
    public static void RestoreAndSplitFromMenu()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        RestoreOriginalTowerTiles.Restore();
        ApplyLayerSplit();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("타일 복구 + 플레이(위)/배경(아래) 레이어 분리 완료.");
    }

    [MenuItem("TowerDefense/Split Visual Layers (Background Below Gameplay)")]
    public static void SplitFromMenu()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        ApplyLayerSplit();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("플레이(위)/배경(아래) 레이어 분리 완료.");
    }

    private static readonly Vector3 ArenaCenter = new(6f, 0f, -3f);

    public static void ApplyLayerSplit()
    {
        RestoreGameplayRootTransforms();
        RemoveArenaBase();
        OrganizeBackgroundRoot();
        ForestBackdropSetup.RepositionExisting(ArenaCenter);
        ApplyRenderQueues();
    }

    private static void RestoreGameplayRootTransforms()
    {
        Transform ground = GameObject.Find("Ground")?.transform;
        if (ground != null)
            ground.position = GroundPosition;

        Transform enemy = GameObject.Find("Enemy")?.transform;
        if (enemy != null)
            enemy.position = EnemyPosition;

        Transform towerGround = GameObject.Find("TowerGround")?.transform;
        if (towerGround != null)
            towerGround.position = TowerGroundPosition;
    }

    private static void RemoveArenaBase()
    {
        GameObject arenaBase = GameObject.Find("ArenaBase");
        if (arenaBase != null)
            Object.DestroyImmediate(arenaBase);
    }

    private static void OrganizeBackgroundRoot()
    {
        GameObject backdrop = GameObject.Find("Backdrop");
        if (backdrop == null)
            return;

        GameObject backgroundRoot = GameObject.Find(BackgroundRootName);
        if (backgroundRoot == null)
            backgroundRoot = new GameObject(BackgroundRootName);

        backdrop.transform.SetParent(backgroundRoot.transform, true);
    }

    private static void ApplyRenderQueues()
    {
        GameObject backgroundRoot = GameObject.Find(BackgroundRootName);
        if (backgroundRoot != null)
            SetRenderQueueRecursive(backgroundRoot.transform, BackgroundRenderQueue);

        Transform ground = GameObject.Find("Ground")?.transform;
        if (ground != null)
            SetRenderQueueRecursive(ground, GameplayRenderQueue);

        Transform towerGround = GameObject.Find("TowerGround")?.transform;
        if (towerGround != null)
            SetRenderQueueRecursive(towerGround, GameplayRenderQueue);

        Transform enemy = GameObject.Find("Enemy")?.transform;
        if (enemy != null)
            SetRenderQueueRecursive(enemy, GameplayRenderQueue);
    }

    private static void SetRenderQueueRecursive(Transform root, int renderQueue)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null)
                continue;

            Material[] materials = renderer.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < materials.Length; i++)
            {
                Material source = materials[i];
                if (source == null || source.renderQueue == renderQueue)
                    continue;

                Material instance = new Material(source);
                instance.renderQueue = renderQueue;
                materials[i] = instance;
                changed = true;
            }

            if (changed)
                renderer.sharedMaterials = materials;
        }
    }
}
#endif
