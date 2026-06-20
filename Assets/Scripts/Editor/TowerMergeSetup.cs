#if UNITY_EDITOR
using CoreSystem.EffectSystem;
using Tower;
using UnityEditor;
using UnityEngine;

public static class TowerMergeSetup
{
    private const string ConfigPath = "Assets/SO/Tower/TowerMergeConfig.asset";
    private const string EffectsFolder = "Assets/SO/Effects/Merge";
    private const string ResourcesFolder = "Assets/Resources";

    private static readonly (TowerGrade grade, string successPrefab, string failurePrefab, float successScale, float failureScale)[] TierVfx =
    {
        (
            TowerGrade.Normal,
            "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Blue Impact.prefab",
            "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Toon Explosion.prefab",
            1.1f,
            1.4f
        ),
        (
            TowerGrade.Rare,
            "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Magic Spiral.prefab",
            "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Explosion Bomb.prefab",
            1.2f,
            1.5f
        ),
        (
            TowerGrade.Epic,
            "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Magic Explosive Spell.prefab",
            "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Explosion Galaxy.prefab",
            1.35f,
            1.65f
        ),
    };

    [MenuItem("TowerDefense/Setup Tower Merge")]
    public static void SetupFromMenu()
    {
        EnsureFolder("Assets/SO", "Tower");
        EnsureFolder("Assets/SO/Effects", "Merge");
        EnsureFolder("Assets", "Resources");

        TowerMergeConfigSO config = AssetDatabase.LoadAssetAtPath<TowerMergeConfigSO>(ConfigPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<TowerMergeConfigSO>();
            AssetDatabase.CreateAsset(config, ConfigPath);
        }

        TowerMergeConfigSO.MergeTierSettings[] tiers = new TowerMergeConfigSO.MergeTierSettings[TierVfx.Length];
        for (int i = 0; i < TierVfx.Length; i++)
        {
            (TowerGrade grade, string successPrefab, string failurePrefab, float successScale, float failureScale) entry = TierVfx[i];
            tiers[i] = new TowerMergeConfigSO.MergeTierSettings
            {
                fromGrade = entry.grade,
                successChancePercent = entry.grade switch
                {
                    TowerGrade.Rare => 70,
                    TowerGrade.Epic => 25,
                    _ => 90
                },
                successVfx = CreateHitEffect(
                    $"{EffectsFolder}/MergeSuccess_{entry.grade}.asset",
                    entry.successPrefab,
                    entry.successScale,
                    2.5f),
                failureVfx = CreateHitEffect(
                    $"{EffectsFolder}/MergeFailure_{entry.grade}.asset",
                    entry.failurePrefab,
                    entry.failureScale,
                    2.8f),
            };
        }

        config.tiers = tiers;
        EditorUtility.SetDirty(config);

        string resourcePath = $"{ResourcesFolder}/TowerMergeConfig.asset";
        TowerMergeConfigSO resourceCopy = AssetDatabase.LoadAssetAtPath<TowerMergeConfigSO>(resourcePath);
        if (resourceCopy == null)
        {
            if (!AssetDatabase.CopyAsset(ConfigPath, resourcePath))
                Debug.LogWarning("TowerMergeConfig Resources 복사에 실패했습니다.");
        }

        EnsureMergeControllerInScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("타워 합성 설정 완료.");
    }

    private static HitEffectDataSO CreateHitEffect(string assetPath, string prefabPath, float scale, float lifetime)
    {
        HitEffectDataSO data = AssetDatabase.LoadAssetAtPath<HitEffectDataSO>(assetPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<HitEffectDataSO>();
            AssetDatabase.CreateAsset(data, assetPath);
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
            Debug.LogWarning($"합성 이펙트 프리팹을 찾을 수 없습니다: {prefabPath}");

        data.effectPrefab = prefab;
        data.scale = scale;
        data.lifetime = lifetime;
        EditorUtility.SetDirty(data);
        return data;
    }

    private static void EnsureMergeControllerInScene()
    {
        TileManager tileManager = Object.FindFirstObjectByType<TileManager>();
        if (tileManager == null)
        {
            Debug.LogWarning("씬에서 TileManager를 찾을 수 없습니다.");
            return;
        }

        TowerMergeController controller = tileManager.GetComponent<TowerMergeController>();
        if (controller == null)
            controller = tileManager.gameObject.AddComponent<TowerMergeController>();

        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("tileManager").objectReferenceValue = tileManager;
        so.FindProperty("mergeConfig").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<TowerMergeConfigSO>(ConfigPath);
        so.FindProperty("allTowerList").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<AllPlayerListSO>("Assets/SO/Spawn/AllPlayerListSO.asset");
        so.FindProperty("inputSO").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<InputSO>("Assets/SO/InputSO.asset");
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
    }

    private static void EnsureFolder(string parent, string child)
    {
        if (!AssetDatabase.IsValidFolder(parent))
            return;

        string path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }
}
#endif
