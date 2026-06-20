using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [SerializeField] private EconomyConfigSO config;

    public EconomyConfigSO Config => config;
    public int Gold { get; private set; }
    public int SummonCount { get; private set; }

    public event Action<int> OnGoldChanged;
    public event Action OnSummonCostChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResolveConfig();
        Gold = config != null ? config.startingGold : 0;
    }

    private void ResolveConfig()
    {
        if (config != null)
            return;

        EconomyConfigSO[] configs = Resources.FindObjectsOfTypeAll<EconomyConfigSO>();
        foreach (EconomyConfigSO candidate in configs)
        {
            if (candidate != null && candidate.name == "EconomyConfig")
            {
                config = candidate;
                return;
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        OnGoldChanged?.Invoke(Gold);
    }

    public void ApplyTutorialStartingGold(int amount)
    {
        Gold = Mathf.Max(0, amount);
        OnGoldChanged?.Invoke(Gold);
    }

    public bool CanAfford(int cost) => Gold >= cost;

    public bool TrySpend(int cost)
    {
        if (!CanAfford(cost))
            return false;

        Gold -= cost;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }

    public void AddGold(int amount, bool playCoinSfx = true)
    {
        if (amount <= 0)
            return;

        Gold += amount;
        OnGoldChanged?.Invoke(Gold);

        if (playCoinSfx)
            GameAudioManager.Instance?.PlayCoin();
    }

    public void ApplyWaveInterest()
    {
        ResolveConfig();
        if (config == null || config.waveGoldInterestPercent <= 0f || Gold <= 0)
            return;

        int interest = Mathf.RoundToInt(Gold * config.waveGoldInterestPercent);
        if (interest <= 0)
            return;

        AddGold(interest, playCoinSfx: false);
    }

    public int GetUpgradeCost(int currentLevel)
    {
        if (config == null)
            return 0;

        return Mathf.RoundToInt(
            config.upgradeBaseCost *
            Mathf.Pow(config.upgradeCostMultiplier, currentLevel));
    }

    public int GetSummonCost()
    {
        if (config == null)
            return 0;

        return Mathf.RoundToInt(
            config.summonCost *
            Mathf.Pow(config.summonCostMultiplier, SummonCount));
    }

    public void RegisterSummon()
    {
        SummonCount++;
        OnSummonCostChanged?.Invoke();
    }

    public void ResetSession()
    {
        ResolveConfig();
        Gold = config != null ? config.startingGold : 0;
        SummonCount = 0;
        OnGoldChanged?.Invoke(Gold);
        OnSummonCostChanged?.Invoke();
    }
}
