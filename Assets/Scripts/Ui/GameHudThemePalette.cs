using UnityEngine;

public readonly struct GameHudThemePalette
{
    public readonly Color PanelBackground;
    public readonly Color PanelOutline;
    public readonly Color GoldText;
    public readonly Color BodyText;
    public readonly Color MutedText;
    public readonly Color BarTrack;
    public readonly Color EnemyFill;
    public readonly Color ButtonNormal;
    public readonly Color ButtonHighlight;
    public readonly Color Accent;

    public GameHudThemePalette(
        Color panelBackground,
        Color panelOutline,
        Color goldText,
        Color bodyText,
        Color mutedText,
        Color barTrack,
        Color enemyFill,
        Color buttonNormal,
        Color buttonHighlight,
        Color accent)
    {
        PanelBackground = panelBackground;
        PanelOutline = panelOutline;
        GoldText = goldText;
        BodyText = bodyText;
        MutedText = mutedText;
        BarTrack = barTrack;
        EnemyFill = enemyFill;
        ButtonNormal = buttonNormal;
        ButtonHighlight = buttonHighlight;
        Accent = accent;
    }
}
