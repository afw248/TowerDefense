using UnityEngine;

public static class GameSessionReturnToTitle
{
    public static void Return()
    {
        Time.timeScale = 1f;
        GameSessionMode.IsTutorial = false;

        if (TitleGameFlow.Instance != null)
            TitleGameFlow.Instance.ReturnToTitle();
        else
        {
            TitlePreviewMode.Active = true;
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameSceneNames.Title);
        }
    }
}
