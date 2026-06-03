using Agents;
using Agents.FSM;
using Player;
using UnityEngine;

namespace FSM
{
    public abstract class AbstractPlayerState  : AgentState
    {
        protected AbstractPlayer _player;
        protected const float INPUT_DEADZONE = 0.1f;
        
        protected AbstractPlayerState(Agent agent, int clipHash) : base(agent, clipHash)
        {
            _player = agent as AbstractPlayer;
            Debug.Assert(_player != null, "플레이어 상태는 반드시 플레이어에게 붙어야 합니다.");
        }
        //protected void RotateTowards(Vector3 targetPosition)
        //{
        //    if (_player == null)
        //        return;

        //    // y축 고정
        //    //targetPosition.y = _player.transform.position.y;

        //    _player.transform.LookAt(targetPosition);
        //}
        protected void RotateTowards(Vector3 targetPosition, float rotationSpeed = 7f)
        {
            if (_player == null) return;

            Vector3 direction = targetPosition - _player.transform.position;
            direction.y = 0;

            if (direction.magnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            _player.transform.rotation = Quaternion.Lerp(
                _player.transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }
}