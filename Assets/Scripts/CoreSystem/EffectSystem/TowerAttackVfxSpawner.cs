using GGMLib.ModuleSystem;
using UnityEngine;

namespace CoreSystem.EffectSystem
{
    public static class TowerAttackVfxSpawner
    {
        private static LayerMask _enemyMask = -1;
        private static bool _enemyMaskInitialized;

        public static void Spawn(
            TowerAttackVfxDataSO data,
            Vector3 position,
            Quaternion rotation,
            float effectDamage,
            ModuleOwner attacker,
            GameObject primaryTarget)
        {
            SpawnInternal(data, position, rotation, effectDamage, attacker, primaryTarget, attachToTarget: data.followPrimaryTarget);
        }

        /// <summary>
        /// 명중 지점 월드 좌표에 이펙트를 재생합니다. (투사체 명중 등)
        /// </summary>
        public static void SpawnAtWorldPosition(
            TowerAttackVfxDataSO data,
            Vector3 position,
            Quaternion rotation,
            float effectDamage,
            ModuleOwner attacker,
            GameObject primaryTarget)
        {
            if (data == null || data.effectPrefab == null || effectDamage <= 0f)
                return;

            bool useFastVisual = data.fastProjectileImpact || data.damageRadius < 0.5f;
            if (useFastVisual)
            {
                SpawnFastProjectileImpact(
                    data,
                    position,
                    rotation,
                    effectDamage,
                    attacker,
                    primaryTarget,
                    spawnDamageZone: data.damageRadius >= 0.5f);
                return;
            }

            SpawnInternal(data, position, rotation, effectDamage, attacker, primaryTarget, attachToTarget: false);
        }

        private static void SpawnInternal(
            TowerAttackVfxDataSO data,
            Vector3 position,
            Quaternion rotation,
            float effectDamage,
            ModuleOwner attacker,
            GameObject primaryTarget,
            bool attachToTarget)
        {
            if (data == null || data.effectPrefab == null || effectDamage <= 0f)
                return;

            EnsureEnemyMask();
            VfxPoolManager.EnsureExists();

            if (VfxPoolManager.Instance == null)
                return;

            GameObject instance = VfxPoolManager.Instance.GetInstance(data.effectPrefab);
            if (instance == null)
                return;

            Transform followTarget = attachToTarget && primaryTarget != null
                ? primaryTarget.transform
                : null;

            if (followTarget != null)
            {
                Vector3 spawnPosition = followTarget.TransformPoint(data.positionOffset);
                instance.transform.SetParent(followTarget, true);
                instance.transform.SetPositionAndRotation(spawnPosition, rotation);
            }
            else
            {
                instance.transform.SetParent(null);
                instance.transform.SetPositionAndRotation(position + data.positionOffset, rotation);
            }

            TowerAttackVfxInstance vfx = instance.GetComponent<TowerAttackVfxInstance>();
            if (vfx == null)
                vfx = instance.AddComponent<TowerAttackVfxInstance>();

            vfx.Play(
                data.effectPrefab,
                data,
                effectDamage,
                attacker,
                primaryTarget,
                _enemyMask);
        }

        private static void SpawnFastProjectileImpact(
            TowerAttackVfxDataSO data,
            Vector3 position,
            Quaternion rotation,
            float effectDamage,
            ModuleOwner attacker,
            GameObject primaryTarget,
            bool spawnDamageZone = true)
        {
            if (data == null || data.effectPrefab == null || effectDamage <= 0f)
                return;

            EnsureEnemyMask();
            VfxPoolManager.EnsureExists();

            if (VfxPoolManager.Instance == null)
                return;

            Vector3 spawnPosition = position + data.positionOffset;

            GameObject visual = VfxPoolManager.Instance.GetInstance(data.effectPrefab);
            if (visual != null)
            {
                visual.transform.SetParent(null);
                visual.transform.SetPositionAndRotation(spawnPosition, rotation);

                PackParticleEffect player = visual.GetComponent<PackParticleEffect>();
                if (player == null)
                    player = visual.AddComponent<PackParticleEffect>();

                ApplyPlaybackSpeed(visual, data.playbackSpeed);
                player.Configure(data.effectPrefab, data.lifetime, data.scale);
                player.Play();
            }

            if (spawnDamageZone)
            {
                SpawnDamageZone(
                    spawnPosition,
                    data,
                    effectDamage,
                    attacker,
                    primaryTarget);
            }
        }

        private static void SpawnDamageZone(
            Vector3 position,
            TowerAttackVfxDataSO data,
            float effectDamage,
            ModuleOwner attacker,
            GameObject primaryTarget)
        {
            GameObject zoneObject = new GameObject("TowerProjectileDamageZone");
            zoneObject.transform.position = position;

            TowerAttackDamageZone zone = zoneObject.AddComponent<TowerAttackDamageZone>();
            zone.Initialize(
                data.damageRadius,
                effectDamage,
                data.lifetime,
                data.damageTickInterval,
                attacker,
                primaryTarget,
                data.includePrimaryInEffectDamage,
                _enemyMask);

            Object.Destroy(zoneObject, Mathf.Max(0.05f, data.lifetime));
        }

        private static void ApplyPlaybackSpeed(GameObject root, float playbackSpeed)
        {
            float speed = Mathf.Max(0.1f, playbackSpeed);
            if (Mathf.Approximately(speed, 1f))
                return;

            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in particleSystems)
            {
                if (ps == null)
                    continue;

                ParticleSystem.MainModule main = ps.main;
                main.loop = false;
                main.simulationSpeed = speed;
            }
        }

        private static void EnsureEnemyMask()
        {
            if (_enemyMaskInitialized)
                return;

            int enemyLayer = LayerMask.NameToLayer("Enemy");
            _enemyMask = enemyLayer >= 0 ? (1 << enemyLayer) : Physics.DefaultRaycastLayers;
            _enemyMaskInitialized = true;
        }
    }
}
