using UnityEngine;

/// <summary>
/// 창 최소화·포커스 상실 시에도 게임 루프가 계속 돌도록 합니다.
/// </summary>
public static class GameBackgroundRunner
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnableRunInBackground()
    {
        Application.runInBackground = true;
    }
}
