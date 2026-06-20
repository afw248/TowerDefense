using UnityEngine;

public static class GameplayViewSettings
{
    public const int TargetWidth = 1920;
    public const int TargetHeight = 1080;
    public const float TargetAspect = 16f / 9f;

    public const float OrthographicSize = 17f;
    public const float TitleOrthographicSize = 26f;
    public const float HudUniformScale = 1f;
    public const float PlayerUiScale = 1f;
    public const float CameraPanY = -0.7f;
    public const float CameraPanZ = 0.35f;
    public const float FocusPanY = -0.5f;
    public const float FocusPanZ = 0.35f;

    public static readonly Vector3 DefaultPlayfieldFocus = new(9f, 0f, 1.5f);
    public static readonly Vector3 PlayfieldFocusPan = new(-0.2f, 0.6f, 1.1f);
    public static readonly Vector3 CameraOffsetFromFocus = new(9.6f, 25.3f, 13f);
    public static readonly Vector3 CameraCompositionOffset = new(0.2f, -3.2f, 1.6f);
    public static readonly Vector3 GameplayCameraEuler = new(50.711f, -140f, 0f);

    public static Quaternion GameplayCameraRotation => Quaternion.Euler(GameplayCameraEuler);

    public static Vector3 ResolvePlayfieldFocus()
    {
        return GameplayCameraFraming.TryGetPlayfieldFocus(out Vector3 focus)
            ? focus
            : DefaultPlayfieldFocus;
    }

    public static Vector3 ResolveGameplayCameraPosition() =>
        GameplayCameraFraming.GetCameraPosition(ResolvePlayfieldFocus());
}
