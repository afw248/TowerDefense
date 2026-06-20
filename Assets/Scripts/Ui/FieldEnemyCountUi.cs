using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FieldEnemyCountUi : MonoBehaviour
{
    private static readonly Color SafeFill = new(0.45f, 0.62f, 0.78f, 1f);
    private static readonly Color WarningFill = new(0.95f, 0.72f, 0.18f, 1f);
    private static readonly Color DangerFill = new(0.92f, 0.24f, 0.20f, 1f);

    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image fillBar;
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private Image trackBar;
    [SerializeField] private float fillSmoothSpeed = 10f;

    private float _targetFill;
    private float _displayFill;
    private int _current;
    private int _max;

    private void Awake()
    {
        CacheReferences();
        ApplyHpBarLayout();
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        if (Mathf.Approximately(_displayFill, _targetFill))
            return;

        _displayFill = Mathf.Lerp(_displayFill, _targetFill, Time.deltaTime * fillSmoothSpeed);
        if (Mathf.Abs(_displayFill - _targetFill) < 0.001f)
            _displayFill = _targetFill;

        ApplyFillVisual(_displayFill);
    }

    public void SetCount(int current, int max)
    {
        _current = current;
        _max = max;

        if (countText != null)
            countText.text = $"{current} / {max}";

        _targetFill = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

        if (!Application.isPlaying)
        {
            _displayFill = _targetFill;
            ApplyFillVisual(_displayFill);
        }
    }

    public void ApplyHpBarLayout()
    {
        CacheReferences();
        EnsureBarStructure();

        if (transform is RectTransform panelRect)
        {
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(-20f, -74f);
            panelRect.sizeDelta = new Vector2(320f, 112f);
        }

        Transform title = transform.Find("StatusTitle");
        if (title is RectTransform titleRect)
        {
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -4f);
            titleRect.sizeDelta = new Vector2(-16f, 22f);
        }

        Transform barBg = transform.Find("MobBarBg");
        if (barBg is RectTransform barBgRect)
        {
            barBgRect.anchorMin = new Vector2(0f, 0f);
            barBgRect.anchorMax = new Vector2(1f, 0f);
            barBgRect.pivot = new Vector2(0.5f, 0f);
            barBgRect.anchoredPosition = new Vector2(0f, 38f);
            barBgRect.sizeDelta = new Vector2(-16f, 28f);
        }

        if (trackBar != null)
        {
            trackBar.color = GameHudTheme.BarTrack;
            trackBar.raycastTarget = false;
        }

        if (fillRect != null)
        {
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }

        if (countText != null)
        {
            RectTransform textRect = countText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8f, 38f);
            textRect.offsetMax = new Vector2(-8f, -66f);
            GameHudTheme.StyleLabel(countText, 20f, FontStyles.Bold);
            countText.alignment = TextAlignmentOptions.Center;
            countText.color = GameHudTheme.BodyText;
        }

        Transform legacyIcon = transform.Find("MobIcon");
        if (legacyIcon != null)
            legacyIcon.gameObject.SetActive(false);

        ApplyFillVisual(_displayFill);
    }

    private void CacheReferences()
    {
        countText ??= transform.Find("MobCountText")?.GetComponent<TextMeshProUGUI>();
        trackBar ??= transform.Find("MobBarBg")?.GetComponent<Image>();

        Transform fillTransform = transform.Find("MobBarBg/MobBarFill");
        fillBar ??= fillTransform?.GetComponent<Image>();
        fillRect ??= fillTransform?.GetComponent<RectTransform>();
    }

    private void EnsureBarStructure()
    {
        Transform barBgTransform = transform.Find("MobBarBg");
        if (barBgTransform == null)
        {
            GameObject barBgGo = new GameObject("MobBarBg", typeof(RectTransform), typeof(Image));
            barBgGo.transform.SetParent(transform, false);
            barBgTransform = barBgGo.transform;
        }

        trackBar = barBgTransform.GetComponent<Image>();

        Transform fillTransform = barBgTransform.Find("MobBarFill");
        if (fillTransform == null)
        {
            GameObject fillGo = new GameObject("MobBarFill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(barBgTransform, false);
            fillTransform = fillGo.transform;
        }

        fillBar = fillTransform.GetComponent<Image>();
        fillRect = fillTransform.GetComponent<RectTransform>();

        if (fillBar != null)
        {
            fillBar.type = Image.Type.Simple;
            fillBar.raycastTarget = false;
        }

        if (countText == null)
        {
            GameObject textGo = new GameObject("MobCountText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(transform, false);
            countText = textGo.GetComponent<TextMeshProUGUI>();
        }
    }

    private void ApplyFillVisual(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);

        if (fillRect != null)
        {
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(ratio, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }
        else if (fillBar != null)
        {
            fillBar.type = Image.Type.Filled;
            fillBar.fillMethod = Image.FillMethod.Horizontal;
            fillBar.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillBar.fillAmount = ratio;
        }

        if (fillBar != null)
            fillBar.color = GetFillColor(ratio);
    }

    private static Color GetFillColor(float ratio)
    {
        if (ratio >= 0.75f)
            return Color.Lerp(WarningFill, DangerFill, (ratio - 0.75f) / 0.25f);

        if (ratio >= 0.45f)
            return Color.Lerp(SafeFill, WarningFill, (ratio - 0.45f) / 0.30f);

        return SafeFill;
    }
}
