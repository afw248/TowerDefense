using Tower;
using UnityEngine;

[System.Serializable]
public struct TowerInfoUiPalette
{
    public Color panelColor;
    public Color headerColor;
    public Color statsColor;
    public Color titleTextColor;
    public Color bodyTextColor;
    public Color accentTextColor;
    public Color sellButtonColor;
    public Color outlineColor;
    public Color rangeRingColor;
}

public static class TowerInfoUiThemes
{
    public static TowerInfoUiPalette Get(TowerGrade grade) => grade switch
    {
        TowerGrade.Legendary => new TowerInfoUiPalette
        {
            panelColor = new Color(0.22f, 0.12f, 0.38f, 0.98f),
            headerColor = new Color(0.34f, 0.18f, 0.52f, 1f),
            statsColor = new Color(0.12f, 0.08f, 0.22f, 0.95f),
            titleTextColor = new Color(1f, 0.92f, 0.45f, 1f),
            bodyTextColor = new Color(0.95f, 0.93f, 1f, 1f),
            accentTextColor = new Color(1f, 0.84f, 0.2f, 1f),
            sellButtonColor = new Color(0.72f, 0.52f, 0.08f, 1f),
            outlineColor = new Color(0.08f, 0.04f, 0.14f, 1f),
            rangeRingColor = new Color(1f, 0.84f, 0.2f, 0.45f),
        },
        TowerGrade.Epic => new TowerInfoUiPalette
        {
            panelColor = new Color(0.18f, 0.1f, 0.34f, 0.98f),
            headerColor = new Color(0.28f, 0.14f, 0.48f, 1f),
            statsColor = new Color(0.1f, 0.06f, 0.2f, 0.95f),
            titleTextColor = new Color(0.92f, 0.78f, 1f, 1f),
            bodyTextColor = new Color(0.94f, 0.9f, 1f, 1f),
            accentTextColor = new Color(0.82f, 0.55f, 1f, 1f),
            sellButtonColor = new Color(0.48f, 0.22f, 0.78f, 1f),
            outlineColor = new Color(0.08f, 0.04f, 0.16f, 1f),
            rangeRingColor = new Color(0.82f, 0.55f, 1f, 0.42f),
        },
        TowerGrade.Rare => new TowerInfoUiPalette
        {
            panelColor = new Color(0.08f, 0.22f, 0.48f, 0.98f),
            headerColor = new Color(0.12f, 0.34f, 0.68f, 1f),
            statsColor = new Color(0.05f, 0.14f, 0.32f, 0.95f),
            titleTextColor = Color.white,
            bodyTextColor = new Color(0.9f, 0.96f, 1f, 1f),
            accentTextColor = new Color(0.55f, 0.95f, 1f, 1f),
            sellButtonColor = new Color(0.1f, 0.42f, 0.82f, 1f),
            outlineColor = new Color(0.02f, 0.08f, 0.18f, 1f),
            rangeRingColor = new Color(0.55f, 0.95f, 1f, 0.4f),
        },
        _ => new TowerInfoUiPalette
        {
            panelColor = new Color(0.14f, 0.15f, 0.18f, 0.98f),
            headerColor = new Color(0.22f, 0.24f, 0.28f, 1f),
            statsColor = new Color(0.08f, 0.09f, 0.12f, 0.95f),
            titleTextColor = new Color(0.95f, 0.96f, 0.98f, 1f),
            bodyTextColor = new Color(0.86f, 0.88f, 0.92f, 1f),
            accentTextColor = new Color(0.78f, 0.82f, 0.88f, 1f),
            sellButtonColor = new Color(0.34f, 0.38f, 0.44f, 1f),
            outlineColor = new Color(0.04f, 0.05f, 0.08f, 1f),
            rangeRingColor = new Color(0.78f, 0.82f, 0.88f, 0.35f),
        },
    };
}
