using Agents;
using FSM;
using System;
using UnityEngine;

public class PlayerInstallState : AbstractPlayerState
{
    
    public PlayerInstallState(Agent agent, int clipHash) : base(agent, clipHash)
    {
    }
    public override void Enter(float transitionDuration, int layerIndex = 0)
    {
        base.Enter(transitionDuration, layerIndex);
        _renderer?.PlayClip(_stateClipHash, 0, transitionDuration, layerIndex);
        _player.Trigger.OnAnimationEnd += HandleRemoveAnimatioEnd;
    }
    public override void Exit()
    {
        base.Exit();
        _player.Trigger.OnAnimationEnd -= HandleRemoveAnimatioEnd;
    }

    private void HandleRemoveAnimatioEnd()
    {
        _player.ChangeState(PlayerState.IDLE,0.3f);
    }
}
