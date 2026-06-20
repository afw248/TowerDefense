using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private SpawnManager spawner;
    [SerializeField] private WaveUi waveUi;
    [SerializeField] private WaveDataSO dataSO;
    [SerializeField] private BossWaveConfigSO bossConfig;
    [SerializeField] private int victoryWave = 80;
    [SerializeField] private float preFirstWaveDelay = 5f;
    [SerializeField] private float skipUnlockDelay = 20f;

    private Coroutine _spawnCoroutine;
    private float _waveTimer;
    private float _waveDuration;
    private bool _isPreFirstWaveDelay;
    private bool _isBossWaveActive;
    private bool _bossWaveResolved;
    private bool _bossTimedOut;
    private bool _victoryAchieved;
    private bool _isWaveTransitionRunning;
    private float _wavePausedUntilUnscaled;
    private BossEnemy _activeBoss;
    private TutorialWaveConfigSO _tutorialConfig;
    private TutorialWaveStep _activeTutorialStep;

    public float RemainingWaveTime => Mathf.Max(0f, _waveTimer);
    public float WaveElapsedTime => Mathf.Max(0f, _waveDuration - RemainingWaveTime);
    public bool IsBossWaveActive => _isBossWaveActive;
    public bool IsWaveTimePaused => Time.unscaledTime < _wavePausedUntilUnscaled;


    
    public int CurrentWave => dataSO != null ? dataSO.currentWave : 0;

    public static event System.Action<int> WaveStarted;
