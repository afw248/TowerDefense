using System;
using UnityEngine;

public static class GameDisplaySettings
{
    public readonly struct ResolutionPreset
    {
        public readonly int Width;
        public readonly int Height;
        public readonly string Label;

        public ResolutionPreset(int width, int height)
        {
            Width = width;
            Height = height;
            Label = $"{width} x {height}";
        }
    }

    private const string PresetIndexKey = "GameDisplay.ResolutionPresetIndex";
    private const string FullScreenKey = "GameDisplay.FullScreen";

    public static readonly ResolutionPreset[] Presets =
    {
        new(1280, 720),
        new(1600, 900),
        new(1920, 1080),
        new(2560, 1440),
    };

    public const int DefaultPresetIndex = 2;

    public static event Action Changed;

    public static int PresetIndex
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(PresetIndexKey, DefaultPresetIndex), 0, Presets.Length - 1);
        set
        {
            int clamped = Mathf.Clamp(value, 0, Presets.Length - 1);
            if (PresetIndex == clamped)
                return;

            PlayerPrefs.SetInt(PresetIndexKey, clamped);
            PlayerPrefs.Save();
            Apply();
            Changed?.Invoke();
        }
    }

    public static ResolutionPreset CurrentPreset => Presets[PresetIndex];

    public static bool IsFullScreen
    {
        get => PlayerPrefs.GetInt(FullScreenKey, 0) == 1;
        set
        {
            if (IsFullScreen == value)
                return;

            PlayerPrefs.SetInt(FullScreenKey, value ? 1 : 0);
            PlayerPrefs.Save();
            Apply();
            Changed?.Invoke();
        }
    }

    public static void Apply()
    {
#if UNITY_STANDALONE
        ResolutionPreset preset = CurrentPreset;
        if (IsFullScreen)
        {
            Resolution native = Screen.currentResolution;
            Screen.SetResolution(native.width, native.height, FullScreenMode.FullScreenWindow);
            return;
        }

        Screen.SetResolution(preset.Width, preset.Height, FullScreenMode.Windowed);
#endif
    }
}
