using System;
using UnityEngine;

public static class GameAudioSettings
{
    private const string MasterKey = "GameAudio.Master";
    private const string BgmKey = "GameAudio.Bgm";
    private const string VfxKey = "GameAudio.Vfx";
    private const string LegacySfxKey = "GameAudio.Sfx";

    public const float DefaultMasterVolume = 0.85f;
    public const float DefaultBgmVolume = 0.55f;
    public const float DefaultVfxVolume = 0.9f;

    public static event Action Changed;

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(MasterKey, DefaultMasterVolume);
        set => SetVolume(MasterKey, value);
    }

    public static float BgmVolume
    {
        get => PlayerPrefs.GetFloat(BgmKey, DefaultBgmVolume);
        set => SetVolume(BgmKey, value);
    }

    public static float VfxVolume
    {
        get
        {
            if (PlayerPrefs.HasKey(VfxKey))
                return PlayerPrefs.GetFloat(VfxKey, DefaultVfxVolume);

            return PlayerPrefs.GetFloat(LegacySfxKey, DefaultVfxVolume);
        }
        set => SetVolume(VfxKey, value);
    }

    public static float EffectiveBgmVolume => MasterVolume * BgmVolume;
    public static float EffectiveVfxVolume => MasterVolume * VfxVolume;

    private static void SetVolume(string key, float value)
    {
        float clamped = Mathf.Clamp01(value);
        if (PlayerPrefs.HasKey(key) && Mathf.Approximately(PlayerPrefs.GetFloat(key), clamped))
            return;

        PlayerPrefs.SetFloat(key, clamped);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
