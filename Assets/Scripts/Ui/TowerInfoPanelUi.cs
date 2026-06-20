using Player;
using TMPro;
using Tower;
using UnityEngine;
using UnityEngine.UI;

public class TowerInfoPanelUi : MonoBehaviour
{
    [SerializeField] private GameObject panelContainer;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Image panelBackground;
    [SerializeField] private Image headerAreaImage;
    [SerializeField] private Image statsAreaImage;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image rangeRingImage;
    [SerializeField] private TextMeshProUGUI portraitFallbackText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI gradeText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackTypeText;
    [SerializeField] private TextMeshProUGUI rangeText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private Button sellButton;
    [SerializeField] private Image sellButtonImage;
    [SerializeField] private TextMeshProUGUI sellButtonLabel;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private TowerInfoVisualPreview visualPreview;

    private AbstractPlayer _target;
    private bool _listenersRegistered;
    private bool _portraitFrameReady;
    private bool _uiChromeReady;
    private CanvasGroup _canvasGroup;
    private bool _visibilityInitialized;
    private bool _layoutReady;
    private Coroutine _portraitCoroutine;

    private void Awake()
    {
        EnsureInitialized();
    }

    public void PrepareHidden()
    {
        EnsureInitialized();
        Hide();
    }

    private void EnsureInitialized()
    {
        tileManager ??= FindFirstObjectByType<TileManager>();
        ResolvePanelContainer();

        if (panelRoot == null)
            panelRoot = gameObject;

        ResolveReferences();
        EnsureUiChrome();
        EnsurePortraitFrame();
        RepairLayout();

        if (sellButton != null && !_listenersRegistered)
        {
            sellButton.onClick.AddListener(HandleSellClick);
            _listenersRegistered = true;
        }
    }

    private void ResolvePanelContainer()
    {
        if (panelContainer != null)
            return;

        Transform parent = transform.parent;
        while (parent != null)
        {
            if (parent.name == "RightPanel")
            {
                panelContainer = parent.gameObject;
                break;
            }

            parent = parent.parent;
        }
    }

    private void ResolveReferences()
    {
        panelBackground ??= panelRoot != null ? panelRoot.GetComponent<Image>() : GetComponent<Image>();
        titleText ??= transform.Find("TowerTitle")?.GetComponent<TextMeshProUGUI>();
        gradeText ??= transform.Find("TowerGrade")?.GetComponent<TextMeshProUGUI>();
        damageText ??= transform.Find("TowerDamage")?.GetComponent<TextMeshProUGUI>()
            ?? transform.Find("TowerAttack")?.GetComponent<TextMeshProUGUI>();
        attackTypeText ??= transform.Find("TowerAttackType")?.GetComponent<TextMeshProUGUI>();
        rangeText ??= transform.Find("TowerRange")?.GetComponent<TextMeshProUGUI>();
        attackSpeedText ??= transform.Find("TowerAttackSpeed")?.GetComponent<TextMeshProUGUI>();
        sellButton ??= transform.Find("SellButton")?.GetComponent<Button>();
        sellButtonImage ??= sellButton?.GetComponent<Image>();
        sellButtonLabel ??= sellButton?.GetComponentInChildren<TextMeshProUGUI>();
        portraitImage ??= transform.Find("Portrait")?.GetComponent<Image>();
        rangeRingImage ??= transform.Find("Portrait/RangeRing")?.GetComponent<Image>();
        portraitFallbackText ??= transform.Find("Portrait/PortraitLabel")?.GetComponent<TextMeshProUGUI>();
        headerAreaImage ??= transform.Find("HeaderArea")?.GetComponent<Image>();
        statsAreaImage ??= transform.Find("StatsArea")?.GetComponent<Image>();
    }

    private void EnsureUiChrome()
    {
        if (_uiChromeReady)
            return;

        Transform root = panelRoot != null ? panelRoot.transform : transform;

        headerAreaImage = EnsureAreaImage(root, "HeaderArea", "HeaderArea", new Vector2(0f, 0.42f), Vector2.one);
        statsAreaImage = EnsureAreaImage(root, "StatsArea", "StatsArea", Vector2.zero, new Vector2(1f, 0.42f));

        damageText = EnsureStatLabel(root, "TowerDamage", damageText, "데미지: 0");
        attackTypeText = EnsureStatLabel(root, "TowerAttackType", attackTypeText, "공격형태: 단일");
        rangeText = EnsureStatLabel(root, "TowerRange", rangeText, "사거리 반경: 0");
        attackSpeedText = EnsureStatLabel(root, "TowerAttackSpeed", attackSpeedText, "초당 0회");

        if (gradeText != null)
            gradeText.gameObject.SetActive(true);

        _uiChromeReady = true;
    }

