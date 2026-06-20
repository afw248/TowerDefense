using TMPro;
using Tower;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ???? ????? HUD Rect/TMP ?????? ?????? ????????.
/// </summary>
public class GameHudLayoutBootstrap : MonoBehaviour
{
    private const float CornerMargin = 20f;
    private const float SpawnButtonWidth = 196f;
    private const float SpawnButtonHeight = 156f;
    private const float UpgradeButtonWidth = 148f;
    private const float UpgradeButtonHeight = 88f;
    private const float SummonProbabilityPanelHeight = 42f;
    private const float CombatAbilityButtonWidth = 96f;
    private const float CombatAbilityButtonHeight = 88f;
    private const float ResourceBarWidth = 400f;
    private const float ResourceBarHeight = 56f;
    private const float ResourceBarTopOffset = 104f;
    private const float UpgradePanelTitleFontSize = 26f;
    private const float UpgradeRowFontSize = 15f;
    private const float UpgradeButtonFontSize = 14f;

    [SerializeField] private ArchetypeUpgradePanelUi archetypeUpgradePanel;
    [SerializeField] private TowerInfoPanelUi towerInfoPanel;

    private static bool _heavyHudLayoutApplied;

    private void Awake()
    {
        GameHudCanvasHelper.EnsureCanvasScales();

        if (TitlePreviewMode.Active)
            return;

        EnsureReady();
    }

    public void EnsureReady()
    {
        GameHudSettings.LoadSavedTheme();
        GameHudCanvasHelper.EnsureCanvasScales();

        if (GetComponent<UiInputBootstrap>() == null)
            gameObject.AddComponent<UiInputBootstrap>();

        UiInputBootstrap.EnsureUiInputModule();

        archetypeUpgradePanel ??= FindFirstObjectByType<ArchetypeUpgradePanelUi>(FindObjectsInactive.Include);
        towerInfoPanel ??= FindFirstObjectByType<TowerInfoPanelUi>(FindObjectsInactive.Include);

        FixSettingsPanel();
        EnsureWarningMessage();
        EnsureTowerGradeReveal();
        EnsureWaveTimer();
        EnsureFieldEnemyCountLayout();
        EnsureWaveUiVisible();
        EnsureCombatActiveAbilitySystems();
        EnsureCombatActiveAbilityPanel();
        EnsureBossHealthBar();
        EnsureGameOverUi();
        EnsureTutorialSystems();
        SettingsPanelUi.HideAllPanels();
        archetypeUpgradePanel?.PrepareHidden();
        towerInfoPanel?.PrepareHidden();

        if (_heavyHudLayoutApplied)
        {
            ApplyFinalPresentation();
            return;
        }

        ApplyGameplayCameraSettings();
        FixCornerWidgetLayout();
        FixResourceBarLayout();
        FixPlayerSpawnLayout();
        FixUpgradeButtonLayout();
        FixTowerInfoPanel();
        FixArchetypeUpgradePanel();
        EnsureSpeedTurboButton();
        SpawnerVisualLayout.Apply();
        FixAllTextInHierarchy(transform);
        _heavyHudLayoutApplied = true;

        ApplyFinalPresentation();
    }

    public static void ApplyFinalPresentation()
    {
        GameHudSettings.UseSlateMinimalTheme();
        UiFonts.ApplyNexonToAllUiText();
        GameHudVisualPolish.ApplyGameplayStyle();
    }

    private void Start()
    {
        if (TitlePreviewMode.Active)
            return;

        StartCoroutine(ApplyPresentationNextFrame());
    }

    private System.Collections.IEnumerator ApplyPresentationNextFrame()
    {
        yield return null;
        ApplyFinalPresentation();
    }

    private static void EnsureTutorialSystems()
    {
        if (!GameSessionMode.IsTutorial)
            return;

        TutorialSessionBootstrap.EnsureExists();
        TutorialManager.EnsureExists();
    }

    private static void EnsureGameOverUi()
    {
        GameObject canvas = GameObject.Find("GameHudCanvas");
        if (canvas == null)
            return;

        GameOverUi ui = GameOverUi.EnsureExists(canvas.transform);
        if (ui == null)
            return;

        GameOverController controller = Object.FindFirstObjectByType<GameOverController>();
        controller?.BindGameOverUi(ui);
    }

    private void EnsureWaveTimer()
    {
        WaveTimerUi ui = WaveTimerUi.EnsureUnderFieldEnemyCount();
        if (ui != null)
            ui.RefreshDisplay();

        // 웨이브 스킵 버튼 제거됨
    }

