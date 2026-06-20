using System;
using UnityEngine;

namespace Tower
{
    [Serializable]
    public class TowerGradeTierSettings
    {
        public TowerGrade grade;

        [Tooltip("타일 스폰 가중치 (0~100, 등급별 상대 비율)")]
        [Range(0f, 100f)]
        public float spawnWeight = 10f;

        [Min(0.1f)]
        public float attackMultiplier = 1f;

        [Min(0.1f)]
        public float effectRadiusMultiplier = 1f;

        [Min(0.1f)]
        public float vfxScaleMultiplier = 1f;

        [Min(0.1f)]
        public float vfxLifetimeMultiplier = 1f;

        [Tooltip("FattyPoly 0=Small, 3=XL")]
        [Range(0, 3)]
        public int fattyPolySizeIndex;
    }
}
