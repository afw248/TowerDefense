#if UNITY_EDITOR
using CoreSystem.EffectSystem;
using UnityEditor;
using UnityEngine;

public static class HitEffectSetup
{
    private const string EffectsFolder = "Assets/SO/Effects";
    private const string BowHitAssetPath = EffectsFolder + "/BowHitEffect.asset";
    private const string CulverinHitAssetPath = EffectsFolder + "/CulverinHitEffect.asset";

    private const string BowEffectPrefabPath =
        "Assets/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_20_RapidFire/Effect_20_Base/Effect_20_HitEffects.prefab";

    private const string CulverinEffectPrefabPath =
        "Assets/SpecialSkillsEffectsPack/AllEffects/EffectsSet_1(NotScriptBased)/Effects/Effect_13_DangerClose/Effect_13_Base/Effect_13_Explosion.prefab";

    [MenuItem("TowerDefense/Setup Hit Effects")]
    public static void SetupFromMenu()
    {
        SetupAll();
        Debug.Log("타워 타격 이펙트 SO 생성 및 타워 프리팹 연결 완료.");
    }

    // TowerAttackVfxSetup이 AllIn1 타워 이펙트를 담당합니다.

    private static void SetupAll()
    {
        EnsureFolder("Assets/SO", "Effects");

        HitEffectDataSO bowHit = CreateOrLoadHitEffect(
            BowHitAssetPath,
            BowEffectPrefabPath,
            0.45f,
            1.5f);

        HitEffectDataSO culverinHit = CreateOrLoadHitEffect(
            CulverinHitAssetPath,
            CulverinEffectPrefabPath,
            0.55f,
            2f);

        AssignToTowerPrefab(
            "Assets/Prefab/Tower/NormalBow.prefab",
            typeof(BowSkillModule),
            bowHit);

        AssignToTowerPrefab(
            "Assets/Prefab/Tower/NormalCulverin.prefab",
            typeof(CulverinSkill),
            culverinHit);

        AssignToProjectilePrefab(
            "Assets/Prefab/Tower/Arrow.prefab",
            bowHit);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureFolder(string parent, string child)
    {
        if (!AssetDatabase.IsValidFolder(parent))
            return;

        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }

    private static HitEffectDataSO CreateOrLoadHitEffect(
        string assetPath,
        string prefabPath,
        float scale,
        float lifetime)
    {
        HitEffectDataSO data = AssetDatabase.LoadAssetAtPath<HitEffectDataSO>(assetPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<HitEffectDataSO>();
            AssetDatabase.CreateAsset(data, assetPath);
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"이펙트 프리팹을 찾을 수 없습니다: {prefabPath}");
            return data;
        }

        data.effectPrefab = prefab;
        data.scale = scale;
        data.lifetime = lifetime;
        EditorUtility.SetDirty(data);
        return data;
    }

    private static void AssignToTowerPrefab(string prefabPath, System.Type skillType, HitEffectDataSO hitEffect)
    {
        GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefabRoot == null || hitEffect == null)
            return;

        Component skill = prefabRoot.GetComponentInChildren(skillType, true);
        if (skill == null)
            return;

        SerializedObject so = new SerializedObject(skill);
        SerializedProperty prop = so.FindProperty("hitEffect");
        if (prop == null)
            return;

        prop.objectReferenceValue = hitEffect;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(prefabRoot);
    }

    private static void AssignToProjectilePrefab(string prefabPath, HitEffectDataSO hitEffect)
    {
        GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefabRoot == null || hitEffect == null)
            return;

        AbstractBow bow = prefabRoot.GetComponent<AbstractBow>();
        if (bow == null)
            return;

        SerializedObject so = new SerializedObject(bow);
        SerializedProperty prop = so.FindProperty("hitEffect");
        if (prop == null)
            return;

        prop.objectReferenceValue = hitEffect;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(prefabRoot);
    }
}
#endif
