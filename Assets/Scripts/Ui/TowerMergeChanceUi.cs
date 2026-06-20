using Player;
using TMPro;
using Tower;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(500)]
public class TowerMergeChanceUi : MonoBehaviour
{
    private const float PanelWorldWidth = 4.8f;
    private const float PanelWorldHeight = 2.1f;
    private const float PixelsPerUnit = 100f;
    private const float HeightPadding = 0.45f;
    private const float MinHeightOffset = 2f;
    private const float MaxHeightOffset = 3.6f;
    private const float MissileVisualBlend = 0.42f;
    private const float MissileHeightPadding = 0.55f;
    private const float MissileMinHeightOffset = 2.3f;
    private const float MissileMaxHeightOffset = 3.2f;
    private const float CameraForwardOffset = 0.55f;
    private const int SortingOrder = 32767;

    private static TowerMergeChanceUi _instance;
    private static Sprite _whiteSprite;

    [SerializeField] private TowerMergeUiThemeId themeId = TowerMergeUiThemeId.MinimalDark;

    private Transform _anchor;
    private Transform _targetTower;
    private Transform _draggedTower;
    private TowerArchetype _targetArchetype;
    private TowerArchetype _draggedArchetype;
    private Image _panelImage;
    private Image _titleAreaImage;
    private Image _chanceAreaImage;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _chanceText;
    private Camera _camera;
    private bool _built;

