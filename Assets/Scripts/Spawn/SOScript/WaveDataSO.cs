using UnityEngine;

[CreateAssetMenu(fileName = "WaveDataSO", menuName = "Scriptable Objects/WaveDataSO")]
public class WaveDataSO : ScriptableObject
{
    public int maxEnemyCount = 20;
    public float waveDelay = 20f;
    public int currentWave;
    public bool isWaveRunning;
}
