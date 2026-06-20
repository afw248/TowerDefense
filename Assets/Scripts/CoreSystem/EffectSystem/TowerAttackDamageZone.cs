using System.Collections.Generic;
using Agents;
using CombatSystem;
using GGMLib.ModuleSystem;
using UnityEngine;

namespace CoreSystem.EffectSystem
{
    /// <summary>
    /// 이펙트 주변 Enemy(CharacterController)에게 거리 판정으로 데미지를 적용합니다.
    /// </summary>
    public class TowerAttackDamageZone : MonoBehaviour
    {
        private float _radius;
        private float _damageAmount;
        private float _tickInterval;
        private float _nextTickTime;
        private float _endTime;
        private ModuleOwner _attacker;
        private GameObject _primaryTarget;
        private bool _includePrimaryInEffectDamage;
        private LayerMask _enemyMask;
        private readonly HashSet<Agent> _damagedAgents = new();
        private Collider[] _overlapBuffer;

        private const int OverlapBufferSize = 32;

        public void Initialize(
            float radius,
            float damageAmount,
            float lifetime,
            float tickInterval,
            ModuleOwner attacker,
            GameObject primaryTarget,
            bool includePrimaryInEffectDamage,
            LayerMask enemyMask)
        {
            _radius = radius;
            _damageAmount = damageAmount;
            _tickInterval = tickInterval;
            _attacker = attacker;
            _primaryTarget = primaryTarget;
            _includePrimaryInEffectDamage = includePrimaryInEffectDamage;
            _enemyMask = enemyMask;
            _endTime = Time.time + lifetime;
            _nextTickTime = Time.time;
            _damagedAgents.Clear();
            _overlapBuffer ??= new Collider[OverlapBufferSize];

            if (_tickInterval <= 0f)
                ApplyDamageOnce();
        }

        private void Update()
        {
            if (Time.time >= _endTime)
                return;

            if (_tickInterval <= 0f)
                return;

            if (Time.time < _nextTickTime)
                return;

            _nextTickTime = Time.time + _tickInterval;
            ApplyDamageTick();
        }

        private void ApplyDamageOnce()
        {
            ApplyDamageInRadius(trackPerAgent: true);
        }

        private void ApplyDamageTick()
        {
            ApplyDamageInRadius(trackPerAgent: false);
        }

        private void ApplyDamageInRadius(bool trackPerAgent)
        {
            if (_overlapBuffer == null)
                _overlapBuffer = new Collider[OverlapBufferSize];

            int count = Physics.OverlapSphereNonAlloc(
                transform.position,
                _radius,
                _overlapBuffer,
                _enemyMask);

            for (int i = 0; i < count; i++)
            {
                Collider hit = _overlapBuffer[i];
                if (hit == null)
                    continue;

                Agent agent = hit.GetComponentInParent<Agent>();
                if (agent == null || agent.IsDead)
                    continue;

                if (!IsEnemyLayer(agent.gameObject.layer))
                    continue;

                if (!_includePrimaryInEffectDamage && _primaryTarget != null
                    && agent.gameObject == _primaryTarget)
                    continue;

                if (trackPerAgent && _damagedAgents.Contains(agent))
                    continue;

                Vector3 agentCenter = GetAgentDamageCenter(agent);
                float sqrDist = (agentCenter - transform.position).sqrMagnitude;
                if (sqrDist > _radius * _radius)
                    continue;

                agent.ApplyDamage(new DamageData(_damageAmount, agentCenter, _attacker));

                if (trackPerAgent)
                    _damagedAgents.Add(agent);
            }
        }

        private bool IsEnemyLayer(int layer)
        {
            return (_enemyMask.value & (1 << layer)) != 0;
        }

        private static Vector3 GetAgentDamageCenter(Agent agent)
        {
            return AgentImpactPoints.GetBodyCenter(agent);
        }
    }
}
