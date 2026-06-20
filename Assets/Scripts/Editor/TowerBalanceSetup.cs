#if UNITY_EDITOR
using CombatSystem;
using CoreSystem.EffectSystem;
using Player;
using System.Collections.Generic;
using System.IO;
using Tower;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 타워 등급/종류별 밸런스를 일괄 적용합니다.
/// 전설 대포는 30대 웨이브 기준 몬스터 체력을 10의 배수로 올려 한 방에 처치합니다.
/// </summary>
public static class TowerBalanceSetup
{
    private const string SkillFolder = "Assets/SO/Player/Skill";
    private const string GradeSkillFolder = "Assets/SO/Player/Skill/Grades";
    private const string PlayerDataFolder = "Assets/SO/Player";
    private const string GradePlayerDataFolder = "Assets/SO/Player/Grades";
    private const string BaseVfxFolder = "Assets/SO/Effects";
    private const string GradeVfxFolder = "Assets/SO/Effects/Grades";
    private const string PrefabFolder = "Assets/Prefab/Tower";
    private const string SpawnListPath = "Assets/SO/Spawn/EnemySpawn/SpawnEnemyListSO.asset";
    private const string MergeConfigPath = "Assets/SO/Tower/TowerMergeConfig.asset";
    private const string ResourcesMergeConfigPath = "Assets/Resources/TowerMergeConfig.asset";

    private const float NormalDamage = 12f;
    private const float LegendaryReferenceScale = 0.82f;
    private const int ReferenceWave = 39;
    private const int BossInterval = 10;
    private const int NormalToRareMergeChance = 60;
    private const int RareToEpicMergeChance = 20;
    private const int EpicToLegendaryMergeChance = 5;

    private static readonly float[] BowCooldownByGrade = { 0.50f, 0.44f, 0.38f, 0.28f };
    private static readonly float[] MissileCooldownByGrade = { 0.65f, 0.60f, 0.55f, 0.44f };
    private static readonly float[] CulverinCooldownByGrade = { 0.85f, 0.80f, 0.75f, 0.62f };

    private static readonly float[] BowCulverinDetectByGrade = { 8f, 12f, 20f, 33f };
    private static readonly float[] MissileDetectByGrade = { 15f, 19f, 27f, 40f };
    private static readonly float[] CulverinAoEByGrade = { 2.5f, 4.5f, 7f, 14f };
    private static readonly float[] MissileAoEByGrade = { 2f, 2.5f, 3f, 4f };

    private static readonly float[] BowDamageByGrade = { 12f, 14f, 75f, 240f };
    private static readonly float[] MissileDamageByGrade = { 12f, 18f, 85f, 280f };
    private static readonly float[] CulverinDamageByGrade = { 12f, 16f, 80f, 300f };

    [MenuItem("TowerDefense/Apply Tower Balance")]
    public static void ApplyFromMenu()
    {
        ApplyAll();
        float referenceHp = ComputeReferenceEnemyHp();
        float legendaryCulverin = GetLegendaryCulverinDamage();
        Debug.Log($"타워 밸런스 적용 완료. 30대 웨이브 기준 HP={referenceHp:0.#}, 전설 대포={legendaryCulverin:0.#}");
    }

