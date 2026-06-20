#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Agents;
using CombatSystem;
using CoreSystem.EffectSystem;
using Player;
using Tower;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Normal 타워를 베이스로 Rare/Epic/Legendary 프리팹·SO·스폰 테이블을 생성합니다.
/// </summary>
public static class TowerGradeContentGenerator
{
    private const string GradeConfigPath = "Assets/SO/Tower/TowerGradeConfig.asset";
    private const string VariantFolder = "Assets/SO/Tower/Variants";
    private const string PlayerDataFolder = "Assets/SO/Player/Grades";
    private const string VfxFolder = "Assets/SO/Effects/Grades";
    private const string PrefabFolder = "Assets/Prefab/Tower";
    private const string GradeListFolder = "Assets/SO/Spawn/Grades";

    private const string BowBasePrefab = PrefabFolder + "/NormalBow.prefab";
    private const string CulverinBasePrefab = PrefabFolder + "/NormalCulverin.prefab";

    private const string NormalBowData = "Assets/SO/Player/Normal Bow.asset";
    private const string NormalCulverinData = "Assets/SO/Player/Normal Culverin.asset";
    private const string BowBaseVfx = "Assets/SO/Effects/BowTowerAttackVfx.asset";
    private const string CulverinBaseVfx = "Assets/SO/Effects/CulverinTowerAttackVfx.asset";

    private static readonly string[] BowAnimPaths =
    {
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/Anim/CrossBowY00_Anim.prefab",
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/Anim/CrossBowY01_Anim.prefab",
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/Anim/CrossBowY02_Anim.prefab",
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/Anim/CrossBowY03_Anim.prefab",
    };

    private static readonly string[] BowVisualPaths =
    {
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/CrossBowY00.prefab",
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/CrossBowY01.prefab",
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/CrossBowY02.prefab",
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/CrossBowY03.prefab",
    };

    private static readonly string[] CulverinAnimPaths =
    {
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/Anim/CulverinY00_Anim.prefab",
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/Anim/CulverinY01_Anim.prefab",
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/Anim/CulverinY02_Anim.prefab",
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/Anim/CulverinY03_Anim.prefab",
    };

    private static readonly string[] CulverinVisualPaths =
    {
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/CulverinB00.prefab",
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/CulverinY01.prefab",
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/CulverinY02.prefab",
        "Assets/Graphics/FattyPolyTurretPart7/Prefabs/CulverinY03.prefab",
    };

    private static readonly string[] BowVfxPaths =
    {
        "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Blue Impact.prefab",
        "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Ice Impact.prefab",
        "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Purple Impact.prefab",
        "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Lightning Strike.prefab",
    };

    private static readonly string[] CulverinVfxPaths =
    {
        "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Explosion Bomb.prefab",
        "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Toon Explosion.prefab",
        "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Magic Explosive Spell.prefab",
        "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Explosion Galaxy.prefab",
    };

    [MenuItem("TowerDefense/Generate Grade Towers (Rare~Legendary)")]
    public static void GenerateFromMenu()
    {
        GenerateAll();
        Debug.Log("등급별 타워(Rare/Epic/Legendary) 생성 완료. Normal은 기존 프리팹을 Variant에 연결했습니다.");
    }

