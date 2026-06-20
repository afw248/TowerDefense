#if UNITY_EDITOR
using CoreSystem.EffectSystem;
using UnityEditor;
using UnityEngine;

public static class TowerAttackVfxSetup
{
    private const string EffectsFolder = "Assets/SO/Effects";

    private const string BowVfxPath = EffectsFolder + "/BowTowerAttackVfx.asset";
    private const string CulverinVfxPath = EffectsFolder + "/CulverinTowerAttackVfx.asset";

    private const string BowAllIn1Prefab =
        "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Blue Impact.prefab";

    private const string CulverinAllIn1Prefab =
        "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Explosion Bomb.prefab";

    [MenuItem("TowerDefense/Setup Tower Attack VFX (AllIn1)")]
    public static void SetupFromMenu()
    {
        SetupAll();
        Debug.Log("AllIn1 타워 공격 이펙트 SO 생성 및 타워 프리팹 연결 완료.");
    }

    [InitializeOnLoadMethod]
    private static void SetupOnLoad()
    {
        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.LoadAssetAtPath<TowerAttackVfxDataSO>(BowVfxPath) != null)
                return;

            SetupAll();
        };
    }

    private static void SetupAll()
    {
        if (!AssetDatabase.IsValidFolder("Assets/SO"))
            return;

        if (!AssetDatabase.IsValidFolder(EffectsFolder))
            AssetDatabase.CreateFolder("Assets/SO", "Effects");

        TowerAttackVfxDataSO bowVfx = CreateOrLoad(
            BowVfxPath,
            BowAllIn1Prefab,
            scale: 1.2f,
            lifetime: 1.2f,
            directMult: 0.35f,
            effectMult: 0.65f,
            radius: 1.8f);

        TowerAttackVfxDataSO culverinVfx = CreateOrLoad(
            CulverinVfxPath,
            CulverinAllIn1Prefab,
            scale: 1.5f,
            lifetime: 2f,
            directMult: 0.25f,
            effectMult: 0.75f,
            radius: 3f);

        AssignAttackVfx("Assets/Prefab/Tower/NormalBow.prefab", typeof(BowSkillModule), bowVfx);
        AssignAttackVfx("Assets/Prefab/Tower/NormalCulverin.prefab", typeof(CulverinSkill), culverinVfx);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static TowerAttackVfxDataSO CreateOrLoad(
        string assetPath,
        string prefabPath,
        float scale,
        float lifetime,
        float directMult,
        float effectMult,
        float radius)
    {
        TowerAttackVfxDataSO data = AssetDatabase.LoadAssetAtPath<TowerAttackVfxDataSO>(assetPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<TowerAttackVfxDataSO>();
            AssetDatabase.CreateAsset(data, assetPath);
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"AllIn1 이펙트 프리팹을 찾을 수 없습니다: {prefabPath}");
            return data;
        }

        data.effectPrefab = prefab;
        data.scale = scale;
        data.lifetime = lifetime;
        data.directDamageMultiplier = directMult;
        data.effectDamageMultiplier = effectMult;
        data.damageRadius = radius;
        data.damageTickInterval = 0f;
        data.includePrimaryInEffectDamage = false;
        data.positionOffset = new Vector3(0f, 0.5f, 0f);

        EditorUtility.SetDirty(data);
        return data;
    }

    private static void AssignAttackVfx(string prefabPath, System.Type skillType, TowerAttackVfxDataSO attackVfx)
    {
        GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefabRoot == null || attackVfx == null)
            return;

        Component skill = prefabRoot.GetComponentInChildren(skillType, true);
        if (skill == null)
            return;

        SerializedObject so = new SerializedObject(skill);
        SerializedProperty prop = so.FindProperty("attackVfx");
        if (prop == null)
            return;

        prop.objectReferenceValue = attackVfx;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(prefabRoot);
    }
}
#endif
