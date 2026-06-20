using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// <summary>
/// 정보창 Portrait 영역에 타워 비주얼 프리팹을 생성하고, 전용 URP 카메라로 RenderTexture에 렌더합니다.
/// </summary>
public class TowerInfoVisualPreview : MonoBehaviour
{
    private const int PreviewLayer = 9;
    private static readonly Vector3 PreviewWorldOffset = new(0f, -5000f, 0f);

    [SerializeField] private RawImage rawImage;
    [SerializeField] private Vector3 modelRotation = new(0f, 145f, 0f);
    [SerializeField] private float modelScale = 1f;
    [SerializeField] private float previewFill = 0.72f;

    private GameObject _currentVisual;
    private Transform _stageRoot;
    private Camera _previewCamera;
    private RenderTexture _renderTexture;
    private Coroutine _refreshRoutine;
    private static bool _mainCameraMaskAdjusted;

    public bool ShowVisual(GameObject visualPrefab)
    {
        ClearVisualInstance();

        if (visualPrefab == null)
            return false;

        EnsurePreviewStage();

        _currentVisual = Instantiate(visualPrefab, _stageRoot);
        PrepareVisualForDisplay(_currentVisual);

        if (!HasRenderableGeometry(_currentVisual))
        {
            Destroy(_currentVisual);
            _currentVisual = null;
            return false;
        }

        UpdateRendererBounds(_currentVisual);
        FitModelToStage(_currentVisual.transform);
        FrameCameraToModel(_currentVisual.transform);
        ActivatePreviewOutput();
        RenderPreviewImmediate();
        return true;
    }

    public void ClearVisual()
    {
        ClearVisualInstance();
        DeactivatePreviewOutput();
    }

    public void BindRawImage(RawImage image)
    {
        rawImage = image;
        if (_renderTexture != null && rawImage != null && _previewCamera != null && _previewCamera.enabled)
            rawImage.texture = _renderTexture;
    }

    private void Awake()
    {
        rawImage ??= GetComponent<RawImage>();
    }

    private void OnDestroy()
    {
        ClearVisual();

        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
            _renderTexture = null;
        }

