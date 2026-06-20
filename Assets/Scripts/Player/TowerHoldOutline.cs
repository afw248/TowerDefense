using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Player
{
    [DisallowMultipleComponent]
    public class TowerHoldOutline : MonoBehaviour
    {
        [SerializeField] private Material outlineMaterialTemplate;
        [SerializeField] private Color outlineColor = new(1f, 0.92f, 0.35f, 1f);
        [SerializeField] private Color dragOutlineColor = new(0.2f, 1f, 1f, 1f);
        [SerializeField] private float outlineWidth = 0.035f;
        [SerializeField] private float dragOutlineWidth = 0.095f;
        [SerializeField] private float pulseSpeed = 4f;
        [SerializeField] private float pulseMin = 0.65f;
        [SerializeField] private float pulseMax = 1.15f;
        [SerializeField] private float dragPulseMax = 1.55f;

        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");

        private Material _outlineMaterial;
        private readonly List<Renderer> _outlineRenderers = new();
        private MaterialPropertyBlock _propertyBlock;
        private bool _isActive;
        private bool _initialized;
        private Color _defaultOutlineColor;
        private float _defaultOutlineWidth;
        private float _defaultPulseMax;
        private bool _dragHighlightActive;

        private void Awake()
        {
            _defaultOutlineColor = outlineColor;
            _defaultOutlineWidth = outlineWidth;
            _defaultPulseMax = pulseMax;
            _propertyBlock = new MaterialPropertyBlock();
            EnsureInitialized();
            SetOutlineEnabled(false);
        }

        private void OnDestroy()
        {
            if (_outlineMaterial != null)
                Destroy(_outlineMaterial);

            for (int i = 0; i < _outlineRenderers.Count; i++)
            {
                if (_outlineRenderers[i] != null)
                    Destroy(_outlineRenderers[i].gameObject);
            }

            _outlineRenderers.Clear();
        }

        private void Update()
        {
            if (!_isActive || _outlineMaterial == null || _propertyBlock == null)
                return;

            float pulse = Mathf.Lerp(pulseMin, pulseMax, (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
            Color color = outlineColor * pulse;

            _propertyBlock.SetColor(OutlineColorId, color);
            _propertyBlock.SetFloat(OutlineWidthId, outlineWidth);

            for (int i = 0; i < _outlineRenderers.Count; i++)
            {
                Renderer renderer = _outlineRenderers[i];
                if (renderer != null && renderer.enabled)
                    renderer.SetPropertyBlock(_propertyBlock);
            }
        }

        public void EnsureInitialized()
        {
            if (_initialized)
                return;

            if (_propertyBlock == null)
                _propertyBlock = new MaterialPropertyBlock();

            _initialized = true;
            EnsureOutlineMaterial();
            BuildOutlineRenderers();
        }

        public void SetOutlineEnabled(bool enabled)
        {
            EnsureInitialized();

            _isActive = enabled && _outlineMaterial != null;

            for (int i = 0; i < _outlineRenderers.Count; i++)
            {
                if (_outlineRenderers[i] != null)
                    _outlineRenderers[i].enabled = _isActive;
            }

            if (_isActive)
                Update();
        }

        public void SetOutlineColor(Color color)
        {
            outlineColor = color;
            if (_dragHighlightActive)
                outlineWidth = dragOutlineWidth;

            if (_isActive)
                Update();
        }

        public void ResetOutlineColor()
        {
            if (_dragHighlightActive)
                outlineColor = dragOutlineColor;
            else
                outlineColor = _defaultOutlineColor;

            if (_isActive)
                Update();
        }

        public void SetDragHighlight(bool enabled)
        {
            _dragHighlightActive = enabled;

            if (enabled)
            {
                outlineColor = dragOutlineColor;
                outlineWidth = dragOutlineWidth;
                pulseMax = dragPulseMax;
                SetOutlineEnabled(true);
                return;
            }

            outlineColor = _defaultOutlineColor;
            outlineWidth = _defaultOutlineWidth;
            pulseMax = _defaultPulseMax;
        }

        public void SetOutlineWidth(float width)
        {
            outlineWidth = width;
            if (_isActive)
                Update();
        }

        public void ResetOutlineWidth()
        {
            outlineWidth = _defaultOutlineWidth;
            if (_isActive)
                Update();
        }

        private void EnsureOutlineMaterial()
        {
            if (_outlineMaterial != null)
                return;

            if (outlineMaterialTemplate == null)
                outlineMaterialTemplate = Resources.Load<Material>("TowerSelectionOutline");

            if (outlineMaterialTemplate != null)
            {
                _outlineMaterial = new Material(outlineMaterialTemplate)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                return;
            }

            Shader shader = Shader.Find("TowerDefense/SelectionOutline");
            if (shader == null)
            {
                Debug.LogWarning("TowerDefense/SelectionOutline shader not found. Selection outline disabled.");
                return;
            }

            _outlineMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        private void BuildOutlineRenderers()
        {
            if (_outlineMaterial == null)
                return;

            Renderer[] sources = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < sources.Length; i++)
            {
                Renderer source = sources[i];
                if (!IsValidOutlineSource(source))
                    continue;

                Renderer outlineRenderer = CreateOutlineRenderer(source);
                if (outlineRenderer != null)
                    _outlineRenderers.Add(outlineRenderer);
            }
        }

        private static bool IsValidOutlineSource(Renderer source)
        {
            if (source == null)
                return false;

            if (source is ParticleSystemRenderer or TrailRenderer or LineRenderer)
                return false;

            if (source.gameObject.name.EndsWith("_SelectionOutline"))
                return false;

            if (source.gameObject.name.StartsWith("Eff_"))
                return false;

            if (source is SkinnedMeshRenderer skinned)
                return skinned.sharedMesh != null;

            MeshFilter filter = source.GetComponent<MeshFilter>();
            return filter != null && filter.sharedMesh != null;
        }

        private Renderer CreateOutlineRenderer(Renderer source)
        {
            GameObject outlineObject = new($"{source.gameObject.name}_SelectionOutline");
            outlineObject.transform.SetParent(source.transform, false);
            outlineObject.layer = source.gameObject.layer;

            if (source is SkinnedMeshRenderer skinnedSource)
            {
                SkinnedMeshRenderer outlineSkinned = outlineObject.AddComponent<SkinnedMeshRenderer>();
                outlineSkinned.sharedMesh = skinnedSource.sharedMesh;
                outlineSkinned.bones = skinnedSource.bones;
                outlineSkinned.rootBone = skinnedSource.rootBone;
                outlineSkinned.updateWhenOffscreen = true;
                outlineSkinned.sharedMaterial = _outlineMaterial;
                outlineSkinned.shadowCastingMode = ShadowCastingMode.Off;
                outlineSkinned.receiveShadows = false;
                outlineSkinned.allowOcclusionWhenDynamic = false;
                outlineSkinned.enabled = false;
                return outlineSkinned;
            }

            MeshFilter sourceFilter = source.GetComponent<MeshFilter>();
            MeshFilter outlineFilter = outlineObject.AddComponent<MeshFilter>();
            outlineFilter.sharedMesh = sourceFilter.sharedMesh;

            MeshRenderer outlineMesh = outlineObject.AddComponent<MeshRenderer>();
            outlineMesh.sharedMaterial = _outlineMaterial;
            outlineMesh.shadowCastingMode = ShadowCastingMode.Off;
            outlineMesh.receiveShadows = false;
            outlineMesh.allowOcclusionWhenDynamic = false;
            outlineMesh.enabled = false;
            return outlineMesh;
        }
    }
}
