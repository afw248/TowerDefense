using Player;
using UnityEngine;

namespace Tower
{
    /// <summary>
    /// 타워 프리팹에 붙여 어떤 등급/변종인지 표시하고, 런타임에 Variant SO를 조회합니다.
    /// </summary>
    public class TowerVariantReference : MonoBehaviour
    {
        [SerializeField]
        private TowerVariantSO variant;

        public TowerVariantSO Variant => variant;
        public TowerGrade Grade => variant != null ? variant.grade : TowerGrade.Normal;
        public TowerArchetype Archetype => variant != null ? variant.archetype : TowerArchetype.Bow;

        private void Reset()
        {
            AbstractPlayer player = GetComponent<AbstractPlayer>();
            if (player == null)
                return;

            variant = null;
        }
    }
}
