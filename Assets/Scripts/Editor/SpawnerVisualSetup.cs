#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 스포너 포탈 연출 크기 조정. 타일/경로는 건드리지 않습니다.
/// </summary>
public static class SpawnerVisualSetup
{
    private const float PortalFxScale = 0.25f;
    private const float FrameStoneScale = 0.38f;
    private const float FrameSideOffset = 0.42f;
    private static readonly Vector3 PortalFxLocalPosition = new(0f, 0.45f, 0.1f);

    [MenuItem("TowerDefense/Fix Spawner Visual Scale")]
    public static void ApplyFromMenu()
    {
        Apply();
        Debug.Log("스포너 연출 크기 조정 완료.");
    }

    public static void Apply()
    {
        GameObject spawner = GameObject.Find("Spawner");
        if (spawner == null)
            return;

        MeshRenderer renderer = spawner.GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.enabled = false;

        spawner.transform.localScale = Vector3.one;

        Transform portalFx = spawner.transform.Find("PortalVisual");
        if (portalFx != null)
        {
            portalFx.localPosition = PortalFxLocalPosition;
            portalFx.localRotation = Quaternion.identity;
            portalFx.localScale = Vector3.one * PortalFxScale;
        }

        Transform frameRoot = spawner.transform.Find("PortalFrame");
        if (frameRoot == null)
            return;

        frameRoot.localPosition = Vector3.zero;
        Transform frameL = frameRoot.Find("PortalFrame_L");
        Transform frameR = frameRoot.Find("PortalFrame_R");

        if (frameL != null)
        {
            frameL.localPosition = new Vector3(-FrameSideOffset, 0f, -0.15f);
            frameL.localRotation = Quaternion.Euler(0f, 18f, 0f);
            frameL.localScale = Vector3.one * FrameStoneScale;
        }

        if (frameR != null)
        {
            frameR.localPosition = new Vector3(FrameSideOffset, 0f, -0.15f);
            frameR.localRotation = Quaternion.Euler(0f, -18f, 0f);
            frameR.localScale = Vector3.one * FrameStoneScale;
        }

        Transform spawnPoint = spawner.transform.Find("SpawnPoint");
        if (spawnPoint != null)
            spawnPoint.localPosition = new Vector3(0f, 0f, 0.35f);
    }
}
#endif
