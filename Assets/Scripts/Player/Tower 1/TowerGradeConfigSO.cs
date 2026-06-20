using UnityEngine;

namespace Tower
{
    [CreateAssetMenu(fileName = "TowerGradeConfig", menuName = "Tower/Grade Config", order = 0)]
    public class TowerGradeConfigSO : ScriptableObject
    {
        public TowerGradeTierSettings[] tiers =
        {
            new() { grade = TowerGrade.Normal, spawnWeight = 57.6f, attackMultiplier = 1f, effectRadiusMultiplier = 1f, vfxScaleMultiplier = 1f, vfxLifetimeMultiplier = 1f, fattyPolySizeIndex = 0 },
            new() { grade = TowerGrade.Rare, spawnWeight = 29.3f, attackMultiplier = 1.442f, effectRadiusMultiplier = 1.15f, vfxScaleMultiplier = 1.2f, vfxLifetimeMultiplier = 1.1f, fattyPolySizeIndex = 1 },
            new() { grade = TowerGrade.Epic, spawnWeight = 12.6f, attackMultiplier = 2.08f, effectRadiusMultiplier = 1.35f, vfxScaleMultiplier = 1.45f, vfxLifetimeMultiplier = 1.2f, fattyPolySizeIndex = 2 },
            new() { grade = TowerGrade.Legendary, spawnWeight = 0.5f, attackMultiplier = 3f, effectRadiusMultiplier = 1.6f, vfxScaleMultiplier = 1.85f, vfxLifetimeMultiplier = 1.35f, fattyPolySizeIndex = 3 },
        };

        public bool TryGetTier(TowerGrade grade, out TowerGradeTierSettings tier)
        {
            foreach (TowerGradeTierSettings t in tiers)
            {
                if (t.grade != grade)
                    continue;

                tier = t;
                return true;
            }

            tier = null;
            return false;
        }
    }
}