    public static void GenerateAll()
    {
        EnsureFolders();

        TowerGradeConfigSO config = LoadOrCreateGradeConfig();
        var gradeLists = new Dictionary<TowerGrade, GradeList>();

        GenerateArchetype(TowerArchetype.Bow, BowBasePrefab, NormalBowData, BowBaseVfx, BowAnimPaths, BowVisualPaths, BowVfxPaths, config, gradeLists);
        GenerateArchetype(TowerArchetype.Culverin, CulverinBasePrefab, NormalCulverinData, CulverinBaseVfx, CulverinAnimPaths, CulverinVisualPaths, CulverinVfxPaths, config, gradeLists);

        UpdateAllPlayerList(gradeLists);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureFolders()
    {
        CreateFolderChain("Assets/SO/Tower");
        CreateFolderChain(VariantFolder);
        CreateFolderChain(PlayerDataFolder);
        CreateFolderChain(VfxFolder);
        CreateFolderChain(GradeListFolder);
    }

    private static void CreateFolderChain(string path)
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

    private static TowerGradeConfigSO LoadOrCreateGradeConfig()
    {
        TowerGradeConfigSO config = AssetDatabase.LoadAssetAtPath<TowerGradeConfigSO>(GradeConfigPath);
        if (config != null)
            return config;

        config = ScriptableObject.CreateInstance<TowerGradeConfigSO>();
        AssetDatabase.CreateAsset(config, GradeConfigPath);
        return config;
    }

    private static void GenerateArchetype(
        TowerArchetype archetype,
        string basePrefabPath,
        string baseDataPath,
        string baseVfxPath,
        string[] animPaths,
        string[] visualPaths,
        string[] vfxPaths,
        TowerGradeConfigSO config,
        Dictionary<TowerGrade, GradeList> gradeLists)
    {
        PlayerDataSO baseData = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(baseDataPath);
        TowerAttackVfxDataSO baseVfx = AssetDatabase.LoadAssetAtPath<TowerAttackVfxDataSO>(baseVfxPath);
        string archetypeName = archetype.ToString();

        foreach (TowerGradeTierSettings tier in config.tiers)
        {
            int sizeIndex = Mathf.Clamp(tier.fattyPolySizeIndex, 0, 3);
            GameObject animPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(animPaths[sizeIndex]);
            GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(visualPaths[sizeIndex]);
            GameObject vfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(vfxPaths[sizeIndex]);
            string gradeName = tier.grade.ToString();

            PlayerDataSO playerData;
            TowerAttackVfxDataSO attackVfx;
            GameObject towerPrefab;

            if (tier.grade == TowerGrade.Normal)
            {
                playerData = baseData;
                attackVfx = baseVfx;
                towerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);
            }
            else
            {
                string prefabPath = $"{PrefabFolder}/{gradeName}{archetypeName}.prefab";

                playerData = CreatePlayerData(
                    $"{PlayerDataFolder}/{gradeName} {archetypeName}.asset",
                    baseData,
                    tier.attackMultiplier,
                    $"{gradeName} {archetypeName}");

                attackVfx = CreateAttackVfx(
                    $"{VfxFolder}/{gradeName}{archetypeName}AttackVfx.asset",
                    baseVfx,
                    vfxPrefab,
                    tier);

                towerPrefab = BuildTowerPrefab(
                    tier.grade,
                    archetype,
                    basePrefabPath,
                    prefabPath,
                    animPrefab,
                    playerData,
                    attackVfx);
            }

            TowerVariantSO variant = CreateVariant(
                $"{VariantFolder}/{gradeName}{archetypeName}Variant.asset",
                tier.grade,
                archetype,
                playerData,
                attackVfx,
                towerPrefab,
                visualPrefab ?? animPrefab);

            WireVariantOnPrefab(towerPrefab, variant);
            AddToGradeList(tier, towerPrefab, gradeLists);
        }
    }

    private static PlayerDataSO CreatePlayerData(string path, PlayerDataSO baseData, float mult, string assetName)
    {
        PlayerDataSO data = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(path);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<PlayerDataSO>();
            AssetDatabase.CreateAsset(data, path);
        }

