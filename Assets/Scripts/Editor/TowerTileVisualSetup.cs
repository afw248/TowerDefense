#if UNITY_EDITOR
using System.Collections.Generic;
using Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 타워 배치 타일을 Fantasy_Map 스타일로 교체하고, 잔디 섬 베이스를 추가합니다.
/// </summary>
public static class TowerTileVisualSetup
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string TowerTilePrefabPath = "Assets/Prefab/Tile/TowerTile_Fantasy.prefab";
    private const string VisualSourcePath =
        "Assets/3D/Next_Log/Fantasy_Map/Map/Brick/Prefab/Tile_03.prefab";

    private const float TargetFootprint = 3f;
    private const int TileLayer = 8;

    [MenuItem("TowerDefense/Setup Fantasy Tower Tiles + Grass Base (Experimental)")]
    public static void ApplyFromMenu()
    {
        if (!EditorUtility.DisplayDialog(
                "실험적 기능",
                "Fantasy 타일 교체는 버그 이력이 있습니다.\n원상복구는 'Restore Original Tower Tiles' 메뉴를 사용하세요.\n계속할까요?",
                "계속",
                "취소"))
            return;

        ApplyAll();
        Debug.Log("Fantasy 타워 타일 + 잔디 베이스 적용 완료.");
    }

    public static void ApplyAll()
    {
        GameObject towerTilePrefab = CreateOrUpdateTowerTilePrefab();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        ReplaceTowerGroundTiles(towerTilePrefab);
        FixArenaGroundAndFog();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    private static GameObject CreateOrUpdateTowerTilePrefab()
    {
        EnsureFolder("Assets/Prefab", "Tile");

        GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VisualSourcePath);
        if (visualPrefab == null)
        {
            Debug.LogError($"타일 비주얼을 찾을 수 없습니다: {VisualSourcePath}");
            return null;
        }

        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(TowerTilePrefabPath);
        if (existing != null)
            AssetDatabase.DeleteAsset(TowerTilePrefabPath);

        GameObject root = new GameObject("TowerTile_Fantasy");
        root.layer = TileLayer;

        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(visualPrefab, root.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;

        float scale = CalculateUniformScale(visual, TargetFootprint);
        visual.transform.localScale = Vector3.one * scale;
        SnapVisualToRootBase(root, visual);

        BoxCollider collider = root.AddComponent<BoxCollider>();
        FitBoxColliderToRenderers(root, collider);

        root.AddComponent<Tile>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, TowerTilePrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void ReplaceTowerGroundTiles(GameObject towerTilePrefab)
    {
        if (towerTilePrefab == null)
            return;

        Transform towerGround = GameObject.Find("TowerGround")?.transform;
        if (towerGround == null)
        {
            Debug.LogError("TowerGround 오브젝트를 찾을 수 없습니다.");
            return;
        }

        List<TileSwapData> swaps = new();
        foreach (Transform child in towerGround)
        {
            Tile tile = child.GetComponent<Tile>();
            if (tile == null)
                continue;

            swaps.Add(new TileSwapData
            {
                Name = child.name,
                LocalPosition = child.localPosition,
                LocalRotation = child.localRotation,
                Occupant = tile.CurrentOccupant,
            });
        }

        foreach (TileSwapData data in swaps)
        {
            Transform oldTransform = null;
            foreach (Transform child in towerGround)
            {
                if (child.name == data.Name)
                {
                    oldTransform = child;
                    break;
                }
            }

            if (oldTransform == null)
                continue;

            Vector3 position = oldTransform.localPosition;
            Quaternion rotation = oldTransform.localRotation;
            Object.DestroyImmediate(oldTransform.gameObject);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(towerTilePrefab, towerGround);
            instance.name = data.Name;
            position.y = -4.5f;
            instance.transform.localPosition = position;
            instance.transform.localRotation = rotation;
            instance.transform.localScale = Vector3.one;

            Tile newTile = instance.GetComponent<Tile>();
            if (data.Occupant != null)
                newTile.Occupy(data.Occupant);
        }
    }

    private static void FixArenaGroundAndFog()
    {
        RenderSettings.fog = false;

        Transform arenaBaseRoot = GameObject.Find("ArenaBase")?.transform;
        if (arenaBaseRoot != null)
            Object.DestroyImmediate(arenaBaseRoot.gameObject);

        GameObject arenaBase = new GameObject("ArenaBase");
        Vector3 center = new Vector3(6f, -2.2f, -3f);

        string[] patchPrefabs =
        {
            "Assets/3D/Next_Log/Fantasy_Map/Map/Ground/Prefab/Ground_02.prefab",
            "Assets/3D/Next_Log/Fantasy_Map/Map/Ground/Prefab/Ground_03.prefab",
            "Assets/3D/Next_Log/Fantasy_Map/Map/Ground/Prefab/Ground_05.prefab",
        };

        float patchSpacing = 8f;
        for (int x = -2; x <= 3; x++)
        {
            for (int z = -2; z <= 2; z++)
            {
                string path = patchPrefabs[(Mathf.Abs(x) + Mathf.Abs(z)) % patchPrefabs.Length];
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    continue;

                GameObject patch = (GameObject)PrefabUtility.InstantiatePrefab(prefab, arenaBase.transform);
                patch.transform.position = center + new Vector3(x * patchSpacing, 0f, z * patchSpacing);
                patch.transform.rotation = Quaternion.Euler(0f, (x + z) * 15f, 0f);
                patch.transform.localScale = Vector3.one * 2.8f;
            }
        }

        PlaceEdgeRocks(arenaBase.transform, center);

        GameObject groundRoot = GameObject.Find("Ground");
        if (groundRoot != null)
        {
            foreach (Transform child in groundRoot.transform)
            {
                if (child.name.StartsWith("Cube_Rock_2"))
                    child.gameObject.SetActive(false);
            }
        }
    }

    private static void PlaceEdgeRocks(Transform parent, Vector3 center)
    {
        string[] rocks =
        {
            "Assets/3D/Next_Log/Fantasy_Map/Map/Stone/Prefab/Stone_A_05.prefab",
            "Assets/3D/Next_Log/Fantasy_Map/Map/Stone/Prefab/Stone_B_07.prefab",
            "Assets/3D/Next_Log/Fantasy_Map/Map/Plant/Prefab/Flower_03.prefab",
        };

        for (int i = 0; i < 10; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(rocks[i % rocks.Length]);
            if (prefab == null)
                continue;

            float angle = i * 36f;
            float radius = 22f + (i % 3);
            Vector3 pos = center + new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                -0.5f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius);

            GameObject rock = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            rock.transform.position = pos;
            rock.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            rock.transform.localScale = Vector3.one * Random.Range(1.6f, 2.4f);
        }
    }

    private static float CalculateUniformScale(GameObject visual, float targetFootprint)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return 1f;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        float size = Mathf.Max(bounds.size.x, bounds.size.z);
        if (size <= 0.001f)
            return 1f;

        return targetFootprint / size;
    }

    private static void SnapVisualToRootBase(GameObject root, GameObject visual)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        float bottomY = root.transform.InverseTransformPoint(bounds.min).y;
        visual.transform.localPosition -= new Vector3(0f, bottomY, 0f);
    }

    private static void FitBoxColliderToRenderers(GameObject root, BoxCollider collider)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            collider.size = Vector3.one * TargetFootprint;
            collider.center = new Vector3(0f, 0.2f, 0f);
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        Vector3 localCenter = root.transform.InverseTransformPoint(bounds.center);
        Vector3 localSize = bounds.size;
        collider.center = localCenter;
        collider.size = new Vector3(
            Mathf.Max(localSize.x, TargetFootprint * 0.85f),
            Mathf.Max(localSize.y, 0.4f),
            Mathf.Max(localSize.z, TargetFootprint * 0.85f));
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }

    private struct TileSwapData
    {
        public string Name;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public AbstractPlayer Occupant;
    }
}
#endif