    public static void ApplyAll()
    {
        EnsureFolder(GradeSkillFolder);

        var bowSkills = CreateGradeSkills("Bow", "BowNorAttackSkill", BowCooldownByGrade);
        var missileSkills = CreateGradeSkills("Missile", "MissileNorAttackSkill", MissileCooldownByGrade);
        var culverinSkills = CreateGradeSkills("Culverin", "CulverinNorAttackSkill", CulverinCooldownByGrade);

        UpdatePlayerData();
        UpdateAttackVfx();
        UpdateMergeConfig();

        WirePrefabSkills<BowSkillModule>(bowSkills, "Bow");
        WirePrefabSkills<MissileSkillModule>(missileSkills, "Missile");
        WirePrefabSkills<CulverinSkill>(culverinSkills, "Culverin");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static float GetLegendaryCulverinDamage() =>
        CeilToTen(ComputeReferenceEnemyHp() * LegendaryReferenceScale);

    private static float[] BuildDamageByGrade(float legendaryArchetypeScale)
    {
        float legendaryCulverin = GetLegendaryCulverinDamage();
        float gradeRatio = Mathf.Pow(legendaryCulverin / NormalDamage, 1f / 3f);

        return new[]
        {
            NormalDamage,
            RoundToTen(NormalDamage * gradeRatio),
            RoundToTen(NormalDamage * gradeRatio * gradeRatio),
            RoundToTen(legendaryCulverin * legendaryArchetypeScale),
        };
    }

    private static float ComputeReferenceEnemyHp()
    {
        SpawnEnemyListSO spawnList = AssetDatabase.LoadAssetAtPath<SpawnEnemyListSO>(SpawnListPath);
        if (spawnList == null || spawnList.enemy == null || spawnList.enemy.Count == 0)
            return 220f;

        int enemyIndex = Mathf.Clamp(ReferenceWave / BossInterval, 0, spawnList.enemy.Count - 1);
        GameObject enemyPrefab = spawnList.enemy[enemyIndex];
        if (enemyPrefab == null)
            return 220f;

        HealthModule health = enemyPrefab.GetComponentInChildren<HealthModule>(true);
        if (health == null)
            return 220f;

        float baseHp = health.maxHealth;
        float waveMultiplier = WaveDataSO.GetMultiplierForWave(ReferenceWave);
        return baseHp * waveMultiplier;
    }

    private static float CeilToTen(float value) => Mathf.Ceil(value / 10f) * 10f;

    private static float RoundToTen(float value) => Mathf.Round(value / 10f) * 10f;

    private static Dictionary<TowerGrade, SkillDataSO> CreateGradeSkills(
        string archetypeName,
        string normalAssetName,
        float[] cooldownByGrade)
    {
        var result = new Dictionary<TowerGrade, SkillDataSO>();
        TowerGrade[] grades = { TowerGrade.Normal, TowerGrade.Rare, TowerGrade.Epic, TowerGrade.Legendary };

        for (int i = 0; i < grades.Length; i++)
        {
            TowerGrade grade = grades[i];
            string assetName = grade == TowerGrade.Normal
                ? normalAssetName
                : $"{grade}{archetypeName}AttackSkill";
            string path = grade == TowerGrade.Normal
                ? $"{SkillFolder}/{assetName}.asset"
                : $"{GradeSkillFolder}/{assetName}.asset";

            SkillDataSO data = LoadOrCreate<SkillDataSO>(path);
            data.skillIndex = 0;
            data.skillName = archetypeName == "Missile" ? "MissileAttack" : "AttackSkill";
            data.cooldown = cooldownByGrade[i];
            data.damageMultiplier = 1f;
            EditorUtility.SetDirty(data);
            result[grade] = data;
        }

        return result;
    }

    private static void UpdatePlayerData()
    {
        ApplyArchetypePlayerData("Bow", BowDamageByGrade, BowCulverinDetectByGrade);
        ApplyArchetypePlayerData("Missile", MissileDamageByGrade, MissileDetectByGrade);
        ApplyArchetypePlayerData("Culverin", CulverinDamageByGrade, BowCulverinDetectByGrade);
    }

    private static void ApplyArchetypePlayerData(string archetype, float[] damageByGrade, float[] detectByGrade)
    {
        SetPlayerData($"{PlayerDataFolder}/Normal {archetype}.asset", damageByGrade[0], detectByGrade[0]);
        SetPlayerData($"{GradePlayerDataFolder}/Rare {archetype}.asset", damageByGrade[1], detectByGrade[1]);
        SetPlayerData($"{GradePlayerDataFolder}/Epic {archetype}.asset", damageByGrade[2], detectByGrade[2]);
        SetPlayerData($"{GradePlayerDataFolder}/Legendary {archetype}.asset", damageByGrade[3], detectByGrade[3]);
    }

    private static void SetPlayerData(string path, float attack, float detectRadius)
    {
        PlayerDataSO data = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(path);
        if (data == null)
            return;

        data.Attack = attack;
        data.DetectRadius = detectRadius;
        EditorUtility.SetDirty(data);
    }

    private static void UpdateMergeConfig()
    {
        WireMergeVfx(MergeConfigPath);
        WireMergeVfx(ResourcesMergeConfigPath);
        SetMergeChance(MergeConfigPath);
        SetMergeChance(ResourcesMergeConfigPath);
        UpdateMergeConfigDefaults();
    }

    private static void WireMergeVfx(string path)
    {
        TowerMergeConfigSO config = AssetDatabase.LoadAssetAtPath<TowerMergeConfigSO>(path);
        if (config?.tiers == null)
            return;

        foreach (TowerMergeConfigSO.MergeTierSettings tier in config.tiers)
        {
            tier.successVfx = AssetDatabase.LoadAssetAtPath<HitEffectDataSO>(
                $"Assets/SO/Effects/Merge/MergeSuccess_{tier.fromGrade}.asset");
            tier.failureVfx = AssetDatabase.LoadAssetAtPath<HitEffectDataSO>(
                $"Assets/SO/Effects/Merge/MergeFailure_{tier.fromGrade}.asset");
        }

        EditorUtility.SetDirty(config);
    }

    private static void SetMergeChance(string path)
    {
        TowerMergeConfigSO config = AssetDatabase.LoadAssetAtPath<TowerMergeConfigSO>(path);
        if (config?.tiers == null)
            return;

        foreach (TowerMergeConfigSO.MergeTierSettings tier in config.tiers)
        {
            tier.successChancePercent = tier.fromGrade switch
            {
                TowerGrade.Normal => NormalToRareMergeChance,
                TowerGrade.Rare => RareToEpicMergeChance,
                TowerGrade.Epic => EpicToLegendaryMergeChance,
                _ => tier.successChancePercent,
            };
        }

        EditorUtility.SetDirty(config);
    }

    private static void UpdateMergeConfigDefaults()
    {
        TowerMergeConfigSO config = AssetDatabase.LoadAssetAtPath<TowerMergeConfigSO>(MergeConfigPath);
        if (config == null)
            return;

        SerializedObject so = new SerializedObject(config);
        SerializedProperty tiers = so.FindProperty("tiers");
        for (int i = 0; i < tiers.arraySize; i++)
        {
            SerializedProperty tier = tiers.GetArrayElementAtIndex(i);
            TowerGrade fromGrade = (TowerGrade)tier.FindPropertyRelative("fromGrade").enumValueIndex;
            int chance = fromGrade switch
            {
                TowerGrade.Normal => NormalToRareMergeChance,
                TowerGrade.Rare => RareToEpicMergeChance,
                TowerGrade.Epic => EpicToLegendaryMergeChance,
                _ => tier.FindPropertyRelative("successChancePercent").intValue,
            };

            tier.FindPropertyRelative("successChancePercent").intValue = chance;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void UpdateAttackVfx()
    {
        ConfigureBowVfx($"{BaseVfxFolder}/BowTowerAttackVfx.asset");
        ConfigureBowVfx($"{GradeVfxFolder}/RareBowAttackVfx.asset");
        ConfigureBowVfx($"{GradeVfxFolder}/EpicBowAttackVfx.asset");
        ConfigureBowVfx($"{GradeVfxFolder}/LegendaryBowAttackVfx.asset");

        ConfigureCulverinVfx($"{BaseVfxFolder}/CulverinTowerAttackVfx.asset", CulverinAoEByGrade[0]);
        ConfigureCulverinVfx($"{GradeVfxFolder}/RareCulverinAttackVfx.asset", CulverinAoEByGrade[1]);
        ConfigureCulverinVfx($"{GradeVfxFolder}/EpicCulverinAttackVfx.asset", CulverinAoEByGrade[2]);
        ConfigureCulverinVfx($"{GradeVfxFolder}/LegendaryCulverinAttackVfx.asset", CulverinAoEByGrade[3]);

        ConfigureMissileVfx($"{BaseVfxFolder}/MissileTowerAttackVfx.asset", MissileAoEByGrade[0]);
        ConfigureMissileVfx($"{GradeVfxFolder}/RareMissileAttackVfx.asset", MissileAoEByGrade[1]);
        ConfigureMissileVfx($"{GradeVfxFolder}/EpicMissileAttackVfx.asset", MissileAoEByGrade[2]);
        ConfigureMissileVfx($"{GradeVfxFolder}/LegendaryMissileAttackVfx.asset", MissileAoEByGrade[3]);
    }

    private static void ConfigureBowVfx(string path)
    {
        TowerAttackVfxDataSO data = AssetDatabase.LoadAssetAtPath<TowerAttackVfxDataSO>(path);
        if (data == null)
            return;

        data.directDamageMultiplier = 0f;
        data.effectDamageMultiplier = 1f;
        data.damageRadius = 0f;
        data.includePrimaryInEffectDamage = false;
        EditorUtility.SetDirty(data);
    }

    private static void ConfigureCulverinVfx(string path, float radius)
    {
        TowerAttackVfxDataSO data = AssetDatabase.LoadAssetAtPath<TowerAttackVfxDataSO>(path);
        if (data == null)
            return;

        data.directDamageMultiplier = 0f;
        data.effectDamageMultiplier = 1f;
        data.damageRadius = radius;
        data.includePrimaryInEffectDamage = false;
        EditorUtility.SetDirty(data);
    }

    private static void ConfigureMissileVfx(string path, float radius)
    {
        TowerAttackVfxDataSO data = AssetDatabase.LoadAssetAtPath<TowerAttackVfxDataSO>(path);
        if (data == null)
            return;

        data.directDamageMultiplier = 0f;
        data.effectDamageMultiplier = 1f;
        data.damageRadius = radius;
        data.includePrimaryInEffectDamage = false;
        data.fastProjectileImpact = true;
        EditorUtility.SetDirty(data);
    }

    private static void WirePrefabSkills<TSkill>(
        Dictionary<TowerGrade, SkillDataSO> skills,
        string prefabArchetypeName) where TSkill : Component
    {
        WireSkillOnPrefab<TSkill>($"{PrefabFolder}/Normal{prefabArchetypeName}.prefab", skills[TowerGrade.Normal]);
        WireSkillOnPrefab<TSkill>($"{PrefabFolder}/Rare{prefabArchetypeName}.prefab", skills[TowerGrade.Rare]);
        WireSkillOnPrefab<TSkill>($"{PrefabFolder}/Epic{prefabArchetypeName}.prefab", skills[TowerGrade.Epic]);
        WireSkillOnPrefab<TSkill>($"{PrefabFolder}/Legendary{prefabArchetypeName}.prefab", skills[TowerGrade.Legendary]);
    }

    private static void WireSkillOnPrefab<TSkill>(string prefabPath, SkillDataSO skillData) where TSkill : Component
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
        if (contents == null)
            return;

        TSkill skill = contents.GetComponentInChildren<TSkill>(true);
        if (skill != null)
        {
            SerializedObject so = new SerializedObject(skill);
            so.FindProperty("<SkillData>k__BackingField").objectReferenceValue = skillData;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
        PrefabUtility.UnloadPrefabContents(contents);
    }

    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null)
            return asset;

        string folder = Path.GetDirectoryName(path)?.Replace('\\', '/');
        if (!string.IsNullOrEmpty(folder))
            EnsureFolder(folder);

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
