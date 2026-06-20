using CombatSystem;
using UnityEngine;

public class CulverinSkill : PlayerAttackSkill
{
    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private ParticleSystem dust;

    [SerializeField]
    private Transform recoilDustAnchor;

    private GameObject _currentTarget;
    private Transform _resolvedRecoilAnchor;

    public override void InitializeSkill(
        ISkillModule skillModule)
    {
        base.InitializeSkill(skillModule);
        _resolvedRecoilAnchor = ResolveRecoilDustAnchor();
        StopRecoilDust();
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

        PlayRecoilDust();
        LaunchHomingProjectile(_currentTarget, firePoint);
    }

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

    private void PlayRecoilDust()
    {
        if (dust == null)
            return;

        StopRecoilDust();

        Transform anchor = recoilDustAnchor != null ? recoilDustAnchor : _resolvedRecoilAnchor;
        Transform dustTransform = dust.transform;

        ParticleSystem.MainModule main = dust.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        if (anchor != null)
        {
            dustTransform.SetPositionAndRotation(anchor.position, anchor.rotation);
        }
        else if (_ownerPlayer != null)
        {
            Vector3 groundPos = _ownerPlayer.transform.position + Vector3.up * 0.05f;
            dustTransform.SetPositionAndRotation(groundPos, Quaternion.identity);
        }
        else
        {
            dustTransform.position = transform.position;
        }

        dust.Play();
    }

    private void StopRecoilDust()
    {
        if (dust == null)
            return;

        dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private Transform ResolveRecoilDustAnchor()
    {
        if (recoilDustAnchor != null)
            return recoilDustAnchor;

        Transform searchRoot = _ownerPlayer != null ? _ownerPlayer.transform : transform.root;
        foreach (Transform child in searchRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains("Wheel"))
                return child;
        }

        return searchRoot;
    }
}
