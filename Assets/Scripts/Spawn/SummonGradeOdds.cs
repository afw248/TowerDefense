using System.Collections.Generic;
using System.Text;
using Tower;
using UnityEngine;

public static class SummonGradeOdds
{
    public static TowerGrade GetMaxSpawnGradeForWave(int wave)
    {
        if (wave < 10)
            return TowerGrade.Rare;

        if (wave <= 20)
            return TowerGrade.Epic;

        return TowerGrade.Legendary;
    }

    public static void CollectEffectiveWeights(
        AllPlayerListSO allTowers,
        int wave,
        EconomyConfigSO config,
        int summonUpgradeLevel,
        List<GradeList> eligibleGrades,
        List<float> effectiveWeights)
    {
        eligibleGrades?.Clear();
        effectiveWeights?.Clear();

        if (allTowers == null || allTowers.towerList == null)
            return;

        TowerGrade maxGrade = GetMaxSpawnGradeForWave(wave);
        int normalIndex = -1;
        float upgradeBonusTotal = 0f;

        foreach (GradeList gradeList in allTowers.towerList)
        {
            if (!TryGetGrade(gradeList, out TowerGrade towerGrade) || towerGrade > maxGrade)
                continue;

            float weight = ApplyEarlyPenalty(gradeList.weight, towerGrade, wave, config);
            eligibleGrades.Add(gradeList);
            effectiveWeights.Add(weight);

            if (towerGrade == TowerGrade.Normal)
                normalIndex = eligibleGrades.Count - 1;
        }

        if (config == null || summonUpgradeLevel <= 0)
            return;

        for (int i = 0; i < eligibleGrades.Count; i++)
        {
            if (!TryGetGrade(eligibleGrades[i], out TowerGrade towerGrade))
                continue;

            float bonus = summonUpgradeLevel * towerGrade switch
            {
                TowerGrade.Rare => config.summonRareBonusPerLevel,
                TowerGrade.Epic => config.summonEpicBonusPerLevel,
                TowerGrade.Legendary => config.summonLegendaryBonusPerLevel,
                _ => 0f
            };

            if (bonus <= 0f)
                continue;

            effectiveWeights[i] += bonus;
            upgradeBonusTotal += bonus;
        }

        if (normalIndex >= 0 && upgradeBonusTotal > 0f)
            effectiveWeights[normalIndex] = Mathf.Max(0f, effectiveWeights[normalIndex] - upgradeBonusTotal);
    }

    public static GradeList PickRandomGrade(
        AllPlayerListSO allTowers,
        int wave,
        EconomyConfigSO config,
        int summonUpgradeLevel)
    {
        CollectEffectiveWeights(allTowers, wave, config, summonUpgradeLevel, _gradeBuffer, _weightBuffer);

        if (_gradeBuffer.Count == 0)
            return GetFallbackGradeList(allTowers, TowerGrade.Normal);

        float totalWeight = 0f;
        foreach (float weight in _weightBuffer)
            totalWeight += weight;

        if (totalWeight <= 0f)
            return GetFallbackGradeList(allTowers, TowerGrade.Normal);

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < _gradeBuffer.Count; i++)
        {
            currentWeight += _weightBuffer[i];
            if (randomValue < currentWeight)
                return _gradeBuffer[i];
        }

        return _gradeBuffer[_gradeBuffer.Count - 1];
    }

    public static string FormatDisplayText(
        AllPlayerListSO allTowers,
        int wave,
        EconomyConfigSO config,
        int summonUpgradeLevel)
    {
        CollectEffectiveWeights(allTowers, wave, config, summonUpgradeLevel, _gradeBuffer, _weightBuffer);

        if (_gradeBuffer.Count == 0)
            return string.Empty;

        float totalWeight = 0f;
        foreach (float weight in _weightBuffer)
            totalWeight += weight;

        if (totalWeight <= 0f)
            return string.Empty;

        StringBuilder builder = new();
        for (int i = 0; i < _gradeBuffer.Count; i++)
        {
            if (!TryGetGrade(_gradeBuffer[i], out TowerGrade grade))
                continue;

            float percent = _weightBuffer[i] / totalWeight * 100f;
            if (builder.Length > 0)
                builder.Append("  ");

            builder.Append(GetShortGradeLabel(grade));
            builder.Append(' ');
            builder.Append(percent >= 10f ? $"{percent:0}%" : $"{percent:0.#}%");
        }

        return builder.ToString();
    }

    private static float ApplyEarlyPenalty(
        float baseWeight,
        TowerGrade grade,
        int wave,
        EconomyConfigSO config)
    {
        if (baseWeight <= 0f || config == null || wave <= 0 || wave > config.earlySummonPenaltyEndWave)
            return baseWeight;

        float multiplier = grade switch
        {
            TowerGrade.Rare => config.earlyRareWeightMultiplier,
            TowerGrade.Epic => config.earlyEpicWeightMultiplier,
            TowerGrade.Legendary => config.earlyLegendaryWeightMultiplier,
            _ => 1f
        };

        return baseWeight * multiplier;
    }

    private static readonly List<GradeList> _gradeBuffer = new();
    private static readonly List<float> _weightBuffer = new();

    private static GradeList GetFallbackGradeList(AllPlayerListSO allTowers, TowerGrade grade)
    {
        if (allTowers?.towerList == null || allTowers.towerList.Count == 0)
            return null;

        foreach (GradeList candidate in allTowers.towerList)
        {
            if (TryGetGrade(candidate, out TowerGrade towerGrade) && towerGrade == grade)
                return candidate;
        }

        return allTowers.towerList[0];
    }

    private static bool TryGetGrade(GradeList gradeList, out TowerGrade grade)
    {
        grade = TowerGrade.Normal;
        if (gradeList == null || string.IsNullOrWhiteSpace(gradeList.gradeName))
            return false;

        return System.Enum.TryParse(gradeList.gradeName, true, out grade);
    }

    private static string GetShortGradeLabel(TowerGrade grade) => grade switch
    {
        TowerGrade.Rare => "R",
        TowerGrade.Epic => "E",
        TowerGrade.Legendary => "L",
        _ => "N"
    };
}
