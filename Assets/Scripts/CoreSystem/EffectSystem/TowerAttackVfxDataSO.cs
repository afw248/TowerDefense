using UnityEngine;

namespace CoreSystem.EffectSystem
{
    public enum VfxImpactPlacement
    {
        BodyCenter = 0,
        Head = 1,
        Ground = 2
    }

    /// <summary>
    /// 타워 공격 시 소환할 AllIn1VfxToolkit(또는 기타) 이펙트와 데미지 비율 설정.
    /// </summary>
    [CreateAssetMenu(fileName = "TowerAttackVfx", menuName = "Effects/Tower Attack VFX", order = 1)]
    public class TowerAttackVfxDataSO : ScriptableObject
    {
        [Header("VFX")]
        public GameObject effectPrefab;

        [Min(0.05f)]
        public float scale = 1f;

        [Min(0.1f)]
        public float lifetime = 2f;

        public Vector3 positionOffset;

        [Tooltip("명중 이펙트/데미지 존 스폰 위치. 큰 폭발·번개는 BodyCenter 권장")]
        public VfxImpactPlacement impactPlacement = VfxImpactPlacement.BodyCenter;

        [Tooltip("켜면 투사체 명중 시 경량 재생(미사일 화염 타격). 폭발형 AllIn1 VFX는 끄세요")]
        public bool fastProjectileImpact;

        [Header("Damage Split (기본 공격력 대비 배율)")]
        [Tooltip("타겟에게 즉시 들어가는 직접 타격 데미지 배율")]
        [Min(0f)]
        public float directDamageMultiplier = 0.35f;

        [Tooltip("이펙트 범위에 닿은 적에게 들어가는 데미지 배율")]
        [Min(0f)]
        public float effectDamageMultiplier = 0.65f;

        [Header("Effect Zone")]
        [Min(0.1f)]
        public float damageRadius = 2f;

        [Tooltip("0이면 스폰 시 1회만 판정. 0보다 크면 해당 간격으로 반복 판정")]
        [Min(0f)]
        public float damageTickInterval = 0f;

        [Tooltip("켜면 직접 타격을 받은 적도 이펙트 범위 데미지를 추가로 받음")]
        public bool includePrimaryInEffectDamage;

        [Tooltip("켜면 이펙트가 주 타겟에 붙어 이동합니다. (느린 폭발 VFX 동반용)")]
        public bool followPrimaryTarget;

        [Tooltip("재생 속도 배율. 1보다 크면 더 빠르게 터집니다.")]
        [Min(0.1f)]
        public float playbackSpeed = 1.6f;

        [Tooltip("(사용 안 함) 이전 워밍업 스킵. playbackSpeed 사용 권장.")]
        [Min(0f)]
        public float particleWarmupSkip;
    }
}
