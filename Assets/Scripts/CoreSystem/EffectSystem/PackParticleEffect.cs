using System.Collections;
using UnityEngine;

namespace CoreSystem.EffectSystem
{
    /// <summary>
    /// SpecialSkillsEffectsPack 프리팹을 풀에서 꺼내 재생하고, 종료 후 반환합니다.
    /// </summary>
    public class PackParticleEffect : MonoBehaviour
    {
        private ParticleSystem[] _particleSystems;
        private GameObject _sourcePrefab;
        private Coroutine _despawnRoutine;

        private void Awake()
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }

        public void Configure(GameObject sourcePrefab, float lifetimeSeconds, float scale)
        {
            _sourcePrefab = sourcePrefab;
            transform.localScale = Vector3.one * scale;

            if (_despawnRoutine != null)
            {
                StopCoroutine(_despawnRoutine);
                _despawnRoutine = null;
            }

            _despawnRoutine = StartCoroutine(DespawnAfter(lifetimeSeconds));
        }

        public void Play()
        {
            if (_particleSystems == null || _particleSystems.Length == 0)
                _particleSystems = GetComponentsInChildren<ParticleSystem>(true);

            gameObject.SetActive(true);

            foreach (ParticleSystem ps in _particleSystems)
            {
                if (ps == null)
                    continue;

                ParticleSystem.MainModule main = ps.main;
                main.loop = false;

                ps.Clear(true);
                ps.Play(true);
            }
        }

        private IEnumerator DespawnAfter(float lifetime)
        {
            yield return new WaitForSeconds(Mathf.Max(0.05f, lifetime));
            Finish();
        }

        private void Finish()
        {
            _despawnRoutine = null;

            foreach (ParticleSystem ps in _particleSystems)
            {
                if (ps != null)
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (VfxPoolManager.Instance != null && _sourcePrefab != null)
                VfxPoolManager.Instance.ReturnToPool(_sourcePrefab, gameObject);
            else
                Destroy(gameObject);
        }

        private void OnDisable()
        {
            if (_despawnRoutine != null)
            {
                StopCoroutine(_despawnRoutine);
                _despawnRoutine = null;
            }
        }
    }
}
