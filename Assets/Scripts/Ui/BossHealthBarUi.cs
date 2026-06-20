using CombatSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class BossHealthBarUi : MonoBehaviour
{
    public static BossHealthBarUi Instance { get; private set; }

    private const float PanelWidth = 520f;
    private const float PanelHeight = 56f;
    private const float BarHeight = 26f;
    private const float BottomMargin = 56f;

    [SerializeField] private Image fillBar;
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI bossLabel;

    private BossEnemy _trackedBoss;
    private HealthModule _trackedHealth;
    private static Sprite _whiteSprite;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Application.isPlaying)
                Destroy(gameObject);

            return;
        }

        Instance = this;
        CacheReferences();
        ApplyLayout();
        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!Application.isPlaying || _trackedHealth == null)
            return;

        if (_trackedBoss == null || _trackedBoss.IsDead)
        {
            Hide();
            return;
        }

        RefreshBar();
    }

    public void BindBoss(BossEnemy boss)
    {
        _trackedBoss = boss;
        _trackedHealth = boss != null ? boss.Health : null;

        if (_trackedHealth == null)
        {
            Hide();
            return;
        }

        CacheReferences();
        ApplyLayout();
        gameObject.SetActive(true);
        RefreshBar();
    }

    public void Hide()
    {
        _trackedBoss = null;
        _trackedHealth = null;
        gameObject.SetActive(false);
    }

    private void RefreshBar()
    {
        float ratio = Mathf.Clamp01(_trackedHealth.HealthRatio);

        if (fillRect != null)
        {
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(ratio, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }
        else if (fillBar != null)
        {
            fillBar.fillAmount = ratio;
        }

        if (hpText != null)
        {
            int current = Mathf.CeilToInt(_trackedHealth.CurrentHealth);
            int max = Mathf.CeilToInt(_trackedHealth.maxHealth);
            hpText.text = $"{current:N0} / {max:N0}";
        }
    }

    private void CacheReferences()
    {
        fillBar ??= transform.Find("BossBarBg/BossBarFill")?.GetComponent<Image>();
        fillRect ??= transform.Find("BossBarBg/BossBarFill")?.GetComponent<RectTransform>();
        hpText ??= transform.Find("BossHpText")?.GetComponent<TextMeshProUGUI>();
        bossLabel ??= transform.Find("BossLabel")?.GetComponent<TextMeshProUGUI>();
    }

    private void ApplyLayout()
    {
        if (transform is RectTransform barRootRect)
        {
            // 화면 하단 중앙에 독립 배치
            barRootRect.anchorMin = new Vector2(0.5f, 0f);
            barRootRect.anchorMax = new Vector2(0.5f, 0f);
            barRootRect.pivot = new Vector2(0.5f, 0f);
            barRootRect.anchoredPosition = new Vector2(0f, BottomMargin);
            barRootRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
        }

        Transform bg = transform.Find("BossBarBg");
        if (bg is RectTransform bgRect)
        {
            bgRect.anchorMin = new Vector2(0f, 0f);
            bgRect.anchorMax = new Vector2(1f, 0f);
            bgRect.pivot = new Vector2(0.5f, 0f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = new Vector2(0f, BarHeight);
        }

        Transform bgTransform = transform.Find("BossBarBg");
        if (bgTransform != null)
        {
            Image bgImage = bgTransform.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.sprite = GetWhiteSprite();
                bgImage.type = Image.Type.Simple;
                bgImage.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);
            }
        }

        if (fillBar != null)
        {
            fillBar.sprite = GetWhiteSprite();
            fillBar.type = Image.Type.Simple;
            fillBar.color = new Color(0.85f, 0.18f, 0.18f, 1f);
        }

        if (bossLabel != null)
        {
            if (bossLabel.transform is RectTransform labelRect)
            {
                labelRect.anchorMin = new Vector2(0f, 1f);
                labelRect.anchorMax = new Vector2(1f, 1f);
                labelRect.pivot = new Vector2(0.5f, 0f);
                labelRect.anchoredPosition = new Vector2(0f, 2f);
                labelRect.sizeDelta = new Vector2(0f, 22f);
            }

            bossLabel.fontSize = 14f;
            bossLabel.fontStyle = FontStyles.Bold;
            bossLabel.alignment = TextAlignmentOptions.Center;
            bossLabel.color = new Color(1f, 0.35f, 0.35f, 1f);
            bossLabel.text = "★ BOSS ★";
        }

        if (hpText != null)
        {
            if (hpText.transform is RectTransform hpRect)
            {
                hpRect.anchorMin = new Vector2(0f, 0f);
                hpRect.anchorMax = new Vector2(1f, 0f);
                hpRect.pivot = new Vector2(0.5f, 0f);
                hpRect.anchoredPosition = Vector2.zero;
                hpRect.sizeDelta = new Vector2(0f, BarHeight);
            }

            hpText.fontSize = 13f;
            hpText.fontStyle = FontStyles.Bold;
            hpText.color = Color.white;
            hpText.alignment = TextAlignmentOptions.Center;
        }
    }

    /// <summary>화면 하단 중앙에 독립적인 보스 HP 바 생성.</summary>
    public static BossHealthBarUi EnsureAtBottomCenter()
    {
        if (Instance != null)
        {
            Instance.ApplyLayout();
            return Instance;
        }

        Canvas hudCanvas = FindHudCanvas();
        if (hudCanvas == null)
            return null;

        Transform existing = hudCanvas.transform.Find("BossHealthBar");
        if (existing != null)
        {
            BossHealthBarUi existingUi = existing.GetComponent<BossHealthBarUi>();
            if (existingUi != null)
            {
                existingUi.CacheReferences();
                existingUi.ApplyLayout();
                return existingUi;
            }
        }

        Sprite sprite = GetWhiteSprite();

        GameObject barRootGo = new GameObject("BossHealthBar", typeof(RectTransform), typeof(BossHealthBarUi));
        barRootGo.transform.SetParent(hudCanvas.transform, false);

        // 보스 레이블
        GameObject bossLabelGo = new GameObject("BossLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        bossLabelGo.transform.SetParent(barRootGo.transform, false);
        TextMeshProUGUI bossLabelTmp = bossLabelGo.GetComponent<TextMeshProUGUI>();
        bossLabelTmp.fontSize = 14f;
        bossLabelTmp.fontStyle = FontStyles.Bold;
        bossLabelTmp.alignment = TextAlignmentOptions.Center;
        bossLabelTmp.color = new Color(1f, 0.35f, 0.35f, 1f);
        bossLabelTmp.text = "★ BOSS ★";
        bossLabelTmp.raycastTarget = false;
        UiFonts.ApplyNexon(bossLabelTmp);

        // 바 배경
        GameObject barBgGo = new GameObject("BossBarBg", typeof(RectTransform), typeof(Image));
        barBgGo.transform.SetParent(barRootGo.transform, false);
        RectTransform barBgRect = barBgGo.GetComponent<RectTransform>();
        barBgRect.anchorMin = Vector2.zero;
        barBgRect.anchorMax = Vector2.one;
        barBgRect.offsetMin = Vector2.zero;
        barBgRect.offsetMax = Vector2.zero;
        Image barBgImage = barBgGo.GetComponent<Image>();
        barBgImage.sprite = sprite;
        barBgImage.type = Image.Type.Simple;
        barBgImage.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);
        barBgImage.raycastTarget = false;

        // 바 채움
        GameObject barFillGo = new GameObject("BossBarFill", typeof(RectTransform), typeof(Image));
        barFillGo.transform.SetParent(barBgGo.transform, false);
        RectTransform barFillRect = barFillGo.GetComponent<RectTransform>();
        barFillRect.anchorMin = Vector2.zero;
        barFillRect.anchorMax = Vector2.one;
        barFillRect.pivot = new Vector2(0f, 0.5f);
        barFillRect.offsetMin = Vector2.zero;
        barFillRect.offsetMax = Vector2.zero;
        Image fillImage = barFillGo.GetComponent<Image>();
        fillImage.sprite = sprite;
        fillImage.type = Image.Type.Simple;
        fillImage.color = new Color(0.85f, 0.18f, 0.18f, 1f);
        fillImage.raycastTarget = false;

        // HP 텍스트
        GameObject hpLabelGo = new GameObject("BossHpText", typeof(RectTransform), typeof(TextMeshProUGUI));
        hpLabelGo.transform.SetParent(barRootGo.transform, false);
        RectTransform hpLabelRect = hpLabelGo.GetComponent<RectTransform>();
        hpLabelRect.anchorMin = Vector2.zero;
        hpLabelRect.anchorMax = Vector2.one;
        hpLabelRect.offsetMin = Vector2.zero;
        hpLabelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI hpLabel = hpLabelGo.GetComponent<TextMeshProUGUI>();
        hpLabel.fontSize = 13f;
        hpLabel.fontStyle = FontStyles.Bold;
        hpLabel.alignment = TextAlignmentOptions.Center;
        hpLabel.color = Color.white;
        hpLabel.raycastTarget = false;
        hpLabel.text = "0 / 0";
        UiFonts.ApplyNexon(hpLabel);

        BossHealthBarUi ui = barRootGo.GetComponent<BossHealthBarUi>();
        ui.fillBar = fillImage;
        ui.fillRect = barFillRect;
        ui.hpText = hpLabel;
        ui.bossLabel = bossLabelTmp;
        ui.ApplyLayout();
        ui.Hide();
        return ui;
    }

    private static Canvas FindHudCanvas()
    {
        GameObject go = GameObject.Find("GameHudCanvas");
        if (go != null)
            return go.GetComponent<Canvas>();

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Canvas c in canvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay && c.name.Contains("Hud"))
                return c;
        }

        return canvases.Length > 0 ? canvases[0] : null;
    }

    private static Sprite GetWhiteSprite()
    {
        if (_whiteSprite != null)
            return _whiteSprite;

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        _whiteSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 2f, 2f),
            new Vector2(0.5f, 0.5f),
            100f);

        return _whiteSprite;
    }
}
