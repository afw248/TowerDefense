using UnityEngine;
using UnityEngine.Splines;

public static class GameplayCameraFraming
{
    public static bool TryGetPlayfieldFocus(out Vector3 focus)
    {
        focus = GameplayViewSettings.DefaultPlayfieldFocus;
        bool hasBounds = false;
        Bounds combined = default;

        TileManager tileManager = Object.FindFirstObjectByType<TileManager>();
        Bounds tileBounds = default;
        bool hasTileBounds = false;
        if (tileManager != null)
            hasTileBounds = TryGetTileBounds(tileManager, out tileBounds);

        if (hasTileBounds)
        {
            combined = tileBounds;
            hasBounds = true;
        }

        SpawnManager spawner = Object.FindFirstObjectByType<SpawnManager>();
        if (spawner != null && spawner.spline != null && TryGetSplineBounds(spawner.spline, out Bounds splineBounds))
        {
            if (hasBounds)
                combined.Encapsulate(splineBounds);
            else
                combined = splineBounds;

            hasBounds = true;
        }

        if (!hasBounds)
            return false;

        Vector3 pan = GameplayViewSettings.PlayfieldFocusPan;
        focus = combined.center + pan;

        if (hasTileBounds)
        {
            focus.x = tileBounds.center.x + pan.x;
            focus.y = tileBounds.center.y + pan.y;
        }

        return true;
    }

    public static Vector3 GetCameraPosition(Vector3 focusPoint) =>
        focusPoint + GameplayViewSettings.CameraOffsetFromFocus + GameplayViewSettings.CameraCompositionOffset;

    private static bool TryGetTileBounds(TileManager tileManager, out Bounds bounds)
    {
        bounds = default;
        if (tileManager.TowerTile == null)
            return false;

        bool hasPoint = false;
        for (int y = 0; y < tileManager.TowerTile.Length; y++)
        {
            TileRow row = tileManager.TowerTile[y];
            if (row?.row == null)
                continue;

            for (int x = 0; x < row.row.Length; x++)
            {
                Tile tile = row.row[x];
                if (tile == null)
                    continue;

                IncludePoint(ref bounds, ref hasPoint, tile.transform.position);
            }
        }

        return hasPoint;
    }

    private static bool TryGetSplineBounds(SplineContainer splineContainer, out Bounds bounds)
    {
        bounds = default;
        if (splineContainer == null)
            return false;

        bool hasPoint = false;
        const int sampleCount = 12;
        for (int i = 0; i <= sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            Vector3 point = splineContainer.EvaluatePosition(t);
            IncludePoint(ref bounds, ref hasPoint, point);
        }

        return hasPoint;
    }

    private static void IncludePoint(ref Bounds bounds, ref bool hasPoint, Vector3 point)
    {
        if (!hasPoint)
        {
            bounds = new Bounds(point, Vector3.zero);
            hasPoint = true;
            return;
        }

        bounds.Encapsulate(point);
    }
}
