using UnityEngine;

[DefaultExecutionOrder(10)]
public class TutorialSessionBootstrap : MonoBehaviour
{
    [SerializeField] private TutorialWaveConfigSO config;

    private void Awake()
    {
        if (!GameSessionMode.IsTutorial)
            Destroy(this);
    }

    private void Start()
    {
        if (!GameSessionMode.IsTutorial)
            return;

        config ??= TutorialWaveConfigSO.GetActive(FindSpawnList());
        if (config == null)
            return;

        ApplyEconomy(config);
        ApplyTrackers(config);
    }

    private static void ApplyEconomy(TutorialWaveConfigSO tutorialConfig)
    {
        EconomyManager economy = EconomyManager.Instance;
        if (economy == null)
            return;

        economy.ApplyTutorialStartingGold(tutorialConfig.startingGold);
    }

    private static void ApplyTrackers(TutorialWaveConfigSO tutorialConfig)
    {
        FieldEnemyTracker fieldTracker = FieldEnemyTracker.Instance;
        if (fieldTracker != null)
            fieldTracker.ApplyTutorialLimits(tutorialConfig.maxFieldEnemies);

        LeakTracker leakTracker = LeakTracker.Instance;
        if (leakTracker != null)
            leakTracker.ApplyTutorialLimits(tutorialConfig.maxLeakCount);
    }

    public static void Reapply()
    {
        if (!GameSessionMode.IsTutorial)
            return;

        TutorialWaveConfigSO tutorialConfig = TutorialWaveConfigSO.GetActive(FindSpawnList());
        if (tutorialConfig == null)
            return;

        ApplyEconomy(tutorialConfig);
        ApplyTrackers(tutorialConfig);
    }

    public static void EnsureExists()
    {
        if (!GameSessionMode.IsTutorial)
            return;

        if (FindFirstObjectByType<TutorialSessionBootstrap>() != null)
            return;

        new GameObject(nameof(TutorialSessionBootstrap)).AddComponent<TutorialSessionBootstrap>();
    }

    private static SpawnEnemyListSO FindSpawnList()
    {
        SpawnManager spawner = FindFirstObjectByType<SpawnManager>();
        return spawner != null ? spawner.spawn : null;
    }
}
