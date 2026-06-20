#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 배경(숲 원경)만 재배치합니다. 타일·Ground·Enemy 경로는 건드리지 않습니다.
/// </summary>
public static class MapLayoutSetup
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private static readonly Vector3 ArenaCenter = new(6f, 0f, -3f);

    [MenuItem("TowerDefense/Rebuild Map Layout (Background Only)")]
    public static void ApplyFromMenu()
    {
        ApplyAll();
        Debug.Log("배경 레이아웃 재배치 완료 (타일/경로 미변경).");
    }

    public static void ApplyAll()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        ForestBackdropSetup.Apply(ArenaCenter);
        GameplayVisualLayerSetup.ApplyLayerSplit();
        SpawnerVisualSetup.Apply();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }
}
#endif
