#if UNITY_EDITOR
using CoreSystem.EffectSystem;
using UnityEditor;
using UnityEngine;

public static class TowerProjectileSetup
{
    private const string ProjectileFolder = "Assets/Prefab/Projectile";
    private const string DataFolder = "Assets/SO/Projectile";

    private const string BowVisual =
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/Bullet14R_Bolt00.prefab";
    private const string CulverinVisual =
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/Bullet16_Ball00.prefab";
    private const string MissileVisual =
        "Assets/Graphics/FattyPolyTurretPart2/Prefabs/Bullet02.prefab";

    [MenuItem("TowerDefense/Setup Tower Homing Projectiles")]
    public static void SetupFromMenu()
    {
        SetupAll();
        Debug.Log("타워 유도탄 프리팹·SO 생성 및 타워 연결 완료.");
    }

    public static void SetupAll()
    {
        EnsureFolder("Assets/Prefab", "Projectile");
        EnsureFolder("Assets/SO", "Projectile");

        GameObject bowPrefab = CreateProjectilePrefab(
            $"{ProjectileFolder}/TowerProjectile_Bow.prefab",
            BowVisual,
            new Vector3(1.2f, 1.2f, 1.2f));

        GameObject culverinPrefab = CreateProjectilePrefab(
            $"{ProjectileFolder}/TowerProjectile_Culverin.prefab",
            CulverinVisual,
            new Vector3(1.4f, 1.4f, 1.4f));

        GameObject missilePrefab = CreateProjectilePrefab(
            $"{ProjectileFolder}/TowerProjectile_Missile.prefab",
            MissileVisual,
            new Vector3(1.6f, 1.6f, 1.6f));

        TowerProjectileDataSO bowData = CreateProjectileData(
            $"{DataFolder}/BowProjectileData.asset",
            bowPrefab,
            speed: 24f,
            turnSpeedDegrees: 320f,
            hitRadius: 0.35f,
            straightDistance: 2f);

        TowerProjectileDataSO culverinData = CreateProjectileData(
            $"{DataFolder}/CulverinProjectileData.asset",
            culverinPrefab,
            speed: 16f,
            turnSpeedDegrees: 200f,
            hitRadius: 0.5f,
            straightDistance: 2.5f);

        TowerProjectileDataSO missileData = CreateProjectileData(
            $"{DataFolder}/MissileProjectileData.asset",
            missilePrefab,
            speed: 14f,
            turnSpeedDegrees: 260f,
            hitRadius: 0.55f,
            straightDistance: 3f);

        AssignToTower("Assets/Prefab/Tower/NormalBow.prefab", typeof(BowSkillModule), bowData);
        AssignToTower("Assets/Prefab/Tower/NormalCulverin.prefab", typeof(CulverinSkill), culverinData);
        AssignToTower("Assets/Prefab/Tower/NormalMissile.prefab", typeof(MissileSkillModule), missileData);

        SetEffectOnlyDamage("Assets/SO/Effects/BowTowerAttackVfx.asset");
        SetEffectOnlyDamage("Assets/SO/Effects/CulverinTowerAttackVfx.asset");
        SetEffectOnlyDamage("Assets/SO/Effects/MissileTowerAttackVfx.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }

    private static GameObject CreateProjectilePrefab(string path, string visualPath, Vector3 visualScale)
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
            AssetDatabase.DeleteAsset(path);

        GameObject root = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path));
        root.AddComponent<HomingTowerProjectile>();

        GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(visualPath);
        if (visualPrefab != null)
        {
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(visualPrefab, root.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = visualScale;
        }

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    private static TowerProjectileDataSO CreateProjectileData(
        string path,
        GameObject projectilePrefab,
        float speed,
        float turnSpeedDegrees,
        float hitRadius,
        float straightDistance)
    {
        TowerProjectileDataSO data = AssetDatabase.LoadAssetAtPath<TowerProjectileDataSO>(path);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<TowerProjectileDataSO>();
            AssetDatabase.CreateAsset(data, path);
        }

        data.projectilePrefab = projectilePrefab;
        data.speed = speed;
        data.turnSpeedDegrees = turnSpeedDegrees;
        data.straightPursuitDistance = straightDistance;
        data.hitRadius = hitRadius;
        data.maxLifetime = 5f;
        data.aimHeightOffset = new Vector3(0f, 1f, 0f);
        EditorUtility.SetDirty(data);
        return data;
    }

    private static void AssignToTower(string prefabPath, System.Type skillType, TowerProjectileDataSO projectileData)
    {
        GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefabRoot == null)
        {
            Debug.LogWarning($"타워 프리팹을 찾을 수 없습니다: {prefabPath}");
            return;
        }

        Component skill = prefabRoot.GetComponentInChildren(skillType, true);
        if (skill == null)
            return;

        SerializedObject so = new SerializedObject(skill);
        SerializedProperty prop = so.FindProperty("projectileData");
        if (prop == null)
            return;

        prop.objectReferenceValue = projectileData;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(prefabRoot);
    }

    private static void SetEffectOnlyDamage(string vfxPath)
    {
        TowerAttackVfxDataSO vfx = AssetDatabase.LoadAssetAtPath<TowerAttackVfxDataSO>(vfxPath);
        if (vfx == null)
            return;

        vfx.directDamageMultiplier = 0f;
        vfx.effectDamageMultiplier = 1f;
        EditorUtility.SetDirty(vfx);
    }
}
#endif