    private static void EnsureFieldEnemyCountLayout()
    {
        foreach (FieldEnemyCountUi ui in Object.FindObjectsByType<FieldEnemyCountUi>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
            ui.ApplyHpBarLayout();
    }

    private static void EnsureWaveUiVisible()
    {
        GameObject waveCanvas = GameHudCanvasHelper.FindCanvas("WaveUiCanvas");
        if (waveCanvas != null)
            waveCanvas.SetActive(true);

        foreach (WaveUi waveUi in Object.FindObjectsByType<WaveUi>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            waveUi.gameObject.SetActive(true);
            waveUi.RepairWaveTextLayout();
        }

        foreach (PopUpWave popup in Object.FindObjectsByType<PopUpWave>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            popup.gameObject.SetActive(true);
    }

    private static void EnsureBossHealthBar()
    {
        BossHealthBarUi.EnsureAtBottomCenter();
    }

    private void EnsureWarningMessage()
    {
        GameObject canvas = GameObject.Find("GameHudCanvas");
        if (canvas == null)
            canvas = gameObject;

        WarningMessageUi.EnsureExists(canvas.transform);
    }

    private static void EnsureTowerGradeReveal()
    {
        GameObject canvas = GameObject.Find("GameHudCanvas");
        if (canvas == null)
            return;

        TowerGradeRevealUi.EnsureExists(canvas.transform);
    }

    public static void FixAllTextInHierarchy(Transform root)
    {
        if (root == null)
            return;

        foreach (TextMeshProUGUI tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            FixTextMeshPro(tmp);
    }

    public static void FixTextMeshPro(TextMeshProUGUI tmp)
    {
        if (tmp == null)
            return;

        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;

        RectTransform rect = tmp.rectTransform;
        if (rect == null)
            return;

        float width = rect.rect.width;
        if (width >= 8f)
            return;

        tmp.ForceMeshUpdate();
        float preferredWidth = tmp.preferredWidth;
        if (preferredWidth < 8f)
            preferredWidth = 48f;

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth + 8f);
    }

    private void FixTowerInfoPanel()
    {
        towerInfoPanel ??= FindFirstObjectByType<TowerInfoPanelUi>(FindObjectsInactive.Include);
        if (towerInfoPanel == null)
            return;

        towerInfoPanel.RepairLayout();
        towerInfoPanel.PrepareHidden();
    }

    private static void FixSettingsPanel()
    {
        GameObject canvas = GameObject.Find("GameHudCanvas");
        if (canvas == null)
            return;

        Transform panelTransform = canvas.transform.Find("SettingsPanel");
        if (panelTransform == null)
        {
            SettingsPanelUi.EnsureOnCanvas(canvas.transform, titlePanel: false);
            return;
        }

        SettingsPanelUi panel = panelTransform.GetComponent<SettingsPanelUi>();
        if (panel == null)
        {
            SettingsPanelUi.EnsureOnCanvas(canvas.transform, titlePanel: false);
            return;
        }

        if (!SettingsPanelUi.HasBuiltLayout(panelTransform))
            SettingsPanelLayoutBuilder.Rebuild(panel, showReturnToTitle: true);
        else
        {
            SettingsPanelUi.RefreshVolumeLabels(panelTransform);
            panel.PrepareHidden();
        }
    }

    private static void ApplyGameplayCameraSettings()
    {
        Camera camera = Camera.main;
        if (camera == null || !camera.orthographic)
            return;

        if (TitlePreviewMode.Active)
            return;

        Vector3 targetPosition = GameplayViewSettings.ResolveGameplayCameraPosition();
        Quaternion targetRotation = GameplayViewSettings.GameplayCameraRotation;
        float targetSize = GameplayViewSettings.OrthographicSize;

        bool sizeMatches = Mathf.Approximately(camera.orthographicSize, targetSize);
        bool positionMatches = Vector3.Distance(camera.transform.position, targetPosition) < 0.05f;
        bool rotationMatches = Quaternion.Angle(camera.transform.rotation, targetRotation) < 0.5f;
        if (!sizeMatches || !positionMatches || !rotationMatches)
        {
            camera.orthographicSize = targetSize;
            camera.transform.SetPositionAndRotation(targetPosition, targetRotation);
        }

        GameplayCameraShake shake = camera.GetComponent<GameplayCameraShake>();
        if (shake == null)
            shake = camera.gameObject.AddComponent<GameplayCameraShake>();

        shake.RecacheBasePose();
        GameplayCameraShake.PrepareForGameplay();
    }

    private static void FixResourceBarLayout()
    {
        Transform playerSpawn = GameObject.Find("PlayerSpawn")?.transform;
        Transform resourceBar = playerSpawn != null ? playerSpawn.Find("BottomResourceBar") : null;
        if (resourceBar == null)
            return;

        Image background = resourceBar.GetComponent<Image>();
        if (background != null)
            background.type = Image.Type.Sliced;

        SetResourceLabelRect(resourceBar.Find("CoinIcon") as RectTransform, 18f, 52f, 34f);
        SetResourceLabelRect(resourceBar.Find("GoldText") as RectTransform, 56f, 196f, 36f);
        SetResourceLabelRect(resourceBar.Find("UnitIcon") as RectTransform, 210f, 244f, 34f);

        RectTransform unitText = resourceBar.Find("UnitCountText") as RectTransform;
        if (unitText != null)
        {
            unitText.anchorMin = new Vector2(0f, 0.5f);
            unitText.anchorMax = new Vector2(1f, 0.5f);
            unitText.pivot = new Vector2(0.5f, 0.5f);
            unitText.offsetMin = new Vector2(248f, -18f);
            unitText.offsetMax = new Vector2(-18f, 18f);
        }

        foreach (Transform child in resourceBar)
        {
            TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
            if (tmp == null)
                continue;

            if (child.name == "CoinIcon")
                tmp.fontSize = 26;
            else if (child.name == "UnitIcon")
                tmp.fontSize = 22;
            else
                tmp.fontSize = 28;

            FixTextMeshPro(tmp);
        }
    }

    private static void SetResourceLabelRect(RectTransform rect, float left, float right, float height)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(left, 0f);
        rect.sizeDelta = new Vector2(Mathf.Max(32f, right - left), height);
    }

    private static void FixCornerWidgetLayout()
    {
        Transform playerSpawn = GameObject.Find("PlayerSpawn")?.transform;
        if (playerSpawn == null)
            return;

        RectTransform root = playerSpawn as RectTransform;
        if (root == null)
            return;

        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = Vector2.zero;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        Transform legacyButton = playerSpawn.Find("Button");
        if (legacyButton != null)
            Object.Destroy(legacyButton.gameObject);

        Transform resourceBar = playerSpawn.Find("BottomResourceBar");
        if (resourceBar is RectTransform resourceRect)
        {
            resourceRect.anchorMin = new Vector2(0f, 1f);
            resourceRect.anchorMax = new Vector2(0f, 1f);
            resourceRect.pivot = new Vector2(0f, 1f);
            resourceRect.anchoredPosition = new Vector2(CornerMargin, -ResourceBarTopOffset);
            resourceRect.sizeDelta = new Vector2(ResourceBarWidth, ResourceBarHeight);
        }

        Transform spawnButton = playerSpawn.Find("Spawn");
        if (spawnButton is RectTransform spawnRect)
        {
            spawnRect.anchorMin = new Vector2(1f, 0f);
            spawnRect.anchorMax = new Vector2(1f, 0f);
            spawnRect.pivot = new Vector2(1f, 0f);
            spawnRect.anchoredPosition = new Vector2(-CornerMargin, CornerMargin);
            spawnRect.sizeDelta = new Vector2(SpawnButtonWidth, SpawnButtonHeight);
        }
    }

    private static void FixPlayerSpawnLayout()
    {
        Transform playerSpawn = GameObject.Find("PlayerSpawn")?.transform;
        if (playerSpawn == null)
            return;

        Transform spawnButton = playerSpawn.Find("Spawn");
        Transform costGroup = playerSpawn.Find("SummonCostGroup");

        if (spawnButton != null)
        {
            Transform costLabel = spawnButton.Find("SummonCostText");
            if (costLabel == null && costGroup != null)
                costLabel = costGroup.Find("SummonCostText");

            if (costLabel != null)
            {
                RectTransform costRect = costLabel as RectTransform;
                costRect.SetParent(spawnButton, false);
                costRect.anchorMin = new Vector2(0.5f, 0f);
                costRect.anchorMax = new Vector2(0.5f, 0f);
                costRect.pivot = new Vector2(0.5f, 0f);
                costRect.anchoredPosition = new Vector2(0f, 10f);
                costRect.sizeDelta = new Vector2(140f, 30f);

                TextMeshProUGUI costText = costLabel.GetComponent<TextMeshProUGUI>();
                if (costText != null)
                {
                    costText.fontSize = 26;
                    costText.alignment = TextAlignmentOptions.Center;
                    FixTextMeshPro(costText);
                }
            }

            if (costGroup != null)
                Object.Destroy(costGroup.gameObject);

            foreach (Transform child in spawnButton)
            {
                if (child.name != "SummonCostText" && child.name.StartsWith("SummonCost"))
                    Object.Destroy(child.gameObject);
            }

            TextMeshProUGUI labelText = spawnButton.Find("Text (TMP)")?.GetComponent<TextMeshProUGUI>();
            if (labelText != null)
            {
                labelText.fontSize = 40;
                labelText.fontStyle = FontStyles.Bold;
                labelText.alignment = TextAlignmentOptions.Center;
                labelText.color = new Color(1f, 0.97f, 0.88f, 1f);
                RectTransform labelRect = labelText.rectTransform;
                labelRect.anchorMin = new Vector2(0f, 0.35f);
                labelRect.anchorMax = new Vector2(1f, 1f);
                labelRect.offsetMin = new Vector2(4f, 0f);
                labelRect.offsetMax = new Vector2(-4f, -6f);
                FixTextMeshPro(labelText);
            }

            StyleSummonButtonAppearance(spawnButton);
        }

        EnsureSummonProbabilityPanel(playerSpawn);
    }

    private static void EnsureSummonProbabilityPanel(Transform playerSpawn)
    {
        if (playerSpawn == null)
            return;

        float panelWidth = UpgradeButtonWidth + 8f + SpawnButtonWidth;
        float panelBottom = CornerMargin + SpawnButtonHeight + 8f;

        Transform panel = playerSpawn.Find("SummonProbabilityPanel");
        if (panel == null)
        {
            GameObject panelGo = new GameObject("SummonProbabilityPanel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(playerSpawn, false);
            panel = panelGo.transform;

            GameObject labelGo = new GameObject("SummonProbabilityText", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(panel, false);
            RectTransform labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10f, 4f);
            labelRect.offsetMax = new Vector2(-10f, -4f);
        }

        RectTransform panelRect = panel as RectTransform;
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(1f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(1f, 0f);
            panelRect.sizeDelta = new Vector2(panelWidth, SummonProbabilityPanelHeight);
            panelRect.anchoredPosition = new Vector2(-CornerMargin, panelBottom);
        }

        Image background = panel.GetComponent<Image>();
        if (background == null)
            background = panel.gameObject.AddComponent<Image>();
        background.color = new Color(0.08f, 0.10f, 0.14f, 0.92f);
        background.raycastTarget = false;

        Outline outline = panel.GetComponent<Outline>();
        if (outline == null)
            outline = panel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.65f);
        outline.effectDistance = new Vector2(1f, -1f);

        TextMeshProUGUI probabilityText = panel.Find("SummonProbabilityText")?.GetComponent<TextMeshProUGUI>();
        if (probabilityText != null)
        {
            probabilityText.fontSize = 17f;
            probabilityText.fontStyle = FontStyles.Normal;
            probabilityText.alignment = TextAlignmentOptions.Center;
            probabilityText.color = new Color(0.82f, 0.88f, 0.95f, 1f);
            probabilityText.textWrappingMode = TextWrappingModes.NoWrap;
            probabilityText.overflowMode = TextOverflowModes.Overflow;
            UiFonts.ApplyNexon(probabilityText);
            FixTextMeshPro(probabilityText);
        }

        Transform legacyOnButton = playerSpawn.Find("Spawn/SummonProbabilityText");
        if (legacyOnButton != null)
            Object.Destroy(legacyOnButton.gameObject);
    }

    private static void StyleSummonButtonAppearance(Transform spawnButton)
    {
        Shadow shadow = spawnButton.GetComponent<Shadow>();
        if (shadow == null)
            shadow = spawnButton.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
        shadow.effectDistance = new Vector2(0f, -4f);

        TextMeshProUGUI costText = spawnButton.Find("SummonCostText")?.GetComponent<TextMeshProUGUI>();
        if (costText != null)
        {
            costText.fontSize = 28;
            costText.alignment = TextAlignmentOptions.Center;
            FixTextMeshPro(costText);
        }
    }

    private static void FixUpgradeButtonLayout()
    {
        Transform playerSpawn = GameObject.Find("PlayerSpawn")?.transform;
        if (playerSpawn == null)
            return;

        Transform upgradeRoot = playerSpawn.Find("EnChance");
        if (upgradeRoot == null)
            return;

        RectTransform rect = upgradeRoot as RectTransform;
        RectTransform spawnRect = playerSpawn.Find("Spawn") as RectTransform;

        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.sizeDelta = new Vector2(UpgradeButtonWidth, UpgradeButtonHeight);
        rect.anchoredPosition = spawnRect != null
            ? spawnRect.anchoredPosition + new Vector2(-(SpawnButtonWidth + 8f), 34f)
            : new Vector2(-(CornerMargin + SpawnButtonWidth + 8f), CornerMargin + 34f);

        Image background = upgradeRoot.GetComponent<Image>();
        if (background != null)
            background.type = Image.Type.Sliced;

        Button button = upgradeRoot.GetComponent<Button>();
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.92f, 1f, 0.96f, 1f);
            colors.pressedColor = new Color(0.78f, 0.9f, 0.84f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
        }

        Shadow shadow = upgradeRoot.GetComponent<Shadow>();
        if (shadow == null)
            shadow = upgradeRoot.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
        shadow.effectDistance = new Vector2(0f, -3f);

        EnsureUpgradeButtonAccent(upgradeRoot);
        EnsureUpgradeButtonIcon(upgradeRoot);
        EnsureUpgradeButtonLabels(upgradeRoot);
    }

