using UnityEngine;

/// <summary>
/// Standalone 빌드 시작 시 저장된 해상도/전체화면 설정을 적용합니다.
/// </summary>
public static class DisplaySettingsBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyFixedDisplay()
    {
#if UNITY_STANDALONE
        GameDisplaySettings.Apply();
#endif
    }
}
