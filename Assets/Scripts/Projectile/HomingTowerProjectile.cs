using Agents;

using CombatSystem;

using CoreSystem.EffectSystem;

using GGMLib.ModuleSystem;

using UnityEngine;



public class HomingTowerProjectile : MonoBehaviour
{
    private Transform _target;

    private GameObject _targetObject;

    private GameObject _sourcePrefab;

    private TowerProjectileDataSO _data;

    private TowerAttackVfxDataSO _hitVfx;

    private HitEffectDataSO _impactHitEffect;

    private ModuleOwner _attacker;

    private float _effectDamage;

    private float _spawnTime;

    private float _spawnGraceUntil;

    private bool _isActive;

    private int _enemyLayerMask;

    private bool _useFallbackAim;

    private Vector3 _fallbackAimPoint;

    private float _lastDistanceToTarget = float.MaxValue;

    private int _orbitStuckFrames;



    public void Launch(

        GameObject sourcePrefab,

        TowerProjectileDataSO data,

        Vector3 position,

        Quaternion rotation,

        Transform target,

        float effectDamage,

        TowerAttackVfxDataSO hitVfx,

        HitEffectDataSO impactHitEffect,

        ModuleOwner attacker,

        GameObject targetObject)

    {

        _sourcePrefab = sourcePrefab;

        _data = data;

        _effectDamage = effectDamage;

        _hitVfx = hitVfx;

        _impactHitEffect = impactHitEffect;

        _attacker = attacker;

        _spawnTime = Time.time;

        _spawnGraceUntil = _spawnTime + GetSpawnHitGraceDuration();

        _isActive = true;

        _useFallbackAim = false;
        _lastDistanceToTarget = float.MaxValue;
        _orbitStuckFrames = 0;

        _enemyLayerMask = 1 << LayerMask.NameToLayer("Enemy");



        BindTarget(target, targetObject);

        transform.SetParent(null, true);
        transform.localScale = Vector3.one;
        transform.SetPositionAndRotation(position, rotation);
        gameObject.SetActive(true);
    }

    private float GetSpawnHitGraceDuration()
    {
        if (_data == null || _data.spawnHitGraceDuration <= 0f)
            return 0f;

        return _data.spawnHitGraceDuration;
    }



    private void BindTarget(Transform target, GameObject targetObject)

    {

        Agent agent = null;



        if (targetObject != null)

            agent = targetObject.GetComponentInParent<Agent>();



        if (agent == null && target != null)

            agent = target.GetComponentInParent<Agent>();



        if (agent != null)

        {

            _targetObject = agent.gameObject;

            _target = agent.transform;

            return;

        }



        _targetObject = targetObject;

        _target = target;

    }



    private void Update()

    {

        if (!_isActive || _data == null)

            return;



        if (Time.time - _spawnTime > _data.maxLifetime)

        {

            ReturnToPool();

            return;

        }



        bool canHit = Time.time >= _spawnGraceUntil;

        if (canHit && TryHitAnyEnemy(out _))
        {
            OnHit();
            return;
        }

        if (!IsTargetValid())
        {
            if (_spawnGraceUntil > _spawnTime && Time.time < _spawnGraceUntil)
            {
                MoveForward(transform.forward);
                return;
            }

            BeginFallbackFlight();
            FlyTowardFallbackAim(canHit);
            return;
        }

        Vector3 aimPoint = GetPredictedAimPoint();
        _fallbackAimPoint = aimPoint;

        Vector3 toTarget = aimPoint - transform.position;
        float distance = toTarget.magnitude;
        float hitRadius = GetEffectiveHitRadius(distance);

        if (canHit && ShouldForceImpact(distance, toTarget, hitRadius))
        {
            OnHit();
            return;
        }

        TrackOrbitProgress(distance);

        if (canHit && _orbitStuckFrames >= 10)
        {
            OnHit();
            return;
        }

        if (canHit && distance <= hitRadius)
        {
            OnHit();
            return;
        }

        if (canHit && distance <= 0.001f)
        {
            OnHit();
            return;
        }



        Vector3 desiredDirection = toTarget / distance;



        if (distance <= _data.straightPursuitDistance)
        {
            MoveForward(desiredDirection);
            return;
        }

        float turnMultiplier = distance <= _data.straightPursuitDistance * 2f ? 2.5f : 1f;
        float maxRadians = _data.turnSpeedDegrees * turnMultiplier * Mathf.Deg2Rad * Time.deltaTime;

        Vector3 steeredDirection = Vector3.RotateTowards(

            transform.forward,

            desiredDirection,

            maxRadians,

            0f);



        if (steeredDirection.sqrMagnitude < 0.0001f)

            steeredDirection = desiredDirection;



        MoveForward(steeredDirection.normalized);

    }

