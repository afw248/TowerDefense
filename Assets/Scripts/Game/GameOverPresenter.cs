using UnityEngine;

public static class GameOverPresenter
{
    public static void ShowFieldOverflow(int maxCount)
    {
        GameOverUi ui = ResolveGameOverUi();
        ui?.ShowDefeat($"적이 {maxCount}마리에 도달했습니다!");
    }

    public static void ShowLeakOverflow()
    {
        GameOverUi ui = ResolveGameOverUi();
        ui?.ShowDefeat("누수 한도를 초과했습니다!");
    }

    public static void ShowBossTimeout()
    {
        GameOverUi ui = ResolveGameOverUi();
        ui?.ShowDefeat("보스를 제한 시간 내에 처치하지 못했습니다!");
    }

    public static void ShowVictory(int clearedWave)
    {
        GameOverUi ui = ResolveGameOverUi();
        ui?.ShowVictory($"웨이브 {clearedWave} 클리어!");
    }

    public static void ShowTutorialComplete()
    {
        GameOverUi ui = ResolveGameOverUi();
        ui?.ShowTutorialComplete("튜토리얼을 완료했습니다! 이제 본게임에서 80웨이브까지 도전해 보세요.");
    }

    private static GameOverUi ResolveGameOverUi()
    {
        if (GameOverUi.Instance != null)
            return GameOverUi.Instance;

        GameObject canvasGo = GameObject.Find("GameHudCanvas");
        return canvasGo != null
            ? GameOverUi.EnsureExists(canvasGo.transform)
            : null;
    }
}
