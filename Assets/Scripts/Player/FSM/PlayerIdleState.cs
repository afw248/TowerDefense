using Agents;
using FSM;
using Player;
using UnityEngine;

public class PlayerIdleState : AbstractPlayerState
{
    private float _attackReadyTimer;

    private const float ATTACK_DELAY = 0.15f;

    public PlayerIdleState(Agent agent, int clipHash) : base(agent, clipHash)
    {
    }

    public override void Enter(float transitionDuration, int layerIndex = 0)
    {
        base.Enter(transitionDuration, layerIndex);
        _renderer.PlayClip(_stateClipHash, 0, transitionDuration, layerIndex);
        _attackReadyTimer = 0f;
    }

    public override void Update()
    {
        base.Update();

        CheckAttack();
    }

    public override void Exit()
    {
        base.Exit();
    }

    private void CheckAttack()
    {
        int detectedCount = _player.Sensor.FindTargetsInRadius(_player.PlayerData.DetectRadius);

        if (detectedCount <= 0)
        {
            _attackReadyTimer = 0f;
            return;
        }

        Collider targetCollider = _player.Sensor.ColliderResults[0];

        if (targetCollider == null)
        {
            _attackReadyTimer = 0f;
            return;
        }

        if (!targetCollider.gameObject.activeInHierarchy)
        {
            _attackReadyTimer = 0f;
            return;
        }

        _attackReadyTimer += Time.deltaTime;

        if (_attackReadyTimer < ATTACK_DELAY)
            return;

        if (_player.SkillModule.CanUseSkill(0, targetCollider.gameObject))
        {
            _attackReadyTimer = 0f;

            _player.ChangeState(PlayerState.FIRE, 0.1f);
        }
    }
}