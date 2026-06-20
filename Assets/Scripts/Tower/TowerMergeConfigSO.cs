using CoreSystem.EffectSystem;
using Tower;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerMergeConfig", menuName = "Tower/Merge Config", order = 2)]
public class TowerMergeConfigSO : ScriptableObject
{
    [System.Serializable]
    public class MergeTierSettings
    {
        public TowerGrade fromGrade = TowerGrade.Normal;

        [Range(0, 100)]
        public int successChancePercent = 90;

        public HitEffectDataSO successVfx;
        public HitEffectDataSO failureVfx;
    }

    public MergeTierSettings[] tiers =
    {
        new() { fromGrade = TowerGrade.Normal, successChancePercent = 60 },
        new() { fromGrade = TowerGrade.Rare, successChancePercent = 20 },
        new() { fromGrade = TowerGrade.Epic, successChancePercent = 5 },
    };

    public bool TryGetTier(TowerGrade fromGrade, out MergeTierSettings tier)
    {
        foreach (MergeTierSettings candidate in tiers)
        {
            if (candidate.fromGrade != fromGrade)
                continue;

            tier = candidate;
            return true;
        }

        tier = null;
        return false;
    }

    public int GetSuccessChancePercent(TowerGrade fromGrade)
    {
        return TryGetTier(fromGrade, out MergeTierSettings tier)
            ? tier.successChancePercent
            : 0;
    }
}
