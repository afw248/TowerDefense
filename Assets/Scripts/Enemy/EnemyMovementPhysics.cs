using UnityEngine;

/// <summary>
/// 적끼리 CharacterController 충돌로 멈칫거리는 현상을 줄입니다.
/// </summary>
public static class EnemyMovementPhysics
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ConfigureLayerCollisions()
    {
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer < 0)
            return;

        Physics.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
    }
}
