using UnityEngine;

public static class GameHudSettings
{
    private const string ThemePrefKey = "GameHudThemeId";

    public static GameHudThemeId ActiveTheme
    {
        get => (GameHudThemeId)PlayerPrefs.GetInt(ThemePrefKey, (int)GameHudThemeId.SlateMinimal);
        set
        {
            PlayerPrefs.SetInt(ThemePrefKey, (int)value);
            PlayerPrefs.Save();
            GameHudTheme.SetActive(value);
        }
    }

    public static void LoadSavedTheme()
    {
        GameHudTheme.SetActive(ActiveTheme);
    }

    public static void UseSlateMinimalTheme()
    {
        ActiveTheme = GameHudThemeId.SlateMinimal;
    }
}
