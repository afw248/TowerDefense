using Agents;
using CombatSystem;
using GGMLib.AnimatorSystem;
using System.Net.NetworkInformation;
using UnityEngine;

public class Enemy : Agent
{
    [SerializeField] private AnimParamSO dieAnim;
    private IRenderer _renderer;
    private AgentTrigger _trigger;
    private float crossDuration = 0.15f;
    private bool _isDead;
    private CharacterController characterController;
    private SplineMove spline;

    protected override void InitializeModules()
    {
        base.InitializeModules();
        _trigger = GetModule<AgentTrigger>();
        _renderer = GetModule<IRenderer>();
        spline = GetComponent<SplineMove>();
        characterController = GetComponent<CharacterController>();
    }

    public void Remove()
    {
        if (_isDead) return;
        _isDead = true;

        if (spline != null)
        {
            spline.moveSpeed = 0;
            
            spline.enabled = false;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        _trigger.OnAnimationEnd -= HandleDieAnimationEnd;
        _trigger.OnAnimationEnd += HandleDieAnimationEnd;

        _renderer.PlayClip(dieAnim.ParamHash, 0, crossDuration);
    }

    private void HandleDieAnimationEnd()
    {
        _trigger.OnAnimationEnd -= HandleDieAnimationEnd;
        OnDeathComplete();
    }

    protected virtual void OnDeathComplete()
    {
        Destroy(gameObject);
    }

    protected virtual void OnDisable()
    {
        if (_trigger != null)
        {
            _trigger.OnAnimationEnd -= HandleDieAnimationEnd;
        }
        _isDead = false;
    }
}