        if (_stageRoot != null)
            Destroy(_stageRoot.gameObject);
    }

    private void ClearVisualInstance()
    {
        if (_refreshRoutine != null)
        {
            StopCoroutine(_refreshRoutine);
            _refreshRoutine = null;
        }

        if (_currentVisual != null)
        {
            Destroy(_currentVisual);
            _currentVisual = null;
        }
    }

    private void ActivatePreviewOutput()
    {
        EnsureRenderTexture();

        if (_previewCamera != null)
            _previewCamera.enabled = true;

        if (rawImage != null)
        {
            rawImage.color = Color.white;
            rawImage.texture = _renderTexture;
        }

        if (_refreshRoutine != null)
            StopCoroutine(_refreshRoutine);

        if (isActiveAndEnabled)
            _refreshRoutine = StartCoroutine(RefreshPreviewNextFrame());
        else
            RenderPreviewImmediate();
    }

    private void DeactivatePreviewOutput()
    {
        if (_refreshRoutine != null)
        {
            StopCoroutine(_refreshRoutine);
            _refreshRoutine = null;
        }

        if (_previewCamera != null)
            _previewCamera.enabled = false;

        if (rawImage != null)
            rawImage.texture = null;
    }

    private IEnumerator RefreshPreviewNextFrame()
    {
        yield return null;
        UpdateRendererBounds(_currentVisual);
        if (_currentVisual != null)
            FrameCameraToModel(_currentVisual.transform);

        RenderPreviewImmediate();
        _refreshRoutine = null;
    }

    private void RenderPreviewImmediate()
    {
        if (_previewCamera == null || _renderTexture == null)
            return;

        _previewCamera.Render();

        if (rawImage != null)
            rawImage.texture = _renderTexture;
    }

    private void EnsurePreviewStage()
    {
        EnsureMainCameraIgnoresPreviewLayer();

        if (_stageRoot != null)
            return;

        GameObject stageGo = new GameObject("TowerInfoPreviewStage");
        stageGo.transform.SetParent(null, false);
        stageGo.transform.position = PreviewWorldOffset;
        stageGo.hideFlags = HideFlags.HideAndDontSave;
        _stageRoot = stageGo.transform;

        GameObject cameraGo = new GameObject("TowerInfoPreviewCamera");
        cameraGo.transform.SetParent(_stageRoot, false);
        cameraGo.transform.localPosition = new Vector3(0f, 1.1f, -2.8f);
        cameraGo.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
        cameraGo.layer = PreviewLayer;

        _previewCamera = cameraGo.AddComponent<Camera>();
        _previewCamera.clearFlags = CameraClearFlags.SolidColor;
        _previewCamera.backgroundColor = new Color(0.88f, 0.9f, 0.94f, 1f);
        _previewCamera.cullingMask = 1 << PreviewLayer;
        _previewCamera.nearClipPlane = 0.05f;
        _previewCamera.farClipPlane = 20f;
        _previewCamera.fieldOfView = 28f;
        _previewCamera.depth = -100f;
        _previewCamera.enabled = false;

        UniversalAdditionalCameraData urpData = cameraGo.AddComponent<UniversalAdditionalCameraData>();
        urpData.renderType = CameraRenderType.Base;
        urpData.renderPostProcessing = false;
        urpData.requiresColorTexture = false;
        urpData.requiresDepthTexture = false;

        GameObject ambientGo = new GameObject("TowerInfoPreviewAmbient");
        ambientGo.transform.SetParent(_stageRoot, false);
        ambientGo.layer = PreviewLayer;

        Light ambientLight = ambientGo.AddComponent<Light>();
        ambientLight.type = LightType.Directional;
        ambientLight.intensity = 0.35f;
        ambientLight.color = new Color(0.85f, 0.9f, 1f, 1f);
        ambientLight.transform.localRotation = Quaternion.Euler(55f, 25f, 0f);
        ambientLight.cullingMask = 1 << PreviewLayer;

        GameObject keyLightGo = new GameObject("TowerInfoPreviewKeyLight");
        keyLightGo.transform.SetParent(_stageRoot, false);
        keyLightGo.transform.localRotation = Quaternion.Euler(42f, -35f, 0f);
        keyLightGo.layer = PreviewLayer;

        Light keyLight = keyLightGo.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.intensity = 1.15f;
        keyLight.cullingMask = 1 << PreviewLayer;

        GameObject fillLightGo = new GameObject("TowerInfoPreviewFillLight");
        fillLightGo.transform.SetParent(_stageRoot, false);
        fillLightGo.transform.localRotation = Quaternion.Euler(20f, 140f, 0f);
        fillLightGo.layer = PreviewLayer;

        Light fillLight = fillLightGo.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.intensity = 0.55f;
        fillLight.cullingMask = 1 << PreviewLayer;

        EnsureRenderTexture();
    }

    private void EnsureRenderTexture()
    {
        if (_renderTexture != null)
            return;

        _renderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32)
        {
            antiAliasing = 2,
            name = "TowerInfoPreviewRT"
        };
        _renderTexture.Create();

        if (_previewCamera != null)
            _previewCamera.targetTexture = _renderTexture;
    }

    private void FrameCameraToModel(Transform modelTransform)
    {
        if (_previewCamera == null || modelTransform == null)
            return;

        Bounds bounds = CalculateRendererBounds(modelTransform);
        if (bounds.size.sqrMagnitude <= 0.0001f)
            return;

        Vector3 center = bounds.center;
        float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z, 0.05f);
        float distance = maxExtent * 2.4f / Mathf.Tan(_previewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        distance = Mathf.Clamp(distance, 1.2f, 8f);

        Vector3 viewDir = (_previewCamera.transform.position - center).normalized;
        if (viewDir.sqrMagnitude < 0.0001f)
            viewDir = new Vector3(0f, 0.15f, -1f).normalized;

        _previewCamera.transform.position = center + viewDir * distance;
        _previewCamera.transform.LookAt(center);
    }

    private static void EnsureMainCameraIgnoresPreviewLayer()
    {
        if (_mainCameraMaskAdjusted)
            return;

        Camera main = Camera.main;
        if (main == null)
            return;

        main.cullingMask &= ~(1 << PreviewLayer);
        _mainCameraMaskAdjusted = true;
    }

    private void FitModelToStage(Transform modelTransform)
    {
        modelTransform.localRotation = Quaternion.Euler(modelRotation);
        modelTransform.localPosition = Vector3.zero;
        modelTransform.localScale = Vector3.one * modelScale;

        Bounds bounds = CalculateRendererBounds(modelTransform);
        if (bounds.size.sqrMagnitude <= 0.0001f)
            return;

        Vector3 localCenter = _stageRoot.InverseTransformPoint(bounds.center);
        modelTransform.localPosition -= localCenter;

        bounds = CalculateRendererBounds(modelTransform);
        float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
        if (maxExtent > 0.001f)
            modelTransform.localScale = Vector3.one * (previewFill / (maxExtent * 2f)) * modelScale;
    }

    private static void UpdateRendererBounds(GameObject root)
    {
        if (root == null)
            return;

        foreach (SkinnedMeshRenderer skinnedMesh in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            skinnedMesh.updateWhenOffscreen = true;
        }

        foreach (Animator animator in root.GetComponentsInChildren<Animator>(true))
            animator.Update(0f);
    }

    private static void PrepareVisualForDisplay(GameObject root)
    {
        SetLayerRecursively(root, PreviewLayer);

        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
            collider.enabled = false;

        foreach (CharacterController controller in root.GetComponentsInChildren<CharacterController>(true))
            controller.enabled = false;

        foreach (ParticleSystem particleSystem in root.GetComponentsInChildren<ParticleSystem>(true))
        {
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystemRenderer particleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            if (particleRenderer != null)
                particleRenderer.enabled = false;
        }

        foreach (SkinnedMeshRenderer skinnedMesh in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            skinnedMesh.updateWhenOffscreen = true;
        }

        foreach (Animator animator in root.GetComponentsInChildren<Animator>(true))
        {
            animator.enabled = true;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.Play(0, 0, 0f);
            animator.Update(0f);
        }
    }

    private static void SetLayerRecursively(GameObject root, int layer)
    {
        root.layer = layer;
        foreach (Transform child in root.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private static bool HasRenderableGeometry(GameObject root)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null || !renderer.enabled || renderer is ParticleSystemRenderer)
                continue;

            return true;
        }

        return false;
    }

    private static Bounds CalculateRendererBounds(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;
        Bounds bounds = default;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !renderer.enabled || renderer is ParticleSystemRenderer)
                continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return hasBounds ? bounds : new Bounds(root.position, Vector3.zero);
    }
}
