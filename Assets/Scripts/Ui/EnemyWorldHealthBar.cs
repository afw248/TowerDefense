using Agents;
using CombatSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(200)]
public class EnemyWorldHealthBar : MonoBehaviour
{
    private const float BarWorldWidth = 1.4f;
    private const float BarWorldHeight = 0.1f;
    private const float PixelsPerUnit = 100f;

    [SerializeField] private float heightOffset = 0.35f;

    private static Sprite _whiteSprite;

    private HealthModule _health;
    private Agent _agent;
    private Transform _anchor;
    private RectTransform _fillRect;
    private TextMeshProUGUI _hpText;
    private Camera _camera;
    private bool _showNumbers;
    private bool _built;

    private void Start()
    {
        _agent = GetComponent<Agent>();
        ResolveHealth();

        if (_health == null)
        {
            enabled = false;
            return;
        }

        _camera = Camera.main;
        BuildBar();

        _showNumbers = GetComponent<BossEnemy>() != null;
        if (_showNumbers)
            EnsureHpText();
    }

    private void LateUpdate()
    {
        if (!_built || _health == null || _anchor == null)
            return;

        if (_health.CurrentHealth <= 0f || _health.maxHealth <= 0f)
        {
            _anchor.gameObject.SetActive(false);
            return;
        }

        _anchor.gameObject.SetActive(true);
        PositionAnchor();
        AlignToCamera();
        RefreshFill();
    }

    private void ResolveHealth()
    {
        if (TryGetComponent<Enemy>(out Enemy enemy) && enemy.Health != null)
        {
            _health = enemy.Health;
            return;
        }

        Transform healthChild = transform.Find("HealthModule");
        if (healthChild != null)
            _health = healthChild.GetComponent<HealthModule>();
    }

    private void BuildBar()
    {
        if (_built)
            return;

        _built = true;
        Sprite sprite = GetWhiteSprite();
        float canvasScale = 1f / PixelsPerUnit;
        Vector2 barPixelSize = new Vector2(BarWorldWidth * PixelsPerUnit, BarWorldHeight * PixelsPerUnit);

        GameObject anchorGo = new GameObject("HealthBarAnchor");
        _anchor = anchorGo.transform;
        _anchor.SetParent(transform, false);

        GameObject canvasGo = new GameObject("HealthBarCanvas", typeof(RectTransform), typeof(Canvas));
        canvasGo.transform.SetParent(_anchor, false);

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = _camera != null ? _camera : Camera.main;
        canvas.sortingOrder = 50;

        RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
        canvasRect.sizeDelta = barPixelSize;
        canvasRect.pivot = new Vector2(0.5f, 0.5f);
        canvasGo.transform.localScale = Vector3.one * canvasScale;
        canvasGo.transform.localPosition = Vector3.zero;
        canvasGo.transform.localRotation = Quaternion.identity;

        GameObject bgGo = new GameObject("BarBg", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(canvasGo.transform, false);
        RectTransform bgRect = bgGo.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = bgGo.GetComponent<Image>();
        bgImage.sprite = sprite;
        bgImage.type = Image.Type.Simple;
        bgImage.color = new Color(0.1f, 0.1f, 0.12f, 0.95f);
        bgImage.raycastTarget = false;

        GameObject fillGo = new GameObject("BarFill", typeof(RectTransform), typeof(Image));
        fillGo.transform.SetParent(bgGo.transform, false);
        _fillRect = fillGo.GetComponent<RectTransform>();
        _fillRect.anchorMin = Vector2.zero;
        _fillRect.anchorMax = Vector2.one;
        _fillRect.pivot = new Vector2(0f, 0.5f);
        _fillRect.offsetMin = Vector2.zero;
        _fillRect.offsetMax = Vector2.zero;
        Image fillImage = fillGo.GetComponent<Image>();
        fillImage.sprite = sprite;
        fillImage.type = Image.Type.Simple;
        fillImage.color = new Color(0.28f, 0.9f, 0.34f, 1f);
        fillImage.raycastTarget = false;
    }

    private void EnsureHpText()
    {
        if (_hpText != null || _anchor == null)
            return;

        Transform canvas = _anchor.Find("HealthBarCanvas");
        if (canvas == null)
            return;

        GameObject textGo = new GameObject("HpText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(canvas, false);

        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 1f);
        textRect.anchorMax = new Vector2(0.5f, 1f);
        textRect.pivot = new Vector2(0.5f, 0f);
        textRect.anchoredPosition = new Vector2(0f, 4f);
        textRect.sizeDelta = new Vector2(BarWorldWidth * PixelsPerUnit, 16f);

        _hpText = textGo.GetComponent<TextMeshProUGUI>();
        _hpText.fontSize = 12f;
        _hpText.alignment = TextAlignmentOptions.Center;
        _hpText.color = Color.white;
        _hpText.raycastTarget = false;
    }

    private void PositionAnchor()
    {
        Vector3 headPosition = _agent != null
            ? AgentImpactPoints.GetHead(_agent, 1f)
            : transform.position + Vector3.up * 1.5f;

        _anchor.position = headPosition + Vector3.up * heightOffset;
    }

    private void AlignToCamera()
    {
        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null)
            return;

        _anchor.rotation = _camera.transform.rotation;
    }

    private void RefreshFill()
    {
        if (_fillRect != null)
        {
            float ratio = Mathf.Clamp01(_health.HealthRatio);
            _fillRect.anchorMin = Vector2.zero;
            _fillRect.anchorMax = new Vector2(ratio, 1f);
            _fillRect.offsetMin = Vector2.zero;
            _fillRect.offsetMax = Vector2.zero;
        }

        if (_showNumbers && _hpText != null)
        {
            int current = Mathf.CeilToInt(_health.CurrentHealth);
            int max = Mathf.CeilToInt(_health.maxHealth);
            _hpText.text = $"{current}/{max}";
        }
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
            PixelsPerUnit);

        return _whiteSprite;
    }
}
