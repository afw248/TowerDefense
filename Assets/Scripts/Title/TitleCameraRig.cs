using DG.Tweening;
using UnityEngine;

public class TitleCameraRig : MonoBehaviour
{
    [SerializeField] private Vector3 focusPoint = new(9f, 0f, 1.5f);
    [SerializeField] private float orbitRadius = 18f;
    [SerializeField] private float orbitHeight = 24f;
    [SerializeField] private float orbitSpeed = 6f;
    [SerializeField] private float lookAtHeight = 2f;
    [SerializeField] private float orthographicSize = 26f;
    [SerializeField] private float snapshotOrbitAngle = 40f;

    private float _orbitAngle;
    private bool _orbitEnabled = true;
    private Vector3 _snapshotPosition;
    private Quaternion _snapshotRotation;
    private Tween _cameraTween;

    private void Awake()
    {
        Camera camera = GetComponent<Camera>();
        if (camera == null)
            camera = gameObject.AddComponent<Camera>();

        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = GameplayViewSettings.TitleOrthographicSize;
        orthographicSize = GameplayViewSettings.TitleOrthographicSize;
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.backgroundColor = new Color(0.36f, 0.58f, 0.88f, 1f);

        if (GetComponent<AudioListener>() == null)
            gameObject.AddComponent<AudioListener>();

        RefreshSnapshotFromAngle(snapshotOrbitAngle);
        _orbitAngle = 0f;
        ResumeTitleOrbit();
    }

    private void OnDestroy()
    {
        _cameraTween?.Kill();
    }

    private void Update()
    {
        if (!_orbitEnabled)
            return;

        _orbitAngle += orbitSpeed * Time.deltaTime;
        ApplyOrbitPosition();
    }

    public void ResumeTitleOrbit()
    {
        _cameraTween?.Kill();
        _orbitEnabled = true;
        _orbitAngle = snapshotOrbitAngle;

        Camera camera = GetComponent<Camera>();
        if (camera != null)
        {
            camera.orthographicSize = GameplayViewSettings.TitleOrthographicSize;
            orthographicSize = GameplayViewSettings.TitleOrthographicSize;
        }

        ApplyOrbitPosition();
        GameplayCameraShake.ReleaseForTitle();
    }

    public Tween TweenToSnapshotView(float duration)
    {
        _orbitEnabled = false;
        _cameraTween?.Kill();

        Camera camera = GetComponent<Camera>();
        Vector3 targetPosition = GameplayViewSettings.ResolveGameplayCameraPosition();
        Quaternion targetRotation = GameplayViewSettings.GameplayCameraRotation;
        float targetOrtho = GameplayViewSettings.OrthographicSize;

        _snapshotPosition = targetPosition;
        _snapshotRotation = targetRotation;

        Sequence sequence = DOTween.Sequence();
        sequence.Join(transform.DOMove(targetPosition, duration).SetEase(Ease.InOutCubic));
        sequence.Join(transform.DORotateQuaternion(targetRotation, duration).SetEase(Ease.InOutCubic));

        if (camera != null)
        {
            sequence.Join(DOTween.To(
                () => camera.orthographicSize,
                value => camera.orthographicSize = value,
                targetOrtho,
                duration).SetEase(Ease.InOutCubic));
            sequence.OnComplete(() => orthographicSize = targetOrtho);
        }

        _cameraTween = sequence;
        return sequence;
    }

    public void SnapToGameplayView()
    {
        _orbitEnabled = false;
        _cameraTween?.Kill();

        Vector3 targetPosition = GameplayViewSettings.ResolveGameplayCameraPosition();
        Quaternion targetRotation = GameplayViewSettings.GameplayCameraRotation;
        _snapshotPosition = targetPosition;
        _snapshotRotation = targetRotation;
        transform.SetPositionAndRotation(targetPosition, targetRotation);

        Camera camera = GetComponent<Camera>();
        if (camera != null)
        {
            camera.orthographicSize = GameplayViewSettings.OrthographicSize;
            orthographicSize = GameplayViewSettings.OrthographicSize;
        }
    }

    public void ApplyGameCameraSettings(Camera sourceCamera)
    {
        if (sourceCamera == null)
            return;

        Camera camera = GetComponent<Camera>();
        camera.orthographic = sourceCamera.orthographic;
        camera.orthographicSize = sourceCamera.orthographicSize;
        orthographicSize = sourceCamera.orthographicSize;
    }

    public void RefreshSnapshotFromAngle(float angleDegrees)
    {
        snapshotOrbitAngle = angleDegrees;

        float radians = angleDegrees * Mathf.Deg2Rad;
        Vector3 focus = EffectiveFocusPoint;
        Vector3 offset = new(
            Mathf.Sin(radians) * orbitRadius,
            orbitHeight,
            Mathf.Cos(radians) * orbitRadius);

        _snapshotPosition = focus + offset;
        Vector3 lookTarget = focus + Vector3.up * lookAtHeight;
        _snapshotRotation = Quaternion.LookRotation(lookTarget - _snapshotPosition, Vector3.up);
    }

    private void ApplyOrbitPosition()
    {
        float radians = _orbitAngle * Mathf.Deg2Rad;
        Vector3 focus = EffectiveFocusPoint;
        Vector3 offset = new(
            Mathf.Sin(radians) * orbitRadius,
            orbitHeight,
            Mathf.Cos(radians) * orbitRadius);

        transform.position = focus + offset;
        transform.rotation = Quaternion.LookRotation(
            focus + Vector3.up * lookAtHeight - transform.position,
            Vector3.up);
    }

    private Vector3 EffectiveFocusPoint =>
        focusPoint + new Vector3(0f, GameplayViewSettings.FocusPanY, GameplayViewSettings.FocusPanZ);

#if UNITY_EDITOR
    private void OnValidate()
    {
        RefreshSnapshotFromAngle(snapshotOrbitAngle);
    }
#endif
}
