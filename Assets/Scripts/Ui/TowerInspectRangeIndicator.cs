using Player;
using Tower;
using UnityEngine;

public class TowerInspectRangeIndicator : MonoBehaviour
{
    private const int Segments = 72;
    private const float GroundOffset = 0.1f;
    private const float DetectLineWidth = 0.14f;
    private const float EffectLineWidth = 0.1f;

    private static TowerInspectRangeIndicator _instance;

    private LineRenderer _detectRing;
    private LineRenderer _effectRing;
    private AbstractPlayer _target;
    private float _detectRadius;
    private float _effectRadius;

    public static TowerInspectRangeIndicator Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            GameObject host = new GameObject(nameof(TowerInspectRangeIndicator));
            _instance = host.AddComponent<TowerInspectRangeIndicator>();
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

        _detectRing = CreateRing(transform, "DetectRangeRing", DetectLineWidth);
        _effectRing = CreateRing(transform, "EffectRangeRing", EffectLineWidth);
        Hide();
    }

    public void Show(AbstractPlayer tower)
    {
        if (tower == null)
        {
            Hide();
            return;
        }

        _target = tower;

        if (!TowerCombatStats.TryGet(tower, out TowerCombatStats.Snapshot stats))
        {
            Hide();
            return;
        }

        TowerInfoUiPalette palette = TowerInfoUiThemes.Get(tower.Grade);
        Color detectColor = palette.rangeRingColor;
        Color effectColor = new Color(palette.accentTextColor.r, palette.accentTextColor.g, palette.accentTextColor.b, 0.55f);

        _detectRadius = stats.DetectRadius;
        _effectRadius = stats.IsAreaAttack && stats.EffectRadius > 0f ? stats.EffectRadius : 0f;

        ConfigureRing(_detectRing, detectColor, _detectRadius > 0f);
        ConfigureRing(_effectRing, effectColor, _effectRadius > 0f);

        UpdateRingPositions();
    }

    public void Hide()
    {
        _target = null;
        _detectRadius = 0f;
        _effectRadius = 0f;

        if (_detectRing != null)
            _detectRing.enabled = false;

        if (_effectRing != null)
            _effectRing.enabled = false;
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        UpdateRingPositions();
    }

    private void UpdateRingPositions()
    {
        if (_target == null)
            return;

        Vector3 center = GetTowerGroundCenter(_target);

        if (_detectRing != null && _detectRing.enabled)
            FillCircle(_detectRing, center, _detectRadius);

        if (_effectRing != null && _effectRing.enabled)
            FillCircle(_effectRing, center, _effectRadius);
    }

    private static Vector3 GetTowerGroundCenter(AbstractPlayer tower)
    {
        Vector3 position = tower.transform.position;
        position.y = tower.PlacementGroundY + GroundOffset;
        return position;
    }

    private static void ConfigureRing(LineRenderer ring, Color color, bool visible)
    {
        if (ring == null)
            return;

        ring.enabled = visible;
        if (!visible)
            return;

        ring.startColor = color;
        ring.endColor = color;
    }

    private static void FillCircle(LineRenderer ring, Vector3 center, float radius)
    {
        if (ring == null || radius <= 0f)
            return;

        ring.positionCount = Segments;

        for (int i = 0; i < Segments; i++)
        {
            float angle = i / (float)Segments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            ring.SetPosition(i, center + new Vector3(x, 0f, z));
        }
    }

    private static LineRenderer CreateRing(Transform parent, string name, float width)
    {
        GameObject ringGo = new GameObject(name);
        ringGo.transform.SetParent(parent, false);

        LineRenderer line = ringGo.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.textureMode = LineTextureMode.Stretch;
        line.alignment = LineAlignment.View;
        line.widthCurve = AnimationCurve.Constant(0f, 1f, width);
        line.numCornerVertices = 4;
        line.numCapVertices = 4;
        line.material = CreateLineMaterial();
        line.enabled = false;
        return line;
    }

    private static Material _lineMaterial;

    private static Material CreateLineMaterial()
    {
        if (_lineMaterial != null)
            return _lineMaterial;

        Shader shader = Shader.Find("Sprites/Default")
            ?? Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Unlit/Color");

        _lineMaterial = shader != null
            ? new Material(shader)
            : new Material(Shader.Find("Hidden/Internal-Colored"));

        if (_lineMaterial.HasProperty("_Color"))
            _lineMaterial.color = Color.white;

        return _lineMaterial;
    }
}
