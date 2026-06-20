using Tower;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class GameplayCameraShake : MonoBehaviour
{
    public static GameplayCameraShake Instance { get; private set; }

    private const float ShakeFrequency = 28f;
    private const float RecoverPerSecond = 2.8f;
    private const float MinShakeInterval = 0.1f;

    [SerializeField] private Vector3 maxTranslation = new(0.14f, 0.1f, 0.08f);
    [SerializeField] private Vector3 maxAngular = new(1.2f, 1.4f, 0.8f);

    private Transform _shakeTarget;
    private Vector3 _baseLocalPosition;
    private Quaternion _baseLocalRotation;
    private float _currentShake;
    private float _seed;
    private float _nextShakeTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        _shakeTarget = transform;
        _seed = Random.value;
        CacheBasePose();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void LateUpdate()
    {
        if (!enabled || TitlePreviewMode.Active)
            return;

        if (_shakeTarget == null)
            return;

        float shake = Mathf.Pow(_currentShake, 1.35f);
        if (shake <= 0.001f)
        {
            _shakeTarget.localPosition = _baseLocalPosition;
            _shakeTarget.localRotation = _baseLocalRotation;
            _currentShake = 0f;
            return;
        }

        float time = Time.time * ShakeFrequency;
        _shakeTarget.localPosition = _baseLocalPosition + new Vector3(
            maxTranslation.x * (Mathf.PerlinNoise(_seed, time) * 2f - 1f),
            maxTranslation.y * (Mathf.PerlinNoise(_seed + 1f, time) * 2f - 1f),
            maxTranslation.z * (Mathf.PerlinNoise(_seed + 2f, time) * 2f - 1f)) * shake;

        _shakeTarget.localRotation = _baseLocalRotation * Quaternion.Euler(
            maxAngular.x * (Mathf.PerlinNoise(_seed + 3f, time) * 2f - 1f) * shake,
            maxAngular.y * (Mathf.PerlinNoise(_seed + 4f, time) * 2f - 1f) * shake,
            maxAngular.z * (Mathf.PerlinNoise(_seed + 5f, time) * 2f - 1f) * shake);

        _currentShake = Mathf.Clamp01(_currentShake - RecoverPerSecond * Time.deltaTime);
    }

    public static void RequestShake(float intensity)
    {
        if (Instance == null)
            EnsureOnMainCamera();

        Instance?.AddShake(intensity);
    }

    public static void RequestLegendaryAttackShake(TowerArchetype archetype)
    {
        // 석궁은 공속이 빠르면 쉐이크가 연속으로 들어가 어지러질 수 있어 제외합니다.
        if (archetype == TowerArchetype.Bow)
            return;

        float intensity = archetype switch
        {
            TowerArchetype.Culverin => 0.22f,
            TowerArchetype.Missile => 0.3f,
            _ => 0f,
        };

        if (intensity <= 0f)
            return;

        RequestShake(intensity);
    }

    public static void EnsureOnMainCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
            return;

        if (camera.GetComponent<GameplayCameraShake>() == null)
            camera.gameObject.AddComponent<GameplayCameraShake>();
    }

    public static void ReleaseForTitle()
    {
        if (Instance == null)
            return;

        Instance._currentShake = 0f;
        Instance.enabled = false;
    }

    public static void PrepareForGameplay()
    {
        EnsureOnMainCamera();
        if (Instance == null)
            return;

        Instance.enabled = true;
        Instance._currentShake = 0f;
        Instance.RecacheBasePose();
    }

    public void RecacheBasePose()
    {
        CacheBasePose();
    }

    private void CacheBasePose()
    {
        if (_shakeTarget == null)
            return;

        _baseLocalPosition = _shakeTarget.localPosition;
        _baseLocalRotation = _shakeTarget.localRotation;
    }

    private void AddShake(float intensity)
    {
        if (intensity <= 0f || Time.unscaledTime < _nextShakeTime)
            return;

        _nextShakeTime = Time.unscaledTime + MinShakeInterval;
        _currentShake = Mathf.Clamp01(Mathf.Max(_currentShake, intensity));
    }
}
