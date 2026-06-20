using Agents;
using CombatSystem;
using CoreSystem.EffectSystem;
using GGMLib.AnimatorSystem;
using Player;
using System;
using Tower;
using UnityEngine;

public abstract class PlayerAttackSkill : MonoBehaviour, ISkill
{
    [field: SerializeField]
    public SkillDataSO SkillData { get; private set; }

    [SerializeField]
    protected AnimParamSO skillParamSO;

    [SerializeField]
    protected float crossfadeDuration = 0.15f;

    [SerializeField]
    protected HitEffectDataSO hitEffect;

    [SerializeField]
    protected TowerAttackVfxDataSO attackVfx;

    [SerializeField]
    protected TowerProjectileDataSO projectileData;

    [SerializeField]
    protected LayerMask enemyLayer = 1 << 6;

    protected AbstractPlayer _ownerPlayer;
    protected float _lastUseTime;
    protected IRenderer _renderer;
    protected AgentTrigger _trigger;

    public bool IsUsing { get; set; }

    public float NormalizedCooldown =>
        Mathf.Approximately(SkillData.cooldown, 0)
        ? 1f
        : Mathf.Clamp01(
            (Time.time - _lastUseTime)
            / SkillData.cooldown);

    public event Action OnSkillEnd;

    public virtual void InitializeSkill(ISkillModule skillModule)
    {
        _ownerPlayer =
            skillModule.Owner as AbstractPlayer;
        _renderer =
            skillModule.Owner.GetModule<IRenderer>();
        _trigger =
            skillModule.Owner.GetModule<AgentTrigger>();
    }
    public abstract bool CanUseSkill(GameObject target = null);
    public virtual void UseSkill(GameObject target = null)
    {
        ClearAnimationEvents();

        _trigger.OnAnimationEnd += CompleteSkill;
        _trigger.OnDamageCast += OnDamageCast;
        IsUsing = true;
        _lastUseTime = Time.time;
        if (_renderer != null &&skillParamSO != null)
        {
            _renderer.PlayClip(skillParamSO.ParamHash,0,crossfadeDuration);
        }
    }
    protected virtual void CompleteSkill()
    {
        StopSkill();
    }
    protected abstract void OnDamageCast();
        public virtual void StopSkill()
        {
            IsUsing = false;

            ClearAnimationEvents();

            OnSkillEnd?.Invoke();
        }

        public virtual void ForceStopForDrag()
        {
            IsUsing = false;
            _lastUseTime = 0f;
            ClearAnimationEvents();
        }

    protected virtual void ClearAnimationEvents()
    {
        if (_trigger == null)
            return;

        _trigger.OnAnimationEnd -= CompleteSkill;
        _trigger.OnDamageCast -= OnDamageCast;
    }

    protected void PlayHitEffectOnTarget(Vector3 hitPoint, Quaternion rotation)
    {
        HitVfxUtility.Play(hitEffect, hitPoint, rotation);
    }

    /// <summary>
    /// 타겟 위치에 즉시 이펙트/데미지를 적용합니다. (석궁·미사일 등)
    /// </summary>
    protected void ExecuteInstantTowerAttack(GameObject target, Transform firePoint, float aimHeight = 1f)
    {
        if (target == null || firePoint == null || _ownerPlayer == null)
            return;

        Vector3 hitPoint = ResolveInstantHitPoint(target, firePoint, aimHeight);
        Vector3 direction = hitPoint - firePoint.position;
        if (direction.sqrMagnitude < 0.0001f)
            direction = firePoint.forward;

        ExecuteTowerAttack(target, hitPoint, Quaternion.LookRotation(direction.normalized));
    }

    private Vector3 ResolveInstantHitPoint(GameObject target, Transform firePoint, float aimHeight)
    {
        Agent agent = target.GetComponentInParent<Agent>();
        if (agent != null && attackVfx != null)
        {
            VfxImpactPlacement placement = attackVfx.impactPlacement;
            Vector3 point = AgentImpactPoints.Resolve(agent, placement, aimHeight);
            return ApplyLeadPrediction(target, firePoint, point);
        }

        return GetLeadHitPoint(target, firePoint, aimHeight);
    }

