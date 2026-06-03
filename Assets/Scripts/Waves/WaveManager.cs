using System.Collections;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private SpawnManager spawner;
    [SerializeField] private WaveUi WaveUi;
    [SerializeField] private WaveDataSO dataSO;
    private Coroutine _waveCoroutine;
    private void Start()
    {
        StartWaveSystem();
    }

    public void StartWaveSystem()
    {
        if (_waveCoroutine != null)
            StopCoroutine(_waveCoroutine);

        _waveCoroutine = StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        while (true)
        {
            dataSO.currentWave++;
            WaveUi.SetWaves(dataSO.currentWave);
            yield return StartCoroutine(SpawnRoutine());
            yield return new WaitForSeconds(dataSO.waveDelay);
        }
    }

    private IEnumerator SpawnRoutine()
    {
        dataSO.isWaveRunning = true;

        int spawnCount = 0;

        while (spawnCount < dataSO.maxEnemyCount)
        {
            SpawnChoose();

            spawner.Spawn();

            spawnCount++;

            yield return new WaitForSeconds(spawner.spawnDelay);
        }

        dataSO.isWaveRunning = false;
    }

    private void SpawnChoose()
    {
        int stage = dataSO.currentWave / 10;

        int index = Mathf.Clamp(
            stage,
            0,
            spawner.spawn.enemy.Count - 1);

        spawner.spawnEnemy =
            spawner.spawn.enemy[index];
    }
}