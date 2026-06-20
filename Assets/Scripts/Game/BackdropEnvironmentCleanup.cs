using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Fantasy 원경(Visual_Background)의 연기/먼지 파티클을 끕니다.
/// 배경 3.5배 확대 시 FX_Smoke가 플레이 영역 위로 번져 보이는 문제를 방지합니다.
/// </summary>
public static class BackdropEnvironmentCleanup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != GameSceneNames.Game)
            return;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "Visual_Background")
                DisableBackdropParticles(root);
        }
    }

    public static void DisableBackdropParticles(GameObject backgroundRoot)
    {
        if (backgroundRoot == null)
            return;

        foreach (ParticleSystem particle in backgroundRoot.GetComponentsInChildren<ParticleSystem>(true))
            particle.gameObject.SetActive(false);
    }
}
