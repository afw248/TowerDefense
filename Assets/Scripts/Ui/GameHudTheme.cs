using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class GameHudTheme
{
    private static GameHudThemePalette _palette = GameHudThemePresets.Get(GameHudThemeId.FantasyGold);

    public static GameHudThemeId ActiveId { get; private set; } = GameHudThemeId.FantasyGold;
    public static Color PanelBackground => _palette.PanelBackground;
    public static Color PanelOutline => _palette.PanelOutline;
    public static Color GoldText => _palette.GoldText;
    public static Color BodyText => _palette.BodyText;
    public static Color MutedText => _palette.MutedText;
    public static Color BarTrack => _palette.BarTrack;
    public static Color EnemyFill => _palette.EnemyFill;
    public static Color ButtonNormal => _palette.ButtonNormal;
    public static Color ButtonHighlight => _palette.ButtonHighlight;
    public static Color AccentGold => _palette.Accent;

    public static void SetActive(GameHudThemeId id)
    {
        ActiveId = id;
        _palette = GameHudThemePresets.Get(id);
    }

    public static void StylePanel(Image background, bool addOutline = true)
    {
        if (background == null)
            return;

        background.color = PanelBackground;
        background.raycastTarget = false;

        if (!addOutline)
            return;

        Outline outline = background.GetComponent<Outline>();
        if (outline == null)
            outline = background.gameObject.AddComponent<Outline>();
        outline.effectColor = PanelOutline;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        Shadow shadow = background.GetComponent<Shadow>();
        if (shadow == null)
            shadow = background.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
        shadow.effectDistance = new Vector2(0f, -3f);
    }

    public static void StyleLabel(TextMeshProUGUI label, float fontSize, FontStyles style = FontStyles.Bold)
    {
        if (label == null)
            return;

        UiFonts.ApplyNexon(label);
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.color = BodyText;
    }

    public static void EnsureAccentBar(Transform panel, string barName = "AccentBar")
    {
        if (panel == null || panel.Find(barName) != null)
            return;

        GameObject accentGo = new GameObject(barName, typeof(RectTransform), typeof(Image));
        accentGo.transform.SetParent(panel, false);
        accentGo.transform.SetAsFirstSibling();

        RectTransform accentRect = accentGo.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(5f, -10f);

        Image accentImage = accentGo.GetComponent<Image>();
        accentImage.color = AccentGold;
        accentImage.raycastTarget = false;
    }

    public static TextMeshProUGUI EnsureHeaderLabel(Transform panel, string labelName, string text)
    {
        Transform existing = panel.Find(labelName);
        TextMeshProUGUI label = existing != null ? existing.GetComponent<TextMeshProUGUI>() : null;
        if (label == null)
        {
            GameObject labelGo = new GameObject(labelName, typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(panel, false);
            RectTransform rect = labelGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(14f, -6f);
            rect.sizeDelta = new Vector2(72f, 22f);
            label = labelGo.GetComponent<TextMeshProUGUI>();
        }

        label.text = text;
        StyleLabel(label, 16f, FontStyles.Bold);
        label.color = MutedText;
        label.alignment = TextAlignmentOptions.Left;
        return label;
    }
}