    private static void EnsureCombatActiveAbilitySystems()
    {
        if (TitlePreviewMode.Active)
            return;

        GameObject systems = GameObject.Find("GameSystems");
        if (systems == null)
            systems = new GameObject("GameSystems");

        if (systems.GetComponent<CombatActiveAbilityController>() == null)
            systems.AddComponent<CombatActiveAbilityController>();
    }

    private static void EnsureCombatActiveAbilityPanel()
    {
        if (TitlePreviewMode.Active)
            return;

        Transform playerSpawn = GameObject.Find("PlayerSpawn")?.transform;
        if (playerSpawn == null)
            return;

        float panelWidth = CombatAbilityButtonWidth * 2f + 8f;
        RectTransform spawnRect = playerSpawn.Find("Spawn") as RectTransform;

        Transform panel = playerSpawn.Find("CombatActivePanel");
        if (panel == null)
        {
            GameObject panelGo = new GameObject("CombatActivePanel", typeof(RectTransform), typeof(CombatActiveAbilityUi));
            panelGo.transform.SetParent(playerSpawn, false);
            panel = panelGo.transform;

            CreateCombatAbilityButton(panel, "FreezeButton", "시간 정지", 0f, 0.5f);
            CreateCombatAbilityButton(panel, "GlobalDamageButton", "전체 피해", 0.5f, 1f);
        }

        if (panel is RectTransform panelRect)
        {
            panelRect.anchorMin = new Vector2(1f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(1f, 0f);
            panelRect.sizeDelta = new Vector2(panelWidth, CombatAbilityButtonHeight);
            panelRect.anchoredPosition = spawnRect != null
                ? spawnRect.anchoredPosition + new Vector2(-(SpawnButtonWidth + 8f + UpgradeButtonWidth + 8f), 34f)
                : new Vector2(-(CornerMargin + SpawnButtonWidth + 8f + UpgradeButtonWidth + 8f), CornerMargin + 34f);
        }

        panel.GetComponent<CombatActiveAbilityUi>()?.EnsureWired();
        FixAbilityButtonRaycasts(panel);
    }

    private static void FixAbilityButtonRaycasts(Transform panel)
    {
        if (panel == null)
            return;

        foreach (Button button in panel.GetComponentsInChildren<Button>(true))
        {
            foreach (Graphic graphic in button.GetComponentsInChildren<Graphic>(true))
            {
                if (graphic.gameObject != button.gameObject)
                    graphic.raycastTarget = false;
            }
        }
    }

    private static void CreateCombatAbilityButton(
        Transform panel,
        string buttonName,
        string labelText,
        float anchorMinX,
        float anchorMaxX)
    {
        if (panel.Find(buttonName) != null)
            return;

        GameObject buttonGo = new GameObject(buttonName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(panel, false);

        RectTransform rect = buttonGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(anchorMinX, 0f);
        rect.anchorMax = new Vector2(anchorMaxX, 1f);
        rect.offsetMin = anchorMinX <= 0f ? Vector2.zero : new Vector2(4f, 0f);
        rect.offsetMax = anchorMaxX >= 1f ? Vector2.zero : new Vector2(-4f, 0f);

        Image background = buttonGo.GetComponent<Image>();
        background.color = GameHudTheme.PanelBackground;
        background.type = Image.Type.Sliced;

        Outline outline = buttonGo.AddComponent<Outline>();
        outline.effectColor = GameHudTheme.PanelOutline;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        Shadow shadow = buttonGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
        shadow.effectDistance = new Vector2(0f, -3f);

        GameObject accentGo = new GameObject("AccentBar", typeof(RectTransform), typeof(Image));
        accentGo.transform.SetParent(buttonGo.transform, false);
        accentGo.transform.SetAsFirstSibling();
        RectTransform accentRect = accentGo.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 1f);
        accentRect.anchorMax = new Vector2(1f, 1f);
        accentRect.pivot = new Vector2(0.5f, 1f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(-10f, 4f);
        accentGo.GetComponent<Image>().color = new Color(0.35f, 0.72f, 1f, 1f);
        accentGo.GetComponent<Image>().raycastTarget = false;

        GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(buttonGo.transform, false);
        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.22f);
        labelRect.anchorMax = new Vector2(1f, 0.92f);
        labelRect.offsetMin = new Vector2(6f, 0f);
        labelRect.offsetMax = new Vector2(-6f, 0f);
        TextMeshProUGUI label = labelGo.GetComponent<TextMeshProUGUI>();
        label.text = labelText;
        label.fontSize = 17f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = GameHudTheme.BodyText;
        label.raycastTarget = false;
        UiFonts.ApplyNexon(label);

        GameObject fillGo = new GameObject("CooldownFill", typeof(RectTransform), typeof(Image));
        fillGo.transform.SetParent(buttonGo.transform, false);
        RectTransform fillRect = fillGo.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = fillGo.GetComponent<Image>();
        fillImage.color = new Color(0f, 0f, 0f, 0.55f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Vertical;
        fillImage.fillOrigin = (int)Image.OriginVertical.Top;
        fillImage.fillAmount = 0f;
        fillImage.raycastTarget = false;

        GameObject cooldownGo = new GameObject("CooldownText", typeof(RectTransform), typeof(TextMeshProUGUI));
        cooldownGo.transform.SetParent(buttonGo.transform, false);
        RectTransform cooldownRect = cooldownGo.GetComponent<RectTransform>();
        cooldownRect.anchorMin = Vector2.zero;
        cooldownRect.anchorMax = Vector2.one;
        cooldownRect.offsetMin = Vector2.zero;
        cooldownRect.offsetMax = Vector2.zero;
        TextMeshProUGUI cooldownText = cooldownGo.GetComponent<TextMeshProUGUI>();
        cooldownText.fontSize = 22f;
        cooldownText.fontStyle = FontStyles.Bold;
        cooldownText.alignment = TextAlignmentOptions.Center;
        cooldownText.color = new Color(0.92f, 0.95f, 1f, 1f);
        cooldownText.gameObject.SetActive(false);
        cooldownText.raycastTarget = false;
        UiFonts.ApplyNexon(cooldownText);
    }

    private static void EnsureUpgradeButtonAccent(Transform buttonRoot)
    {
        if (buttonRoot.Find("AccentBar") != null)
            return;

        GameObject accentGo = new GameObject("AccentBar", typeof(RectTransform), typeof(Image));
        accentGo.transform.SetParent(buttonRoot, false);
        accentGo.transform.SetAsFirstSibling();

        RectTransform accentRect = accentGo.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(7f, -8f);

        Image accentImage = accentGo.GetComponent<Image>();
        accentImage.color = new Color(1f, 0.78f, 0.15f, 1f);
        accentImage.raycastTarget = false;
    }

    private static void EnsureUpgradeButtonIcon(Transform buttonRoot)
    {
        TextMeshProUGUI icon = buttonRoot.Find("Icon")?.GetComponent<TextMeshProUGUI>();
        if (icon == null)
        {
            GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(TextMeshProUGUI));
            iconGo.transform.SetParent(buttonRoot, false);
            RectTransform iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(24f, 2f);
            iconRect.sizeDelta = new Vector2(34f, 34f);
            icon = iconGo.GetComponent<TextMeshProUGUI>();
        }

        icon.text = "\u25b2";
        icon.fontSize = 24;
        icon.fontStyle = FontStyles.Bold;
        icon.alignment = TextAlignmentOptions.Center;
        icon.color = new Color(1f, 0.84f, 0.2f, 1f);
        icon.raycastTarget = false;
        FixTextMeshPro(icon);
    }

    private static void EnsureUpgradeButtonLabels(Transform buttonRoot)
    {
        TextMeshProUGUI title = buttonRoot.Find("Title")?.GetComponent<TextMeshProUGUI>();
        if (title == null)
        {
            GameObject titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(buttonRoot, false);
            RectTransform titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.5f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(44f, 2f);
            titleRect.offsetMax = new Vector2(-8f, -2f);
            title = titleGo.GetComponent<TextMeshProUGUI>();
        }

        title.text = "강화";
        title.fontSize = 22;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Left;
        title.color = Color.white;
        title.raycastTarget = false;
        FixTextMeshPro(title);

        TextMeshProUGUI subtitle = buttonRoot.Find("Subtitle")?.GetComponent<TextMeshProUGUI>();
        if (subtitle == null)
        {
            GameObject subtitleGo = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            subtitleGo.transform.SetParent(buttonRoot, false);
            RectTransform subtitleRect = subtitleGo.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0f, 0f);
            subtitleRect.anchorMax = new Vector2(1f, 0.5f);
            subtitleRect.offsetMin = new Vector2(44f, 2f);
            subtitleRect.offsetMax = new Vector2(-8f, -2f);
            subtitle = subtitleGo.GetComponent<TextMeshProUGUI>();
        }

        subtitle.gameObject.SetActive(false);
    }

