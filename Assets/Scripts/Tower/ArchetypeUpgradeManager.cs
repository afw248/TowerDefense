using System;
using Tower;
using UnityEngine;

public class ArchetypeUpgradeManager : MonoBehaviour
{
    public static ArchetypeUpgradeManager Instance { get; private set; }

    [SerializeField] private EconomyConfigSO config;
    [SerializeField] private TowerMergeConfigSO mergeConfig;

    private readonly int[] _archetypeLevels = new int[3];
    private int _normalToRareLevel;
    private int _rareToEpicLevel;
    private int _epicToLegendaryLevel;
    private int _summonUpgradeLevel;

    public event Action Changed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResolveConfig();
        mergeConfig ??= Resources.Load<TowerMergeConfigSO>("TowerMergeConfig");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        ResolveConfig();
    }

    private void ResolveConfig()
    {
        if (config != null)
            return;

        config = EconomyManager.Instance != null
            ? EconomyManager.Instance.Config
            : null;
    }

    public int GetArchetypeLevel(TowerArchetype archetype) => _archetypeLevels[(int)archetype];

    public float GetBonusAttack(TowerArchetype archetype)
    {
        EconomyConfigSO economyConfig = config ?? EconomyManager.Instance?.Config;
        if (economyConfig == null)
            return 0f;

        return _archetypeLevels[(int)archetype] * economyConfig.upgradeAttackBonus;
    }

    public int GetArchetypeUpgradeCost(TowerArchetype archetype)
    {
        EconomyManager economy = EconomyManager.Instance;
        return economy != null
            ? economy.GetUpgradeCost(_archetypeLevels[(int)archetype])
            : 0;
    }

    public bool CanUpgradeArchetype(TowerArchetype archetype)
    {
        EconomyConfigSO economyConfig = config ?? EconomyManager.Instance?.Config;
        if (economyConfig == null)
            return false;

        return _archetypeLevels[(int)archetype] < economyConfig.maxUpgradeLevel;
    }

    public bool TryUpgradeArchetype(TowerArchetype archetype)
    {
        ResolveConfig();

        if (!CanUpgradeArchetype(archetype))
            return false;

        EconomyManager economy = EconomyManager.Instance;
        if (economy == null)
            return false;

        int cost = GetArchetypeUpgradeCost(archetype);
        if (!economy.TrySpend(cost))
            return false;

        _archetypeLevels[(int)archetype]++;
        Changed?.Invoke();
        return true;
    }

    public int GetMergeUpgradeLevel(TowerGrade fromGrade) => fromGrade switch
    {
        TowerGrade.Normal => _normalToRareLevel,
        TowerGrade.Rare => _rareToEpicLevel,
        TowerGrade.Epic => _epicToLegendaryLevel,
        _ => 0
    };

    public int GetSummonUpgradeLevel() => _summonUpgradeLevel;

    public int GetEffectiveMergeChancePercent(TowerGrade fromGrade)
    {
        mergeConfig ??= Resources.Load<TowerMergeConfigSO>("TowerMergeConfig");
        int baseChance = mergeConfig != null
            ? mergeConfig.GetSuccessChancePercent(fromGrade)
            : 0;

        if (fromGrade >= TowerGrade.Legendary)
            return baseChance;

        EconomyConfigSO economyConfig = config ?? EconomyManager.Instance?.Config;
        float bonusPerLevel = economyConfig != null ? economyConfig.mergeChanceBonusPerLevel : 1f;
        int level = GetMergeUpgradeLevel(fromGrade);
        int maxChance = fromGrade == TowerGrade.Epic
            ? (economyConfig != null ? economyConfig.mergeChanceMaxEpic : 15)
            : (economyConfig != null ? economyConfig.mergeChanceMax : 65);

        return Mathf.Clamp(Mathf.RoundToInt(baseChance + level * bonusPerLevel), 0, maxChance);
    }

    public int GetSummonUpgradeCost()
    {
        EconomyManager economy = EconomyManager.Instance;
        return economy != null
            ? economy.GetUpgradeCost(_summonUpgradeLevel)
            : 0;
    }

    public bool CanUpgradeSummon()
    {
        EconomyConfigSO economyConfig = config ?? EconomyManager.Instance?.Config;
        if (economyConfig == null)
            return false;

        return _summonUpgradeLevel < economyConfig.maxSummonUpgradeLevel;
    }

    public bool TryUpgradeSummon()
    {
        ResolveConfig();

        if (!CanUpgradeSummon())
            return false;

        EconomyManager economy = EconomyManager.Instance;
        if (economy == null)
            return false;

        int cost = GetSummonUpgradeCost();
        if (!economy.TrySpend(cost))
            return false;

        _summonUpgradeLevel++;
        Changed?.Invoke();
        return true;
    }

    public int GetMergeUpgradeCost(TowerGrade fromGrade)
    {
        EconomyManager economy = EconomyManager.Instance;
        return economy != null
            ? economy.GetUpgradeCost(GetMergeUpgradeLevel(fromGrade))
            : 0;
    }

    public bool CanUpgradeMerge(TowerGrade fromGrade)
    {
        if (fromGrade >= TowerGrade.Legendary)
            return false;

        EconomyConfigSO economyConfig = config ?? EconomyManager.Instance?.Config;
        if (economyConfig == null)
            return false;

        return GetMergeUpgradeLevel(fromGrade) < economyConfig.maxMergeUpgradeLevel;
    }

    public bool TryUpgradeMerge(TowerGrade fromGrade)
    {
        ResolveConfig();

        if (!CanUpgradeMerge(fromGrade))
            return false;

        EconomyManager economy = EconomyManager.Instance;
        if (economy == null)
            return false;

        int cost = GetMergeUpgradeCost(fromGrade);
        if (!economy.TrySpend(cost))
            return false;

        switch (fromGrade)
        {
            case TowerGrade.Normal:
                _normalToRareLevel++;
                break;
            case TowerGrade.Rare:
                _rareToEpicLevel++;
                break;
            case TowerGrade.Epic:
                _epicToLegendaryLevel++;
                break;
            default:
                return false;
        }

        Changed?.Invoke();
        return true;
    }

    public bool IsMergeTierUnlocked(TowerGrade fromGrade) => fromGrade switch
    {
        TowerGrade.Normal or TowerGrade.Rare or TowerGrade.Epic => true,
        _ => false
    };

    public void ResetUpgrades()
    {
        for (int i = 0; i < _archetypeLevels.Length; i++)
            _archetypeLevels[i] = 0;

        _normalToRareLevel = 0;
        _rareToEpicLevel = 0;
        _epicToLegendaryLevel = 0;
        _summonUpgradeLevel = 0;
        Changed?.Invoke();
    }
}
