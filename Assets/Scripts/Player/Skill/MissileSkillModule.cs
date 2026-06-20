using CombatSystem;
using CoreSystem.EffectSystem;
using UnityEngine;

public class MissileSkillModule : PlayerAttackSkill
{
    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private ParticleSystem launchFx;

    private GameObject _currentTarget;
    private bool _launchedThisSkill;

    public override void InitializeSkill(ISkillModule skillModule)
    {
        base.InitializeSkill(skillModule);

        if (launchFx != null)
            launchFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public override bool CanUseSkill(GameObject target = null)
    {
        if (IsUsing)
            return false;

        if (NormalizedCooldown < 1f)
            return false;

        return true;
    }

    public override void UseSkill(GameObject target = null)
    {
        _currentTarget = target;
        _launchedThisSkill = false;
        base.UseSkill(target);
    }

    protected override void OnDamageCast()
    {
        TryLaunchOnce();
    }

    protected override void CompleteSkill()
    {
        TryLaunchOnce();
        base.CompleteSkill();
    }

    private void TryLaunchOnce()
    {
        if (_launchedThisSkill)
            return;

        if (_currentTarget == null || !_currentTarget.activeInHierarchy)
            return;

        if (firePoint == null)
            return;

        _launchedThisSkill = true;

        if (launchFx != null)
        {
            launchFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            launchFx.Play();
        }

        LaunchHomingProjectile(_currentTarget, firePoint);
    }

    protected override HitEffectDataSO GetProjectileImpactHitEffect() => null;

    public override void StopSkill()
    {
        _currentTarget = null;

        base.StopSkill();
    }

    public override void ForceStopForDrag()
    {
        _currentTarget = null;
        base.ForceStopForDrag();
    }
}
