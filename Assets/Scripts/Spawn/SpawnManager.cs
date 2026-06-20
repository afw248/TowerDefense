using System.Collections;
using CombatSystem;
using UnityEngine;
using UnityEngine.Splines;

public class SpawnManager : MonoBehaviour
{
    [field:SerializeField] public SpawnEnemyListSO spawn { get;private set; }
    [field: SerializeField] public Transform spawnPoint { get; private set; }
    [field: SerializeField] public SplineContainer spline { get; private set; }
    public float spawnDelay { get; set; } = 1f;
    public GameObject spawnEnemy { get; set; }

    public void Spawn(float hpMultiplier, float rewardMultiplier = 1f)
    {
        if (spawnEnemy == null)
            return;

        if (FieldEnemyTracker.Instance != null && FieldEnemyTracker.Instance.IsGameOver)
            return;

        AlignSpawnToPathStart();

        Vector3 spawnpos = new Vector3(
            spawnPoint.position.x,
            spawnEnemy.transform.position.y,
            spawnPoint.position.z
        );

        GameObject enemyObject = Instantiate(spawnEnemy, spawnpos, Quaternion.identity);
        EnsureSpawnComponents(enemyObject);
        if (enemyObject.TryGetComponent<Enemy>(out Enemy spEnemy))
        {
            spEnemy.transform.SetParent(spawnPoint,true);
            spEnemy.spline.Path(spline);
            spEnemy.Health.ApplyWaveScaling(hpMultiplier);
        }

        EnemyEconomyBridge bridge = enemyObject.GetComponent<EnemyEconomyBridge>();
        bridge?.SetRewardMultiplier(rewardMultiplier);
    }

    public BossEnemy SpawnBoss(float waveMultiply, float rewardMultiplier, BossWaveConfigSO config, bool useScaleFallback)
    {
        if (spawnEnemy == null || config == null)
            return null;

        if (FieldEnemyTracker.Instance != null && FieldEnemyTracker.Instance.IsGameOver)
            return null;

        AlignSpawnToPathStart();

        Vector3 spawnpos = new Vector3(
            spawnPoint.position.x,
            spawnEnemy.transform.position.y,
            spawnPoint.position.z
        );

        GameObject enemyObject = Instantiate(spawnEnemy, spawnpos, Quaternion.identity);
        EnsureSpawnComponents(enemyObject);
        if (!enemyObject.TryGetComponent<Enemy>(out Enemy spEnemy))
            return null;

        spEnemy.transform.SetParent(spawnPoint, true);
        spEnemy.spline.Path(spline);
        spEnemy.Health.ApplyWaveScaling(waveMultiply);
        spEnemy.Health.ApplyStatMultiplier(config.healthMultiplier);

        if (spEnemy.spline != null)
            spEnemy.spline.moveSpeed *= config.moveSpeedMultiplier;

        if (useScaleFallback)
            spEnemy.transform.localScale = Vector3.one * config.scaleMultiplier;

        BossEnemy boss = enemyObject.GetComponent<BossEnemy>();
        if (boss == null)
            boss = enemyObject.AddComponent<BossEnemy>();

        boss.Initialize(spawnEnemy.name);

        EnemyEconomyBridge bridge = enemyObject.GetComponent<EnemyEconomyBridge>();
        bridge?.SetRewardMultiplier(rewardMultiplier * config.rewardMultiplier);

        return boss;
    }

    public BossEnemy SpawnTutorialBoss(float waveMultiply, float rewardMultiplier, TutorialWaveConfigSO config)
    {
        if (spawnEnemy == null || config == null)
            return null;

        if (FieldEnemyTracker.Instance != null && FieldEnemyTracker.Instance.IsGameOver)
            return null;

        AlignSpawnToPathStart();

        Vector3 spawnpos = new Vector3(
            spawnPoint.position.x,
            spawnEnemy.transform.position.y,
            spawnPoint.position.z
        );

        GameObject enemyObject = Instantiate(spawnEnemy, spawnpos, Quaternion.identity);
        EnsureSpawnComponents(enemyObject);
        if (!enemyObject.TryGetComponent<Enemy>(out Enemy spEnemy))
            return null;

        spEnemy.transform.SetParent(spawnPoint, true);
        spEnemy.spline.Path(spline);
        spEnemy.Health.ApplyWaveScaling(waveMultiply);
        spEnemy.Health.ApplyStatMultiplier(config.bossHealthMultiplier);

        if (spEnemy.spline != null)
            spEnemy.spline.moveSpeed *= config.bossMoveSpeedMultiplier;

        spEnemy.transform.localScale = Vector3.one * config.bossScaleMultiplier;

        BossEnemy boss = enemyObject.GetComponent<BossEnemy>();
        if (boss == null)
            boss = enemyObject.AddComponent<BossEnemy>();

        boss.Initialize(spawnEnemy.name);

        EnemyEconomyBridge bridge = enemyObject.GetComponent<EnemyEconomyBridge>();
        bridge?.SetRewardMultiplier(rewardMultiplier * config.bossRewardMultiplier);

        return boss;
    }

    public int EnemyCounting()
    {
        return transform.childCount;
    }

    private static void EnsureSpawnComponents(GameObject enemyObject)
    {
        if (enemyObject == null)
            return;

        RemoveStrayRootHealthModule(enemyObject);

        if (enemyObject.GetComponent<EnemyEconomyBridge>() == null)
            enemyObject.AddComponent<EnemyEconomyBridge>();

        if (enemyObject.GetComponent<EnemyWorldHealthBar>() == null)
            enemyObject.AddComponent<EnemyWorldHealthBar>();
    }

    private static void RemoveStrayRootHealthModule(GameObject enemyObject)
    {
        HealthModule rootHealth = enemyObject.GetComponent<HealthModule>();
        if (rootHealth == null)
            return;

        if (enemyObject.transform.Find("HealthModule") != null)
            Destroy(rootHealth);
    }

    private void AlignSpawnToPathStart()
    {
        if (spline == null || spawnPoint == null)
            return;

        Vector3 pathStart = spline.EvaluatePosition(0f);
        spawnPoint.position = new Vector3(pathStart.x, spawnPoint.position.y, pathStart.z);
    }
}
