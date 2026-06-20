using Agents;
using FSM;
using Player;
using UnityEngine;

public class PlayerFireState : AbstractPlayerState
{
    private Transform _currentTarget;
    private GameObject _currentTargetObject;

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

        if (!_player.CanAttack)
        {
            _player.ChangeState(PlayerState.IDLE, 0.05f);
            return;
        }

        _player.SkillModule.OnCurrentSkillEnd += HandleSkillEnd;

        FindTarget();
        TryExecuteSkill();
    }

    public override void Update()
    {
        base.Update();

        if (!_player.CanAttack)
        {
            _player.ChangeState(PlayerState.IDLE, 0.05f);
            return;
        }

        if (_currentTarget != null)
            RotateTowards(GetTargetAimPoint(_currentTarget));

        if (_pendingRemove || _isSkillActive)
            return;

        if (_currentTarget == null)
            return;

        if (!IsCurrentTargetValid())
        {
            if (!TryFindNewTarget())
            {
                ClearTarget();
                _player.ChangeState(PlayerState.IDLE, 0.05f);
            }

            return;
        }

        TryExecuteSkill();
    }

    public override void Exit()
    {
        base.Exit();

        _player.SkillModule.OnCurrentSkillEnd -= HandleSkillEnd;

        ClearTarget();
        _isSkillActive = false;
        _pendingRemove = false;
    }

    private void FindTarget()
    {
        int count = _player.Sensor.FindTargetsInRadius(_player.PlayerData.DetectRadius);
        if (count <= 0)
            return;

        if (TryResolveTarget(_player.Sensor.ColliderResults[0], out Transform target, out GameObject targetObject))
            SetTarget(target, targetObject);
    }

    private bool TryFindNewTarget()
    {
        int count = _player.Sensor.FindTargetsInRadius(_player.PlayerData.DetectRadius);
        if (count <= 0)
            return false;

        for (int i = 0; i < count; i++)
        {
            if (!TryResolveTarget(_player.Sensor.ColliderResults[i], out Transform target, out GameObject targetObject))
                continue;

            SetTarget(target, targetObject);
            return true;
        }

        return false;
    }

    private bool IsCurrentTargetValid()
    {
        if (_currentTarget == null || _currentTargetObject == null)
            return false;

        if (!_currentTargetObject.activeInHierarchy)
            return false;

        Agent targetAgent = _currentTargetObject.GetComponentInParent<Agent>();
        if (targetAgent != null && targetAgent.IsDead)
            return false;

        int count = _player.Sensor.FindTargetsInRadius(_player.PlayerData.DetectRadius);
        for (int i = 0; i < count; i++)
        {
            Collider col = _player.Sensor.ColliderResults[i];
            if (col == null)
                continue;

            if (col.transform == _currentTarget || col.gameObject == _currentTargetObject)
                return true;

            if (targetAgent != null)
            {
                Agent colAgent = col.GetComponentInParent<Agent>();
                if (colAgent == targetAgent)
                    return true;
            }
        }

        return false;
    }

    private void TryExecuteSkill()
    {
        if (_currentTargetObject == null)
            return;

        if (_player.SkillModule.CanUseSkill(0, _currentTargetObject))
        {
            _isSkillActive = true;
            _player.SkillModule.UseSkill(0, _currentTargetObject);
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

        ClearTarget();
        _player.ChangeState(PlayerState.IDLE, 0.05f);
    }

    public void RequestRemove()
    {
        if (_isSkillActive)
            _pendingRemove = true;
        else
            _player.ChangeState(PlayerState.REMOVE, 0.1f);
    }

    private static bool TryResolveTarget(Collider col, out Transform target, out GameObject targetObject)
    {
        target = null;
        targetObject = null;

        if (col == null || !col.gameObject.activeInHierarchy)
            return false;

        Agent agent = col.GetComponentInParent<Agent>();
        if (agent != null)
        {
            if (agent.IsDead)
                return false;

            target = agent.transform;
            targetObject = agent.gameObject;
            return true;
        }

        target = col.transform;
        targetObject = col.gameObject;
        return true;
    }

    private void SetTarget(Transform target, GameObject targetObject)
    {
        _currentTarget = target;
        _currentTargetObject = targetObject;
    }

    private void ClearTarget()
    {
        _currentTarget = null;
        _currentTargetObject = null;
    }

    private static Vector3 GetTargetAimPoint(Transform target)
    {
        if (target == null)
            return Vector3.zero;

        Agent agent = target.GetComponentInParent<Agent>();
        if (agent != null)
            return AgentImpactPoints.GetBodyCenter(agent, 1f);

        return target.position + Vector3.up;
    }
}
