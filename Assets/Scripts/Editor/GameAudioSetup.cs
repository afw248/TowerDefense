#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameAudioSetup
{
    private const string ConfigPath = "Assets/Resources/GameAudioConfig.asset";
    private const string SfxFolder = "Assets/Audio/SFX";
    private const string BgmFolder = "Assets/Audio/BGM";
    private const string TitleScenePath = "Assets/Scenes/TitleScene.unity";
    private const string GameScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("TowerDefense/Setup Game Audio")]
    public static void SetupFromMenu()
    {
        SetupAll();
        Debug.Log("게임 오디오 설정 완료.");
    }

    public static GameAudioConfigSO SetupAll()
    {
        AudioImportOptimization.OptimizeAll();
        GameAudioConfigSO config = CreateOrUpdateConfig();
        EnsureAudioManagerInTitleScene(config);
        UpdateSettingsPanelInGameScene();
        AssetDatabase.SaveAssets();
        return config;
    }

    private static GameAudioConfigSO CreateOrUpdateConfig()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        GameAudioConfigSO config = AssetDatabase.LoadAssetAtPath<GameAudioConfigSO>(ConfigPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<GameAudioConfigSO>();
            AssetDatabase.CreateAsset(config, ConfigPath);
        }

        config.titleBgm = LoadClip($"{BgmFolder}/title_bgm.mp3");
        config.gameplayBgm = LoadClip($"{BgmFolder}/gameplay_bgm.ogg");
        config.uiClick = LoadClip($"{SfxFolder}/ui_click.ogg");
        config.uiOpen = LoadClip($"{SfxFolder}/ui_open.ogg");
        config.uiClose = LoadClip($"{SfxFolder}/ui_close.ogg");
        config.bowFire = LoadClip($"{SfxFolder}/bow_fire.ogg");
        config.culverinFire = LoadClip($"{SfxFolder}/culverin_fire.ogg");
        config.missileLaunch = LoadClip($"{SfxFolder}/missile_launch.ogg");
        config.explosion = LoadClip($"{SfxFolder}/explosion.ogg");
        config.enemyHit = LoadClip($"{SfxFolder}/enemy_hit.ogg");
        config.enemyDeath = LoadClip($"{SfxFolder}/enemy_death.ogg");
        config.waveStart = LoadClip($"{SfxFolder}/wave_start.ogg");
        config.bossWarning = LoadClip($"{SfxFolder}/boss_warning.ogg");
        config.coin = LoadClip($"{SfxFolder}/coin.ogg");
        config.towerPlace = LoadClip($"{SfxFolder}/tower_place.ogg");
        config.towerRemove = LoadClip($"{SfxFolder}/tower_remove.ogg");
        config.mergeSuccess = LoadClip($"{SfxFolder}/merge_success.ogg");
        config.mergeFail = LoadClip($"{SfxFolder}/merge_fail.ogg");
        config.epicTowerReveal = LoadClip($"{SfxFolder}/ui_open.ogg");
        config.legendaryTowerReveal = LoadClip($"{SfxFolder}/boss_warning.ogg");
        config.victory = LoadClip($"{SfxFolder}/victory.ogg");
        config.defeat = LoadClip($"{SfxFolder}/defeat.ogg");

        EditorUtility.SetDirty(config);
        return config;
    }

    private static AudioClip LoadClip(string path)
    {
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    private static void EnsureAudioManagerInTitleScene(GameAudioConfigSO config)
    {
        if (!System.IO.File.Exists(TitleScenePath))
            return;

        Scene scene = EditorSceneManager.OpenScene(TitleScenePath, OpenSceneMode.Single);
        GameAudioManager manager = Object.FindFirstObjectByType<GameAudioManager>(FindObjectsInactive.Include);
        if (manager == null)
        {
            GameObject host = GameObject.Find("TitleScreen") ?? new GameObject("TitleScreen");
            manager = host.GetComponent<GameAudioManager>();
            if (manager == null)
                manager = host.AddComponent<GameAudioManager>();
        }

        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("config").objectReferenceValue = config;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void UpdateSettingsPanelInGameScene()
    {
        if (!System.IO.File.Exists(GameScenePath))
            return;

        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        SettingsPanelUi panel = Object.FindFirstObjectByType<SettingsPanelUi>(FindObjectsInactive.Include);
        if (panel != null)
        {
            Transform hint = panel.transform.Find("SettingsHint");
            if (hint != null)
                Object.DestroyImmediate(hint.gameObject);

            SettingsPanelLayoutBuilder.Rebuild(panel, showReturnToTitle: !panel.IsTitlePanel);
            panel.NotifyLayoutBuilt();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
#endif
