using System.Collections;
using GGMLib.ModuleSystem;
using UnityEngine;

namespace CoreSystem.EffectSystem
{
    public class TowerAttackVfxInstance : MonoBehaviour
    {
        private GameObject _sourcePrefab;
        private ParticleSystem[] _particleSystems;
        private TowerAttackDamageZone _damageZone;
        private Coroutine _despawnRoutine;

        private void Awake()
        {
            _damageZone = GetComponent<TowerAttackDamageZone>();
        }

        public void Play(
            GameObject sourcePrefab,
            TowerAttackVfxDataSO data,
            float effectDamage,
            ModuleOwner attacker,
            GameObject primaryTarget,
            LayerMask enemyMask)
        {
            if (data == null)
                return;

            if (_despawnRoutine != null)
            {
                StopCoroutine(_despawnRoutine);
                _despawnRoutine = null;
            }

            _sourcePrefab = sourcePrefab;
            transform.localScale = Vector3.one * data.scale;

            RestartParticles(Mathf.Max(0.1f, data.playbackSpeed));

            if (_damageZone == null)
                _damageZone = gameObject.AddComponent<TowerAttackDamageZone>();

            _damageZone.enabled = true;
            _damageZone.Initialize(
                data.damageRadius,
                effectDamage,
                data.lifetime,
                data.damageTickInterval,
                attacker,
                primaryTarget,
                data.includePrimaryInEffectDamage,
                enemyMask);

            _despawnRoutine = StartCoroutine(DespawnAfter(data.lifetime));
        }

        private void RestartParticles(float playbackSpeed)
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            if (_particleSystems == null)
                return;

            foreach (ParticleSystem ps in _particleSystems)
            {
                if (ps == null)
                    continue;

                ParticleSystem.MainModule main = ps.main;
                main.loop = false;

                if (playbackSpeed > 0.01f && !Mathf.Approximately(playbackSpeed, 1f))
                    main.simulationSpeed = playbackSpeed;

                ps.Clear(true);
                ps.Play(true);
            }

            ParticleSystemRenderer[] renderers = GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (ParticleSystemRenderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                renderer.enabled = true;
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

            if (_damageZone != null)
                _damageZone.enabled = false;

            StopAllParticles();

            if (_sourcePrefab != null && VfxPoolManager.Instance != null)
                VfxPoolManager.Instance.ReturnToPool(_sourcePrefab, gameObject);
            else
                Destroy(gameObject);
        }

        private void StopAllParticles()
        {
            if (_particleSystems == null)
                _particleSystems = GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem ps in _particleSystems)
            {
                if (ps != null)
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
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
