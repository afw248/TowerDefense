using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitFlash : MonoBehaviour
{
    private static Material _flashMaterial;

    [SerializeField] private float flashDuration = 0.12f;

    private readonly struct FlashRenderer
    {
        public readonly Renderer Renderer;
        public readonly Material[] OriginalMaterials;

        public FlashRenderer(Renderer renderer, Material[] originalMaterials)
        {
            Renderer = renderer;
            OriginalMaterials = originalMaterials;
        }
    }

    private FlashRenderer[] _flashRenderers;
    private float _flashEndTime;
    private Coroutine _flashRoutine;
    private bool _isFlashing;

    public void Initialize()
    {
        CacheRenderers();
    }

    public void PlayFlash()
    {
        if (_flashRenderers == null || _flashRenderers.Length == 0)
            CacheRenderers();

        if (_flashRenderers == null || _flashRenderers.Length == 0)
            return;

        _flashEndTime = Time.time + flashDuration;

        if (!_isFlashing)
        {
            ApplyFlashMaterials();
            _isFlashing = true;
        }

        if (_flashRoutine == null)
            _flashRoutine = StartCoroutine(FlashRoutine());
    }

    private void CacheRenderers()
    {
        EnsureFlashMaterial();

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        List<FlashRenderer> targets = new(renderers.Length);

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            if (renderer is not (SkinnedMeshRenderer or MeshRenderer))
                continue;

            Material[] originals = renderer.sharedMaterials;
            if (originals == null || originals.Length == 0)
                continue;

            targets.Add(new FlashRenderer(renderer, originals));
        }

        _flashRenderers = targets.ToArray();
    }

    private static void EnsureFlashMaterial()
    {
        if (_flashMaterial != null)
            return;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        _flashMaterial = new Material(shader);
        if (_flashMaterial.HasProperty("_BaseColor"))
            _flashMaterial.SetColor("_BaseColor", Color.white);
        if (_flashMaterial.HasProperty("_Color"))
            _flashMaterial.SetColor("_Color", Color.white);
    }

    private void ApplyFlashMaterials()
    {
        for (int i = 0; i < _flashRenderers.Length; i++)
        {
            FlashRenderer target = _flashRenderers[i];
            if (target.Renderer == null)
                continue;

            int count = target.OriginalMaterials.Length;
            Material[] flashMaterials = new Material[count];
            for (int m = 0; m < count; m++)
                flashMaterials[m] = _flashMaterial;

            target.Renderer.materials = flashMaterials;
        }
    }

    private IEnumerator FlashRoutine()
    {
        while (Time.time < _flashEndTime)
            yield return null;

        RestoreMaterials();
        _isFlashing = false;
        _flashRoutine = null;
    }

    private void RestoreMaterials()
    {
        if (_flashRenderers == null)
            return;

        for (int i = 0; i < _flashRenderers.Length; i++)
        {
            FlashRenderer target = _flashRenderers[i];
            if (target.Renderer == null || target.OriginalMaterials == null)
                continue;

            target.Renderer.materials = target.OriginalMaterials;
        }
    }

    private void OnDisable()
    {
        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
            _flashRoutine = null;
        }

        if (_isFlashing)
        {
            RestoreMaterials();
            _isFlashing = false;
        }
    }
}
