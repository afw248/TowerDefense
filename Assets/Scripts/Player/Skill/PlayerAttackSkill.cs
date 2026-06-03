using Agents;
using CombatSystem;
using GGMLib.AnimatorSystem;
using Player;
using System;
using UnityEngine;

public abstract class PlayerAttackSkill : MonoBehaviour, ISkill
{
    [field: SerializeField]
    public SkillDataSO SkillData { get; private set; }

    [SerializeField]
    protected AnimParamSO skillParamSO;

    [SerializeField]
    protected float crossfadeDuration = 0.15f;

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

    protected virtual void ClearAnimationEvents()
    {
        if (_trigger == null)
            return;

        _trigger.OnAnimationEnd -= CompleteSkill;
        _trigger.OnDamageCast -= OnDamageCast;
    }
}