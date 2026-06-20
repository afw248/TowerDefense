using UnityEngine;

public enum TowerMergeUiThemeId
{
    MapleTeal = 0,
    LegendGold = 1,
    MinimalDark = 2,
    EnergyBlue = 3,
    SunsetOrange = 4,
}

[System.Serializable]
public struct TowerMergeUiPalette
{
    public Color panelColor;
    public Color titleAreaColor;
    public Color chanceAreaColor;
    public Color titleTextColor;
    public Color chanceTextColor;
    public Color outlineColor;
}

public static class TowerMergeUiThemes
{
    public static TowerMergeUiPalette Get(TowerMergeUiThemeId themeId) => themeId switch
    {
        TowerMergeUiThemeId.LegendGold => new TowerMergeUiPalette
        {
            panelColor = new Color(0.22f, 0.12f, 0.38f, 0.96f),
            titleAreaColor = new Color(0.30f, 0.16f, 0.48f, 1f),
            chanceAreaColor = new Color(0.12f, 0.08f, 0.22f, 0.95f),
            titleTextColor = Color.white,
            chanceTextColor = new Color(1f, 0.84f, 0.2f, 1f),
            outlineColor = Color.black,
        },
        TowerMergeUiThemeId.MinimalDark => new TowerMergeUiPalette
        {
            panelColor = new Color(0.08f, 0.09f, 0.12f, 0.94f),
            titleAreaColor = new Color(0.12f, 0.13f, 0.17f, 1f),
            chanceAreaColor = new Color(0.05f, 0.06f, 0.09f, 0.95f),
            titleTextColor = new Color(0.95f, 0.96f, 0.98f, 1f),
            chanceTextColor = new Color(1f, 0.92f, 0.35f, 1f),
            outlineColor = Color.black,
        },
        TowerMergeUiThemeId.EnergyBlue => new TowerMergeUiPalette
        {
            panelColor = new Color(0.08f, 0.22f, 0.48f, 0.96f),
            titleAreaColor = new Color(0.12f, 0.34f, 0.68f, 1f),
            chanceAreaColor = new Color(0.05f, 0.14f, 0.32f, 0.95f),
            titleTextColor = Color.white,
            chanceTextColor = new Color(0.55f, 0.95f, 1f, 1f),
            outlineColor = new Color(0.02f, 0.08f, 0.18f, 1f),
        },
        TowerMergeUiThemeId.SunsetOrange => new TowerMergeUiPalette
        {
            panelColor = new Color(0.62f, 0.22f, 0.10f, 0.96f),
            titleAreaColor = new Color(0.78f, 0.30f, 0.12f, 1f),
            chanceAreaColor = new Color(0.36f, 0.12f, 0.08f, 0.95f),
            titleTextColor = Color.white,
            chanceTextColor = new Color(1f, 0.95f, 0.55f, 1f),
            outlineColor = new Color(0.22f, 0.06f, 0.02f, 1f),
        },
        _ => new TowerMergeUiPalette
        {
            panelColor = new Color(0.18f, 0.78f, 0.72f, 0.96f),
            titleAreaColor = new Color(0.18f, 0.78f, 0.72f, 1f),
            chanceAreaColor = new Color(0.08f, 0.48f, 0.46f, 0.95f),
            titleTextColor = Color.white,
            chanceTextColor = new Color(1f, 0.92f, 0.2f, 1f),
            outlineColor = Color.black,
        },
    };
}
