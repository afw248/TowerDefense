using Agents;
using FSM;
using Player;
using System;
using UnityEngine;

public class PlayerRemoveState : AbstractPlayerState
{
    private AgentTrigger _agentTrigger;
    public PlayerRemoveState(Agent agent, int clipHash) : base(agent, clipHash)
    {

    }
    public override void Enter(float transitionDuration, int layerIndex = 0)
    {
        base.Enter(transitionDuration, layerIndex);
        _renderer?.PlayClip(_stateClipHash, 0, transitionDuration, layerIndex);
        _player.SkillModule.StopCurrentSkill();
        _player.SkillModule.InvokeSkillEnd();
        _agentTrigger = _player.GetModule<AgentTrigger>();
        if (_agentTrigger != null)
        {
            _agentTrigger.OnAnimationEnd += HandleRemoveAnimationEnd;
        }
    }
    private void HandleRemoveAnimationEnd()
    {
        _player.Remove();
    }
    public override void Exit()
    {
        base.Exit();

        if (_agentTrigger != null)
        {
            _agentTrigger.OnAnimationEnd -= HandleRemoveAnimationEnd;
        }
    }
    //���߿� �̰� DIsable���ֱ�
}