    private void FixArchetypeUpgradePanel()
    {
        if (archetypeUpgradePanel == null)
            return;

        archetypeUpgradePanel.PrepareHidden();
        RepairUpgradePanelLayout(archetypeUpgradePanel);

        Image panelImage = archetypeUpgradePanel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.type = Image.Type.Sliced;
            panelImage.pixelsPerUnitMultiplier = 1f;
        }

        Transform panel = archetypeUpgradePanel.transform;

        RectTransform titleRect = panel.Find("ArchetypeUpgradeTitle") as RectTransform;
        if (titleRect != null)
        {
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0f, -36f);
            titleRect.sizeDelta = new Vector2(-40f, 40f);

            TextMeshProUGUI titleTmp = titleRect.GetComponent<TextMeshProUGUI>();
            if (titleTmp != null)
            {
                titleTmp.fontSize = UpgradePanelTitleFontSize;
                titleTmp.fontStyle = FontStyles.Bold;
                FixTextMeshPro(titleTmp);
            }
        }

        RectTransform closeRect = panel.Find("ArchetypeUpgradeCloseButton") as RectTransform;
        if (closeRect != null)
        {
            closeRect.anchorMin = new Vector2(0.5f, 0f);
            closeRect.anchorMax = new Vector2(0.5f, 0f);
            closeRect.pivot = new Vector2(0.5f, 0f);
            closeRect.anchoredPosition = new Vector2(0f, 16f);
            closeRect.sizeDelta = new Vector2(120f, 40f);

            TextMeshProUGUI closeLabel = closeRect.GetComponentInChildren<TextMeshProUGUI>(true);
            if (closeLabel != null)
            {
                closeLabel.fontSize = UpgradeRowFontSize;
                FixTextMeshPro(closeLabel);
            }
        }
    }

    public static void RepairUpgradePanelLayout(ArchetypeUpgradePanelUi panel)
    {
        if (panel == null)
            return;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0f, 0.5f);
            panelRect.anchorMax = new Vector2(0f, 0.5f);
            panelRect.pivot = new Vector2(0f, 0.5f);
            panelRect.anchoredPosition = new Vector2(20f, 0f);
            panelRect.sizeDelta = new Vector2(400f, 760f);
        }

        Transform root = panel.transform;
        UpdateSectionLabel(root, "ArchetypeUpgradeTitle", "강화");
        ApplyUpgradePanelTypography(root);

        LayoutArchetypeUpgradeRow(root.Find("BowRow"), -96f, "석궁", TowerArchetype.Bow);
        LayoutArchetypeUpgradeRow(root.Find("CulverinRow"), -156f, "대포", TowerArchetype.Culverin);
        LayoutArchetypeUpgradeRow(root.Find("MissileRow"), -216f, "미사일", TowerArchetype.Missile);
        LayoutMergeUpgradeRow(EnsureChildRow(root, "MergeNormalRow", "BowRow"), -276f, TowerGradeLabels.GetMergeChanceUpgradeLabel(TowerGrade.Normal), TowerGrade.Normal);
        LayoutMergeUpgradeRow(EnsureChildRow(root, "MergeRareRow", "BowRow"), -336f, TowerGradeLabels.GetMergeChanceUpgradeLabel(TowerGrade.Rare), TowerGrade.Rare);
        LayoutMergeUpgradeRow(EnsureChildRow(root, "MergeEpicRow", "BowRow"), -396f, TowerGradeLabels.GetMergeChanceUpgradeLabel(TowerGrade.Epic), TowerGrade.Epic);
        LayoutSummonUpgradeRow(EnsureChildRow(root, "SummonUpgradeRow", "BowRow"), -456f);
        HideUpgradeRow(root, "MergeUnlockRareRow");
        HideUpgradeRow(root, "MergeUnlockEpicRow");
        FixAllTextInHierarchy(root);
    }

    private static void HideUpgradeRow(Transform root, string rowName)
    {
        Transform row = root.Find(rowName);
        if (row != null)
            row.gameObject.SetActive(false);
    }

    private static void LayoutSummonUpgradeRow(Transform row, float topOffset)
    {
        if (row == null)
            return;

        LayoutUpgradeRowCommon(row, topOffset);
        row.GetComponent<ArchetypeUpgradeRowUi>()?.SetupSummon("소환 확률");
    }

    
    private static Transform EnsureChildRow(Transform root, string rowName, string templateName)
    {
        Transform row = root.Find(rowName);
        if (row != null)
            return row;

        Transform template = root.Find(templateName);
        if (template == null)
            return null;

        GameObject clone = Object.Instantiate(template.gameObject, root);
        clone.name = rowName;
        clone.SetActive(true);
        return clone.transform;
    }

    private static void UpdateSectionLabel(Transform root, string objectName, string text)
    {
        Transform labelTransform = root.Find(objectName);
        if (labelTransform == null)
            return;

        TextMeshProUGUI tmp = labelTransform.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = text;
    }

    private static void LayoutArchetypeUpgradeRow(Transform row, float topOffset, string label, TowerArchetype archetype)
    {
        if (row == null)
            return;

        LayoutUpgradeRowCommon(row, topOffset);
        row.GetComponent<ArchetypeUpgradeRowUi>()?.SetupArchetype(archetype, label);
    }

    private static void LayoutUpgradeRowCommon(Transform row, float topOffset)
    {
        RectTransform rowRect = row as RectTransform;
        if (rowRect != null)
        {
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.anchoredPosition = new Vector2(0f, topOffset);
            rowRect.sizeDelta = new Vector2(-32f, 52f);
        }

        row.gameObject.SetActive(true);
        SetLabelRect(row.Find("Title"), 6f, 132f);
        SetLabelRect(row.Find("LevelText"), 110f, 146f);
        SetLabelRect(row.Find("BonusText"), 150f, 210f);
        SetLabelRectRight(row.Find("CostText"), 72f, 60f);

        RectTransform buttonRect = row.Find("UpgradeButton") as RectTransform;
        if (buttonRect != null)
        {
            buttonRect.anchorMin = new Vector2(1f, 0.5f);
            buttonRect.anchorMax = new Vector2(1f, 0.5f);
            buttonRect.pivot = new Vector2(1f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(-6f, 0f);
            buttonRect.sizeDelta = new Vector2(60f, 36f);
        }

        TextMeshProUGUI costLabel = row.Find("CostText")?.GetComponent<TextMeshProUGUI>();
        if (costLabel != null)
        {
            costLabel.color = new Color(1f, 0.84f, 0.2f, 1f);
            costLabel.alignment = TextAlignmentOptions.Right;
            ApplyUpgradeRowTextStyle(costLabel, FontStyles.Bold);
        }

        foreach (Transform child in row)
        {
            TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
            if (tmp == null || tmp == costLabel)
                continue;

            FontStyles style = child.name == "Title" ? FontStyles.Bold : FontStyles.Normal;
            ApplyUpgradeRowTextStyle(tmp, style);
        }

        TextMeshProUGUI buttonLabel = row.Find("UpgradeButton")?.GetComponentInChildren<TextMeshProUGUI>(true);
        if (buttonLabel != null)
        {
            buttonLabel.fontSize = UpgradeButtonFontSize;
            buttonLabel.fontStyle = FontStyles.Bold;
            buttonLabel.enableAutoSizing = false;
            FixTextMeshPro(buttonLabel);
        }
    }

    private static void ApplyUpgradePanelTypography(Transform root)
    {
        TextMeshProUGUI title = root.Find("ArchetypeUpgradeTitle")?.GetComponent<TextMeshProUGUI>();
        if (title != null)
        {
            title.fontSize = UpgradePanelTitleFontSize;
            title.fontStyle = FontStyles.Bold;
            FixTextMeshPro(title);
        }

        TextMeshProUGUI closeLabel = root.Find("ArchetypeUpgradeCloseButton")?.GetComponentInChildren<TextMeshProUGUI>(true);
        if (closeLabel != null)
        {
            closeLabel.fontSize = UpgradeRowFontSize;
            FixTextMeshPro(closeLabel);
        }
    }

    private static void ApplyUpgradeRowTextStyle(TextMeshProUGUI tmp, FontStyles style)
    {
        tmp.fontSize = UpgradeRowFontSize;
        tmp.fontStyle = style;
        tmp.enableAutoSizing = false;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        FixTextMeshPro(tmp);
    }

    private static void LayoutMergeUpgradeRow(Transform row, float topOffset, string label, TowerGrade fromGrade)
    {
        if (row == null)
            return;

        LayoutUpgradeRowCommon(row, topOffset);
        row.GetComponent<ArchetypeUpgradeRowUi>()?.SetupMerge(fromGrade, label);
    }

    private static void SetLabelRect(Transform label, float left, float right)
    {
        if (label == null)
            return;

        RectTransform rect = label as RectTransform;
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(left, 0f);
        rect.sizeDelta = new Vector2(Mathf.Max(32f, right - left), 32f);
    }

    private static void SetLabelRectRight(Transform label, float rightOffset, float width)
    {
        if (label == null)
            return;

        RectTransform rect = label as RectTransform;
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = new Vector2(-rightOffset, 0f);
        rect.sizeDelta = new Vector2(Mathf.Max(32f, width), 32f);
    }

    private static void EnsureSpeedTurboButton()
    {
        SpeedControlUi speedUi = Object.FindFirstObjectByType<SpeedControlUi>(FindObjectsInactive.Include);
        if (speedUi == null)
            return;

        Transform speedGroup = speedUi.transform;
        Transform normalTr = speedGroup.Find("SpeedNormalButton");
        Transform fastTr = speedGroup.Find("SpeedFastButton");
        if (normalTr == null || fastTr == null)
            return;

        EnsureSpeedButton(speedGroup, speedUi, fastTr, "SpeedTurboButton", "3x", 0.4f, 0.6f, "turboButton", "turboLabel",
            () => GameSpeedController.Instance?.SetTurboSpeed());
        EnsureSpeedButton(speedGroup, speedUi, fastTr, "SpeedUltraButton", "4x", 0.6f, 0.8f, "ultraButton", "ultraLabel",
            () => GameSpeedController.Instance?.SetUltraSpeed());
        EnsureSpeedButton(speedGroup, speedUi, fastTr, "SpeedHyperButton", "5x", 0.8f, 1f, "hyperButton", "hyperLabel",
            () => GameSpeedController.Instance?.SetHyperSpeed());

        RectTransform groupRect = speedGroup.GetComponent<RectTransform>();
        if (groupRect != null)
            groupRect.offsetMax = new Vector2(304f, groupRect.offsetMax.y);

        Transform toolbar = speedGroup.parent;
        if (toolbar != null && toolbar.GetComponent<RectTransform>() is RectTransform toolbarRect)
            toolbarRect.offsetMin = new Vector2(-460f, toolbarRect.offsetMin.y);

        SetButtonAnchors(normalTr.GetComponent<RectTransform>(), 0f, 0.2f);
        SetButtonAnchors(fastTr.GetComponent<RectTransform>(), 0.2f, 0.4f);
    }

    private static void EnsureSpeedButton(
        Transform speedGroup,
        SpeedControlUi speedUi,
        Transform template,
        string buttonName,
        string labelText,
        float anchorMinX,
        float anchorMaxX,
        string buttonField,
        string labelField,
        UnityEngine.Events.UnityAction onClick)
    {
        if (speedGroup.Find(buttonName) != null)
            return;

        GameObject buttonGo = Object.Instantiate(template.gameObject, speedGroup);
        buttonGo.name = buttonName;
        SetButtonAnchors(buttonGo.GetComponent<RectTransform>(), anchorMinX, anchorMaxX);

        TextMeshProUGUI label = buttonGo.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
            label.text = labelText;

        Button button = buttonGo.GetComponent<Button>();
        var type = typeof(SpeedControlUi);
        SetField(type, speedUi, buttonField, button);
        SetField(type, speedUi, labelField, label);

        if (button != null)
            button.onClick.AddListener(onClick);
    }

    private static void SetButtonAnchors(RectTransform rect, float minX, float maxX)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(minX, rect.anchorMin.y);
        rect.anchorMax = new Vector2(maxX, rect.anchorMax.y);
    }

    private static void SetField(System.Type type, object obj, string fieldName, object value)
    {
        var field = type.GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(obj, value);
    }
}