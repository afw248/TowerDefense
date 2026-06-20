using UnityEngine;

namespace Player
{
    /// <summary>
    /// CharacterController는 레이캐스트에 잡히지 않아, 타워 선택용 BoxCollider를 보장합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public class TowerSelectionHitbox : MonoBehaviour
    {
        [SerializeField] private Vector3 center = new(0f, 1.25f, 0f);
        [SerializeField] private Vector3 size = new(1.2f, 2.5f, 1.2f);

        private void Awake()
        {
            EnsureCollider();
        }

        public void EnsureCollider()
        {
            BoxCollider collider = GetComponent<BoxCollider>();
            if (collider == null)
                collider = gameObject.AddComponent<BoxCollider>();

            collider.isTrigger = false;
            collider.center = center;
            collider.size = size;
        }
    }
}
