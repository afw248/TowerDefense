#if UNITY_EDITOR
using Agents;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class MissileAnimationSetup
{
    private const string MissileControllerPath =
        "Assets/Graphics/FattyPolyTurretPart2/Animations/Missile.controller";
    private const string CatapultControllerPath =
        "Assets/Graphics/FattyPolyTurretPart2/Animations/Catapult.controller";
    private const string FattyAnimFolder =
        "Assets/Graphics/FattyPolyTurretPart2/Prefabs/Anim";
    private const string CrossBowTestControllerPath =
        "Assets/Graphics/FattyPolyTurretPart7/Animations/CrossBow Test.controller";

    private static readonly (string stateName, string clipPath)[] MissileStates =
    {
        ("IDLE", "Assets/Graphics/FattyPolyTurretPart2/Animations/Missile_Idle.anim"),
        ("FIRE", "Assets/Graphics/FattyPolyTurretPart2/Animations/Missile_Fire.anim"),
        ("RELOAD", "Assets/Graphics/FattyPolyTurretPart2/Animations/Missile_Reload.anim"),
        ("INSTALL", "Assets/Graphics/FattyPolyTurretPart2/Animations/Missile_Install.anim"),
        ("REMOVE", "Assets/Graphics/FattyPolyTurretPart2/Animations/Missile_Remove.anim"),
    };

    private const float MissileTopY = 0.4f;
    private const float MissileFirearmY = 1.7f;

    [MenuItem("TowerDefense/Fix Missile Animations")]
    public static void FixFromMenu()
    {
        RebuildMissileController();
        FixAnimPrefabs();
        FixMissileVisualAlignment();
        FixMissileTowerPrefabs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Missile 타워 애니메이션 복구 완료: Missile.controller + Stand + Animator 연결");
    }

    [MenuItem("TowerDefense/Fix Missile Visual Alignment")]
    public static void FixMissileVisualAlignmentFromMenu()
    {
        FixMissileVisualAlignment();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Missile 머리/스탠드 정렬 복구 완료.");
    }

    public static void FixMissileVisualAlignment()
    {
        FixAnimPrefabTransforms();
        FixMissileFirearmAnimationOffsets();
    }

    private static void FixAnimPrefabTransforms()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("FattyMissile t:Prefab", new[] { FattyAnimFolder });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith("_Anim.prefab"))
                continue;

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
            bool changed = false;

            Transform top = prefabRoot.transform.Find("Turret/Top");
            if (top != null)
            {
                Vector3 topPosition = top.localPosition;
                if (!Mathf.Approximately(topPosition.y, MissileTopY))
                {
                    topPosition.y = MissileTopY;
                    top.localPosition = topPosition;
                    changed = true;
                }
            }

            Transform firearm = prefabRoot.transform.Find("Turret/Top/Firearm");
            if (firearm != null)
            {
                Vector3 firearmPosition = firearm.localPosition;
                if (!Mathf.Approximately(firearmPosition.y, MissileFirearmY))
                {
                    firearmPosition.y = MissileFirearmY;
                    firearm.localPosition = firearmPosition;
                    changed = true;
                }
            }

            if (changed)
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);

            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private static void FixMissileFirearmAnimationOffsets()
    {
        foreach ((string stateName, string clipPath) in MissileStates)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
                continue;

            if (!FixFirearmYOffsetCurves(clip))
                continue;

            EditorUtility.SetDirty(clip);
        }
    }

    private static bool FixFirearmYOffsetCurves(AnimationClip clip)
    {
        bool changed = false;
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

        foreach (EditorCurveBinding binding in bindings)
        {
            if (binding.type != typeof(Transform) || binding.propertyName != "m_LocalPosition.y")
                continue;

            if (!binding.path.EndsWith("Turret/Top/Firearm"))
                continue;

            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (curve == null || curve.length == 0)
                continue;

            Keyframe[] keys = curve.keys;
            float maxValue = 0f;
            for (int i = 0; i < keys.Length; i++)
                maxValue = Mathf.Max(maxValue, keys[i].value);

            if (maxValue >= MissileFirearmY - 0.01f)
                continue;

            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].value += MissileFirearmY;
                changed = true;
            }

            if (changed)
            {
                curve.keys = keys;
                AnimationUtility.SetEditorCurve(clip, binding, curve);
            }
        }

        return changed;
    }

    public static void RebuildMissileController()
    {
        AnimatorController catapult =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(CatapultControllerPath);
        if (catapult == null)
        {
            Debug.LogError("Catapult.controller could not be loaded.");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(MissileControllerPath) != null)
            AssetDatabase.DeleteAsset(MissileControllerPath);

        if (!AssetDatabase.CopyAsset(CatapultControllerPath, MissileControllerPath))
        {
            Debug.LogError("Failed to copy Catapult.controller to Missile.controller.");
            return;
        }

        AnimatorController missile =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(MissileControllerPath);
        if (missile == null)
        {
            Debug.LogError("Missile.controller could not be loaded after copy.");
            return;
        }

        missile.name = "Missile";

        var clipByState = new System.Collections.Generic.Dictionary<string, AnimationClip>();
        foreach ((string stateName, string clipPath) in MissileStates)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                Debug.LogWarning($"Missing missile clip: {clipPath}");
                continue;
            }

            clipByState[stateName] = clip;
        }

        AnimatorStateMachine root = missile.layers[0].stateMachine;
        foreach (ChildAnimatorState child in root.states)
        {
            AnimatorState state = child.state;
            string mappedName = MapCatapultStateName(state.name);
            if (string.IsNullOrEmpty(mappedName))
                continue;

            state.name = mappedName;
            if (clipByState.TryGetValue(mappedName, out AnimationClip clip))
                state.motion = clip;
        }

        EditorUtility.SetDirty(missile);
    }

    private static string MapCatapultStateName(string catapultStateName)
    {
        return catapultStateName switch
        {
            "Catapult_Idle" => "IDLE",
            "Catapult_Fire" => "FIRE",
            "Catapult_Reload" => "RELOAD",
            "Catapult_Install" => "INSTALL",
            "Catapult_Remove" => "REMOVE",
            _ => null,
        };
    }

    private static void FixAnimPrefabs()
    {
        AnimatorController missileController =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(MissileControllerPath);
        if (missileController == null)
        {
            Debug.LogError("Missile.controller could not be loaded.");
            return;
        }

        string[] prefabGuids = AssetDatabase.FindAssets("FattyMissile t:Prefab", new[] { FattyAnimFolder });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith("_Anim.prefab"))
                continue;

            FixPrefab(path, missileController);
        }
    }

    public static void FixMissileTowerPrefabs()
    {
        AnimatorController missileController =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(MissileControllerPath);
        if (missileController == null)
        {
            Debug.LogError("Missile.controller could not be loaded.");
            return;
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefab/Tower" });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith("Missile.prefab"))
                continue;

            FixPrefab(path, missileController);
        }
    }

    private static void FixPrefab(string prefabPath, AnimatorController missileController)
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        bool changed = false;

        RuntimeAnimatorController crossBowTestController =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CrossBowTestControllerPath);

        foreach (Animator animator in prefabRoot.GetComponentsInChildren<Animator>(true))
        {
            RuntimeAnimatorController current = animator.runtimeAnimatorController;
            if (current != missileController &&
                (current == crossBowTestController || current == null || current.name != "Missile"))
            {
                animator.runtimeAnimatorController = missileController;
                changed = true;
            }

            changed |= EnsureAgentComponents(animator.gameObject);
        }

        foreach (Transform child in prefabRoot.GetComponentsInChildren<Transform>(true))
        {
            if (!IsStandMeshObject(child.name) || child.gameObject.activeSelf)
                continue;

            child.gameObject.SetActive(true);
            changed = true;
        }

        if (changed)
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);

        PrefabUtility.UnloadPrefabContents(prefabRoot);
    }

    private static bool EnsureAgentComponents(GameObject animatorObject)
    {
        bool changed = false;

        if (animatorObject.GetComponent<AgentRenderer>() == null)
        {
            animatorObject.AddComponent<AgentRenderer>();
            changed = true;
        }

        if (animatorObject.GetComponent<AgentTrigger>() == null)
        {
            animatorObject.AddComponent<AgentTrigger>();
            changed = true;
        }

        return changed;
    }

    private static bool IsStandMeshObject(string objectName)
    {
        return objectName.Contains("_Stand") && objectName.EndsWith("00");
    }
}
#endif