    private void BeginFallbackFlight()
    {
        if (_useFallbackAim)
            return;

        _fallbackAimPoint = GetPredictedAimPoint();
        if (_fallbackAimPoint == transform.position)
            _fallbackAimPoint = transform.position + transform.forward * 2f;

        _useFallbackAim = true;
        _target = null;
        _targetObject = null;
    }

    private void FlyTowardFallbackAim(bool canHit)
    {
        Vector3 toPoint = _fallbackAimPoint - transform.position;
        float distance = toPoint.magnitude;
        float hitRadius = GetEffectiveHitRadius(distance);

        if (canHit && distance <= hitRadius)
        {
            OnHitAtPoint(_fallbackAimPoint);
            return;
        }

        if (distance <= 0.001f)
        {
            OnHitAtPoint(_fallbackAimPoint);
            return;
        }

        MoveForward(toPoint / distance);
    }



    private float GetEffectiveHitRadius(float distanceToTarget)

    {

        float baseRadius = _data.hitRadius;

        if (distanceToTarget <= _data.straightPursuitDistance)

            return baseRadius * 1.75f;



        return baseRadius;

    }



    private bool TryHitAnyEnemy(out Vector3 hitPoint)
    {
        hitPoint = default;

        float checkRadius = GetEffectiveHitRadius(0f);
        Collider[] hits = Physics.OverlapSphere(transform.position, checkRadius, _enemyLayerMask);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hit = hits[i];
            if (hit == null)
                continue;

            Agent agent = hit.GetComponentInParent<Agent>();
            if (agent != null && agent.IsDead)
                continue;

            hitPoint = hit.ClosestPoint(transform.position);
            _targetObject = agent != null ? agent.gameObject : hit.transform.root.gameObject;
            _target = _targetObject != null ? _targetObject.transform : hit.transform;
            return true;
        }

