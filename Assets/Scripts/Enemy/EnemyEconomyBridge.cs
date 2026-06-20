using Agents;
using CombatSystem;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
[DefaultExecutionOrder(100)]
public class EnemyEconomyBridge : MonoBehaviour
{
    [SerializeField] private EnemyRewardSO rewardConfig;

    private float _rewardMultiplier = 1f;
    private Agent _agent;
    private HealthModule _health;
    private SplineMove _splineMove;
    private bool _rewarded;
    private bool _handlersBound;
    private bool _fieldRegistered;
    private static EnemyRewardSO _cachedDefaultReward;

    private void Awake()
    {
        _agent = GetComponent<Agent>();
        _health = GetComponentInChildren<HealthModule>();
        _splineMove = GetComponent<SplineMove>();
    }

    private void OnEnable()
    {
        TryRegisterFieldEnemy();
        BindRewardHandlers();

        if (_splineMove != null && !_splineMove.LoopsPath)
            _splineMove.OnPathComplete += HandleLeak;
    }

    private void Start()
    {
        TryRegisterFieldEnemy();
        BindRewardHandlers();
    }

    private void OnDisable()
    {
        TryUnregisterFieldEnemy();
        UnbindRewardHandlers();

        if (_splineMove != null)
            _splineMove.OnPathComplete -= HandleLeak;
    }

    private void TryRegisterFieldEnemy()
    {
        if (_fieldRegistered)
            return;

        FieldEnemyTracker tracker = FieldEnemyTracker.Instance;
        if (tracker == null)
            return;

        tracker.RegisterEnemy();
        _fieldRegistered = true;
    }

    private void TryUnregisterFieldEnemy()
    {
        if (!_fieldRegistered)
            return;

        FieldEnemyTracker.Instance?.UnregisterEnemy();
        _fieldRegistered = false;
    }

    private void BindRewardHandlers()
    {
        _agent ??= GetComponent<Agent>();
        _health ??= _agent != null ? _agent.Health : GetComponentInChildren<HealthModule>();
        if (_health == null)
            _health = GetComponentInChildren<HealthModule>();

        if (_health != null)
        {
            _health.OnDeath -= GrantKillReward;
            _health.OnDeath += GrantKillReward;
        }

        if (_agent != null)
        {
            _agent.OnDeath.RemoveListener(GrantKillReward);
            _agent.OnDeath.AddListener(GrantKillReward);
        }

        _handlersBound = _health != null || _agent != null;
    }

    private void UnbindRewardHandlers()
    {
        if (_health != null)
            _health.OnDeath -= GrantKillReward;

        if (_agent != null)
            _agent.OnDeath.RemoveListener(GrantKillReward);

        _handlersBound = false;
    }

    public void SetRewardMultiplier(float multiplier)
    {
        _rewardMultiplier = Mathf.Max(0f, multiplier);
    }

    public void TryGrantKillReward()
    {
        GrantKillReward();
    }

    private void GrantKillReward()
    {
        if (_rewarded)
            return;

        _rewarded = true;

        EconomyManager economy = EconomyManager.Instance;
        EnemyRewardSO reward = ResolveRewardConfig();
        if (economy == null || reward == null)
            return;

        int rewardAmount = Mathf.RoundToInt(reward.killReward * _rewardMultiplier);
        economy.AddGold(rewardAmount);
    }

    private EnemyRewardSO ResolveRewardConfig()
    {
        if (rewardConfig != null)
            return rewardConfig;

        EconomyConfigSO economyConfig = EconomyManager.Instance?.Config;
        if (economyConfig != null && economyConfig.defaultEnemyReward != null)
            return economyConfig.defaultEnemyReward;

        if (_cachedDefaultReward != null)
            return _cachedDefaultReward;

        _cachedDefaultReward = Resources.Load<EnemyRewardSO>("EnemyReward");
        if (_cachedDefaultReward != null)
            return _cachedDefaultReward;

        EnemyRewardSO[] configs = Resources.FindObjectsOfTypeAll<EnemyRewardSO>();
        foreach (EnemyRewardSO candidate in configs)
        {
            if (candidate != null && candidate.name == "EnemyReward")
            {
                _cachedDefaultReward = candidate;
                return _cachedDefaultReward;
            }
        }

        return configs.Length > 0 ? configs[0] : null;
    }

    private void HandleLeak()
    {
        if (_rewarded)
            return;

        _rewarded = true;

        Destroy(gameObject);
    }
}
