using FSM;
using GGMLib.ModuleSystem;
using UnityEngine;

namespace Agents
{
    public class AgentSensor : MonoBehaviour, IModule, ISensor
    {
        [SerializeField] private LayerMask whatIsTarget;
        [SerializeField] private LayerMask whatIsObstacle;
        [SerializeField] private int maxColliderCount = 5;

        private ModuleOwner _owner;
        private Collider[] _colliderResults;

        public Collider[] ColliderResults => _colliderResults;

        public void Initialize(ModuleOwner owner)
        {
            _owner = owner;
            Debug.Assert(maxColliderCount > 0, $"최대 컬라이더 수는 0보다 커야 합니다.: {gameObject}");
            _colliderResults = new Collider[maxColliderCount];
        }

        public bool IsTargetInViewAngle(Transform targetTrm, float viewAngle)
        {
            Vector3 direction = targetTrm.position - transform.position;
            direction.y = 0;
            float angle = Vector3.Angle(transform.forward, direction);
            return angle <= viewAngle * 0.5f;
        }

        public bool IsTargetInSight(Transform targetTrm)
        {
            Vector3 targetPosition = targetTrm.position;
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0;
            float distance = direction.magnitude;
            if (Physics.Raycast(transform.position, direction.normalized,
                    out RaycastHit hit, distance, whatIsObstacle))
            {
                Debug.Log(hit.collider.gameObject.name);
                return false;
            }

            return true;
        }

        private void OnDisable()
        {
            if (_colliderResults == null) return;

            for (int i = 0; i < _colliderResults.Length; i++)
            {
                _colliderResults[i] = null;
            }
        }

        public bool IsTargetInViewRadius(Transform targetTrm, float viewRadius)
            => (targetTrm.position - transform.position).sqrMagnitude <= viewRadius * viewRadius;

        public int FindTargetsInRadius(float viewRadius)
        {
            System.Array.Clear(_colliderResults, 0, _colliderResults.Length);
            return Physics.OverlapSphereNonAlloc(transform.position, viewRadius, _colliderResults, whatIsTarget);
        }
    }
}