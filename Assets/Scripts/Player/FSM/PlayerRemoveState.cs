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
        _renderer.PlayClip(_stateClipHash, 0, transitionDuration, layerIndex);
        _player.SkillModule.InvokeSkillEnd();
        _agentTrigger = _player.GetModule<AgentTrigger>();
        if (_agentTrigger != null)
        {
            _agentTrigger.OnAnimationEnd += HandleRemoveAnimationEnd;
        }
    }
    private void HandleRemoveAnimationEnd()
    {
        _player.gameObject.SetActive(false);
    }
    public override void Exit()
    {
        base.Exit();

        if (_agentTrigger != null)
        {
            _agentTrigger.OnAnimationEnd -= HandleRemoveAnimationEnd;
        }
    }
    //나중에 이거 DIsable해주기
}
