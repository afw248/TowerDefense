using UnityEngine;

[CreateAssetMenu(fileName = "WaveDataSO", menuName = "Scriptable Objects/WaveDataSO")]
public class WaveDataSO : ScriptableObject
{
    // 1.040으로 하향: wave 50 기준 HP 약 7.1배 (기존 1.055 → 14배)
    public const float PerWaveScale = 1.035f;

    /// <summary>웨이브 1~N 몹 체력 추가 배율 (튜토리얼~초반)</summary>
    public const int VeryEarlyWaveBoostEnd = 9;
    public const float VeryEarlyWaveHpMultiplier = 1.08f;

    /// <summary>웨이브 10~40 구간 추가 체력 (에픽 타워 파밍 구간 난이도 상향, 40 이후 자연 감쇠)</summary>
    public const int EarlyWaveBoostStart = 10;
    public const int EarlyWaveBoostEnd = 40;
    public const int EarlyWaveHpPeakWave = 25;
    public const float EarlyWaveHpMultiplierPeak = 1.26f;

    public const int EarlyRewardRampEnd = 15;
    public const float EarlyRewardMultiplier = 0.92f;
    public const int LateRewardBoostStart = 20;
    public const float LateRewardMaxMultiplier = 1.7f;

    /// <summary>웨이브 N 이후 매 웨이브 체력 감쇠 (60웨이브대 난이도 완화)</summary>
    public const int LateWaveReductionStart = 50;
    public const float LateWaveReductionPerWave = 0.972f;

    public const int BossHealthReferenceWave = 9;
    public const float BossHealthMultiplier = 7f;

    public int maxEnemyCount = 30;
    public float waveDelay = 20f;
    public float waveMultply = 1f;
    public int currentWave;
    public bool isWaveRunning;

    public void ResetRuntimeState()
    {
        currentWave = 0;
        waveMultply = 1f;
        isWaveRunning = false;
    }

    public void SyncMultiplierForWave(int wave)
    {
        waveMultply = GetMultiplierForWave(wave);
    }

    public static float GetMultiplierForWave(int wave)
    {
        float baseMultiplier = wave <= 1
            ? 1f
            : Mathf.Pow(PerWaveScale, wave - 1);

        return ApplyWaveCurve(baseMultiplier, wave);
    }

    private static float ApplyWaveCurve(float baseMultiplier, int wave)
    {
        float result = baseMultiplier * GetEarlyWaveHpMultiplier(wave);

        if (wave > LateWaveReductionStart)
            result *= Mathf.Pow(
                LateWaveReductionPerWave,
                wave - LateWaveReductionStart);

        return result;
    }

    private static float GetEarlyWaveHpMultiplier(int wave)
    {
        if (wave <= 0)
            return 1f;

        if (wave <= VeryEarlyWaveBoostEnd)
            return VeryEarlyWaveHpMultiplier;

        if (wave > EarlyWaveBoostEnd)
            return 1f;

        if (wave <= EarlyWaveHpPeakWave)
        {
            float t = (wave - EarlyWaveBoostStart) / (float)(EarlyWaveHpPeakWave - EarlyWaveBoostStart);
            return Mathf.Lerp(VeryEarlyWaveHpMultiplier, EarlyWaveHpMultiplierPeak, t);
        }

        float fadeT = (wave - EarlyWaveHpPeakWave) / (float)(EarlyWaveBoostEnd - EarlyWaveHpPeakWave);
        return Mathf.Lerp(EarlyWaveHpMultiplierPeak, 1f, fadeT);
    }

    public static float GetRewardMultiplierForWave(int wave)
    {
        if (wave <= 1)
            return EarlyRewardMultiplier;

        if (wave <= EarlyRewardRampEnd)
        {
            float t = (wave - 1f) / (EarlyRewardRampEnd - 1f);
            return Mathf.Lerp(EarlyRewardMultiplier, 1f, t);
        }

        if (wave < LateRewardBoostStart)
            return 1f;

        float lateT = Mathf.Clamp01((wave - LateRewardBoostStart) / 45f);
        return Mathf.Lerp(1f, LateRewardMaxMultiplier, lateT);
    }
}
