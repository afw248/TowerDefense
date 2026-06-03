using Agents;
using FSM;
using Player;
using UnityEngine;

public class PlayerFireState : AbstractPlayerState
{
    private Transform _currentTarget;

    private bool _isSkillActive;
    private bool _pendingRemove;

    public PlayerFireState(Agent agent, int clipHash) : base(agent, clipHash)
    {
    }

    public override void Enter(float transitionDuration, int layerIndex = 0)
    {
        base.Enter(transitionDuration, layerIndex);

        _isSkillActive = false;
        _pendingRemove = false;

        _player.SkillModule.OnCurrentSkillEnd += HandleSkillEnd;

        FindTarget();
        TryExecuteSkill();
    }
    public override void Update()
    {
        base.Update();

        if (_currentTarget != null)
        {
            RotateTowards(_currentTarget.position);
        }
    }
    public override void Exit()
    {
        base.Exit();

        _player.SkillModule.OnCurrentSkillEnd -= HandleSkillEnd;

        _currentTarget = null;

        _isSkillActive = false;
        _pendingRemove = false;
    }

    private void FindTarget()
    {
        int count = _player.Sensor.FindTargetsInRadius(_player.PlayerData.DetectRadius);

        if (count <= 0)
            return;

        Collider col = _player.Sensor.ColliderResults[0];

        if (col == null)
            return;

        _currentTarget = col.transform;
    }

    private bool TryFindNewTarget()
    {
        int count = _player.Sensor.FindTargetsInRadius(_player.PlayerData.DetectRadius);

        if (count <= 0)
            return false;

        for (int i = 0; i < count; i++)
        {
            Collider col = _player.Sensor.ColliderResults[i];

            if (col == null)
                continue;

            if (!col.gameObject.activeInHierarchy)
                continue;

            _currentTarget = col.transform;

            return true;
        }

        return false;
    }

    private bool IsCurrentTargetValid()
    {
        if (_currentTarget == null)
            return false;

        if (!_currentTarget.gameObject.activeInHierarchy)
            return false;

        int count = _player.Sensor.FindTargetsInRadius(_player.PlayerData.DetectRadius);

        for (int i = 0; i < count; i++)
        {
            Collider col = _player.Sensor.ColliderResults[i];

            if (col == null)
                continue;

            if (col.transform == _currentTarget)
            {
                return true;
            }
        }

        return false;
    }

    private void TryExecuteSkill()
    {
        if (_currentTarget == null)
            return;

        if (_player.SkillModule.CanUseSkill(0, _currentTarget.gameObject))
        {
            _isSkillActive = true;

            _player.SkillModule.UseSkill(0, _currentTarget.gameObject);
        }
    }

    private void HandleSkillEnd()
    {
        _isSkillActive = false;

        if (_pendingRemove)
        {
            _player.ChangeState(PlayerState.REMOVE, 0.1f);
            return;
        }

        if (IsCurrentTargetValid())
        {
            TryExecuteSkill();
            return;
        }

        if (TryFindNewTarget())
        {
            TryExecuteSkill();
            return;
        }

        _currentTarget = null;

        _player.ChangeState(PlayerState.IDLE, 0.2f);
    }

    public void RequestRemove()
    {
        if (_isSkillActive)
        {
            _pendingRemove = true;
        }
        else
        {
            _player.ChangeState(PlayerState.REMOVE, 0.1f);
        }
    }
}