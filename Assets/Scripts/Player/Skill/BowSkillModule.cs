using CombatSystem;
using UnityEngine;

public class BowSkillModule : PlayerAttackSkill
{
    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private ParticleSystem dust;

    private GameObject _currentTarget;

    public override void InitializeSkill(
        ISkillModule skillModule)
    {
        base.InitializeSkill(skillModule);
        dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
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

        base.UseSkill(target);
    }

    protected override void OnDamageCast()
    {
        if (_currentTarget == null)
            return;

        if (!_currentTarget.activeInHierarchy)
            return;

        dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);

        dust.Play();

        Vector3 direction =(_currentTarget.transform.position- firePoint.position).normalized;

        Quaternion rotation =Quaternion.LookRotation(direction);

        _currentTarget.GetComponentInChildren<HealthModule>().ApplyDamage(_ownerPlayer.PlayerData.Attack);

        Debug.Log("화살 발사");
    }

    public override void StopSkill()
    {
        _currentTarget = null;

        base.StopSkill();
    }
}