using System;
using UnityEngine;

public class FieldEnemyTracker : MonoBehaviour
{
    public static FieldEnemyTracker Instance { get; private set; }

    [SerializeField] private EconomyConfigSO config;

    private int _tutorialMaxFieldEnemies;

    public int AliveCount { get; private set; }
    public int MaxCount => _tutorialMaxFieldEnemies > 0
        ? _tutorialMaxFieldEnemies
        : config != null ? config.maxFieldEnemies : 60;
    public bool IsGameOver { get; private set; }

    public event Action<int, int> OnCountChanged;
    public event Action OnGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        ResolveConfig();
        OnCountChanged?.Invoke(AliveCount, MaxCount);
    }

    public void ApplyTutorialLimits(int maxFieldEnemies)
    {
        _tutorialMaxFieldEnemies = Mathf.Max(1, maxFieldEnemies);
        OnCountChanged?.Invoke(AliveCount, MaxCount);
    }

    public void RegisterEnemy()
    {
        if (IsGameOver)
            return;

        AliveCount++;
        OnCountChanged?.Invoke(AliveCount, MaxCount);

        if (AliveCount >= MaxCount)
            TriggerFieldOverflowGameOver();
    }

    public void UnregisterEnemy()
    {
        if (AliveCount <= 0)
            return;

        AliveCount--;
        OnCountChanged?.Invoke(AliveCount, MaxCount);
    }

    public void ResetState()
    {
        AliveCount = 0;
        IsGameOver = false;
        _tutorialMaxFieldEnemies = 0;
        OnCountChanged?.Invoke(AliveCount, MaxCount);
    }

    private void TriggerFieldOverflowGameOver()
    {
        if (IsGameOver)
            return;

        IsGameOver = true;
        OnGameOver?.Invoke();
        GameOverPresenter.ShowFieldOverflow(MaxCount);
    }
}
