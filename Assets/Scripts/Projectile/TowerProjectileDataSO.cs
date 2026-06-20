using UnityEngine;

[CreateAssetMenu(fileName = "TowerProjectile", menuName = "Tower/Projectile Data", order = 2)]
public class TowerProjectileDataSO : ScriptableObject
{
    [Tooltip("풀링에 사용할 루트 프리팹 (HomingTowerProjectile 포함)")]
    public GameObject projectilePrefab;

    [Min(1f)]
    public float speed = 16f;

    [Tooltip("초당 최대 회전 각도(도). Slerp 대신 RotateTowards로 궤도 이탈 방지")]
    [Min(30f)]
    public float turnSpeedDegrees = 240f;

    [Tooltip("이 거리 안에서는 직진 추적 (근접 시 빙글 도는 현상 방지)")]
    [Min(0.5f)]
    public float straightPursuitDistance = 2.5f;

    [Min(0.05f)]
    public float hitRadius = 0.4f;

    [Min(0.5f)]
    public float maxLifetime = 5f;

    public Vector3 aimHeightOffset = new Vector3(0f, 1f, 0f);

    [Tooltip("발사 직후 근접 명중 판정을 지연합니다. 미사일 등")]
    [Min(0f)]
    public float spawnHitGraceDuration;

    [Tooltip("명중 이펙트를 적 발밑(지면)에 표시합니다. 대포 등")]
    public bool useGroundImpactPoint;
}
