using CombatSystem;
using CoreSystem.EffectSystem;
using Agents;
using UnityEngine;

public class BowSkillModule : PlayerAttackSkill
{
    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private ParticleSystem dust;

    private GameObject _currentTarget;
    private bool _firedThisSkill;

    public override void InitializeSkill(
        ISkillModule skillModule)
    {
        base.InitializeSkill(skillModule);
        dust.Stop(/*true,ParticleSystemStopBehavior.StopEmittingAndClear*/);
    }

    public override bool CanUseSkill(
        GameObject target = null)
    {
        if (IsUsing)
            return false;

        if (NormalizedCooldown < 1f)
            return false;

        return true;
    }

    public override void UseSkill(
        GameObject target = null)
    {
        _currentTarget = target;
        _firedThisSkill = false;

        base.UseSkill(target);
        TryFireAndComplete();
    }

    protected override void OnDamageCast()
    {
        TryFireAndComplete();
    }

    private void TryFireAndComplete()
    {
        if (_firedThisSkill)
            return;

        if (_currentTarget == null)
            return;

        if (!_currentTarget.activeInHierarchy)
            return;

        Agent agent = _currentTarget.GetComponentInParent<Agent>();
        if (agent != null && agent.IsDead)
            return;

        _firedThisSkill = true;

        dust.Stop(/*true,ParticleSystemStopBehavior.StopEmittingAndClear*/);
        dust.Play();

        LaunchHomingProjectile(_currentTarget, firePoint);
        StopSkill();
    }

    protected override HitEffectDataSO GetProjectileImpactHitEffect() => null;

    public override void StopSkill()
    {
        _currentTarget = null;
        _firedThisSkill = false;

        base.StopSkill();
    }

    public override void ForceStopForDrag()
    {
        _currentTarget = null;
        _firedThisSkill = false;
        base.ForceStopForDrag();
    }
}
