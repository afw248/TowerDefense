#if UNITY_EDITOR
using TMPro;
using Tower;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameHudSetup
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string EconomyConfigPath = "Assets/SO/Economy/EconomyConfig.asset";
    private const string NexonFontPath = "Assets/Resources/NEXON Football Gothic L SDF.asset";

    private static TMP_FontAsset _nexonFont;

    [MenuItem("TowerDefense/Setup PC Game HUD")]
    public static void SetupFromMenu()
    {
        SetupAll();
        Debug.Log("PC 게임 HUD 설정 완료.");
    }

    [MenuItem("TowerDefense/Apply NEXON Font To UI")]
    public static void ApplyFontFromMenu()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        LoadNexonFont();
        ApplyNexonFontToAllTmp();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("NEXON Football Gothic 폰트 적용 완료.");
    }

    [MenuItem("TowerDefense/Apply Wave Timer UI To Scene")]
    public static void ApplyWaveTimerToSceneFromMenu()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        LoadNexonFont();
        WaveTimerUi waveTimer = WaveTimerUi.EnsureUnderFieldEnemyCount();
        if (waveTimer != null)
            waveTimer.RefreshDisplay();

        Transform legacyTimer = GameObject.Find("WaveUi")?.transform?.Find("WaveTimeText");
        if (legacyTimer != null)
            legacyTimer.gameObject.SetActive(false);

        ApplyNexonFontToAllTmp();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("웨이브 타이머 UI를 FieldEnemyCount 아래에 적용했습니다.");
    }

    public static void SetupAll()
    {
        EconomyConfigSO economyConfig = CreateOrLoadEconomyConfig();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        LoadNexonFont();

        EnsureGameSystems(economyConfig);
        EnsureEnemyEconomyBridge();
        EnsureWaveUi();
        SummonButtonUi summonButton = EnsureLegacySummonButton();
        (MoneyDisplayUi moneyDisplay, UnitCapacityUi unitCapacity) = EnsureBottomResourceBar(economyConfig);
        FieldEnemyCountUi fieldEnemyCount = EnsureFieldEnemyCount(economyConfig);
        WarningMessageUi warningMessage = EnsureWarningMessage();
        GameOverUi gameOverUi = EnsureGameOverUi();
        Canvas hudCanvas = EnsureHudCanvas();
        ArchetypeUpgradePanelUi archetypeUpgradePanel = BuildHudLayout(hudCanvas, economyConfig, summonButton, moneyDisplay, unitCapacity, fieldEnemyCount, warningMessage);
        EnsureArchetypeUpgradeButton(archetypeUpgradePanel);
        WireGameOverController(gameOverUi);
        ApplyNexonFontToAllTmp();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    private static void LoadNexonFont()
    {
        _nexonFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NexonFontPath);
        if (_nexonFont == null)
            Debug.LogWarning($"NEXON 폰트를 찾을 수 없습니다: {NexonFontPath}");
    }

    private static void ApplyNexonFontToAllTmp()
    {
        if (_nexonFont == null)
            return;

        foreach (TextMeshProUGUI tmp in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            tmp.font = _nexonFont;
            EditorUtility.SetDirty(tmp);
        }
    }

    private static EconomyConfigSO CreateOrLoadEconomyConfig()
    {
        EconomyConfigSO config = AssetDatabase.LoadAssetAtPath<EconomyConfigSO>(EconomyConfigPath);
        if (config != null)
        {
            EditorUtility.SetDirty(config);
            return config;
        }

        if (!AssetDatabase.IsValidFolder("Assets/SO/Economy"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/SO"))
                AssetDatabase.CreateFolder("Assets", "SO");

            AssetDatabase.CreateFolder("Assets/SO", "Economy");
        }

        config = ScriptableObject.CreateInstance<EconomyConfigSO>();
        AssetDatabase.CreateAsset(config, EconomyConfigPath);
        AssetDatabase.SaveAssets();
        return config;
    }

    private static void EnsureGameSystems(EconomyConfigSO economyConfig)
    {
        GameObject systems = GameObject.Find("GameSystems") ?? new GameObject("GameSystems");

        EconomyManager economy = GetOrAdd<EconomyManager>(systems);
        SerializedObject economySo = new SerializedObject(economy);
        economySo.FindProperty("config").objectReferenceValue = economyConfig;
        economySo.ApplyModifiedPropertiesWithoutUndo();

        LeakTracker leakTracker = GetOrAdd<LeakTracker>(systems);
        SerializedObject leakSo = new SerializedObject(leakTracker);
        leakSo.FindProperty("config").objectReferenceValue = economyConfig;
        leakSo.ApplyModifiedPropertiesWithoutUndo();

        GetOrAdd<GameSpeedController>(systems);
        GetOrAdd<CombatActiveAbilityController>(systems);

        FieldEnemyTracker fieldEnemyTracker = GetOrAdd<FieldEnemyTracker>(systems);
        SerializedObject fieldSo = new SerializedObject(fieldEnemyTracker);
        fieldSo.FindProperty("config").objectReferenceValue = economyConfig;
        fieldSo.ApplyModifiedPropertiesWithoutUndo();

        GameInputRouter inputRouter = GetOrAdd<GameInputRouter>(systems);
        GetOrAdd<GameOverController>(systems);

        ArchetypeUpgradeManager archetypeUpgrade = GetOrAdd<ArchetypeUpgradeManager>(systems);
        SerializedObject archetypeSo = new SerializedObject(archetypeUpgrade);
        archetypeSo.FindProperty("config").objectReferenceValue = economyConfig;
        archetypeSo.ApplyModifiedPropertiesWithoutUndo();
        InputSO inputSo = AssetDatabase.LoadAssetAtPath<InputSO>("Assets/SO/InputSO.asset");
        if (inputSo != null)
        {
            SerializedObject inputRouterSo = new SerializedObject(inputRouter);
            inputRouterSo.FindProperty("inputSO").objectReferenceValue = inputSo;
            inputRouterSo.ApplyModifiedPropertiesWithoutUndo();
        }

        GameHudController hudController = GetOrAdd<GameHudController>(systems);
        TileManager tileManager = Object.FindFirstObjectByType<TileManager>();

        if (tileManager != null)
        {
            SerializedObject tileSo = new SerializedObject(tileManager);
            tileSo.FindProperty("economyConfig").objectReferenceValue = economyConfig;
            tileSo.ApplyModifiedPropertiesWithoutUndo();
        }

        SerializedObject hudSo = new SerializedObject(hudController);
        hudSo.FindProperty("economy").objectReferenceValue = economy;
        hudSo.FindProperty("tileManager").objectReferenceValue = tileManager;
        hudSo.FindProperty("fieldEnemyTracker").objectReferenceValue = fieldEnemyTracker;
        hudSo.FindProperty("speedController").objectReferenceValue = systems.GetComponent<GameSpeedController>();
        hudSo.FindProperty("archetypeUpgradeManager").objectReferenceValue = archetypeUpgrade;
        hudSo.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureEnemyEconomyBridge()
    {
        string[] enemyGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefab/Enemy" });
        foreach (string guid in enemyGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                continue;

            GameObject instance = PrefabUtility.LoadPrefabContents(path);
            if (instance.GetComponent<EnemyEconomyBridge>() == null)
                instance.AddComponent<EnemyEconomyBridge>();

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            PrefabUtility.UnloadPrefabContents(instance);
        }
    }

    private static Canvas EnsureHudCanvas()
    {
        DisableLegacyCanvas("PlayerUpGradeCanvas");

        GameObject playerCanvas = GameObject.Find("PlayerUiCanvas");
        if (playerCanvas != null)
            playerCanvas.SetActive(true);

        GameObject waveCanvas = GameObject.Find("WaveUiCanvas");
        if (waveCanvas != null)
            waveCanvas.SetActive(true);

        GameObject canvasGo = GameObject.Find("GameHudCanvas");
        if (canvasGo == null)
        {
            canvasGo = new GameObject("GameHudCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        }

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        EnsureEventSystem();
        return canvas;
    }

    private static void DisableLegacyCanvas(string canvasName)
    {
        GameObject legacy = GameObject.Find(canvasName);
        if (legacy != null)
            legacy.SetActive(false);
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            return;
        }

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null &&
            eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private static ArchetypeUpgradePanelUi BuildHudLayout(
        Canvas canvas,
        EconomyConfigSO economyConfig,
        SummonButtonUi summonButton,
        MoneyDisplayUi moneyDisplay,
        UnitCapacityUi unitCapacity,
        FieldEnemyCountUi fieldEnemyCount,
        WarningMessageUi warningMessage)
    {
        Transform root = canvas.transform;
        ClearChildren(root);

        Color panelColor = new Color(0.95f, 0.95f, 0.95f, 0.98f);
        Color upgradeColor = new Color(0.3f, 0.69f, 0.31f, 1f);
        Color sellColor = new Color(0.75f, 0.75f, 0.75f, 1f);

        (SpeedControlUi speedControl, SettingsButtonUi settingsButton) = CreateTopRightToolbar(root);

        RectTransform rightPanel = CreatePanel(root, "RightPanel", panelColor,
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-300f, -260f), new Vector2(-20f, 260f));
        rightPanel.gameObject.SetActive(false);

        TowerInfoPanelUi towerInfoPanel = CreateTowerInfoPanel(rightPanel, sellColor);
        ArchetypeUpgradePanelUi archetypeUpgradePanel = CreateArchetypeUpgradePanel(root, upgradeColor);

        GameHudController hudController = Object.FindFirstObjectByType<GameHudController>();
        if (hudController != null)
        {
            SerializedObject hudSo = new SerializedObject(hudController);
            hudSo.FindProperty("moneyDisplay").objectReferenceValue = moneyDisplay;
            hudSo.FindProperty("summonButton").objectReferenceValue = summonButton;
            hudSo.FindProperty("unitCapacity").objectReferenceValue = unitCapacity;
            hudSo.FindProperty("fieldEnemyCount").objectReferenceValue = fieldEnemyCount;
            hudSo.FindProperty("speedControl").objectReferenceValue = speedControl;
            hudSo.FindProperty("towerInfoPanel").objectReferenceValue = towerInfoPanel;
            hudSo.FindProperty("archetypeUpgradePanel").objectReferenceValue = archetypeUpgradePanel;
            hudSo.ApplyModifiedPropertiesWithoutUndo();
        }

        return archetypeUpgradePanel;
    }

    private static (MoneyDisplayUi, UnitCapacityUi) EnsureBottomResourceBar(EconomyConfigSO economyConfig)
    {
        Transform playerSpawn = GameObject.Find("PlayerSpawn")?.transform;
        if (playerSpawn == null)
            return (null, null);

        Transform existing = playerSpawn.Find("BottomResourceBar");
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        Transform legacyButton = playerSpawn.Find("Button");
        RectTransform bar = CreatePanel(playerSpawn, "BottomResourceBar", new Color(0f, 0f, 0f, 0.5f),
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(20f, -114f), new Vector2(420f, -58f));

        if (legacyButton != null)
        {
            RectTransform legacyRect = legacyButton as RectTransform;
            bar.anchorMin = legacyRect.anchorMin;
            bar.anchorMax = legacyRect.anchorMax;
            bar.pivot = legacyRect.pivot;
            bar.anchoredPosition = legacyRect.anchoredPosition;
            bar.sizeDelta = legacyRect.sizeDelta;
            Object.DestroyImmediate(legacyButton.gameObject);
        }

        CreateLabel(bar, "CoinIcon", "●", 26, FontStyles.Bold,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(52f, 0f),
            new Color(1f, 0.84f, 0.2f, 1f));

        TextMeshProUGUI goldText = CreateLabel(bar, "GoldText", ": 0", 28, FontStyles.Bold,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(56f, 0f), new Vector2(196f, 0f));

        MoneyDisplayUi moneyDisplay = bar.gameObject.AddComponent<MoneyDisplayUi>();
        SerializedObject moneySo = new SerializedObject(moneyDisplay);
        moneySo.FindProperty("goldText").objectReferenceValue = goldText;
        moneySo.ApplyModifiedPropertiesWithoutUndo();

        CreateLabel(bar, "UnitIcon", "●", 22, FontStyles.Bold,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(210f, 0f), new Vector2(244f, 0f),
            Color.white);

        TextMeshProUGUI unitText = CreateLabel(bar, "UnitCountText", $"0 / {economyConfig.maxUnitCapacity}", 28, FontStyles.Bold,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(248f, 0f), new Vector2(-18f, 0f));

        UnitCapacityUi unitCapacity = bar.gameObject.AddComponent<UnitCapacityUi>();
        SerializedObject unitSo = new SerializedObject(unitCapacity);
        unitSo.FindProperty("capacityText").objectReferenceValue = unitText;
        unitSo.ApplyModifiedPropertiesWithoutUndo();

        return (moneyDisplay, unitCapacity);
    }

    private static FieldEnemyCountUi EnsureFieldEnemyCount(EconomyConfigSO economyConfig)
    {
        GameObject waveCanvas = GameObject.Find("WaveUiCanvas");
        if (waveCanvas == null)
            return null;

        Transform existing = waveCanvas.transform.Find("FieldEnemyCount");
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        RectTransform group = CreatePanel(waveCanvas.transform, "FieldEnemyCount", new Color(0.15f, 0.15f, 0.15f, 0.85f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-340f, -186f), new Vector2(-20f, -74f));

        GameObject icon = new GameObject("MobIcon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(group, false);
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        SetRect(iconRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, -16f), new Vector2(44f, 16f));
        icon.GetComponent<Image>().color = new Color(0.85f, 0.2f, 0.15f, 1f);

        GameObject barBg = new GameObject("MobBarBg", typeof(RectTransform), typeof(Image));
        barBg.transform.SetParent(group, false);
        RectTransform barBgRect = barBg.GetComponent<RectTransform>();
        SetRect(barBgRect, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(52f, -12f), new Vector2(-12f, 12f));
        barBg.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 1f);

        GameObject barFill = new GameObject("MobBarFill", typeof(RectTransform), typeof(Image));
        barFill.transform.SetParent(barBg.transform, false);
        SetStretch(barFill.GetComponent<RectTransform>());
        Image fillImage = barFill.GetComponent<Image>();
        fillImage.color = new Color(0.9f, 0.25f, 0.2f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 0f;

        TextMeshProUGUI countText = CreateLabel(group, "MobCountText", $"0 / {economyConfig.maxFieldEnemies}", 24, FontStyles.Bold,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(52f, -18f), new Vector2(-12f, 18f),
            TextAlignmentOptions.Center);

        FieldEnemyCountUi ui = group.gameObject.AddComponent<FieldEnemyCountUi>();
        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("countText").objectReferenceValue = countText;
        so.FindProperty("fillBar").objectReferenceValue = fillImage;
        so.ApplyModifiedPropertiesWithoutUndo();

        WaveTimerUi waveTimer = WaveTimerUi.EnsureUnderFieldEnemyCount();
        if (waveTimer != null)
            waveTimer.RefreshDisplay();

        return ui;
    }

    private static WarningMessageUi EnsureWarningMessage()
    {
        GameObject canvasGo = GameObject.Find("GameHudCanvas");
        if (canvasGo == null)
            return null;

        Transform existing = canvasGo.transform.Find("WarningMessage");
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        RectTransform panel = CreatePanel(canvasGo.transform, "WarningMessage", new Color(0.1f, 0.1f, 0.1f, 0.9f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-260f, -36f), new Vector2(260f, 36f));
        panel.gameObject.SetActive(false);

        TextMeshProUGUI messageText = CreateLabel(panel, "WarningText", "경고", 26, FontStyles.Bold,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);

        WarningMessageUi ui = panel.gameObject.AddComponent<WarningMessageUi>();
        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("panelRoot").objectReferenceValue = panel.gameObject;
        so.FindProperty("messageText").objectReferenceValue = messageText;
        so.ApplyModifiedPropertiesWithoutUndo();
        return ui;
    }

    private static void EnsureWaveUi()
    {
        WaveUi waveUi = Object.FindFirstObjectByType<WaveUi>(FindObjectsInactive.Include);
        if (waveUi != null)
            return;

        GameObject waveCanvas = GameObject.Find("WaveUiCanvas");
        if (waveCanvas == null)
            return;

        Transform canvasTransform = waveCanvas.transform;

        GameObject waveUiGo = new GameObject("WaveUi", typeof(RectTransform), typeof(WaveUi));
        waveUiGo.transform.SetParent(canvasTransform, false);
        RectTransform waveUiRect = waveUiGo.GetComponent<RectTransform>();
        SetStretch(waveUiRect);

        TextMeshProUGUI waveText = CreateLabel(waveUiGo.transform, "WavesText", "Wave 0", 36, FontStyles.Bold,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -50f), new Vector2(222f, 0f));

        TextMeshProUGUI waveTimeText = CreateLabel(waveUiGo.transform, "WaveTimeText", "00:00", 28, FontStyles.Normal,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-80f, -52f), new Vector2(80f, -12f),
            TextAlignmentOptions.Center);

        PopUpWave popup = Object.FindFirstObjectByType<PopUpWave>(FindObjectsInactive.Include);

        WaveUi ui = waveUiGo.GetComponent<WaveUi>();
        SerializedObject waveSo = new SerializedObject(ui);
        waveSo.FindProperty("waveText").objectReferenceValue = waveText;
        waveSo.FindProperty("waveTimeText").objectReferenceValue = waveTimeText;
        SerializedProperty popupProp = waveSo.FindProperty("PopupWave")
            ?? waveSo.FindProperty("<PopupWave>k__BackingField");
        if (popupProp != null)
            popupProp.objectReferenceValue = popup;
        waveSo.ApplyModifiedPropertiesWithoutUndo();

        WaveManager waveManager = Object.FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
        {
            SerializedObject managerSo = new SerializedObject(waveManager);
            managerSo.FindProperty("waveUi").objectReferenceValue = ui;
            managerSo.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static SummonButtonUi EnsureLegacySummonButton()
    {
        TileManager tileManager = Object.FindFirstObjectByType<TileManager>();
        Button spawnButton = null;
        TextMeshProUGUI labelText = null;

        Transform playerSpawn = GameObject.Find("PlayerSpawn")?.transform;
        if (playerSpawn != null)
        {
            foreach (Transform child in playerSpawn)
            {
                if (child.name != "Spawn")
                    continue;

                spawnButton = child.GetComponent<Button>();
                labelText = child.GetComponentInChildren<TextMeshProUGUI>();
                break;
            }
        }

        if (spawnButton == null)
        {
            foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (label == null || label.text != "소환")
                    continue;

                spawnButton = button;
                labelText = label;
                break;
            }
        }

        if (spawnButton == null)
            return null;

        SummonButtonUi summonUi = spawnButton.GetComponent<SummonButtonUi>();
        if (summonUi == null)
            summonUi = spawnButton.gameObject.AddComponent<SummonButtonUi>();

        Transform spawnParent = spawnButton.transform.parent;
        Transform existingCostGroup = spawnButton.transform.Find("SummonCostGroup");
        if (existingCostGroup == null && spawnParent != null)
            existingCostGroup = spawnParent.Find("SummonCostGroup");
        if (existingCostGroup != null)
            Object.DestroyImmediate(existingCostGroup.gameObject);

        TextMeshProUGUI costText = CreateLabel(spawnButton.transform, "SummonCostText", "● 40", 26, FontStyles.Bold,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-70f, 10f), new Vector2(70f, 40f),
            new Color(1f, 0.84f, 0.2f, 1f), TextAlignmentOptions.Center);

        for (int i = spawnButton.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(spawnButton.onClick, i);

        SerializedObject summonSo = new SerializedObject(summonUi);
        summonSo.FindProperty("button").objectReferenceValue = spawnButton;
        summonSo.FindProperty("labelText").objectReferenceValue = labelText;
        summonSo.FindProperty("costText").objectReferenceValue = costText;
        summonSo.FindProperty("tileManager").objectReferenceValue = tileManager;
        summonSo.ApplyModifiedPropertiesWithoutUndo();

        return summonUi;
    }

    private static MoneyDisplayUi CreateMoneyDisplay(Transform parent)
    {
        RectTransform group = CreatePanel(parent, "MoneyGroup", new Color(0f, 0f, 0f, 0.35f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(80f, -64f), new Vector2(280f, -16f));

        CreateLabel(group, "MoneyIcon", "G", 24, FontStyles.Bold,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(32f, 32f),
            new Color(1f, 0.84f, 0.2f, 1f));

        TextMeshProUGUI goldText = CreateLabel(group, "GoldText", "0", 30, FontStyles.Bold,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(56f, 0f), new Vector2(-12f, 0f),
            new Color(1f, 0.92f, 0.55f, 1f));

        MoneyDisplayUi display = group.gameObject.AddComponent<MoneyDisplayUi>();
        SerializedObject so = new SerializedObject(display);
        so.FindProperty("goldText").objectReferenceValue = goldText;
        so.ApplyModifiedPropertiesWithoutUndo();
        return display;
    }

    private static UnitCapacityUi CreateUnitCapacity(Transform parent, int maxCapacity)
    {
        RectTransform group = CreatePanel(parent, "UnitCapacityGroup", new Color(0f, 0f, 0f, 0.35f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-300f, -64f), new Vector2(-120f, -16f));

        CreateLabel(group, "UnitIcon", "유닛", 18, FontStyles.Normal,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(56f, 28f));

        TextMeshProUGUI text = CreateLabel(group, "UnitCountText", $"0 / {maxCapacity}", 24, FontStyles.Bold,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(72f, 0f), new Vector2(-12f, 0f));

        UnitCapacityUi ui = group.gameObject.AddComponent<UnitCapacityUi>();
        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("capacityText").objectReferenceValue = text;
        so.ApplyModifiedPropertiesWithoutUndo();
        return ui;
    }

    private static void WireGameOverController(GameOverUi gameOverUi)
    {
        GameOverController controller = Object.FindFirstObjectByType<GameOverController>();
        if (controller == null || gameOverUi == null)
            return;

        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("gameOverUi").objectReferenceValue = gameOverUi;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameOverUi EnsureGameOverUi()
    {
        GameObject canvasGo = GameObject.Find("GameHudCanvas");
        if (canvasGo == null)
            return null;

        Transform existing = canvasGo.transform.Find("GameOverPanel");
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        RectTransform panel = CreatePanel(canvasGo.transform, "GameOverPanel", new Color(0f, 0f, 0f, 0.82f),
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        panel.gameObject.SetActive(false);

        TextMeshProUGUI title = CreateLabel(panel, "GameOverTitle", "패배", 48, FontStyles.Bold,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-160f, 20f), new Vector2(160f, 80f),
            TextAlignmentOptions.Center);

        TextMeshProUGUI message = CreateLabel(panel, "GameOverMessage", "", 28, FontStyles.Normal,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-300f, -40f), new Vector2(300f, -10f),
            TextAlignmentOptions.Center);

        Button restartButton = CreateButton(panel, "RestartButton", "재시작",
            new Color(0.18f, 0.45f, 0.32f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-230f, -110f), new Vector2(-10f, -58f));

        Button titleButton = CreateButton(panel, "TitleButton", "타이틀로",
            new Color(0.28f, 0.3f, 0.36f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(10f, -110f), new Vector2(230f, -58f));

        GameOverUi ui = panel.gameObject.AddComponent<GameOverUi>();
        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("panelRoot").objectReferenceValue = panel.gameObject;
        so.FindProperty("titleText").objectReferenceValue = title;
        so.FindProperty("messageText").objectReferenceValue = message;
        so.FindProperty("restartButton").objectReferenceValue = restartButton;
        so.FindProperty("titleButton").objectReferenceValue = titleButton;
        so.ApplyModifiedPropertiesWithoutUndo();

        return ui;
    }

    private static (SpeedControlUi, SettingsButtonUi) CreateTopRightToolbar(Transform parent)
    {
        // 버튼 5개(1x·2x·3x·4x·5x) + 설정 버튼으로 툴바 너비 확장
        RectTransform toolbar = CreatePanel(parent, "TopRightToolbar", new Color(0f, 0f, 0f, 0.35f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-460f, -64f), new Vector2(-16f, -16f));

        RectTransform speedGroup = CreatePanel(toolbar, "SpeedGroup", new Color(0f, 0f, 0f, 0f),
            new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(4f, 4f), new Vector2(304f, -4f));

        Button normalButton = CreateButton(speedGroup, "SpeedNormalButton", "1x",
            new Color(0.25f, 0.3f, 0.38f, 1f),
            new Vector2(0f, 0f), new Vector2(0.2f, 1f), new Vector2(2f, 2f), new Vector2(-2f, -2f));

        Button fastButton = CreateButton(speedGroup, "SpeedFastButton", "2x",
            new Color(0.25f, 0.3f, 0.38f, 1f),
            new Vector2(0.2f, 0f), new Vector2(0.4f, 1f), new Vector2(2f, 2f), new Vector2(-2f, -2f));

        Button turboButton = CreateButton(speedGroup, "SpeedTurboButton", "3x",
            new Color(0.25f, 0.3f, 0.38f, 1f),
            new Vector2(0.4f, 0f), new Vector2(0.6f, 1f), new Vector2(2f, 2f), new Vector2(-2f, -2f));

        Button ultraButton = CreateButton(speedGroup, "SpeedUltraButton", "4x",
            new Color(0.25f, 0.3f, 0.38f, 1f),
            new Vector2(0.6f, 0f), new Vector2(0.8f, 1f), new Vector2(2f, 2f), new Vector2(-2f, -2f));

        Button hyperButton = CreateButton(speedGroup, "SpeedHyperButton", "5x",
            new Color(0.25f, 0.3f, 0.38f, 1f),
            new Vector2(0.8f, 0f), new Vector2(1f, 1f), new Vector2(2f, 2f), new Vector2(-2f, -2f));

        SpeedControlUi speedUi = speedGroup.gameObject.AddComponent<SpeedControlUi>();
        SerializedObject speedSo = new SerializedObject(speedUi);
        speedSo.FindProperty("normalButton").objectReferenceValue = normalButton;
        speedSo.FindProperty("fastButton").objectReferenceValue = fastButton;
        speedSo.FindProperty("turboButton").objectReferenceValue = turboButton;
        speedSo.FindProperty("ultraButton").objectReferenceValue = ultraButton;
        speedSo.FindProperty("hyperButton").objectReferenceValue = hyperButton;
        speedSo.FindProperty("normalLabel").objectReferenceValue = normalButton.GetComponentInChildren<TextMeshProUGUI>();
        speedSo.FindProperty("fastLabel").objectReferenceValue = fastButton.GetComponentInChildren<TextMeshProUGUI>();
        speedSo.FindProperty("turboLabel").objectReferenceValue = turboButton.GetComponentInChildren<TextMeshProUGUI>();
        speedSo.FindProperty("ultraLabel").objectReferenceValue = ultraButton.GetComponentInChildren<TextMeshProUGUI>();
        speedSo.FindProperty("hyperLabel").objectReferenceValue = hyperButton.GetComponentInChildren<TextMeshProUGUI>();
        speedSo.ApplyModifiedPropertiesWithoutUndo();

        Button settingsButton = CreateButton(toolbar, "SettingsButton", "설정",
            new Color(0.25f, 0.3f, 0.38f, 1f),
            new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-84f, 4f), new Vector2(-4f, -4f));

        SettingsPanelUi settingsPanel = EnsureSettingsPanel(parent);

        SettingsButtonUi settingsUi = settingsButton.gameObject.AddComponent<SettingsButtonUi>();
        SerializedObject settingsSo = new SerializedObject(settingsUi);
        settingsSo.FindProperty("button").objectReferenceValue = settingsButton;
        settingsSo.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        settingsSo.ApplyModifiedPropertiesWithoutUndo();

        return (speedUi, settingsUi);
    }

    private static SettingsPanelUi EnsureSettingsPanel(Transform parent)
    {
        Transform existing = parent.Find("SettingsPanel");
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        GameObject panelGo = new GameObject("SettingsPanel", typeof(RectTransform), typeof(SettingsPanelUi));
        panelGo.transform.SetParent(parent, false);
        RectTransform panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        SettingsPanelUi ui = panelGo.GetComponent<SettingsPanelUi>();
        SettingsPanelLayoutBuilder.Rebuild(ui, showReturnToTitle: true);
        ui.NotifyLayoutBuilt();
        panelGo.SetActive(true);
        return ui;
    }

    private static Slider CreateSlider(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        float initialValue)
    {
        RectTransform sliderRoot = CreatePanel(parent, name, new Color(0f, 0f, 0f, 0f),
            anchorMin, anchorMax, anchorMin, anchorMax, offsetMin, offsetMax);

        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(sliderRoot, false);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        background.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.28f, 1f);

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderRoot, false);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(8f, 8f);
        fillAreaRect.offsetMax = new Vector2(-8f, -8f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = new Color(0.35f, 0.62f, 0.45f, 1f);

        GameObject handleSlideArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleSlideArea.transform.SetParent(sliderRoot, false);
        RectTransform handleAreaRect = handleSlideArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(8f, 0f);
        handleAreaRect.offsetMax = new Vector2(-8f, 0f);

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(handleSlideArea.transform, false);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(18f, 0f);
        handle.GetComponent<Image>().color = new Color(0.92f, 0.94f, 0.98f, 1f);

        Slider slider = sliderRoot.gameObject.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = initialValue;
        return slider;
    }

    private static void EnsureArchetypeUpgradeButton(ArchetypeUpgradePanelUi upgradePanel)
    {
        Transform playerSpawn = GameObject.Find("PlayerSpawn")?.transform;
        if (playerSpawn == null)
            return;

        Button upgradeButton = null;
        foreach (Transform child in playerSpawn)
        {
            if (child.name != "EnChance")
                continue;

            upgradeButton = child.GetComponent<Button>();
            break;
        }

        if (upgradeButton == null)
            return;

        for (int i = upgradeButton.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(upgradeButton.onClick, i);

        ArchetypeUpgradeButtonUi buttonUi = upgradeButton.GetComponent<ArchetypeUpgradeButtonUi>();
        if (buttonUi == null)
            buttonUi = upgradeButton.gameObject.AddComponent<ArchetypeUpgradeButtonUi>();

        SerializedObject buttonSo = new SerializedObject(buttonUi);
        buttonSo.FindProperty("button").objectReferenceValue = upgradeButton;
        buttonSo.FindProperty("upgradePanel").objectReferenceValue = upgradePanel;
        buttonSo.ApplyModifiedPropertiesWithoutUndo();

        if (Application.isPlaying)
            return;

        RectTransform rect = upgradeButton.GetComponent<RectTransform>();
        RectTransform spawnRect = playerSpawn.Find("Spawn") as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = new Vector2(148f, 88f);
            rect.anchoredPosition = spawnRect != null
                ? spawnRect.anchoredPosition + new Vector2(-204f, 34f)
                : new Vector2(-224f, 54f);
        }

        Image bg = upgradeButton.GetComponent<Image>();
        if (bg != null)
        {
            bg.color = new Color(0.1f, 0.45f, 0.3f, 1f);
            bg.type = Image.Type.Sliced;
        }
    }

    private static TowerInfoPanelUi CreateTowerInfoPanel(Transform parent, Color sellColor)
    {
        GameObject panelRoot = new GameObject("TowerInfoPanel", typeof(RectTransform), typeof(Image));
        panelRoot.transform.SetParent(parent, false);
        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        SetStretch(panelRect);
        panelRoot.GetComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f, 0.98f);
        panelRoot.SetActive(false);

        GameObject portrait = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
        portrait.transform.SetParent(panelRoot.transform, false);
        RectTransform portraitRect = portrait.GetComponent<RectTransform>();
        SetRect(portraitRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-90f, -200f), new Vector2(90f, -20f));
        portrait.GetComponent<Image>().color = Color.black;

        TextMeshProUGUI portraitFallback = CreateLabel(portrait.transform, "PortraitLabel", "석궁", 28, FontStyles.Bold,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white, TextAlignmentOptions.Center);

        Image portraitImage = portrait.GetComponent<Image>();

        TextMeshProUGUI title = CreateLabel(panelRoot.transform, "TowerTitle", "타워 선택", 24, FontStyles.Bold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -236f), new Vector2(220f, -204f),
            Color.black);

        TextMeshProUGUI grade = CreateLabel(panelRoot.transform, "TowerGrade", "노말", 18, FontStyles.Italic,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -268f), new Vector2(220f, -240f),
            new Color(0.3f, 0.35f, 0.45f, 1f));

        TextMeshProUGUI attack = CreateLabel(panelRoot.transform, "TowerAttack", "대미지: 0", 22, FontStyles.Normal,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -310f), new Vector2(220f, -278f),
            Color.black);

        Button sellButton = CreateButton(panelRoot.transform, "SellButton", "판매",
            sellColor,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-110f, 24f), new Vector2(110f, 72f));

        TileManager tileManager = Object.FindFirstObjectByType<TileManager>();

        TowerInfoPanelUi ui = panelRoot.AddComponent<TowerInfoPanelUi>();
        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("panelContainer").objectReferenceValue = parent.gameObject;
        so.FindProperty("panelRoot").objectReferenceValue = panelRoot;
        so.FindProperty("portraitImage").objectReferenceValue = portraitImage;
        so.FindProperty("portraitFallbackText").objectReferenceValue = portraitFallback;
        so.FindProperty("titleText").objectReferenceValue = title;
        so.FindProperty("gradeText").objectReferenceValue = grade;
        so.FindProperty("attackText").objectReferenceValue = attack;
        so.FindProperty("sellButton").objectReferenceValue = sellButton;
        so.FindProperty("sellButtonLabel").objectReferenceValue = sellButton.GetComponentInChildren<TextMeshProUGUI>();
        so.FindProperty("tileManager").objectReferenceValue = tileManager;
        so.ApplyModifiedPropertiesWithoutUndo();
        return ui;
    }

    private static ArchetypeUpgradePanelUi CreateArchetypeUpgradePanel(Transform parent, Color upgradeColor)
    {
        RectTransform panel = CreatePanel(parent, "ArchetypeUpgradePanel", new Color(0.12f, 0.12f, 0.12f, 0.92f),
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(20f, -380f), new Vector2(420f, 560f));
        panel.gameObject.SetActive(false);

        CreateLabel(panel, "ArchetypeUpgradeTitle", "강화", 26, FontStyles.Bold,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -56f), new Vector2(-20f, -16f),
            TextAlignmentOptions.Center);

        Button closeButton = CreateButton(panel, "ArchetypeUpgradeCloseButton", "닫기",
            new Color(0.25f, 0.3f, 0.38f, 1f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-60f, 16f), new Vector2(60f, 56f));

        ArchetypeUpgradeRowUi bowRow = CreateArchetypeUpgradeRow(panel, "BowRow", "석궁", upgradeColor,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -120f), new Vector2(-16f, -72f));
        bowRow.SetupArchetype(Tower.TowerArchetype.Bow, "석궁");

        ArchetypeUpgradeRowUi culverinRow = CreateArchetypeUpgradeRow(panel, "CulverinRow", "대포", upgradeColor,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -188f), new Vector2(-16f, -140f));
        culverinRow.SetupArchetype(Tower.TowerArchetype.Culverin, "대포");

        ArchetypeUpgradeRowUi missileRow = CreateArchetypeUpgradeRow(panel, "MissileRow", "미사일", upgradeColor,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -256f), new Vector2(-16f, -208f));
        missileRow.SetupArchetype(Tower.TowerArchetype.Missile, "미사일");

        ArchetypeUpgradeRowUi mergeNormalRow = CreateArchetypeUpgradeRow(panel, "MergeNormalRow", TowerGradeLabels.GetMergeChanceUpgradeLabel(TowerGrade.Normal), upgradeColor,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -324f), new Vector2(-16f, -276f));
        mergeNormalRow.SetupMerge(Tower.TowerGrade.Normal, TowerGradeLabels.GetMergeChanceUpgradeLabel(TowerGrade.Normal));

        ArchetypeUpgradeRowUi mergeRareRow = CreateArchetypeUpgradeRow(panel, "MergeRareRow", TowerGradeLabels.GetMergeChanceUpgradeLabel(TowerGrade.Rare), upgradeColor,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -384f), new Vector2(-16f, -336f));
        mergeRareRow.SetupMerge(Tower.TowerGrade.Rare, TowerGradeLabels.GetMergeChanceUpgradeLabel(TowerGrade.Rare));

        ArchetypeUpgradeRowUi mergeEpicRow = CreateArchetypeUpgradeRow(panel, "MergeEpicRow", TowerGradeLabels.GetMergeChanceUpgradeLabel(TowerGrade.Epic), upgradeColor,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -444f), new Vector2(-16f, -396f));
        mergeEpicRow.SetupMerge(Tower.TowerGrade.Epic, TowerGradeLabels.GetMergeChanceUpgradeLabel(TowerGrade.Epic));

        ArchetypeUpgradeRowUi summonUpgradeRow = CreateArchetypeUpgradeRow(panel, "SummonUpgradeRow", "소환 확률", upgradeColor,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -504f), new Vector2(-16f, -456f));
        summonUpgradeRow.SetupSummon("소환 확률");

        ArchetypeUpgradePanelUi ui = panel.gameObject.AddComponent<ArchetypeUpgradePanelUi>();
        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("panelRoot").objectReferenceValue = panel.gameObject;
        so.FindProperty("closeButton").objectReferenceValue = closeButton;
        so.FindProperty("bowRow").objectReferenceValue = bowRow;
        so.FindProperty("culverinRow").objectReferenceValue = culverinRow;
        so.FindProperty("missileRow").objectReferenceValue = missileRow;
        so.FindProperty("mergeNormalRow").objectReferenceValue = mergeNormalRow;
        so.FindProperty("mergeRareRow").objectReferenceValue = mergeRareRow;
        so.FindProperty("mergeEpicRow").objectReferenceValue = mergeEpicRow;
        so.FindProperty("summonUpgradeRow").objectReferenceValue = summonUpgradeRow;
        so.ApplyModifiedPropertiesWithoutUndo();
        return ui;
    }

    private static ArchetypeUpgradeRowUi CreateArchetypeUpgradeRow(Transform parent, string name, string label, Color upgradeColor,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform row = CreatePanel(parent, name, new Color(0.2f, 0.2f, 0.2f, 0.85f),
            anchorMin, anchorMax, new Vector2(0.5f, 0.5f), Vector2.zero, offsetMin, offsetMax);

        TextMeshProUGUI title = CreateLabel(row, "Title", label, 15, FontStyles.Bold,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, -16f), new Vector2(88f, 16f));

        TextMeshProUGUI level = CreateLabel(row, "LevelText", "Lv.0", 15, FontStyles.Normal,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(92f, -16f), new Vector2(148f, 16f));

        TextMeshProUGUI bonus = CreateLabel(row, "BonusText", "공격 +0", 15, FontStyles.Normal,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(152f, -16f), new Vector2(228f, 16f));

        TextMeshProUGUI cost = CreateLabel(row, "CostText", "30", 15, FontStyles.Bold,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(232f, -16f), new Vector2(300f, 16f),
            new Color(1f, 0.84f, 0.2f, 1f), TextAlignmentOptions.Right);

        Button upgradeButton = CreateButton(row, "UpgradeButton", "강화",
            upgradeColor,
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-84f, -18f), new Vector2(-8f, 18f));

        ArchetypeUpgradeRowUi ui = row.gameObject.AddComponent<ArchetypeUpgradeRowUi>();
        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("titleText").objectReferenceValue = title;
        so.FindProperty("levelText").objectReferenceValue = level;
        so.FindProperty("bonusText").objectReferenceValue = bonus;
        so.FindProperty("costText").objectReferenceValue = cost;
        so.FindProperty("upgradeButton").objectReferenceValue = upgradeButton;
        so.FindProperty("upgradeButtonLabel").objectReferenceValue = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();
        so.ApplyModifiedPropertiesWithoutUndo();
        return ui;
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }

    private static RectTransform CreatePanel(Transform parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchorPosition,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        RectTransform rect = go.GetComponent<RectTransform>();
        SetRect(rect, anchorMin, anchorMax, offsetMin, offsetMax, pivot);
        rect.anchoredPosition = anchorPosition;
        return rect;
    }

    private static Button CreateButton(Transform parent, string name, string label, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        RectTransform rect = go.GetComponent<RectTransform>();
        SetRect(rect, anchorMin, anchorMax, offsetMin, offsetMax);

        TextMeshProUGUI text = CreateLabel(go.transform, "Label", label, 24, FontStyles.Bold,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);

        return go.GetComponent<Button>();
    }

    private static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, int fontSize, FontStyles style,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
        Color? color = null, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
    {
        return CreateLabelInternal(parent, name, text, fontSize, style, anchorMin, anchorMax, offsetMin, offsetMax, alignment, color);
    }

    private static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, int fontSize, FontStyles style,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
        TextAlignmentOptions alignment)
    {
        return CreateLabelInternal(parent, name, text, fontSize, style, anchorMin, anchorMax, offsetMin, offsetMax, alignment, null);
    }

    private static TextMeshProUGUI CreateLabelInternal(Transform parent, string name, string text, int fontSize, FontStyles style,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
        TextAlignmentOptions alignment, Color? color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        SetRect(rect, anchorMin, anchorMax, offsetMin, offsetMax);

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = alignment;
        tmp.color = color ?? Color.white;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;

        if (_nexonFont != null)
            tmp.font = _nexonFont;

        return tmp;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
        Vector2? pivot = null)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static void SetStretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
#endif
