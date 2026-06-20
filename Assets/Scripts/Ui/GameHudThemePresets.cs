using UnityEngine;

public static class GameHudThemePresets
{
    public static GameHudThemePalette Get(GameHudThemeId id) => id switch
    {
        GameHudThemeId.EmeraldGuard => new GameHudThemePalette(
            panelBackground: new Color(0.05f, 0.11f, 0.09f, 0.94f),
            panelOutline: new Color(0.35f, 0.82f, 0.48f, 0.55f),
            goldText: new Color(0.72f, 1f, 0.78f, 1f),
            bodyText: new Color(0.92f, 0.98f, 0.94f, 1f),
            mutedText: new Color(0.62f, 0.78f, 0.68f, 1f),
            barTrack: new Color(0.08f, 0.14f, 0.11f, 1f),
            enemyFill: new Color(0.82f, 0.28f, 0.32f, 1f),
            buttonNormal: new Color(0.10f, 0.20f, 0.15f, 1f),
            buttonHighlight: new Color(0.16f, 0.30f, 0.22f, 1f),
            accent: new Color(0.42f, 0.92f, 0.55f, 0.95f)),

        GameHudThemeId.CrimsonFortress => new GameHudThemePalette(
            panelBackground: new Color(0.12f, 0.05f, 0.07f, 0.94f),
            panelOutline: new Color(0.88f, 0.28f, 0.24f, 0.55f),
            goldText: new Color(1f, 0.78f, 0.55f, 1f),
            bodyText: new Color(0.98f, 0.92f, 0.90f, 1f),
            mutedText: new Color(0.78f, 0.62f, 0.60f, 1f),
            barTrack: new Color(0.18f, 0.08f, 0.09f, 1f),
            enemyFill: new Color(0.95f, 0.22f, 0.18f, 1f),
            buttonNormal: new Color(0.22f, 0.10f, 0.12f, 1f),
            buttonHighlight: new Color(0.34f, 0.14f, 0.16f, 1f),
            accent: new Color(0.95f, 0.32f, 0.22f, 0.95f)),

        GameHudThemeId.SlateMinimal => new GameHudThemePalette(
            panelBackground: new Color(0.10f, 0.11f, 0.13f, 0.88f),
            panelOutline: new Color(0.55f, 0.58f, 0.62f, 0.35f),
            goldText: new Color(0.88f, 0.90f, 0.94f, 1f),
            bodyText: new Color(0.94f, 0.95f, 0.97f, 1f),
            mutedText: new Color(0.68f, 0.70f, 0.74f, 1f),
            barTrack: new Color(0.16f, 0.17f, 0.19f, 1f),
            enemyFill: new Color(0.62f, 0.66f, 0.72f, 1f),
            buttonNormal: new Color(0.18f, 0.19f, 0.22f, 1f),
            buttonHighlight: new Color(0.26f, 0.28f, 0.32f, 1f),
            accent: new Color(0.72f, 0.74f, 0.78f, 0.95f)),

        _ => new GameHudThemePalette(
            panelBackground: new Color(0.06f, 0.08f, 0.14f, 0.94f),
            panelOutline: new Color(0.78f, 0.64f, 0.22f, 0.55f),
            goldText: new Color(1f, 0.88f, 0.35f, 1f),
            bodyText: new Color(0.93f, 0.95f, 0.98f, 1f),
            mutedText: new Color(0.72f, 0.76f, 0.82f, 1f),
            barTrack: new Color(0.11f, 0.13f, 0.19f, 1f),
            enemyFill: new Color(0.88f, 0.24f, 0.20f, 1f),
            buttonNormal: new Color(0.14f, 0.17f, 0.26f, 1f),
            buttonHighlight: new Color(0.20f, 0.24f, 0.36f, 1f),
            accent: new Color(1f, 0.82f, 0.15f, 0.95f)),
    };

    public static string GetDisplayName(GameHudThemeId id) => id switch
    {
        GameHudThemeId.EmeraldGuard => "에메랄드 가드 — 숲/방어 느낌, 그린 악센트",
        GameHudThemeId.CrimsonFortress => "크림슨 요새 — 다크 레드, 보스전 긴장감",
        GameHudThemeId.SlateMinimal => "슬레이트 미니멀 — 저채도, 깔끔한 HUD",
        _ => "판타지 골드 — 네이비+골드 (현재 적용 중)",
    };
}
