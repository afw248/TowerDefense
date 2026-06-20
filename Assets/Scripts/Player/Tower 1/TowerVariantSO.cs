using CoreSystem.EffectSystem;
using Player;
using UnityEngine;

namespace Tower
{
    /// <summary>
    /// 하나의 타워 변종(등급+종류)에 대한 모든 정적 데이터.
    /// 프리팹·스폰 테이블·UI가 동일 SO를 참조합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "TowerVariant", menuName = "Tower/Tower Variant", order = 1)]
    public class TowerVariantSO : ScriptableObject
    {
        public TowerGrade grade;
        public TowerArchetype archetype;

        [Header("Gameplay")]
        public PlayerDataSO playerData;
        public TowerAttackVfxDataSO attackVfx;

        [Header("Prefab")]
        public GameObject towerPrefab;

        [Header("Visual (FattyPoly)")]
        public GameObject fattyPolyVisualPrefab;

        [Header("UI")]
        public Sprite portrait;

        public string DisplayName => $"{grade} {archetype}";
    }
}
