using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class GameHudVisualPolish
{
    public static void ApplyGameplayStyle()
    {
        CleanupLeakResourceBar();
        ApplyStatusPanels();
        ApplyResourceBar();
        ApplySummonAndUpgradeButtons();
        ApplyTopToolbar();
        ApplyWaveLabels();
        ApplyWaveTimer();
        ApplyWarningToast();
    }

    private static void CleanupLeakResourceBar()
    {
        Transform resourceBar = GameObject.Find("PlayerSpawn")?.transform?.Find("BottomResourceBar");
        if (resourceBar == null)
            return;

        foreach (string childName in new[] { "LeakIcon", "LeakCountText" })
        {
            Transform child = resourceBar.Find(childName);
            if (child != null)
                Object.Destroy(child.gameObject);
        }
    }

    private static void RemoveLegacyLeakUi()
    {
        foreach (string panelName in new[] { "LeakCount", "LeakGroup" })
        {
            GameObject waveCanvas = GameHudCanvasHelper.FindCanvas("WaveUiCanvas");
            Transform leakPanel = waveCanvas != null ? waveCanvas.transform.Find(panelName) : null;
            if (leakPanel != null)
                Object.Destroy(leakPanel.gameObject);
        }
    }

    private static void ApplyStatusPanels()
    {
        FieldEnemyCountUi fieldUi = Object.FindFirstObjectByType<FieldEnemyCountUi>(FindObjectsInactive.Include);
        if (fieldUi == null)
            return;

        StyleFieldEnemyPanel(fieldUi.transform);
        fieldUi.ApplyHpBarLayout();
    }

    private static void StyleFieldEnemyPanel(Transform panel)
    {
        if (panel is RectTransform rect)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-20f, -74f);
            rect.sizeDelta = new Vector2(320f, 112f);
        }

        GameHudTheme.StylePanel(panel.GetComponent<Image>());
        GameHudTheme.EnsureAccentBar(panel);
        GameHudTheme.EnsureHeaderLabel(panel, "StatusTitle", "필드 적");
    }

    private static void ApplyResourceBar()
    {
        Transform resourceBar = GameObject.Find("PlayerSpawn")?.transform?.Find("BottomResourceBar");
        if (resourceBar == null)
            return;

        GameHudTheme.StylePanel(resourceBar.GetComponent<Image>());
        GameHudTheme.EnsureAccentBar(resourceBar);
        EnsureDivider(resourceBar, "ResourceDivider", 200f);

        TextMeshProUGUI coinIcon = resourceBar.Find("CoinIcon")?.GetComponent<TextMeshProUGUI>();
        if (coinIcon != null)
        {
            coinIcon.text = "G";
            GameHudTheme.StyleLabel(coinIcon, 24f, FontStyles.Bold);
            coinIcon.color = GameHudTheme.GoldText;
        }

        TextMeshProUGUI goldText = resourceBar.Find("GoldText")?.GetComponent<TextMeshProUGUI>();
        if (goldText != null)
        {
            GameHudTheme.StyleLabel(goldText, 26f, FontStyles.Bold);
            goldText.color = GameHudTheme.GoldText;
        }

        TextMeshProUGUI unitIcon = resourceBar.Find("UnitIcon")?.GetComponent<TextMeshProUGUI>();
        if (unitIcon != null)
        {
            unitIcon.text = "유닛";
            GameHudTheme.StyleLabel(unitIcon, 18f);
            unitIcon.color = GameHudTheme.MutedText;
        }

        TextMeshProUGUI unitText = resourceBar.Find("UnitCountText")?.GetComponent<TextMeshProUGUI>();
        if (unitText != null)
        {
            GameHudTheme.StyleLabel(unitText, 24f, FontStyles.Bold);
            unitText.color = GameHudTheme.BodyText;
        }
    }

    private static void ApplySummonAndUpgradeButtons()
    {
        Transform playerSpawn = GameObject.Find("PlayerSpawn")?.transform;
        if (playerSpawn == null)
            return;

        StyleActionButton(playerSpawn.Find("Spawn"), accentOnTop: true);
        StyleActionButton(playerSpawn.Find("EnChance"), accentOnTop: false);
        StyleSummonProbabilityPanel(playerSpawn.Find("SummonProbabilityPanel"));
        StyleCombatActiveAbilityPanel(playerSpawn.Find("CombatActivePanel"));
    }

    private static void StyleCombatActiveAbilityPanel(Transform panel)
    {
        if (panel == null)
            return;

        foreach (Button button in panel.GetComponentsInChildren<Button>(true))
        {
            Image image = button.GetComponent<Image>();
            if (image != null)
                image.color = GameHudTheme.PanelBackground;

            foreach (TextMeshProUGUI label in button.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (label.name == "CooldownText")
                {
                    GameHudTheme.StyleLabel(label, 22f, FontStyles.Bold);
                    continue;
                }

                GameHudTheme.StyleLabel(label, 17f, FontStyles.Bold);
            }
        }
    }

    private static void StyleSummonProbabilityPanel(Transform panel)
    {
        if (panel == null)
            return;

        Image background = panel.GetComponent<Image>();
        if (background != null)
            background.color = new Color(0.08f, 0.10f, 0.14f, 0.92f);

        Outline outline = panel.GetComponent<Outline>();
        if (outline == null)
            outline = panel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.65f);
        outline.effectDistance = new Vector2(1f, -1f);

        TextMeshProUGUI probabilityText = panel.Find("SummonProbabilityText")?.GetComponent<TextMeshProUGUI>();
        if (probabilityText != null)
        {
            GameHudTheme.StyleLabel(probabilityText, 17f);
            probabilityText.color = new Color(0.82f, 0.88f, 0.95f, 1f);
        }
    }

    private static void StyleActionButton(Transform buttonRoot, bool accentOnTop)
    {
        if (buttonRoot == null)
            return;

        Transform legacyAccent = buttonRoot.Find("SummonAccentBar");
        if (legacyAccent != null)
            legacyAccent.gameObject.SetActive(false);

        Image bgImage = buttonRoot.GetComponent<Image>();
        if (bgImage != null)
            bgImage.color = GameHudTheme.PanelBackground;

        Outline outline = buttonRoot.GetComponent<Outline>();
        if (outline == null)
            outline = buttonRoot.gameObject.AddComponent<Outline>();
        outline.effectColor = GameHudTheme.PanelOutline;
        outline.effectDistance = new Vector2(2f, -2f);

        Shadow shadow = buttonRoot.GetComponent<Shadow>();
        if (shadow == null)
            shadow = buttonRoot.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
        shadow.effectDistance = new Vector2(0f, -4f);

        EnsureButtonAccent(buttonRoot, accentOnTop);

        foreach (TextMeshProUGUI label in buttonRoot.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            GameHudTheme.StyleLabel(label, label.name == "SummonCostText" ? 26f : 22f, FontStyles.Bold);
            label.color = label.name == "SummonCostText" ? GameHudTheme.GoldText : GameHudTheme.BodyText;
        }
    }

    private static void EnsureButtonAccent(Transform buttonRoot, bool accentOnTop)
    {
        const string accentName = "ThemeAccentBar";
        Transform accent = buttonRoot.Find(accentName);
        if (accent == null)
        {
            GameObject accentGo = new GameObject(accentName, typeof(RectTransform), typeof(Image));
            accentGo.transform.SetParent(buttonRoot, false);
            accentGo.transform.SetAsFirstSibling();
            accent = accentGo.transform;
        }

        RectTransform accentRect = accent as RectTransform;
        if (accentRect != null)
        {
            if (accentOnTop)
            {
                accentRect.anchorMin = new Vector2(0.08f, 1f);
                accentRect.anchorMax = new Vector2(0.92f, 1f);
                accentRect.pivot = new Vector2(0.5f, 1f);
                accentRect.anchoredPosition = new Vector2(0f, -4f);
                accentRect.sizeDelta = new Vector2(0f, 5f);
            }
            else
            {
                accentRect.anchorMin = new Vector2(0f, 0f);
                accentRect.anchorMax = new Vector2(0f, 1f);
                accentRect.pivot = new Vector2(0f, 0.5f);
                accentRect.anchoredPosition = Vector2.zero;
                accentRect.sizeDelta = new Vector2(5f, -10f);
            }
        }

        accent.GetComponent<Image>().color = GameHudTheme.AccentGold;
    }

    private static void EnsureDivider(Transform parent, string name, float xPosition)
    {
        if (parent.Find(name) != null)
            return;

        GameObject dividerGo = new GameObject(name, typeof(RectTransform), typeof(Image));
        dividerGo.transform.SetParent(parent, false);
        RectTransform rect = dividerGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.15f);
        rect.anchorMax = new Vector2(0f, 0.85f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(xPosition, 0f);
        rect.sizeDelta = new Vector2(2f, 0f);
        dividerGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
    }

    private static void ApplyTopToolbar()
    {
        Transform toolbar = GameObject.Find("GameHudCanvas")?.transform?.Find("TopRightToolbar");
        if (toolbar == null)
            return;

        GameHudTheme.StylePanel(toolbar.GetComponent<Image>());

        foreach (Button button in toolbar.GetComponentsInChildren<Button>(true))
            StyleToolbarButton(button);

        TextMeshProUGUI settingsLabel = toolbar.Find("SettingsButton")?.GetComponentInChildren<TextMeshProUGUI>(true);
        if (settingsLabel != null)
        {
            GameHudTheme.StyleLabel(settingsLabel, 20f, FontStyles.Bold);
            settingsLabel.color = GameHudTheme.BodyText;
        }
    }

    private static void StyleToolbarButton(Button button)
    {
        if (button == null)
            return;

        Image image = button.GetComponent<Image>();
        if (image != null)
            image.color = GameHudTheme.ButtonNormal;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = GameHudTheme.ButtonHighlight;
        colors.pressedColor = new Color(0.10f, 0.12f, 0.18f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            GameHudTheme.StyleLabel(label, 20f, FontStyles.Bold);
            label.color = GameHudTheme.BodyText;
        }
    }

    private static void ApplyWaveLabels()
    {
        foreach (WaveUi waveUi in Object.FindObjectsByType<WaveUi>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            foreach (TextMeshProUGUI label in waveUi.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                GameHudTheme.StyleLabel(label, label.fontSize >= 30f ? 34f : 24f, FontStyles.Bold);
                label.color = label.name.Contains("Time") ? GameHudTheme.MutedText : GameHudTheme.BodyText;
            }
        }
    }

    private static void ApplyWaveTimer()
    {
        TextMeshProUGUI timer = Object.FindFirstObjectByType<WaveTimerUi>(FindObjectsInactive.Include)?.GetComponentInChildren<TextMeshProUGUI>(true);
        if (timer == null)
            return;

        GameHudTheme.StyleLabel(timer, 18f, FontStyles.Bold);
        timer.color = GameHudTheme.MutedText;
    }

    private static void ApplyWarningToast()
    {
        WarningMessageUi warning = Object.FindFirstObjectByType<WarningMessageUi>(FindObjectsInactive.Include);
        if (warning == null)
            return;

        Transform panel = warning.transform;
        GameHudTheme.StylePanel(panel.GetComponent<Image>());

        TextMeshProUGUI text = panel.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null)
        {
            GameHudTheme.StyleLabel(text, 22f);
            text.color = GameHudTheme.GoldText;
        }
    }
}
