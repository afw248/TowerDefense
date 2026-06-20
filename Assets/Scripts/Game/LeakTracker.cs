using System;
using UnityEngine;

public class LeakTracker : MonoBehaviour
{
    public static LeakTracker Instance { get; private set; }

    [SerializeField] private EconomyConfigSO config;

    private int _tutorialMaxLeakCount;

    public int LeakCount { get; private set; }
    public int MaxLeakCount => _tutorialMaxLeakCount > 0
        ? _tutorialMaxLeakCount
        : config != null ? config.maxLeakCount : 100;
    public bool IsGameOver { get; private set; }

    public event Action<int, int> OnLeakCountChanged;
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

    private void Start()
    {
        ResolveConfig();
        OnLeakCountChanged?.Invoke(LeakCount, MaxLeakCount);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ApplyTutorialLimits(int maxLeakCount)
    {
        _tutorialMaxLeakCount = Mathf.Max(1, maxLeakCount);
        OnLeakCountChanged?.Invoke(LeakCount, MaxLeakCount);
    }

    public void RegisterLeak()
    {
        if (IsGameOver)
            return;

        LeakCount++;
        OnLeakCountChanged?.Invoke(LeakCount, MaxLeakCount);

        if (LeakCount >= MaxLeakCount)
            TriggerLeakGameOver();
    }

    public void ResetState()
    {
        LeakCount = 0;
        IsGameOver = false;
        _tutorialMaxLeakCount = 0;
        OnLeakCountChanged?.Invoke(LeakCount, MaxLeakCount);
    }

    private void TriggerLeakGameOver()
    {
        if (IsGameOver)
            return;

        IsGameOver = true;
        OnGameOver?.Invoke();
        GameOverPresenter.ShowLeakOverflow();
    }
}
