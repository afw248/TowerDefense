using UnityEngine;

[CreateAssetMenu(fileName = "BossWaveConfig", menuName = "TowerDefense/Boss Wave Config")]
public class BossWaveConfigSO : ScriptableObject
{
    [Header("Wave")]
    [Min(1)]
    public int waveInterval = 10;

    [Min(1f)]
    public float waveDurationSeconds = 120f;

    [Header("Boss Stats")]
    [Min(1f)]
    public float healthMultiplier = 7f;

    [Range(0.1f, 1f)]
    public float moveSpeedMultiplier = 0.6f;

    [Min(1f)]
    public float rewardMultiplier = 5f;

    [Header("Visual")]
    [Min(1f)]
    public float scaleMultiplier = 1.5f;
}
