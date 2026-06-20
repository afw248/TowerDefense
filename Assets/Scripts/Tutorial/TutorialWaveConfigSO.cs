using UnityEngine;

[CreateAssetMenu(fileName = "TutorialWaveConfig", menuName = "TowerDefense/Tutorial Wave Config")]
public class TutorialWaveConfigSO : ScriptableObject
{
    private static TutorialWaveConfigSO _runtimeConfig;

    [Header("Progress")]
    public int victoryWave = 6;
    public float preFirstWaveDelay = 3f;

    [Header("Difficulty")]
    public int startingGold = 9999;
    public int maxFieldEnemies = 999;
    public int maxLeakCount = 9999;
    public float waveMultiply = 1f;

    [Header("Tutorial Boss")]
    public float bossHealthMultiplier = 1.2f;
    public float bossMoveSpeedMultiplier = 0.85f;
    public float bossRewardMultiplier = 3f;
    public float bossScaleMultiplier = 1.2f;
    public float bossWaveDurationSeconds = 90f;

    [Header("Waves")]
    public TutorialWaveStep[] waves;

    public static TutorialWaveConfigSO GetActive(SpawnEnemyListSO spawnList)
    {
        TutorialWaveConfigSO asset = Resources.Load<TutorialWaveConfigSO>("TutorialWaveConfig");
        if (asset != null && asset.HasValidWaves())
            return asset;

        if (_runtimeConfig == null)
        {
            _runtimeConfig = CreateInstance<TutorialWaveConfigSO>();
            _runtimeConfig.ApplyBuiltInDefaults(spawnList);
        }

        return _runtimeConfig;
    }

    public bool TryGetStep(int wave, out TutorialWaveStep step)
    {
        if (waves == null || wave < 1 || wave > waves.Length)
        {
            step = null;
            return false;
        }

        step = waves[wave - 1];
        return step != null;
    }

    private bool HasValidWaves()
    {
        if (waves == null || waves.Length == 0)
            return false;

        foreach (TutorialWaveStep step in waves)
        {
            if (step == null || step.enemyPrefab == null)
                return false;
        }

        return true;
    }

    private void ApplyBuiltInDefaults(SpawnEnemyListSO spawnList)
    {
        victoryWave = 6;
        preFirstWaveDelay = 3f;
        startingGold = 9999;
        maxFieldEnemies = 999;
        maxLeakCount = 9999;
        waveMultiply = 1f;
        bossHealthMultiplier = 1.2f;
        bossMoveSpeedMultiplier = 0.85f;
        bossRewardMultiplier = 3f;
        bossScaleMultiplier = 1.2f;
        bossWaveDurationSeconds = 90f;

        waves = new[]
        {
            CreateStep(
                "웨이브 1 — 기본 조작",
                "하단의 '소환' 버튼으로 타워를 배치하세요.\n적을 처치하면 코인을 얻습니다.\n상단 타이머가 0이 되면 다음 웨이브가 시작됩니다.",
                ResolveEnemy(spawnList, 0),
                maxEnemyCount: 5,
                waveDelay: 35f,
                spawnDelay: 1.4f),
            CreateStep(
                "웨이브 2 — 타워 종류",
                "소환된 타워는 활, 콜버린, 미사일 세 종류입니다.\n타워를 클릭하면 정보 패널에서 공격력과 등급을 확인할 수 있습니다.",
                ResolveEnemy(spawnList, 3),
                maxEnemyCount: 6,
                waveDelay: 35f,
                spawnDelay: 1.3f),
            CreateStep(
                "웨이브 3 — 타워 합성",
                "같은 등급, 같은 종류의 타워를 드래그해서 겹치면 상위 등급으로 합성됩니다.\n합성에 성공하면 더 강력한 타워가 됩니다.\n실패하면 드래그한 타워 1개만 사라지고, 남은 타워는 유지됩니다.",
                ResolveEnemy(spawnList, 4),
                maxEnemyCount: 6,
                waveDelay: 35f,
                spawnDelay: 1.2f),
            CreateStep(
                "웨이브 4 — 전투 스킬",
                CombatSkillTutorialPresenter.BuildMessage(),
                ResolveEnemy(spawnList, 1),
                maxEnemyCount: 7,
                waveDelay: 35f,
                spawnDelay: 1.1f),
            CreateStep(
                "웨이브 5 — 종류별 업그레이드",
                "우측 하단의 '강화' 버튼을 눌러 활/대포/미사일 종류별 공격력을 올릴 수 있습니다.\n합성 확률과 소환 등급 확률도 함께 강화할 수 있습니다.",
                ResolveEnemy(spawnList, 2),
                maxEnemyCount: 7,
                waveDelay: 35f,
                spawnDelay: 1f),
            CreateBossStep(
                "웨이브 6 — 보스",
                "필드에 적이 80마리를 넘으면 패배합니다. 상단 '필드 적' 카운터를 확인하세요.\n약한 보스가 등장합니다 — 처치하면 튜토리얼이 완료됩니다!",
                ResolveEnemy(spawnList, 6),
                ResolveBoss(spawnList, 5),
                waveDelay: 60f),
        };
    }

    private static TutorialWaveStep CreateStep(
        string title,
        string message,
        GameObject enemyPrefab,
        int maxEnemyCount,
        float waveDelay,
        float spawnDelay)
    {
        return new TutorialWaveStep
        {
            title = title,
            message = message,
            enemyPrefab = enemyPrefab,
            isBossWave = false,
            maxEnemyCount = maxEnemyCount,
            waveDelay = waveDelay,
            spawnDelay = spawnDelay,
        };
    }

    private static TutorialWaveStep CreateBossStep(
        string title,
        string message,
        GameObject enemyPrefab,
        GameObject bossPrefab,
        float waveDelay)
    {
        return new TutorialWaveStep
        {
            title = title,
            message = message,
            enemyPrefab = enemyPrefab,
            bossPrefab = bossPrefab,
            isBossWave = true,
            maxEnemyCount = 1,
            waveDelay = waveDelay,
            spawnDelay = 1f,
        };
    }

    private static GameObject ResolveEnemy(SpawnEnemyListSO spawnList, int index)
    {
        if (spawnList?.enemy == null || spawnList.enemy.Count == 0)
            return null;

        index = Mathf.Clamp(index, 0, spawnList.enemy.Count - 1);
        return spawnList.enemy[index];
    }

    private static GameObject ResolveBoss(SpawnEnemyListSO spawnList, int index)
    {
        if (spawnList == null)
            return null;

        GameObject boss = spawnList.GetBossPrefab(index);
        if (boss != null)
            return boss;

        return ResolveEnemy(spawnList, index);
    }
}

[System.Serializable]
public class TutorialWaveStep
{
    public string title = "튜토리얼";
    [TextArea(3, 8)]
    public string message;
    public GameObject enemyPrefab;
    public GameObject bossPrefab;
    public bool isBossWave;
    [Min(1)]
    public int maxEnemyCount = 6;
    [Min(1f)]
    public float waveDelay = 30f;
    [Min(0.1f)]
    public float spawnDelay = 1.2f;
}
