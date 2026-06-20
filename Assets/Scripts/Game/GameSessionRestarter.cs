using UnityEngine;

public static class GameSessionRestarter
{
    public static void RestartActiveScene()
    {
        Time.timeScale = 1f;

        if (TitleGameFlow.Instance != null)
            TitleGameFlow.Instance.RestartSession();
        else
        {
            TitlePreviewMode.Active = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameSceneNames.Game);
        }
    }
}