public bool IsPreFirstWaveDelay => _isPreFirstWaveDelay;
    public bool CanSkipWave =>
        !TitlePreviewMode.Active
        && !GameSessionMode.IsTutorial
        && !_isPreFirstWaveDelay
        && !_isBossWaveActive
        && !_isWaveTransitionRunning
        && !IsWaveTimePaused
        && !IsLocalGameOver()
        && dataSO != null
        && dataSO.currentWave > 0
        && RemainingWaveTime > 0f
        && WaveElapsedTime >= skipUnlockDelay;

    private void Awake()
    {
        bossConfig ??= Resources.Load<BossWaveConfigSO>("BossWaveConfig");

        if (GameSessionMode.IsTutorial)
            _tutorialConfig = TutorialWaveConfigSO.GetActive(spawner != null ? spawner.spawn : null);
    }

    private void OnEnable()
    {
        if (FieldEnemyTracker.Instance != null)
            FieldEnemyTracker.Instance.OnGameOver += HandleGameOver;

        if (LeakTracker.Instance != null)
            LeakTracker.Instance.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        if (FieldEnemyTracker.Instance != null)
            FieldEnemyTracker.Instance.OnGameOver -= HandleGameOver;

        if (LeakTracker.Instance != null)
            LeakTracker.Instance.OnGameOver -= HandleGameOver;
    }

    private void Start()
    {
        if (TitlePreviewMode.Active)
            return;

        BeginSession();
    }

    public void BeginSession()
    {
        StopSpawning();
        _isPreFirstWaveDelay = false;
        _isBossWaveActive = false;
        _bossWaveResolved = false;
        _bossTimedOut = false;
        _victoryAchieved = false;
        _isWaveTransitionRunning = false;
        _wavePausedUntilUnscaled = 0f;
        _activeBoss = null;
        _activeTutorialStep = null;

        if (GameSessionMode.IsTutorial)
        {
            _tutorialConfig = TutorialWaveConfigSO.GetActive(spawner != null ? spawner.spawn : null);
            TutorialSessionBootstrap.EnsureExists();
            TutorialSessionBootstrap.Reapply();
            TutorialManager.EnsureExists();
            ApplyTutorialWaveSettings();
        }
        else
        {
            _tutorialConfig = null;
        }

        dataSO?.ResetRuntimeState();
        BossHealthBarUi.Instance?.Hide();
        CombatActiveAbilityController.Instance?.ResetSession();
        BeginPreFirstWaveDelay();
    }

    private void ApplyTutorialWaveSettings()
    {
        if (_tutorialConfig == null || dataSO == null)
            return;

        dataSO.waveMultply = _tutorialConfig.waveMultiply;
        dataSO.maxEnemyCount = Mathf.Max(1, _tutorialConfig.waves != null && _tutorialConfig.waves.Length > 0
            ? _tutorialConfig.waves[0].maxEnemyCount
            : dataSO.maxEnemyCount);
    }

    private void Update()
    {
        if (IsLocalGameOver())
            return;

        if (IsWaveTimePaused)
        {
            UpdateWaveTimerUi();
            return;
        }

        if (_isPreFirstWaveDelay)
        {
            _waveTimer -= Time.deltaTime;
            UpdateWaveTimerUi();

            if (_waveTimer > 0f)
                return;

            _isPreFirstWaveDelay = false;
            StartNextWave();
            return;
        }

        _waveTimer -= Time.deltaTime;
        UpdateWaveTimerUi();

        if (_waveTimer > 0f)
            return;

        if (_isBossWaveActive && !_bossWaveResolved)
        {
            HandleBossTimeout();
            return;
        }

        TryCompleteWave();
    }

    private void HandleGameOver()
    {
        StopSpawning();
    }

    public void StopSpawning()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }

        if (dataSO != null)
            dataSO.isWaveRunning = false;
    }

    public void TrySkipWave()
    {
        if (!CanSkipWave)
            return;

        StopSpawning();
        _waveTimer = 0f;
        UpdateWaveTimerUi();
        TryCompleteWave();
    }

    public void PauseWaveTime(float durationSeconds)
    {
        if (durationSeconds <= 0f || IsLocalGameOver())
            return;

        _wavePausedUntilUnscaled = Mathf.Max(
            _wavePausedUntilUnscaled,
            Time.unscaledTime + durationSeconds);

        UpdateWaveTimerUi();
    }

    public void HandleBossDefeated(BossEnemy boss)
    {
        if (!_isBossWaveActive || _bossWaveResolved || boss == null || boss != _activeBoss)
            return;

        _bossWaveResolved = true;
        _activeBoss = null;
        BossHealthBarUi.Instance?.Hide();
        TryCompleteWave();
    }

    private void HandleBossTimeout()
    {
        if (_bossWaveResolved || _bossTimedOut)
            return;

        if (GameSessionMode.IsTutorial && dataSO != null && dataSO.currentWave >= GetVictoryWave())
        {
            _bossWaveResolved = true;
            BossHealthBarUi.Instance?.Hide();
            StopSpawning();
            TryCompleteWave();
            return;
        }

        _bossWaveResolved = true;
        _bossTimedOut = true;
        BossHealthBarUi.Instance?.Hide();
        StopSpawning();
        GameOverPresenter.ShowBossTimeout();
    }

    private static bool IsGameOver()
    {
        return (FieldEnemyTracker.Instance != null && FieldEnemyTracker.Instance.IsGameOver)
            || (LeakTracker.Instance != null && LeakTracker.Instance.IsGameOver);
    }

    private bool IsLocalGameOver()
    {
        return _bossTimedOut || _victoryAchieved || IsGameOver();
    }

    private int GetVictoryWave()
    {
        return GameSessionMode.IsTutorial && _tutorialConfig != null
            ? _tutorialConfig.victoryWave
            : victoryWave;
    }

    private void TryCompleteWave()
    {
        if (IsLocalGameOver())
            return;

        ApplyWaveInterest();

        if (dataSO.currentWave >= GetVictoryWave())
        {
            HandleVictory();
            return;
        }

        StartNextWave();
    }

    private void ApplyWaveInterest()
    {
        if (GameSessionMode.IsTutorial || TitlePreviewMode.Active)
            return;

        if (dataSO == null || dataSO.currentWave <= 0)
            return;

        EconomyManager.Instance?.ApplyWaveInterest();
    }

    private void HandleVictory()
    {
        if (_victoryAchieved)
            return;

        _victoryAchieved = true;
        StopSpawning();
        BossHealthBarUi.Instance?.Hide();

        if (GameSessionMode.IsTutorial)
            GameOverPresenter.ShowTutorialComplete();
        else
            GameOverPresenter.ShowVictory(dataSO.currentWave);
    }

    private void BeginPreFirstWaveDelay()
    {
        _isPreFirstWaveDelay = true;
        _waveTimer = GameSessionMode.IsTutorial && _tutorialConfig != null
            ? _tutorialConfig.preFirstWaveDelay
            : preFirstWaveDelay;
        _waveDuration = _waveTimer;

        if (waveUi != null)
            waveUi.SetWaves(1, false);

        UpdateWaveTimerUi();
    }

    private void UpdateWaveTimerUi()
    {
        if (waveUi != null)
            waveUi.SetTimer(RemainingWaveTime, _isBossWaveActive, _isPreFirstWaveDelay);
    }

    private void StartNextWave()
    {
        if (IsLocalGameOver() || _isWaveTransitionRunning)
            return;

        StartCoroutine(StartNextWaveRoutine());
    }

    private IEnumerator StartNextWaveRoutine()
    {
        _isWaveTransitionRunning = true;

        dataSO.currentWave++;
        _activeTutorialStep = null;
        WaveStarted?.Invoke(dataSO.currentWave);

        if (GameSessionMode.IsTutorial)
            ApplyTutorialStep(dataSO.currentWave);
        else
            ApplyNormalWave(dataSO.currentWave);

        if (GameSessionMode.IsTutorial && TutorialManager.Instance != null)
            yield return TutorialManager.Instance.ShowWaveIntroAndWait(dataSO.currentWave);

        WaveUiUpdate();

        if (_isBossWaveActive)
            PrepareBossWave();
        else
            SpawnChoose();

        if (_spawnCoroutine != null)
            StopCoroutine(_spawnCoroutine);

        _spawnCoroutine = StartCoroutine(_isBossWaveActive ? BossSpawnRoutine() : SpawnRoutine());

        _waveTimer = _isBossWaveActive
            ? GetBossWaveDuration()
            : GetCurrentWaveDelay();
        _waveDuration = _waveTimer;

        UpdateWaveTimerUi();
        GameAudioManager.Instance?.PlaySfx(_isBossWaveActive ? GameAudioId.BossWarning : GameAudioId.WaveStart);
        _isWaveTransitionRunning = false;
    }

    private void ApplyNormalWave(int wave)
    {
        _isBossWaveActive = IsBossWave(wave);
        _bossWaveResolved = false;
        _activeBoss = null;
        dataSO.SyncMultiplierForWave(wave);
    }

    private void ApplyTutorialStep(int wave)
    {
        _bossWaveResolved = false;
        _activeBoss = null;

        if (_tutorialConfig == null || !_tutorialConfig.TryGetStep(wave, out TutorialWaveStep step))
        {
            _isBossWaveActive = false;
            return;
        }

        _activeTutorialStep = step;
        _isBossWaveActive = step.isBossWave;
    }

    private float GetCurrentWaveDelay()
    {
        if (GameSessionMode.IsTutorial && _activeTutorialStep != null)
            return _activeTutorialStep.waveDelay;

        return dataSO.waveDelay;
    }

    private IEnumerator SpawnRoutine()
    {
        dataSO.isWaveRunning = true;

        int spawnLimit = GameSessionMode.IsTutorial && _activeTutorialStep != null
            ? _activeTutorialStep.maxEnemyCount
            : dataSO.maxEnemyCount;

        float delay = GameSessionMode.IsTutorial && _activeTutorialStep != null
            ? _activeTutorialStep.spawnDelay
            : spawner.spawnDelay;

        int spawnCount = 0;

        while (spawnCount < spawnLimit)
        {
            if (IsLocalGameOver())
                break;

            spawner.Spawn(dataSO.waveMultply, WaveDataSO.GetRewardMultiplierForWave(dataSO.currentWave));

            spawnCount++;

            yield return WaitForWaveSeconds(delay);
        }

        dataSO.isWaveRunning = false;
        _spawnCoroutine = null;
    }

    private IEnumerator WaitForWaveSeconds(float seconds)
    {
        float remaining = Mathf.Max(0f, seconds);

        while (remaining > 0f)
        {
            if (IsLocalGameOver())
                yield break;

            if (!IsWaveTimePaused)
                remaining -= Time.deltaTime;

            yield return null;
        }
    }

    private IEnumerator BossSpawnRoutine()
    {
        dataSO.isWaveRunning = true;

        if (!IsLocalGameOver())
        {
            if (GameSessionMode.IsTutorial && _tutorialConfig != null)
            {
                _activeBoss = spawner.SpawnTutorialBoss(
                    dataSO.waveMultply,
                    WaveDataSO.GetRewardMultiplierForWave(dataSO.currentWave),
                    _tutorialConfig);
            }
            else if (bossConfig != null)
            {
                int bossIndex = GetBossEnemyIndex(dataSO.currentWave);
                bool useScaleFallback = !spawner.spawn.HasBossPrefabOverride(bossIndex);
                _activeBoss = spawner.SpawnBoss(
                    dataSO.waveMultply,
                    WaveDataSO.GetRewardMultiplierForWave(dataSO.currentWave),
                    bossConfig,
                    useScaleFallback);
            }

            BossHealthBarUi bossHealthBar = BossHealthBarUi.EnsureAtBottomCenter();
            if (_activeBoss != null && bossHealthBar != null)
                bossHealthBar.BindBoss(_activeBoss);
        }

        dataSO.isWaveRunning = false;
        _spawnCoroutine = null;
        yield break;
    }

    private void SpawnChoose()
    {
        if (GameSessionMode.IsTutorial && _activeTutorialStep != null)
        {
            spawner.spawnEnemy = _activeTutorialStep.enemyPrefab;
            return;
        }

        int stage = dataSO.currentWave / GetBossInterval();

        int index = Mathf.Clamp(
            stage,
            0,
            spawner.spawn.enemy.Count - 1);

        spawner.spawnEnemy = spawner.spawn.enemy[index];
    }

    private void PrepareBossWave()
    {
        if (GameSessionMode.IsTutorial && _activeTutorialStep != null)
        {
            spawner.spawnEnemy = _activeTutorialStep.bossPrefab != null
                ? _activeTutorialStep.bossPrefab
                : _activeTutorialStep.enemyPrefab;
            return;
        }

        int bossIndex = GetBossEnemyIndex(dataSO.currentWave);
        spawner.spawnEnemy = spawner.spawn.GetBossPrefab(bossIndex);
    }

    private void WaveUiUpdate()
    {
        waveUi ??= FindFirstObjectByType<WaveUi>(FindObjectsInactive.Include);
        if (waveUi == null)
            return;

        waveUi.gameObject.SetActive(true);
        waveUi.SetWaves(dataSO.currentWave, _isBossWaveActive);

        if (waveUi.PopupWave == null)
            return;

        waveUi.PopupWave.gameObject.SetActive(true);
        waveUi.PopupWave.Popup(dataSO.currentWave, _isBossWaveActive);
    }

    private bool IsBossWave(int wave)
    {
        int interval = GetBossInterval();
        return interval > 0 && wave > 0 && wave % interval == 0;
    }

    private int GetBossEnemyIndex(int wave)
    {
        int interval = GetBossInterval();
        return Mathf.Max(0, (wave / interval) - 1);
    }

    private int GetBossInterval()
    {
        return bossConfig != null ? bossConfig.waveInterval : 10;
    }

    private float GetBossWaveDuration()
    {
        if (GameSessionMode.IsTutorial && _tutorialConfig != null)
            return _tutorialConfig.bossWaveDurationSeconds;

        return bossConfig != null ? bossConfig.waveDurationSeconds : 120f;
    }
}
