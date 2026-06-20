using System;
using UnityEngine;

/// <summary>
/// 지상 몬스터 CharacterController 중심을 모델 높이에 맞게 올립니다.
/// 날개/비행형(Bat, Dragon)은 제외합니다.
/// </summary>
public static class EnemyColliderLayout
{
    private const float GroundCenterYOffset = 0.5f;

    public static void Apply(CharacterController controller, string enemyName)
    {
        if (controller == null || IsFlyingEnemy(enemyName))
            return;

        if (controller.height > 2.5f)
            return;

        Vector3 center = controller.center;
        center.y += GroundCenterYOffset;
        controller.center = center;
    }

    private static bool IsFlyingEnemy(string enemyName)
    {
        if (string.IsNullOrEmpty(enemyName))
            return false;

        return enemyName.IndexOf("Bat", StringComparison.OrdinalIgnoreCase) >= 0
            || enemyName.IndexOf("Dragon", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
