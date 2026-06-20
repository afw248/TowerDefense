using UnityEngine;

public static class SpawnerVisualLayout
{
    private static readonly Vector3 PortalFxLocalPosition = new(0f, 0.45f, 0.1f);

    public static void Apply()
    {
        GameObject spawner = GameObject.Find("Spawner");
        if (spawner == null)
            return;

        Transform portalFx = spawner.transform.Find("PortalVisual");
        if (portalFx == null)
            return;

        portalFx.localPosition = PortalFxLocalPosition;
    }
}
