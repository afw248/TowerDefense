#if UNITY_EDITOR
using Agents;
using Agents.FSM;
using CombatSystem;
using CoreSystem.EffectSystem;
using GGMLib.AnimatorSystem;
using Player;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class MissileTowerSetup
{
    private const string BasePrefabPath = "Assets/Prefab/Tower/NormalCulverin.prefab";
    private const string OutputPrefabPath = "Assets/Prefab/Tower/NormalMissile.prefab";
    private const string FattyVisualPath =
        "Assets/Graphics/FattyPolyTurretPart2/Prefabs/Anim/FattyMissileY00_Anim.prefab";
    private const string VfxPrefabPath =
        "Assets/AllIn1VfxToolkit/Demo & Assets/Demo/Prefabs/Fire Impact.prefab";

    private const string PlayerDataPath = "Assets/SO/Player/Normal Missile.asset";
    private const string SkillDataPath = "Assets/SO/Player/Skill/MissileNorAttackSkill.asset";
    private const string AttackVfxPath = "Assets/SO/Effects/MissileTowerAttackVfx.asset";
    private const string StateListPath = "Assets/SO/Player/State/Missile State list.asset";
    private const string SharedAnimFolder = "Assets/SO/Player/AgentAnim";
    private const string StateFolder = "Assets/SO/Player/State/Missile";

    [MenuItem("TowerDefense/Setup Missile Tower")]
    public static void SetupFromMenu()
    {
        SetupAll();
        Debug.Log("Missile 타워 프리팹 및 데이터 생성 완료: " + OutputPrefabPath);
    }

    public static void SetupAll()
    {
        EnsureFolder(StateFolder);

        AnimParamSO idle = LoadSharedAnimParam("IDLE");
        AnimParamSO fire = LoadSharedAnimParam("FIRE");
        AnimParamSO install = LoadSharedAnimParam("INSTALL");
        AnimParamSO remove = LoadSharedAnimParam("REMOVE");

        StateSO idleState = CreateState($"{StateFolder}/Missile IDLE State.asset", "IDLE", "PlayerIdleState", 0, idle);
        StateSO fireState = CreateState($"{StateFolder}/Missile FIRE State.asset", "FIRE", "PlayerFireState", 1, fire);
        StateSO installState = CreateState($"{StateFolder}/Missile INSTALL State.asset", "INSTALL", "PlayerInstallState", 2, install);
        StateSO removeState = CreateState($"{StateFolder}/Missile REMOVE State.asset", "REMOVE", "PlayerRemoveState", 3, remove);

        StateListSO stateList = CreateStateList(StateListPath, idleState, fireState, installState, removeState);

        PlayerDataSO playerData = CreatePlayerData();
        SkillDataSO skillData = CreateSkillData();
        TowerAttackVfxDataSO attackVfx = CreateAttackVfx();

        GameObject prefab = BuildMissilePrefab(playerData, stateList, skillData, attackVfx);
        AddToGradeList(prefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folderName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
            AssetDatabase.CreateFolder(parent, folderName);
    }

    private static AnimParamSO LoadSharedAnimParam(string paramName)
    {
        AnimParamSO asset = AssetDatabase.LoadAssetAtPath<AnimParamSO>($"{SharedAnimFolder}/{paramName}.asset");
        if (asset == null)
            Debug.LogError($"Missing shared anim param: {SharedAnimFolder}/{paramName}.asset");
        return asset;
    }

    private static StateSO CreateState(string path, string stateName, string className, int index, AnimParamSO anim)
    {
        StateSO state = AssetDatabase.LoadAssetAtPath<StateSO>(path);
        if (state == null)
        {
            state = ScriptableObject.CreateInstance<StateSO>();
            AssetDatabase.CreateAsset(state, path);
        }

        state.stateName = stateName;
        state.className = className;
        state.assetIndex = index;
        state.stateParam = anim;
        EditorUtility.SetDirty(state);
        return state;
    }

    private static StateListSO CreateStateList(string path, params StateSO[] states)
    {
        StateListSO list = AssetDatabase.LoadAssetAtPath<StateListSO>(path);
        if (list == null)
        {
            list = ScriptableObject.CreateInstance<StateListSO>();
            AssetDatabase.CreateAsset(list, path);
        }

        SerializedObject so = new SerializedObject(list);
        SerializedProperty prop = so.FindProperty("states");
        prop.ClearArray();
        for (int i = 0; i < states.Length; i++)
        {
            prop.InsertArrayElementAtIndex(i);
            prop.GetArrayElementAtIndex(i).objectReferenceValue = states[i];
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(list);
        return list;
    }

    private static PlayerDataSO CreatePlayerData()
    {
        PlayerDataSO data = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(PlayerDataPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<PlayerDataSO>();
            AssetDatabase.CreateAsset(data, PlayerDataPath);
        }

        SerializedObject so = new SerializedObject(data);
        so.FindProperty("<DetectRadius>k__BackingField").floatValue = 9f;
        so.FindProperty("<ViewAngle>k__BackingField").floatValue = 360f;
        so.FindProperty("<StopDistance>k__BackingField").floatValue = 1.4f;
        so.FindProperty("<Attack>k__BackingField").floatValue = 14f;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(data);
        return data;
    }

    private static SkillDataSO CreateSkillData()
    {
        SkillDataSO data = AssetDatabase.LoadAssetAtPath<SkillDataSO>(SkillDataPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<SkillDataSO>();
            AssetDatabase.CreateAsset(data, SkillDataPath);
        }

        data.skillIndex = 0;
        data.skillName = "MissileAttack";
        data.cooldown = 0.65f;
        data.damageMultiplier = 1f;
        EditorUtility.SetDirty(data);
        return data;
    }

    private static TowerAttackVfxDataSO CreateAttackVfx()
    {
        TowerAttackVfxDataSO data = AssetDatabase.LoadAssetAtPath<TowerAttackVfxDataSO>(AttackVfxPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<TowerAttackVfxDataSO>();
            AssetDatabase.CreateAsset(data, AttackVfxPath);
        }

        GameObject vfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VfxPrefabPath);
        data.effectPrefab = vfxPrefab;
        data.scale = 1.25f;
        data.lifetime = 0.6f;
        data.positionOffset = Vector3.zero;
        data.directDamageMultiplier = 0f;
        data.effectDamageMultiplier = 1f;
        data.damageRadius = 2.4f;
        data.damageTickInterval = 0f;
        data.includePrimaryInEffectDamage = false;
        data.followPrimaryTarget = false;
        data.playbackSpeed = 2.2f;
        EditorUtility.SetDirty(data);
        return data;
    }

    private static GameObject BuildMissilePrefab(
        PlayerDataSO playerData,
        StateListSO stateList,
        SkillDataSO skillData,
        TowerAttackVfxDataSO attackVfx)
    {
        if (File.Exists(OutputPrefabPath))
            AssetDatabase.DeleteAsset(OutputPrefabPath);

        AssetDatabase.CopyAsset(BasePrefabPath, OutputPrefabPath);
        GameObject root = PrefabUtility.LoadPrefabContents(OutputPrefabPath);
        root.name = "NormalMissile";

        Culverin culverin = root.GetComponent<Culverin>();
        if (culverin != null)
            Object.DestroyImmediate(culverin, true);

        if (root.GetComponent<Missile>() == null)
            root.AddComponent<Missile>();

        AbstractPlayer player = root.GetComponent<AbstractPlayer>();
        if (player != null)
        {
            SerializedObject playerSo = new SerializedObject(player);
            playerSo.FindProperty("<PlayerData>k__BackingField").objectReferenceValue = playerData;
            playerSo.FindProperty("<playerStates>k__BackingField").objectReferenceValue = stateList;
            playerSo.ApplyModifiedPropertiesWithoutUndo();
        }

        CulverinSkill oldSkill = root.GetComponentInChildren<CulverinSkill>(true);
        Transform skillParent = oldSkill != null ? oldSkill.transform.parent : root.transform;
        if (oldSkill != null)
            Object.DestroyImmediate(oldSkill.gameObject, true);

        GameObject skillGo = new GameObject("MissileSkillModule");
        skillGo.transform.SetParent(skillParent, false);
        MissileSkillModule skill = skillGo.AddComponent<MissileSkillModule>();

        ReplaceFattyVisual(root);
        EnsureAgentComponentsOnAnimator(root);
        Transform firePoint = FindDeepChild(root.transform, "ShootPoint");
        ParticleSystem launchFx = FindLaunchFx(root);

        SerializedObject skillSo = new SerializedObject(skill);
        skillSo.FindProperty("<SkillData>k__BackingField").objectReferenceValue = skillData;
        skillSo.FindProperty("skillParamSO").objectReferenceValue = LoadSharedAnimParam("FIRE");
        skillSo.FindProperty("attackVfx").objectReferenceValue = attackVfx;
        skillSo.FindProperty("firePoint").objectReferenceValue = firePoint;
        skillSo.FindProperty("launchFx").objectReferenceValue = launchFx;
        skillSo.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, OutputPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        return AssetDatabase.LoadAssetAtPath<GameObject>(OutputPrefabPath);
    }

    private static void ReplaceFattyVisual(GameObject prefabRoot)
    {
        GameObject fattyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FattyVisualPath);
        if (fattyPrefab == null)
            return;

        Transform existing = FindVisualRoot(prefabRoot.transform);
        if (existing != null)
        {
            Transform parent = existing.parent;
            Vector3 localPos = existing.localPosition;
            Quaternion localRot = existing.localRotation;
            Vector3 localScale = existing.localScale;
            Object.DestroyImmediate(existing.gameObject);

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

    private static Transform FindVisualRoot(Transform root)
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

    private static void EnsureAgentComponentsOnAnimator(GameObject root)
    {
        Animator animator = root.GetComponentInChildren<Animator>(true);
        if (animator == null)
            return;

        if (animator.GetComponent<AgentRenderer>() == null)
            animator.gameObject.AddComponent<AgentRenderer>();

        if (animator.GetComponent<AgentTrigger>() == null)
            animator.gameObject.AddComponent<AgentTrigger>();
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }

    private static ParticleSystem FindLaunchFx(GameObject root)
    {
        Transform fuse = FindDeepChild(root.transform, "Eff_Fuse");
        if (fuse != null)
            return fuse.GetComponent<ParticleSystem>();

        ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);
        return systems.Length > 0 ? systems[0] : null;
    }

    private static void AddToGradeList(GameObject prefab)
    {
        AbstractPlayer playerPrefab = prefab.GetComponent<AbstractPlayer>();
        if (playerPrefab == null)
            return;

        GradeList gradeList = AssetDatabase.LoadAssetAtPath<GradeList>("Assets/SO/Spawn/GradeList.asset");
        if (gradeList == null)
            return;

        if (!gradeList.tower.Contains(playerPrefab))
        {
            gradeList.tower.Add(playerPrefab);
            EditorUtility.SetDirty(gradeList);
        }
    }
}
#endif