        data.name = assetName;
        var so = new SerializedObject(data);
        so.FindProperty("<DetectRadius>k__BackingField").floatValue = baseData.DetectRadius;
        so.FindProperty("<ViewAngle>k__BackingField").floatValue = baseData.ViewAngle;
        so.FindProperty("<StopDistance>k__BackingField").floatValue = baseData.StopDistance;
        so.FindProperty("<Attack>k__BackingField").floatValue = baseData.Attack * mult;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(data);
        return data;
    }

    private static TowerAttackVfxDataSO CreateAttackVfx(
        string path,
        TowerAttackVfxDataSO baseVfx,
        GameObject vfxPrefab,
        TowerGradeTierSettings tier)
    {
        TowerAttackVfxDataSO data = AssetDatabase.LoadAssetAtPath<TowerAttackVfxDataSO>(path);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<TowerAttackVfxDataSO>();
            AssetDatabase.CreateAsset(data, path);
        }

        data.effectPrefab = vfxPrefab != null ? vfxPrefab : baseVfx.effectPrefab;
        data.scale = baseVfx.scale * tier.vfxScaleMultiplier;
        data.lifetime = baseVfx.lifetime * tier.vfxLifetimeMultiplier;
        data.positionOffset = baseVfx.positionOffset;
        data.directDamageMultiplier = baseVfx.directDamageMultiplier;
        data.effectDamageMultiplier = baseVfx.effectDamageMultiplier;
        data.damageRadius = baseVfx.damageRadius * tier.effectRadiusMultiplier;
        data.damageTickInterval = baseVfx.damageTickInterval;
        data.includePrimaryInEffectDamage = baseVfx.includePrimaryInEffectDamage;
        EditorUtility.SetDirty(data);
        return data;
    }

    private static GameObject BuildTowerPrefab(
        TowerGrade grade,
        TowerArchetype archetype,
        string sourcePath,
        string targetPath,
        GameObject fattyPrefab,
        PlayerDataSO playerData,
        TowerAttackVfxDataSO attackVfx)
    {
        if (File.Exists(targetPath))
            AssetDatabase.DeleteAsset(targetPath);

        if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
        {
            Debug.LogError($"프리팹 복사 실패: {sourcePath} -> {targetPath}");
            return null;
        }

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(targetPath);
        if (prefabRoot == null)
            return null;

        prefabRoot.name = $"{grade}{archetype}";

        AbstractPlayer player = prefabRoot.GetComponent<AbstractPlayer>();
        if (player != null)
        {
            SerializedObject playerSo = new SerializedObject(player);
            playerSo.FindProperty("<PlayerData>k__BackingField").objectReferenceValue = playerData;
            playerSo.ApplyModifiedPropertiesWithoutUndo();
        }

        System.Type skillType = archetype == TowerArchetype.Bow ? typeof(BowSkillModule) : typeof(CulverinSkill);
        Component skill = prefabRoot.GetComponentInChildren(skillType, true);
        if (skill != null)
        {
            SerializedObject skillSo = new SerializedObject(skill);
            skillSo.FindProperty("attackVfx").objectReferenceValue = attackVfx;
            skillSo.ApplyModifiedPropertiesWithoutUndo();
        }

        ReplaceFattyVisual(prefabRoot, fattyPrefab);
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, targetPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        return AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
    }

    private static void ReplaceFattyVisual(GameObject prefabRoot, GameObject fattyPrefab)
    {
        if (fattyPrefab == null)
            return;

        Transform existingVisual = FindFattyVisualRoot(prefabRoot.transform);
        if (existingVisual != null)
        {
            Transform parent = existingVisual.parent;
            Vector3 localPos = existingVisual.localPosition;
            Quaternion localRot = existingVisual.localRotation;
            Vector3 localScale = existingVisual.localScale;
            Object.DestroyImmediate(existingVisual.gameObject);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(fattyPrefab, parent);
            instance.transform.localPosition = localPos;
            instance.transform.localRotation = localRot;
            instance.transform.localScale = localScale;
            return;
        }

        GameObject fallback = (GameObject)PrefabUtility.InstantiatePrefab(fattyPrefab, prefabRoot.transform);
        fallback.transform.SetAsFirstSibling();
        fallback.transform.localPosition = Vector3.zero;
        fallback.transform.localRotation = Quaternion.identity;
        fallback.transform.localScale = Vector3.one;
    }

    private static Transform FindFattyVisualRoot(Transform root)
    {
        foreach (Transform child in root)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(child.gameObject))
                return child;

            if (child.GetComponentInChildren<Animator>(true) != null
                && child.GetComponent<AgentSensor>() == null
                && child.name.Contains("Skill") == false
                && child.name.Contains("Health") == false)
                return child;
        }

        return null;
    }

    private static TowerVariantSO CreateVariant(
        string path,
        TowerGrade grade,
        TowerArchetype archetype,
        PlayerDataSO playerData,
        TowerAttackVfxDataSO attackVfx,
        GameObject towerPrefab,
        GameObject fattyPrefab)
    {
        TowerVariantSO variant = AssetDatabase.LoadAssetAtPath<TowerVariantSO>(path);
        if (variant == null)
        {
            variant = ScriptableObject.CreateInstance<TowerVariantSO>();
            AssetDatabase.CreateAsset(variant, path);
        }

        variant.grade = grade;
        variant.archetype = archetype;
        variant.playerData = playerData;
        variant.attackVfx = attackVfx;
        variant.towerPrefab = towerPrefab;
        variant.fattyPolyVisualPrefab = fattyPrefab;
        EditorUtility.SetDirty(variant);
        return variant;
    }

    private static void WireVariantOnPrefab(GameObject prefab, TowerVariantSO variant)
    {
        if (prefab == null || variant == null)
            return;

        GameObject contents = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));
        TowerVariantReference reference = contents.GetComponent<TowerVariantReference>();
        if (reference == null)
            reference = contents.AddComponent<TowerVariantReference>();

        SerializedObject so = new SerializedObject(reference);
        so.FindProperty("variant").objectReferenceValue = variant;
        so.ApplyModifiedPropertiesWithoutUndo();

        AbstractPlayer player = contents.GetComponent<AbstractPlayer>();
        if (player != null)
        {
            SerializedObject playerSo = new SerializedObject(player);
            playerSo.FindProperty("towerVariant").objectReferenceValue = variant;
            playerSo.ApplyModifiedPropertiesWithoutUndo();
        }

        PrefabUtility.SaveAsPrefabAsset(contents, AssetDatabase.GetAssetPath(prefab));
        PrefabUtility.UnloadPrefabContents(contents);
    }

    private static void AddToGradeList(TowerGradeTierSettings tier, GameObject towerPrefab, Dictionary<TowerGrade, GradeList> gradeLists)
    {
        if (!gradeLists.TryGetValue(tier.grade, out GradeList list))
        {
            string listPath = $"{GradeListFolder}/GradeList_{tier.grade}.asset";
            list = AssetDatabase.LoadAssetAtPath<GradeList>(listPath);
            if (list == null)
            {
                list = ScriptableObject.CreateInstance<GradeList>();
                AssetDatabase.CreateAsset(list, listPath);
            }

            list.gradeName = tier.grade.ToString();
            list.weight = tier.spawnWeight;
            list.tower.Clear();
            gradeLists[tier.grade] = list;
        }

        AbstractPlayer playerPrefab = towerPrefab.GetComponent<AbstractPlayer>();
        if (playerPrefab != null && !list.tower.Contains(playerPrefab))
            list.tower.Add(playerPrefab);

        EditorUtility.SetDirty(list);
    }

    private static void UpdateAllPlayerList(Dictionary<TowerGrade, GradeList> gradeLists)
    {
        AllPlayerListSO allList = AssetDatabase.LoadAssetAtPath<AllPlayerListSO>("Assets/SO/Spawn/AllPlayerListSO.asset");
        if (allList == null)
        {
            Debug.LogWarning("AllPlayerListSO를 찾을 수 없습니다.");
            return;
        }

        GradeList normalList = AssetDatabase.LoadAssetAtPath<GradeList>("Assets/SO/Spawn/GradeList.asset");
        if (normalList != null)
        {
            normalList.gradeName = TowerGrade.Normal.ToString();
            if (configTryGetWeight(TowerGrade.Normal, out float w))
                normalList.weight = w;
            EditorUtility.SetDirty(normalList);
            if (!gradeLists.ContainsKey(TowerGrade.Normal))
                gradeLists[TowerGrade.Normal] = normalList;
        }

        allList.towerList.Clear();
        TowerGrade[] order = { TowerGrade.Normal, TowerGrade.Rare, TowerGrade.Epic, TowerGrade.Legendary };
        foreach (TowerGrade grade in order)
        {
            if (gradeLists.TryGetValue(grade, out GradeList list))
                allList.towerList.Add(list);
        }

        EditorUtility.SetDirty(allList);
    }

    private static bool configTryGetWeight(TowerGrade grade, out float weight)
    {
        weight = 0f;
        TowerGradeConfigSO config = AssetDatabase.LoadAssetAtPath<TowerGradeConfigSO>(GradeConfigPath);
        if (config == null || !config.TryGetTier(grade, out TowerGradeTierSettings tier))
            return false;

        weight = tier.spawnWeight;
        return true;
    }
}
#endif