    public static TowerMergeChanceUi Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject host = new GameObject(nameof(TowerMergeChanceUi));
                _instance = host.AddComponent<TowerMergeChanceUi>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUi();
        Hide();
    }

    public void SetTheme(TowerMergeUiThemeId theme)
    {
        themeId = theme;
        if (_built)
            ApplyTheme();
    }

    public void Show(Transform targetTower, TowerGrade fromGrade, int chancePercent)
    {
        Show(targetTower, null, fromGrade, chancePercent);
    }

    public void Show(Transform targetTower, Transform draggedTower, TowerGrade fromGrade, int chancePercent)
    {
        if (targetTower == null)
        {
            Hide();
            return;
        }

        BuildUi();
        ApplyTheme();

        _targetTower = targetTower;
        _draggedTower = draggedTower;
        _targetArchetype = ResolveArchetype(targetTower);
        _draggedArchetype = draggedTower != null ? ResolveArchetype(draggedTower) : _targetArchetype;
        _anchor.SetParent(transform, false);
        _anchor.gameObject.SetActive(true);

        _titleText.text = TowerGradeLabels.GetMergeLabel(fromGrade);
        _chanceText.text = $"{chancePercent}%";
    }

    public void Hide()
    {
        _targetTower = null;
        _draggedTower = null;

        if (_anchor != null)
            _anchor.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_anchor == null || !_anchor.gameObject.activeSelf || _targetTower == null)
            return;

        UpdateWorldPosition();
        AlignToCamera();
    }

    private void UpdateWorldPosition()
    {
        if (_camera == null)
            _camera = Camera.main;

        if (_targetTower == null)
            return;

        Vector3 worldPosition = ResolveAnchorWorldPosition();
        worldPosition += ResolveCameraForwardOffset(worldPosition);
        _anchor.position = worldPosition;
    }

    private Vector3 ResolveAnchorWorldPosition()
    {
        float topY = _targetTower.position.y + ResolveTowerTopOffset(_targetTower, _targetArchetype);

        if (_draggedTower != null)
        {
            float draggedTopY = _draggedTower.position.y + ResolveTowerTopOffset(_draggedTower, _draggedArchetype);
            topY = Mathf.Max(topY, draggedTopY);
        }

        Vector3 center = _draggedTower != null
            ? Vector3.Lerp(_targetTower.position, _draggedTower.position, 0.5f)
            : _targetTower.position;

        center.y = topY + 0.35f;
        return center;
    }

    private static Vector3 ResolveCameraForwardOffset(Vector3 worldPosition)
    {
        Camera camera = Camera.main;
        if (camera == null)
            return Vector3.zero;

        Vector3 toCamera = camera.transform.position - worldPosition;
        if (toCamera.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        return toCamera.normalized * CameraForwardOffset;
    }

    private static TowerArchetype ResolveArchetype(Transform tower)
    {
        AbstractPlayer player = tower.GetComponent<AbstractPlayer>();
        return player != null ? player.Archetype : TowerArchetype.Bow;
    }

    private static float ResolveTowerTopOffset(Transform tower, TowerArchetype archetype)
    {
        float visualTop = ResolveVisualTop(tower, archetype);
        float padding = archetype == TowerArchetype.Missile ? MissileHeightPadding : HeightPadding;
        float minOffset = archetype == TowerArchetype.Missile ? MissileMinHeightOffset : MinHeightOffset;
        float maxOffset = archetype == TowerArchetype.Missile ? MissileMaxHeightOffset : MaxHeightOffset;

        return Mathf.Clamp(visualTop + padding, minOffset, maxOffset);
    }

    private static float ResolveVisualTop(Transform tower, TowerArchetype archetype)
    {
        float colliderTop = ResolveColliderTop(tower);
        if (archetype != TowerArchetype.Missile)
            return colliderTop > 0f ? colliderTop : ResolveRendererTop(tower);

        float rendererTop = ResolveRendererTop(tower);
        if (colliderTop <= 0f)
            return rendererTop;

        return Mathf.Lerp(colliderTop, rendererTop, MissileVisualBlend);
    }

    private static float ResolveColliderTop(Transform tower)
    {
        BoxCollider col = tower.GetComponent<BoxCollider>();
        return col != null ? col.bounds.max.y - tower.position.y : 0f;
    }

    private static float ResolveRendererTop(Transform tower)
    {
        Renderer[] renderers = tower.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return MinHeightOffset;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || !renderer.enabled)
                continue;

            bounds.Encapsulate(renderer.bounds);
        }

        return bounds.max.y - tower.position.y;
    }

    private void BuildUi()
    {
        if (_built)
            return;

        _built = true;
        _camera = Camera.main;

        Sprite sprite = GetWhiteSprite();
        float canvasScale = 1f / PixelsPerUnit;
        Vector2 panelPixelSize = new Vector2(PanelWorldWidth * PixelsPerUnit, PanelWorldHeight * PixelsPerUnit);

        GameObject anchorGo = new GameObject("MergeChanceAnchor");
        _anchor = anchorGo.transform;
        _anchor.SetParent(transform, false);

        GameObject canvasGo = new GameObject(
            "MergeChanceCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasRenderer));

        canvasGo.transform.SetParent(_anchor, false);

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = _camera != null ? _camera : Camera.main;
        canvas.overrideSorting = true;
        canvas.sortingOrder = SortingOrder;

        RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
        canvasRect.sizeDelta = panelPixelSize;
        canvasRect.pivot = new Vector2(0.5f, 0.5f);
        canvasGo.transform.localScale = Vector3.one * canvasScale;
        canvasGo.transform.localPosition = Vector3.zero;
        canvasGo.transform.localRotation = Quaternion.identity;

        GameObject panelGo = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(canvasGo.transform, false);
        RectTransform panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        _panelImage = panelGo.GetComponent<Image>();
        _panelImage.sprite = sprite;
        _panelImage.type = Image.Type.Sliced;
        _panelImage.raycastTarget = false;

        GameObject titleAreaGo = new GameObject("TitleArea", typeof(RectTransform), typeof(Image));
        titleAreaGo.transform.SetParent(panelGo.transform, false);
        RectTransform titleAreaRect = titleAreaGo.GetComponent<RectTransform>();
        titleAreaRect.anchorMin = new Vector2(0f, 0.42f);
        titleAreaRect.anchorMax = Vector2.one;
        titleAreaRect.offsetMin = new Vector2(8f, 0f);
        titleAreaRect.offsetMax = new Vector2(-8f, -8f);
        _titleAreaImage = titleAreaGo.GetComponent<Image>();
        _titleAreaImage.sprite = sprite;
        _titleAreaImage.raycastTarget = false;

        GameObject chanceAreaGo = new GameObject("ChanceArea", typeof(RectTransform), typeof(Image));
        chanceAreaGo.transform.SetParent(panelGo.transform, false);
        RectTransform chanceAreaRect = chanceAreaGo.GetComponent<RectTransform>();
        chanceAreaRect.anchorMin = Vector2.zero;
        chanceAreaRect.anchorMax = new Vector2(1f, 0.42f);
        chanceAreaRect.offsetMin = new Vector2(8f, 8f);
        chanceAreaRect.offsetMax = new Vector2(-8f, 0f);
        _chanceAreaImage = chanceAreaGo.GetComponent<Image>();
        _chanceAreaImage.sprite = sprite;
        _chanceAreaImage.raycastTarget = false;

        _titleText = CreateOutlinedText(titleAreaGo.transform, "TitleText", 52f, Color.white, new Vector2(0.5f, 0.5f));
        _chanceText = CreateOutlinedText(chanceAreaGo.transform, "ChanceText", 46f, new Color(1f, 0.92f, 0.2f), new Vector2(0.5f, 0.5f));

        ApplyTheme();
    }

    private void ApplyTheme()
    {
        if (!_built)
            return;

        TowerMergeUiPalette palette = TowerMergeUiThemes.Get(themeId);

        if (_panelImage != null)
            _panelImage.color = palette.panelColor;
        if (_titleAreaImage != null)
            _titleAreaImage.color = palette.titleAreaColor;
        if (_chanceAreaImage != null)
            _chanceAreaImage.color = palette.chanceAreaColor;
        if (_titleText != null)
        {
            _titleText.color = palette.titleTextColor;
            _titleText.outlineColor = palette.outlineColor;
        }

        if (_chanceText != null)
        {
            _chanceText.color = palette.chanceTextColor;
            _chanceText.outlineColor = palette.outlineColor;
        }
    }

    private static TextMeshProUGUI CreateOutlinedText(Transform parent, string name, float fontSize, Color color, Vector2 anchor)
    {
        GameObject textGo = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(parent, false);

        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = anchor;
        textRect.anchorMax = anchor;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(PanelWorldWidth * PixelsPerUnit - 16f, 72f);

        TextMeshProUGUI text = textGo.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.raycastTarget = false;
        text.outlineWidth = 0.35f;
        text.outlineColor = Color.black;
        UiFonts.ApplyNexon(text);
        return text;
    }

    private void AlignToCamera()
    {
        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null)
            return;

        _anchor.rotation = _camera.transform.rotation;
    }

    private static Sprite GetWhiteSprite()
    {
        if (_whiteSprite != null)
            return _whiteSprite;

        Texture2D texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[64];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;

        texture.SetPixels(pixels);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        _whiteSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 8f, 8f),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit,
            0,
            SpriteMeshType.FullRect,
            new Vector4(2f, 2f, 2f, 2f));

        return _whiteSprite;
    }
}
