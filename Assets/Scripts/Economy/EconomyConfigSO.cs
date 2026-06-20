using Tower;
using UnityEngine;

[CreateAssetMenu(fileName = "EconomyConfig", menuName = "TowerDefense/Economy Config")]
public class EconomyConfigSO : ScriptableObject
{
    [Header("Currency")]
    public int startingGold = 240;
    public int summonCost = 40;
    public float summonCostMultiplier = 1.05f;
    [Range(0f, 1f)]
    public float waveGoldInterestPercent = 0.03f;

    [Header("Units")]
    public int maxUnitCapacity = 28;

    [Header("Field Enemies")]
    public int maxFieldEnemies = 80;

    [Header("Kill Rewards")]
    public EnemyRewardSO defaultEnemyReward;

    [Header("Leak / Lose")]
    public int maxLeakCount = 100;

    [Header("Sell Refund By Grade")]
    public int sellRefundNormal = 10;
    public int sellRefundRare = 20;
    public int sellRefundEpic = 35;
    public int sellRefundLegendary = 60;

    [Header("Merge Chance Upgrade")]
    public float mergeChanceBonusPerLevel = 0.8f;
    public int maxMergeUpgradeLevel = 5;
    public int mergeChanceMax = 65;
    public int mergeChanceMaxEpic = 15;

    [Header("Summon Grade Upgrade")]
    public float summonRareBonusPerLevel = 0.4f;
    public float summonEpicBonusPerLevel = 0.25f;
    public float summonLegendaryBonusPerLevel = 0.08f;
    public int maxSummonUpgradeLevel = 5;

    [Header("Early Summon Penalty")]
    public int earlySummonPenaltyEndWave = 9;
    public float earlyRareWeightMultiplier = 0.65f;
    public float earlyEpicWeightMultiplier = 0.5f;
    public float earlyLegendaryWeightMultiplier = 0.3f;

    [Header("Merge Tier Unlock")]
    public int mergeUnlockRareToEpicCost = 500;
    public int mergeUnlockEpicToLegendaryCost = 1000;

    [Header("Archetype Upgrade")]
    public int upgradeBaseCost = 20;
    public float upgradeAttackBonus = 2f;
    public float upgradeCostMultiplier = 1.18f;
    public int maxUpgradeLevel = 99;

    public int GetSellRefund(TowerGrade grade) => grade switch
    {
        TowerGrade.Rare => sellRefundRare,
        TowerGrade.Epic => sellRefundEpic,
        TowerGrade.Legendary => sellRefundLegendary,
        _ => sellRefundNormal
    };
}
