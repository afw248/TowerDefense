using Tower;
using UnityEngine;

[CreateAssetMenu(fileName = "GameAudioConfig", menuName = "TowerDefense/Game Audio Config")]
public class GameAudioConfigSO : ScriptableObject
{
    [Header("BGM")]
    public AudioClip titleBgm;
    public AudioClip gameplayBgm;

    [Header("UI")]
    public AudioClip uiClick;
    public AudioClip uiOpen;
    public AudioClip uiClose;

    [Header("Towers")]
    public AudioClip bowFire;
    public AudioClip culverinFire;
    public AudioClip missileLaunch;
    public AudioClip explosion;

    [Header("Enemies")]
    public AudioClip enemyHit;
    public AudioClip enemyDeath;

    [Header("Gameplay")]
    public AudioClip waveStart;
    public AudioClip bossWarning;
    public AudioClip coin;
    public AudioClip towerPlace;
    public AudioClip towerRemove;
    public AudioClip mergeSuccess;
    public AudioClip mergeFail;
    public AudioClip epicTowerReveal;
    public AudioClip legendaryTowerReveal;
    public AudioClip victory;
    public AudioClip defeat;

    public AudioClip GetClip(GameAudioId id)
    {
        return id switch
        {
            GameAudioId.UiClick => uiClick,
            GameAudioId.UiOpen => uiOpen,
            GameAudioId.UiClose => uiClose,
            GameAudioId.BowFire => bowFire,
            GameAudioId.CulverinFire => culverinFire,
            GameAudioId.MissileLaunch => missileLaunch,
            GameAudioId.Explosion => explosion,
            GameAudioId.EnemyHit => enemyHit,
            GameAudioId.EnemyDeath => enemyDeath,
            GameAudioId.WaveStart => waveStart,
            GameAudioId.BossWarning => bossWarning,
            GameAudioId.Coin => coin,
            GameAudioId.TowerPlace => towerPlace,
            GameAudioId.TowerRemove => towerRemove,
            GameAudioId.MergeSuccess => mergeSuccess,
            GameAudioId.MergeFail => mergeFail,
            GameAudioId.EpicTowerReveal => epicTowerReveal,
            GameAudioId.LegendaryTowerReveal => legendaryTowerReveal,
            GameAudioId.Victory => victory,
            GameAudioId.Defeat => defeat,
            _ => null,
        };
    }

    public AudioClip GetBgm(GameBgmTrack track)
    {
        return track switch
        {
            GameBgmTrack.Title => titleBgm,
            GameBgmTrack.Gameplay => gameplayBgm,
            _ => null,
        };
    }

    public GameAudioId GetTowerFireId(TowerArchetype archetype)
    {
        return archetype switch
        {
            TowerArchetype.Culverin => GameAudioId.CulverinFire,
            TowerArchetype.Missile => GameAudioId.MissileLaunch,
            _ => GameAudioId.BowFire,
        };
    }
}
