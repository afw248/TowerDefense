using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class SettingsPanelLayoutBuilder
{
    private static readonly Color OverlayColor = new(0f, 0f, 0f, 0.28f);
    private static readonly Color CardColor = new(0.11f, 0.13f, 0.17f, 0.97f);
    private static readonly Color RowLabelColor = new(0.86f, 0.89f, 0.94f, 1f);
    private static readonly Color PrimaryButtonColor = new(0.22f, 0.42f, 0.34f, 1f);
    private static readonly Color SecondaryButtonColor = new(0.24f, 0.27f, 0.33f, 1f);

    public struct BuiltLayout
    {
        public RectTransform Overlay;
        public RectTransform Card;
        public Slider MasterSlider;
        public Slider BgmSlider;
        public Slider SfxSlider;
        public TextMeshProUGUI ResolutionValueLabel;
        public Button ResolutionPreviousButton;
        public Button ResolutionNextButton;
        public TextMeshProUGUI FullScreenValueLabel;
        public Toggle FullScreenToggle;
        public Button CloseButton;
        public Button ReturnToTitleButton;
    }

    public static BuiltLayout Build(RectTransform host, bool showReturnToTitle)
    {
        ClearChildren(host);

        CanvasGroup hostGroup = host.GetComponent<CanvasGroup>();
        if (hostGroup == null)
            hostGroup = host.gameObject.AddComponent<CanvasGroup>();

        Image hostImage = host.GetComponent<Image>();
        if (hostImage == null)
            hostImage = host.gameObject.AddComponent<Image>();
        hostImage.color = OverlayColor;
        hostImage.raycastTarget = true;

        Stretch(host);

        RectTransform card = CreateRect("SettingsCard", host, CardColor);
        card.anchorMin = new Vector2(0.5f, 0.5f);
        card.anchorMax = new Vector2(0.5f, 0.5f);
        card.pivot = new Vector2(0.5f, 0.5f);
        card.sizeDelta = new Vector2(520f, 500f);

        CreateLabel(card, "SettingsTitle", "설정", 34, FontStyles.Bold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(-220f, -56f), new Vector2(220f, -8f), TextAlignmentOptions.Center);

        BuiltLayout layout = default;
        layout.Overlay = host;
        layout.Card = card;
        layout.MasterSlider = CreateVolumeRow(card, "MasterVolume", "전체", 0.64f, GameAudioSettings.MasterVolume);
        layout.BgmSlider = CreateVolumeRow(card, "BgmVolume", "BGM", 0.52f, GameAudioSettings.BgmVolume);
        layout.SfxSlider = CreateVolumeRow(card, "SfxVolume", "VFX", 0.40f, GameAudioSettings.VfxVolume);
        CreateResolutionRow(card, out layout.ResolutionValueLabel, out layout.ResolutionPreviousButton, out layout.ResolutionNextButton);
        CreateFullScreenRow(card, out layout.FullScreenValueLabel, out layout.FullScreenToggle);

        RectTransform footer = CreateRect("Footer", card, new Color(0f, 0f, 0f, 0f));
        footer.anchorMin = new Vector2(0f, 0f);
        footer.anchorMax = new Vector2(1f, 0f);
        footer.pivot = new Vector2(0.5f, 0f);
        footer.anchoredPosition = new Vector2(0f, 24f);
        footer.sizeDelta = new Vector2(-48f, 52f);

        HorizontalLayoutGroup footerLayout = footer.gameObject.AddComponent<HorizontalLayoutGroup>();
        footerLayout.spacing = 16f;
        footerLayout.childAlignment = TextAnchor.MiddleCenter;
        footerLayout.childControlWidth = true;
        footerLayout.childControlHeight = true;
        footerLayout.childForceExpandWidth = true;
        footerLayout.childForceExpandHeight = true;

        layout.CloseButton = CreateFooterButton(footer, "SettingsCloseButton", "닫기", SecondaryButtonColor);
        layout.ReturnToTitleButton = CreateFooterButton(footer, "ReturnToTitleButton", "타이틀로", PrimaryButtonColor);
        layout.ReturnToTitleButton.gameObject.SetActive(showReturnToTitle);

        return layout;
    }

    public static void Rebuild(SettingsPanelUi panel, bool showReturnToTitle)
    {
        if (panel == null)
            return;

        RectTransform host = panel.transform as RectTransform;
        if (host == null)
            return;

        BuiltLayout layout = Build(host, showReturnToTitle);

        SettingsVolumeUi volumeUi = panel.GetComponent<SettingsVolumeUi>();
        if (volumeUi == null)
            volumeUi = panel.gameObject.AddComponent<SettingsVolumeUi>();

        SettingsDisplayUi displayUi = panel.GetComponent<SettingsDisplayUi>();
        if (displayUi == null)
            displayUi = panel.gameObject.AddComponent<SettingsDisplayUi>();

        volumeUi.Bind(layout.MasterSlider, layout.BgmSlider, layout.SfxSlider, layout.ReturnToTitleButton);
        displayUi.Bind(
            layout.ResolutionValueLabel,
            layout.ResolutionPreviousButton,
            layout.ResolutionNextButton,
            layout.FullScreenValueLabel,
            layout.FullScreenToggle);
        panel.Configure(layout.Overlay.gameObject, layout.CloseButton, volumeUi, displayUi);
        panel.NotifyLayoutBuilt();
    }

    private static void CreateResolutionRow(
        RectTransform card,
        out TextMeshProUGUI valueLabel,
        out Button previousButton,
        out Button nextButton)
    {
        RectTransform row = CreateRect("ResolutionRow", card, new Color(0f, 0f, 0f, 0f));
        row.anchorMin = new Vector2(0f, 0.28f);
        row.anchorMax = new Vector2(1f, 0.28f);
        row.pivot = new Vector2(0.5f, 0.5f);
        row.anchoredPosition = Vector2.zero;
        row.sizeDelta = new Vector2(-56f, 44f);

        CreateLabel(row, "ResolutionLabel", "해상도", 24, FontStyles.Normal,
            new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(0f, 0f), new Vector2(96f, 0f), TextAlignmentOptions.MidlineLeft, RowLabelColor);

        RectTransform controls = CreateRect("ResolutionControls", row, new Color(0f, 0f, 0f, 0f));
        controls.anchorMin = new Vector2(0f, 0f);
        controls.anchorMax = new Vector2(1f, 1f);
        controls.offsetMin = new Vector2(108f, 0f);
        controls.offsetMax = Vector2.zero;

        HorizontalLayoutGroup layout = controls.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        previousButton = CreateResolutionCycleButton(controls, "ResolutionPreviousButton", "<");
        valueLabel = CreateResolutionValueLabel(controls, "ResolutionValueLabel");
        nextButton = CreateResolutionCycleButton(controls, "ResolutionNextButton", ">");
    }

    private static void CreateFullScreenRow(
        RectTransform card,
        out TextMeshProUGUI valueLabel,
        out Toggle toggle)
    {
        RectTransform row = CreateRect("FullScreenRow", card, new Color(0f, 0f, 0f, 0f));
        row.anchorMin = new Vector2(0f, 0.18f);
        row.anchorMax = new Vector2(1f, 0.18f);
        row.pivot = new Vector2(0.5f, 0.5f);
        row.anchoredPosition = Vector2.zero;
        row.sizeDelta = new Vector2(-56f, 44f);

        CreateLabel(row, "FullScreenLabel", "전체 화면", 24, FontStyles.Normal,
            new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(0f, 0f), new Vector2(128f, 0f), TextAlignmentOptions.MidlineLeft, RowLabelColor);

        RectTransform controls = CreateRect("FullScreenControls", row, new Color(0f, 0f, 0f, 0f));
        controls.anchorMin = new Vector2(0f, 0f);
        controls.anchorMax = new Vector2(1f, 1f);
        controls.offsetMin = new Vector2(140f, 0f);
        controls.offsetMax = Vector2.zero;

        HorizontalLayoutGroup layout = controls.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleRight;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        valueLabel = CreateFullScreenValueLabel(controls, "FullScreenValueLabel");
        toggle = CreateFullScreenToggle(controls);
    }

    private static Button CreateResolutionCycleButton(RectTransform parent, string name, string label)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);

        LayoutElement layout = go.GetComponent<LayoutElement>();
        layout.minWidth = 44f;
        layout.preferredWidth = 44f;
        layout.minHeight = 36f;
        layout.preferredHeight = 36f;

        Image image = go.GetComponent<Image>();
        image.color = SecondaryButtonColor;

        CreateLabel(go.transform, "Text", label, 24, FontStyles.Bold,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);

        return go.GetComponent<Button>();
    }

    private static TextMeshProUGUI CreateResolutionValueLabel(RectTransform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        go.transform.SetParent(parent, false);

        LayoutElement layout = go.GetComponent<LayoutElement>();
        layout.minWidth = 180f;
        layout.preferredWidth = 180f;
        layout.flexibleWidth = 1f;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = GameDisplaySettings.CurrentPreset.Label;
        tmp.fontSize = 22f;
        tmp.fontStyle = FontStyles.Normal;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = RowLabelColor;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static TextMeshProUGUI CreateFullScreenValueLabel(RectTransform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        go.transform.SetParent(parent, false);

        LayoutElement layout = go.GetComponent<LayoutElement>();
        layout.minWidth = 84f;
        layout.preferredWidth = 84f;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = GameDisplaySettings.IsFullScreen ? "켜짐" : "꺼짐";
        tmp.fontSize = 22f;
        tmp.fontStyle = FontStyles.Normal;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = RowLabelColor;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static Toggle CreateFullScreenToggle(RectTransform parent)
    {
        GameObject go = new GameObject("FullScreenToggle", typeof(RectTransform), typeof(Image), typeof(Toggle), typeof(LayoutElement));
        go.transform.SetParent(parent, false);

        LayoutElement layout = go.GetComponent<LayoutElement>();
        layout.minWidth = 68f;
        layout.preferredWidth = 68f;
        layout.minHeight = 36f;
        layout.preferredHeight = 36f;

        Image background = go.GetComponent<Image>();
        background.color = SecondaryButtonColor;
        background.raycastTarget = true;

        RectTransform checkRect = CreateRect("Checkmark", go.transform, PrimaryButtonColor);
        checkRect.anchorMin = new Vector2(0.5f, 0.5f);
        checkRect.anchorMax = new Vector2(0.5f, 0.5f);
        checkRect.pivot = new Vector2(0.5f, 0.5f);
        checkRect.anchoredPosition = Vector2.zero;
        checkRect.sizeDelta = new Vector2(48f, 22f);
        Image checkImage = checkRect.GetComponent<Image>();

        Toggle toggle = go.GetComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.graphic = checkImage;
        toggle.isOn = GameDisplaySettings.IsFullScreen;
        return toggle;
    }

    private static void ClearChildren(RectTransform host)
    {
        for (int i = host.childCount - 1; i >= 0; i--)
            Object.Destroy(host.GetChild(i).gameObject);
    }

    private static Slider CreateVolumeRow(RectTransform card, string id, string label, float anchorY, float value)
    {
        RectTransform row = CreateRect($"{id}Row", card, new Color(0f, 0f, 0f, 0f));
        row.anchorMin = new Vector2(0f, anchorY);
        row.anchorMax = new Vector2(1f, anchorY);
        row.pivot = new Vector2(0.5f, 0.5f);
        row.anchoredPosition = Vector2.zero;
        row.sizeDelta = new Vector2(-56f, 44f);

        CreateLabel(row, $"{id}Label", label, 24, FontStyles.Normal,
            new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(0f, 0f), new Vector2(96f, 0f), TextAlignmentOptions.MidlineLeft, RowLabelColor);

        return CreateSlider(row, $"{id}Slider",
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(108f, 6f), new Vector2(0f, -6f), value);
    }

    private static Button CreateFooterButton(RectTransform footer, string name, string label, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(footer, false);

        LayoutElement layout = go.GetComponent<LayoutElement>();
        layout.minHeight = 48f;
        layout.preferredHeight = 48f;

        Image image = go.GetComponent<Image>();
        image.color = color;

        CreateLabel(go.transform, "Text", label, 24, FontStyles.Bold,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);

        return go.GetComponent<Button>();
    }

    private static RectTransform CreateRect(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return go.GetComponent<RectTransform>();
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static TextMeshProUGUI CreateLabel(
        Transform parent,
        string name,
        string text,
        float fontSize,
        FontStyles style,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        TextAlignmentOptions alignment,
        Color? color = null)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = alignment;
        tmp.color = color ?? Color.white;
        tmp.raycastTarget = false;
        return tmp;
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
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Image background = CreateRect("Background", go.transform, new Color(0.16f, 0.19f, 0.24f, 1f)).GetComponent<Image>();
        background.raycastTarget = true;
        Stretch(background.rectTransform);

        RectTransform fillArea = CreateRect("Fill Area", go.transform, new Color(0f, 0f, 0f, 0f));
        Stretch(fillArea);
        fillArea.offsetMin = new Vector2(8f, 8f);
        fillArea.offsetMax = new Vector2(-8f, -8f);

        RectTransform fill = CreateRect("Fill", fillArea, new Color(0.34f, 0.62f, 0.46f, 1f));
        Stretch(fill);

        RectTransform handleArea = CreateRect("Handle Slide Area", go.transform, new Color(0f, 0f, 0f, 0f));
        Stretch(handleArea);
        handleArea.offsetMin = new Vector2(8f, 0f);
        handleArea.offsetMax = new Vector2(-8f, 0f);

        RectTransform handle = CreateRect("Handle", handleArea, new Color(0.93f, 0.95f, 0.98f, 1f));
        handle.sizeDelta = new Vector2(18f, 0f);

        Slider slider = go.GetComponent<Slider>();
        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = initialValue;
        return slider;
    }
}
