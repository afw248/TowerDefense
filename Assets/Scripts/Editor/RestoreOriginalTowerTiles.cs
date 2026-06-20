#if UNITY_EDITOR
using System.Collections.Generic;
using Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// KUBIKOS Cube_GroundWGrass 타워 타일 4x7 그리드를 원래대로 복구합니다.
/// </summary>
public static class RestoreOriginalTowerTiles
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string KubikosTilePrefabPath =
        "Assets/Graphics/KUBIKOS - World/Prefabs/Cubes/Cube_GroundWGrass.prefab";

    private const int Rows = 4;
    private const int Cols = 7;
    private const float Spacing = 3f;
    private const float TileLocalY = -6f;
    private const int TileLayer = 8;

    [MenuItem("TowerDefense/Restore Original Tower Tiles")]
    public static void RestoreFromMenu()
    {
        Restore();
        Debug.Log("KUBIKOS 타워 타일 28개 복구 완료.");
    }

    public static void Restore()
    {
        GameObject kubikosPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(KubikosTilePrefabPath);
        if (kubikosPrefab == null)
        {
            Debug.LogError($"타일 프리팹을 찾을 수 없습니다: {KubikosTilePrefabPath}");
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Transform towerGround = GameObject.Find("TowerGround")?.transform;
        if (towerGround == null)
        {
            Debug.LogError("TowerGround 오브젝트를 찾을 수 없습니다.");
            return;
        }

        towerGround.localPosition = new Vector3(6f, 1.5f, -3f);
        towerGround.localRotation = Quaternion.identity;
        towerGround.localScale = Vector3.one;

        Dictionary<string, AbstractPlayer> occupants = new();
        List<Transform> children = new();
        foreach (Transform child in towerGround)
            children.Add(child);

        foreach (Transform child in children)
        {
            Tile tile = child.GetComponent<Tile>();
            if (tile != null && tile.CurrentOccupant != null)
                occupants[child.name] = tile.CurrentOccupant;

            Object.DestroyImmediate(child.gameObject);
        }

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                string tileName = $"{row},{col}";
                Vector3 localPosition = new Vector3(
                    col * Spacing - 6f,
                    TileLocalY,
                    row * -Spacing + 3f);

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(kubikosPrefab, towerGround);
                instance.name = tileName;
                instance.layer = TileLayer;
                instance.transform.localPosition = localPosition;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one * 3f;

                if (instance.GetComponent<BoxCollider>() == null)
                {
                    BoxCollider collider = instance.AddComponent<BoxCollider>();
                    collider.size = new Vector3(1f, 1.0000005f, 1f);
                    collider.center = new Vector3(0f, 0.50000024f, 0f);
                }

                Tile newTile = instance.GetComponent<Tile>();
                if (newTile == null)
                    newTile = instance.AddComponent<Tile>();

                if (occupants.TryGetValue(tileName, out AbstractPlayer occupant))
                    newTile.Occupy(occupant);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
#endif