        return false;
    }



    private void MoveForward(Vector3 direction)

    {

        transform.rotation = Quaternion.LookRotation(direction);

        transform.position += direction * (_data.speed * Time.deltaTime);

    }



    private Vector3 GetPredictedAimPoint()
    {
        Vector3 basePoint = GetTargetAimPoint();
        float distance = Vector3.Distance(transform.position, basePoint);

        if (distance <= _data.straightPursuitDistance * 2.5f)
            return basePoint;

        Vector3 targetVelocity = GetTargetVelocity();
        if (targetVelocity.sqrMagnitude < 0.01f)
            return basePoint;

        float travelTime = distance / Mathf.Max(_data.speed, 0.01f);
        for (int i = 0; i < 2; i++)
        {
            basePoint = GetTargetAimPoint() + targetVelocity * travelTime;
            travelTime = Vector3.Distance(transform.position, basePoint) / Mathf.Max(_data.speed, 0.01f);
        }

        return basePoint;
    }

    private Vector3 GetTargetAimPoint()
    {
        if (TryGetAgent(out Agent agent))
            return AgentImpactPoints.GetBodyCenter(agent, GetFallbackAimHeight());

        if (_target != null)
            return _target.position + _data.aimHeightOffset;

        return _fallbackAimPoint;
    }

    private bool ShouldForceImpact(float distance, Vector3 toTarget, float hitRadius)
    {
        if (distance <= hitRadius * 1.25f)
            return true;

        if (distance > _data.straightPursuitDistance * 2.5f)
            return false;

        if (toTarget.sqrMagnitude < 0.0001f)
            return true;

        Vector3 flatToTarget = toTarget;
        flatToTarget.y = 0f;
        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;

        if (flatToTarget.sqrMagnitude < 0.01f || flatForward.sqrMagnitude < 0.01f)
            return distance <= hitRadius * 2f;

        float approachDot = Vector3.Dot(flatForward.normalized, flatToTarget.normalized);
        return approachDot < -0.15f;
    }

    private void TrackOrbitProgress(float distance)
    {
        if (distance <= _data.straightPursuitDistance * 3f
            && distance >= _lastDistanceToTarget - 0.03f)
        {
            _orbitStuckFrames++;
        }
        else
        {
            _orbitStuckFrames = 0;
        }

        _lastDistanceToTarget = distance;
    }



    private Vector3 GetTargetVelocity()

    {

        if (_targetObject == null)

            return Vector3.zero;



        SplineMove splineMove = _targetObject.GetComponentInParent<SplineMove>();

        if (splineMove != null)

            return splineMove.GetEstimatedWorldVelocity();



        return Vector3.zero;

    }



    private bool IsTargetValid()

    {

        if (_target == null || _targetObject == null)

            return false;



        if (!_targetObject.activeInHierarchy)

            return false;



        Agent agent = _targetObject.GetComponentInParent<Agent>();

        if (agent != null && agent.IsDead)

            return false;



        return true;

    }



    private void OnHit()
    {
        Vector3 impactPoint = ResolveImpactPoint();
        ApplyImpact(impactPoint, ResolveImpactRotation(impactPoint));
        ReturnToPool();
    }

    private void OnHitAtPoint(Vector3 impactPoint)
    {
        ApplyImpact(impactPoint, ResolveImpactRotation(impactPoint));
        ReturnToPool();
    }

    private void ApplyImpact(Vector3 impactPoint, Quaternion hitRotation)
    {
        if (_effectDamage > 0f)
            DealDamageToPrimaryTarget(impactPoint);

        if (_hitVfx != null && _effectDamage > 0f)
        {
            TowerAttackVfxSpawner.SpawnAtWorldPosition(
                _hitVfx,
                impactPoint,
                hitRotation,
                _effectDamage,
                _attacker,
                _targetObject);
        }

        if (ShouldPlayImpactHitEffect())
            HitVfxUtility.Play(_impactHitEffect, impactPoint, hitRotation);

        if (_hitVfx != null && _hitVfx.damageRadius > 0.5f)
            GameAudioManager.Instance?.PlayExplosion(0.9f);
    }

    private Quaternion ResolveImpactRotation(Vector3 impactPoint)
    {
        if (ResolveImpactPlacement() == VfxImpactPlacement.Ground)
            return Quaternion.identity;

        Vector3 direction = _target != null
            ? impactPoint - transform.position
            : transform.forward;
        if (direction.sqrMagnitude < 0.0001f && _target != null)
            direction = _target.position - transform.position;
        if (direction.sqrMagnitude < 0.0001f)
            direction = transform.forward;

        return Quaternion.LookRotation(direction.normalized);
    }



    private Vector3 ResolveImpactPoint()
    {
        if (TryGetAgent(out Agent agent))
            return AgentImpactPoints.Resolve(agent, ResolveImpactPlacement(), GetFallbackAimHeight());

        return transform.position;
    }

    private VfxImpactPlacement ResolveImpactPlacement()
    {
        bool useGround = _data != null && _data.useGroundImpactPoint;
        if (!useGround && _hitVfx != null && ShouldUseGroundImpact(_hitVfx))
            useGround = true;

        return AgentImpactPoints.ResolvePlacement(_hitVfx, useGround);
    }

    private static bool ShouldUseGroundImpact(TowerAttackVfxDataSO hitVfx)
    {
        return hitVfx.fastProjectileImpact || hitVfx.damageRadius >= 1f;
    }

    private float GetFallbackAimHeight()
    {
        return _data != null ? _data.aimHeightOffset.y : 1f;
    }



    private bool TryGetAgent(out Agent agent)

    {

        agent = null;



        if (_targetObject != null)

            agent = _targetObject.GetComponentInParent<Agent>();



        if (agent == null && _target != null)

            agent = _target.GetComponentInParent<Agent>();



        return agent != null;

    }



    private bool ShouldPlayImpactHitEffect()
    {
        if (_impactHitEffect == null || _impactHitEffect.effectPrefab == null)
            return false;

        if (UsesSameEffectPrefab(_impactHitEffect, _hitVfx))
            return false;

        // 범위 공격은 attackVfx가 이미 폭발 연출을 담당합니다.
        if (_hitVfx != null && _hitVfx.damageRadius >= 0.5f)
            return false;

        return true;
    }

    private static bool UsesSameEffectPrefab(HitEffectDataSO hitEffect, TowerAttackVfxDataSO attackVfx)
    {
        if (hitEffect == null || attackVfx == null)
            return false;

        return hitEffect.effectPrefab == attackVfx.effectPrefab;
    }

    private void DealDamageToPrimaryTarget(Vector3 hitPoint)

    {

        if (_targetObject == null || _attacker == null)

            return;



        Agent agent = _targetObject.GetComponentInParent<Agent>();

        if (agent != null)

        {

            if (!agent.IsDead)

                agent.ApplyDamage(new DamageData(_effectDamage, hitPoint, _attacker));

            return;

        }



        HealthModule health = _targetObject.GetComponentInChildren<HealthModule>();

        health?.ApplyDamage(_effectDamage);

    }



    private void ReturnToPool()
    {
        if (!_isActive)
            return;

        _isActive = false;

        _target = null;

        _targetObject = null;

        _useFallbackAim = false;
        _lastDistanceToTarget = float.MaxValue;
        _orbitStuckFrames = 0;

        TowerProjectileSpawner.ReturnToPool(_sourcePrefab, gameObject);

    }

}