    private static Vector3 ApplyLeadPrediction(GameObject target, Transform firePoint, Vector3 point)
    {
        SplineMove splineMove = target.GetComponentInParent<SplineMove>();
        if (splineMove == null)
            return point;

        Vector3 velocity = splineMove.GetEstimatedWorldVelocity();
        if (velocity.sqrMagnitude < 0.01f)
            return point;

        float distance = firePoint != null
            ? Vector3.Distance(firePoint.position, point)
            : 0f;
        float leadTime = Mathf.Clamp(distance / 28f, 0.06f, 0.22f);
        return point + velocity * leadTime;
    }

    protected static Vector3 GetLeadHitPoint(
        GameObject target,
        Transform firePoint = null,
        float aimHeight = 1f)
    {
        Agent agent = target.GetComponentInParent<Agent>();
        Vector3 point = agent != null
            ? AgentImpactPoints.GetBodyCenter(agent, aimHeight)
            : target.transform.position + Vector3.up * aimHeight;

        SplineMove splineMove = target.GetComponentInParent<SplineMove>();
        if (splineMove == null)
            return point;

        Vector3 velocity = splineMove.GetEstimatedWorldVelocity();
        if (velocity.sqrMagnitude < 0.01f)
            return point;

        float distance = firePoint != null
            ? Vector3.Distance(firePoint.position, point)
            : 0f;
        float leadTime = Mathf.Clamp(distance / 28f, 0.06f, 0.22f);
        return point + velocity * leadTime;
    }

    /// <summary>
    /// 유도탄을 발사합니다. 명중 시 이펙트 데미지만 적용됩니다.
    /// </summary>
    protected void LaunchHomingProjectile(GameObject target, Transform firePoint)
    {
        if (target == null || firePoint == null || _ownerPlayer == null || projectileData == null)
            return;

        float baseDamage = _ownerPlayer.EffectiveAttack * SkillData.damageMultiplier;
        float effectDamage = attackVfx != null
            ? baseDamage * attackVfx.effectDamageMultiplier
            : baseDamage;

        Agent agent = target.GetComponentInParent<Agent>();
        GameObject targetRoot = agent != null ? agent.gameObject : target;
        Transform targetTransform = agent != null ? agent.transform : target.transform;

        TowerProjectileSpawner.Launch(
            projectileData,
            firePoint,
            targetTransform,
            effectDamage,
            attackVfx,
            GetProjectileImpactHitEffect(),
            _ownerPlayer,
            targetRoot);

        if (_ownerPlayer != null)
            GameAudioManager.Instance?.PlayTowerFire(_ownerPlayer.Archetype);

        if (_ownerPlayer != null && _ownerPlayer.Grade == TowerGrade.Legendary)
            GameplayCameraShake.RequestLegendaryAttackShake(_ownerPlayer.Archetype);
    }

    protected virtual HitEffectDataSO GetProjectileImpactHitEffect() => hitEffect;

    /// <summary>
    /// 직접 타격 데미지 + 이펙트 존 데미지를 분리해 적용합니다.
    /// </summary>
    protected void ExecuteTowerAttack(GameObject target, Vector3 hitPoint, Quaternion rotation)
    {
        if (target == null || _ownerPlayer == null)
            return;

        float baseDamage = _ownerPlayer.EffectiveAttack * SkillData.damageMultiplier;

        if (attackVfx == null)
        {
            DealDamageToTarget(target, hitPoint, baseDamage);
            PlayHitEffectOnTarget(hitPoint, rotation);
            return;
        }

        float directDamage = baseDamage * attackVfx.directDamageMultiplier;
        float effectDamage = baseDamage * attackVfx.effectDamageMultiplier;

        if (directDamage > 0f)
            DealDamageToTarget(target, hitPoint, directDamage);

        if (_ownerPlayer != null)
            GameAudioManager.Instance?.PlayTowerFire(_ownerPlayer.Archetype);

        if (_ownerPlayer != null && _ownerPlayer.Grade == TowerGrade.Legendary)
            GameplayCameraShake.RequestLegendaryAttackShake(_ownerPlayer.Archetype);

        TowerAttackVfxSpawner.Spawn(
            attackVfx,
            hitPoint,
            rotation,
            effectDamage,
            _ownerPlayer,
            target);
    }

    protected void DealDamageToTarget(GameObject target, Vector3 hitPoint, float damage)
    {
        if (target == null || _ownerPlayer == null || damage <= 0f)
            return;

        Agent agent = target.GetComponentInParent<Agent>();
        if (agent != null)
        {
            agent.ApplyDamage(new DamageData(damage, hitPoint, _ownerPlayer));
            return;
        }

        HealthModule health = target.GetComponentInChildren<HealthModule>();
        health?.ApplyDamage(damage);
    }
}