    private static Image EnsureAreaImage(Transform root, string objectName, string fallbackName, Vector2 anchorMin, Vector2 anchorMax)
    {
        Transform existing = root.Find(objectName) ?? root.Find(fallbackName);
        if (existing != null)
            return existing.GetComponent<Image>();

        GameObject areaGo = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        areaGo.transform.SetParent(root, false);
        areaGo.transform.SetAsFirstSibling();

        RectTransform rect = areaGo.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = new Vector2(8f, 8f);
        rect.offsetMax = new Vector2(-8f, -8f);

        Image image = areaGo.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI EnsureStatLabel(
        Transform root,
        string objectName,
        TextMeshProUGUI existing,
        string defaultText)
    {
        if (existing != null)
            return existing;

        Transform found = root.Find(objectName);
        if (found != null)
            return found.GetComponent<TextMeshProUGUI>();

        GameObject labelGo = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(root, false);

        TextMeshProUGUI label = labelGo.GetComponent<TextMeshProUGUI>();
        label.text = defaultText;
        UiFonts.ApplyNexon(label);
        return label;
    }

    private void EnsurePortraitFrame()
    {
        if (_portraitFrameReady)
            return;

        if (portraitImage == null)
            return;

        Transform portraitTransform = portraitImage.transform;

        if (portraitTransform.GetComponent<RectMask2D>() == null)
            portraitTransform.gameObject.AddComponent<RectMask2D>();

        Outline outline = portraitTransform.GetComponent<Outline>();
        if (outline == null)
            outline = portraitTransform.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.72f, 0.76f, 0.82f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        portraitImage.color = new Color(0.12f, 0.14f, 0.18f, 1f);
        portraitImage.raycastTarget = false;

        rangeRingImage = EnsureRangeRing(portraitTransform);

        Transform previewTransform = portraitTransform.Find("PortraitPreview");
        RawImage previewRawImage;
        if (previewTransform == null)
        {
            GameObject previewGo = new GameObject("PortraitPreview", typeof(RectTransform), typeof(RawImage));
            previewGo.transform.SetParent(portraitTransform, false);

            RectTransform previewRect = previewGo.GetComponent<RectTransform>();
            previewRect.anchorMin = Vector2.zero;
            previewRect.anchorMax = Vector2.one;
            previewRect.offsetMin = new Vector2(8f, 8f);
            previewRect.offsetMax = new Vector2(-8f, -8f);

            previewTransform = previewGo.transform;
            previewRawImage = previewGo.GetComponent<RawImage>();
        }
        else
        {
            previewRawImage = previewTransform.GetComponent<RawImage>();
            if (previewRawImage == null)
                previewRawImage = previewTransform.gameObject.AddComponent<RawImage>();
        }

        previewRawImage.color = Color.white;
        previewRawImage.raycastTarget = false;

        visualPreview = previewTransform.GetComponent<TowerInfoVisualPreview>();
        if (visualPreview == null)
            visualPreview = previewTransform.gameObject.AddComponent<TowerInfoVisualPreview>();

        visualPreview.BindRawImage(previewRawImage);

        TowerInfoVisualPreview legacyPreview = portraitTransform.GetComponent<TowerInfoVisualPreview>();
        if (legacyPreview != null && legacyPreview != visualPreview)
            Destroy(legacyPreview);

        if (portraitFallbackText != null)
        {
            RectTransform fallbackRect = portraitFallbackText.rectTransform;
            fallbackRect.anchorMin = Vector2.zero;
            fallbackRect.anchorMax = Vector2.one;
            fallbackRect.offsetMin = Vector2.zero;
            fallbackRect.offsetMax = Vector2.zero;
        }

        _portraitFrameReady = true;
    }

    private static Image EnsureRangeRing(Transform portraitTransform)
    {
        Transform ringTransform = portraitTransform.Find("RangeRing");
        GameObject ringGo;
        Image ringImage;
        if (ringTransform == null)
        {
            ringGo = new GameObject("RangeRing", typeof(RectTransform), typeof(Image));
            ringGo.transform.SetParent(portraitTransform, false);
            ringGo.transform.SetAsFirstSibling();

            RectTransform ringRect = ringGo.GetComponent<RectTransform>();
            ringRect.anchorMin = new Vector2(0.5f, 0.5f);
            ringRect.anchorMax = new Vector2(0.5f, 0.5f);
            ringRect.pivot = new Vector2(0.5f, 0.5f);
            ringRect.sizeDelta = new Vector2(148f, 148f);

            ringImage = ringGo.GetComponent<Image>();
        }
        else
        {
            ringGo = ringTransform.gameObject;
            ringImage = ringTransform.GetComponent<Image>();
        }

        ringImage.raycastTarget = false;
        ringImage.sprite = TowerInfoUiHelpers.GetUiSprite();
        ringImage.type = Image.Type.Sliced;
        ringImage.color = new Color(1f, 1f, 1f, 0.04f);

        Outline ringOutline = ringGo.GetComponent<Outline>();
        if (ringOutline == null)
            ringOutline = ringGo.AddComponent<Outline>();
        ringOutline.effectColor = new Color(0.55f, 0.95f, 1f, 0.45f);
        ringOutline.effectDistance = new Vector2(3f, -3f);

        return ringImage;
    }

    public void RepairLayout()
    {
        ResolveReferences();

        SetTopCenterRect(portraitImage?.rectTransform, new Vector2(0f, -104f), new Vector2(168f, 168f));
        SetTopCenterRect(titleText?.rectTransform, new Vector2(0f, -228f), new Vector2(236f, 30f));
        SetTopCenterRect(gradeText?.rectTransform, new Vector2(0f, -258f), new Vector2(236f, 24f));
        SetTopCenterRect(damageText?.rectTransform, new Vector2(0f, -300f), new Vector2(236f, 24f));
        SetTopCenterRect(attackTypeText?.rectTransform, new Vector2(0f, -326f), new Vector2(236f, 22f));
        SetTopCenterRect(rangeText?.rectTransform, new Vector2(0f, -350f), new Vector2(236f, 22f));
        SetTopCenterRect(attackSpeedText?.rectTransform, new Vector2(0f, -374f), new Vector2(236f, 22f));
        SetBottomCenterRect(sellButton?.GetComponent<RectTransform>(), new Vector2(0f, 56f), new Vector2(196f, 40f));

        ApplyTextLayout();
    }

    private void OnDestroy()
    {
        if (sellButton != null)
            sellButton.onClick.RemoveListener(HandleSellClick);

        visualPreview?.ClearVisual();
    }

    public bool IsVisible => _canvasGroup != null && _canvasGroup.alpha > 0.01f;

    public void Show(AbstractPlayer tower)
    {
        if (tower == null)
            return;

        _target = tower;
        SetVisible(true);
        EnsureInitialized();
        RefreshText();
        SchedulePortraitRefresh();
    }

    private void SchedulePortraitRefresh()
    {
        if (_portraitCoroutine != null)
            StopCoroutine(_portraitCoroutine);

        _portraitCoroutine = StartCoroutine(ApplyPortraitNextFrame());
    }

    private System.Collections.IEnumerator ApplyPortraitNextFrame()
    {
        yield return null;

        if (_target == null || !IsVisible)
            yield break;

        ApplyPortrait(_target.TowerVariant, _target);
        _portraitCoroutine = null;
    }

    private void EnsureVisibilityGroup()
    {
        if (_visibilityInitialized)
            return;

        ResolvePanelContainer();

        if (panelRoot == null)
            panelRoot = gameObject;

        if (panelContainer != null && !panelContainer.activeSelf)
            panelContainer.SetActive(true);

        if (panelRoot != null && !panelRoot.activeSelf)
            panelRoot.SetActive(true);

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        Transform groupHost = panelContainer != null ? panelContainer.transform : panelRoot.transform;
        _canvasGroup = groupHost.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = groupHost.gameObject.AddComponent<CanvasGroup>();

        _visibilityInitialized = true;
    }

    private void SetVisible(bool visible)
    {
        EnsureVisibilityGroup();
        if (_canvasGroup == null)
            return;

        if (visible)
        {
            if (panelContainer != null)
                panelContainer.SetActive(true);

            if (panelRoot != null)
                panelRoot.SetActive(true);

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.interactable = visible;
        _canvasGroup.blocksRaycasts = visible;
    }

    public void Hide()
    {
        if (_portraitCoroutine != null)
        {
            StopCoroutine(_portraitCoroutine);
            _portraitCoroutine = null;
        }

        EnsureInitialized();
        _target = null;
        visualPreview?.ClearVisual();

        if (portraitFallbackText != null)
            portraitFallbackText.gameObject.SetActive(false);

        SetVisible(false);
    }

    public void Refresh()
    {
        if (_target == null)
        {
            if (IsVisible)
                Hide();
            return;
        }

        RefreshText();

        if (!IsVisible)
            return;

        SchedulePortraitRefresh();
    }

    private void RefreshText()
    {
        if (_target == null)
            return;

        if (!_layoutReady)
        {
            RepairLayout();
            _layoutReady = true;
        }

        TowerVariantSO variant = _target.TowerVariant;
        TowerInfoUiPalette palette = TowerInfoUiThemes.Get(_target.Grade);
        ApplyGradeTheme(palette);

        if (titleText != null)
            titleText.text = GetArchetypeLabel(variant != null ? variant.archetype : TowerArchetype.Bow);

        if (gradeText != null)
            gradeText.text = GetGradeLabel(_target.Grade);

        if (!TowerCombatStats.TryGet(_target, out TowerCombatStats.Snapshot stats))
            return;

        if (damageText != null)
            damageText.text = $"데미지: {stats.Damage:0.#}";

        if (attackTypeText != null)
            attackTypeText.text = $"공격형태: {stats.AttackTypeLabel}";

        if (rangeText != null)
        {
            rangeText.text = stats.IsAreaAttack && stats.EffectRadius > 0f
                ? $"사거리 반경: {stats.DetectRadius:0.#} / 효과 반경: {stats.EffectRadius:0.#}"
                : $"사거리 반경: {stats.DetectRadius:0.#}";
        }

        if (attackSpeedText != null)
            attackSpeedText.text = $"초당 {stats.AttacksPerSecond:0.#}회";

        TowerInfoUiHelpers.ApplyRangeRing(rangeRingImage, stats.DetectRadius, palette.rangeRingColor);

        EconomyConfigSO config = EconomyManager.Instance?.Config;
        int sellRefund = config != null ? config.GetSellRefund(_target.Grade) : 0;

        if (sellButtonLabel != null)
            sellButtonLabel.text = $"판매 +{sellRefund}";
    }

    private void ApplyGradeTheme(TowerInfoUiPalette palette)
    {
        if (panelBackground != null)
            panelBackground.color = palette.panelColor;
        if (headerAreaImage != null)
            headerAreaImage.color = palette.headerColor;
        if (statsAreaImage != null)
            statsAreaImage.color = palette.statsColor;
        if (sellButtonImage != null)
            sellButtonImage.color = palette.sellButtonColor;

        ConfigureText(titleText, 24f, FontStyles.Bold, palette.titleTextColor, palette.outlineColor);
        ConfigureText(gradeText, 18f, FontStyles.Bold, palette.accentTextColor, palette.outlineColor);
        ConfigureText(damageText, 20f, FontStyles.Bold, palette.bodyTextColor, palette.outlineColor);
        ConfigureText(attackTypeText, 18f, FontStyles.Normal, palette.bodyTextColor, palette.outlineColor);
        ConfigureText(rangeText, 18f, FontStyles.Normal, palette.accentTextColor, palette.outlineColor);
        ConfigureText(attackSpeedText, 18f, FontStyles.Normal, palette.bodyTextColor, palette.outlineColor);
        ConfigureText(sellButtonLabel, 20f, FontStyles.Bold, Color.white, palette.outlineColor);
        ConfigureText(portraitFallbackText, 28f, FontStyles.Bold, Color.white, palette.outlineColor);
    }

    public static void ApplyTextLayout(TextMeshProUGUI tmp)
    {
        if (tmp == null)
            return;

        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private void ApplyTextLayout()
    {
        ConfigureText(titleText, 24f, FontStyles.Bold, Color.white, Color.black);
        ConfigureText(gradeText, 18f, FontStyles.Bold, Color.white, Color.black);
        ConfigureText(damageText, 20f, FontStyles.Bold, Color.white, Color.black);
        ConfigureText(attackTypeText, 18f, FontStyles.Normal, Color.white, Color.black);
        ConfigureText(rangeText, 18f, FontStyles.Normal, Color.white, Color.black);
        ConfigureText(attackSpeedText, 18f, FontStyles.Normal, Color.white, Color.black);
        ConfigureText(sellButtonLabel, 20f, FontStyles.Bold, Color.white, Color.black);
        ConfigureText(portraitFallbackText, 28f, FontStyles.Bold, Color.white, Color.black);
    }

    private static void ConfigureText(
        TextMeshProUGUI tmp,
        float fontSize,
        FontStyles style,
        Color color,
        Color outlineColor)
    {
        if (tmp == null)
            return;

        UiFonts.ApplyNexon(tmp);
        ApplyTextLayout(tmp);
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;

        if (!CanUseOutline(tmp))
            return;

        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = outlineColor;
    }

    private static bool CanUseOutline(TextMeshProUGUI tmp)
    {
        if (tmp == null || tmp.font == null)
            return false;

        tmp.ForceMeshUpdate();
        return tmp.fontMaterial != null || tmp.fontSharedMaterial != null;
    }

    private void ApplyPortrait(TowerVariantSO variant, AbstractPlayer tower)
    {
        try
        {
            GameObject visualSource = ResolveFattyPolyUiPrefab(variant, tower);

            if (visualSource != null && visualPreview != null && visualPreview.ShowVisual(visualSource))
            {
                if (portraitFallbackText != null)
                    portraitFallbackText.gameObject.SetActive(false);
                return;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Tower info portrait preview failed: {ex.Message}", this);
        }

        visualPreview?.ClearVisual();

        if (portraitFallbackText != null)
        {
            portraitFallbackText.gameObject.SetActive(true);
            portraitFallbackText.text = variant != null
                ? GetArchetypeLabel(variant.archetype)
                : "?";
        }
    }

    private static GameObject ResolveFattyPolyUiPrefab(TowerVariantSO variant, AbstractPlayer tower)
    {
        GameObject configured = variant != null ? variant.fattyPolyVisualPrefab : null;
        if (configured != null && !configured.name.EndsWith("_Anim"))
            return configured;

        if (variant?.towerPrefab != null)
        {
            GameObject fromPrefab = FindFattyPolyVisualFromTower(variant.towerPrefab.transform);
            if (fromPrefab != null)
                return fromPrefab;
        }

        return FindFattyPolyVisualFromTower(tower != null ? tower.transform : null);
    }

    private static GameObject FindFattyPolyVisualFromTower(Transform towerRoot)
    {
        if (towerRoot == null)
            return null;

        Transform visual = towerRoot.Find("Visual");
        if (visual != null)
        {
            GameObject fattyChild = FindFattyPolyChild(visual);
            if (fattyChild != null)
                return fattyChild;
        }

        return FindFattyPolyChild(towerRoot);
    }

    private static GameObject FindFattyPolyChild(Transform root)
    {
        if (root == null)
            return null;

        foreach (Transform child in root)
        {
            if (IsIgnoredVisualChild(child.name))
                continue;

            if (!IsFattyPolyVisualName(child.name))
                continue;

            if (child.name.EndsWith("_Anim"))
                continue;

            if (child.GetComponentInChildren<Renderer>(true) != null)
                return child.gameObject;
        }

        foreach (Transform child in root)
        {
            if (IsIgnoredVisualChild(child.name))
                continue;

            if (child.GetComponentInChildren<MeshRenderer>(true) != null)
                return child.gameObject;
        }

        return null;
    }

    private static bool IsFattyPolyVisualName(string childName)
    {
        return childName.Contains("CrossBow")
            || childName.Contains("Culverin")
            || childName.Contains("FattyMissile")
            || childName.Contains("FattyCatapult");
    }

    private static bool IsIgnoredVisualChild(string childName)
    {
        return childName.Contains("Skill")
            || childName.Contains("Health")
            || childName.Contains("AgentSensor");
    }

    private static void SetTopCenterRect(RectTransform rect, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    private static void SetBottomCenterRect(RectTransform rect, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    private void HandleSellClick()
    {
        if (_target == null || tileManager == null)
            return;

        if (tileManager.TrySellTower(_target))
        {
            TowerInspectRangeIndicator.Instance.Hide();
            Hide();
        }
    }

    private static string GetGradeLabel(TowerGrade grade) => grade switch
    {
        TowerGrade.Rare => "레어",
        TowerGrade.Epic => "에픽",
        TowerGrade.Legendary => "전설",
        _ => "노말"
    };

    private static string GetArchetypeLabel(TowerArchetype archetype) => archetype switch
    {
        TowerArchetype.Culverin => "대포",
        TowerArchetype.Missile => "미사일",
        _ => "석궁"
    };
}
