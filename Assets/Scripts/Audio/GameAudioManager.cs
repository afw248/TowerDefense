using Player;

using Tower;

using UnityEngine;

using UnityEngine.SceneManagement;



[DefaultExecutionOrder(-500)]

public class GameAudioManager : MonoBehaviour

{

    public static GameAudioManager Instance { get; private set; }



    private const int SfxPoolSize = 6;

    private const float EnemyHitCooldown = 0.12f;

    private const float CoinCooldown = 0.15f;

    private const float ExplosionCooldown = 0.07f;

    private const float EnemyDeathCooldown = 0.35f;

    private const int MaxTowerFirePerFrame = 4;

    private const int MaxEnemyDeathPerFrame = 1;



    [SerializeField] private GameAudioConfigSO config;



    private AudioSource _bgmSource;

    private AudioSource[] _sfxPool;

    private int _sfxPoolIndex;

    private GameBgmTrack _currentBgmTrack = GameBgmTrack.None;



    private float _nextEnemyHitTime;

    private float _nextCoinTime;

    private float _nextExplosionTime;

    private float _nextEnemyDeathTime;



    private int _towerFireFrame = -1;

    private int _towerFireCount;

    private int _enemyDeathFrame = -1;

    private int _enemyDeathCount;



    public static void EnsureExists()

    {

        if (Instance != null)

            return;



        GameAudioManager existing = FindFirstObjectByType<GameAudioManager>(FindObjectsInactive.Include);

        if (existing != null)

        {

            Instance = existing;

            existing.InitializeIfNeeded();

            return;

        }



        GameObject host = new GameObject(nameof(GameAudioManager));

        host.AddComponent<GameAudioManager>();

    }



    private void Awake()

    {

        if (Instance != null && Instance != this)

        {

            Destroy(gameObject);

            return;

        }



        Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializeIfNeeded();

    }



    private void OnEnable()

    {

        GameAudioSettings.Changed += ApplyVolumeSettings;

        SceneManager.sceneLoaded += HandleSceneLoaded;

    }



    private void Start()

    {

        if (TitlePreviewMode.Active || SceneManager.GetActiveScene().name == GameSceneNames.Title)

            PlayBgm(GameBgmTrack.Title);

    }



    private void OnDisable()

    {

        GameAudioSettings.Changed -= ApplyVolumeSettings;

        SceneManager.sceneLoaded -= HandleSceneLoaded;

    }



    private void OnDestroy()

    {

        if (Instance == this)

            Instance = null;

    }



    private void InitializeIfNeeded()

    {

        config ??= Resources.Load<GameAudioConfigSO>("GameAudioConfig");



        if (_bgmSource == null)

        {

            _bgmSource = gameObject.AddComponent<AudioSource>();

            _bgmSource.loop = true;

            _bgmSource.playOnAwake = false;

            _bgmSource.spatialBlend = 0f;

        }



        if (_sfxPool == null || _sfxPool.Length == 0)

        {

            _sfxPool = new AudioSource[SfxPoolSize];

            for (int i = 0; i < SfxPoolSize; i++)

            {

                _sfxPool[i] = gameObject.AddComponent<AudioSource>();

                _sfxPool[i].loop = false;

                _sfxPool[i].playOnAwake = false;

                _sfxPool[i].spatialBlend = 0f;

                _sfxPool[i].volume = 1f;

            }

        }



        ApplyVolumeSettings();

    }



    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)

    {

        if (scene.name != GameSceneNames.Title)

            return;



        if (TitlePreviewMode.Active)

            PlayBgm(GameBgmTrack.Title);

    }



    private void ApplyVolumeSettings()

    {

        if (_bgmSource != null)

            _bgmSource.volume = GameAudioSettings.EffectiveBgmVolume;

    }



    public void PlayBgm(GameBgmTrack track)

    {

        if (config == null || _bgmSource == null)

            return;



        if (_currentBgmTrack == track && _bgmSource.isPlaying)

            return;



        AudioClip clip = config.GetBgm(track);

        if (clip == null)

        {

            _bgmSource.Stop();

            _currentBgmTrack = GameBgmTrack.None;

            return;

        }



        _currentBgmTrack = track;

        _bgmSource.clip = clip;

        _bgmSource.volume = GameAudioSettings.EffectiveBgmVolume;

        _bgmSource.Play();

    }



    public void StopBgm()

    {

        _currentBgmTrack = GameBgmTrack.None;

        _bgmSource?.Stop();

    }



    public void PlaySfx(GameAudioId id, float volumeScale = 1f)

    {

        if (config == null || _sfxPool == null || _sfxPool.Length == 0)

            return;



        AudioClip clip = config.GetClip(id);

        if (clip == null)

            return;



        float volume = GameAudioSettings.EffectiveVfxVolume * Mathf.Clamp01(volumeScale);
        PlayPooledOneShot(clip, volume);

    }



    public void PlayUiClick() => PlaySfx(GameAudioId.UiClick);



    public void PlayTowerFire(TowerArchetype archetype)

    {

        if (config == null)

            return;



        int frame = Time.frameCount;

        if (frame != _towerFireFrame)

        {

            _towerFireFrame = frame;

            _towerFireCount = 0;

        }



        if (_towerFireCount >= MaxTowerFirePerFrame)

            return;



        _towerFireCount++;



        float volumeScale = archetype switch

        {

            TowerArchetype.Culverin => 0.72f,

            TowerArchetype.Missile => 0.68f,

            _ => 0.88f,

        };



        PlaySfx(config.GetTowerFireId(archetype), volumeScale);

    }



    public void PlayExplosion(float volumeScale = 1f)

    {

        if (Time.unscaledTime < _nextExplosionTime)

            return;



        _nextExplosionTime = Time.unscaledTime + ExplosionCooldown;

        PlaySfx(GameAudioId.Explosion, volumeScale);

    }



    public void PlayEnemyHit()

    {

        if (Time.unscaledTime < _nextEnemyHitTime)

            return;



        _nextEnemyHitTime = Time.unscaledTime + EnemyHitCooldown;

        PlaySfx(GameAudioId.EnemyHit, 0.75f);

    }



    public void PlayEnemyDeath()

    {

        int frame = Time.frameCount;

        if (frame != _enemyDeathFrame)

        {

            _enemyDeathFrame = frame;

            _enemyDeathCount = 0;

        }



        if (_enemyDeathCount >= MaxEnemyDeathPerFrame)

            return;



        if (Time.unscaledTime < _nextEnemyDeathTime)

            return;



        _nextEnemyDeathTime = Time.unscaledTime + EnemyDeathCooldown;

        _enemyDeathCount++;

        PlaySfx(GameAudioId.EnemyDeath, 0.58f);

    }



    public void PlayCoin()

    {

        if (Time.unscaledTime < _nextCoinTime)

            return;



        _nextCoinTime = Time.unscaledTime + CoinCooldown;

        PlaySfx(GameAudioId.Coin, 0.85f);

    }



    public void PlayGradeReveal(TowerGrade grade)

    {

        if (grade == TowerGrade.Legendary)

            PlaySfx(GameAudioId.LegendaryTowerReveal, 0.95f);

        else if (grade == TowerGrade.Epic)

            PlaySfx(GameAudioId.EpicTowerReveal, 0.9f);

    }



    private void PlayPooledOneShot(AudioClip clip, float volume)

    {

        AudioSource source = _sfxPool[_sfxPoolIndex];

        _sfxPoolIndex = (_sfxPoolIndex + 1) % _sfxPool.Length;

        source.volume = 1f;

        source.PlayOneShot(clip, volume);

    }

}


