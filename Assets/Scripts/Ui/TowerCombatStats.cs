using CombatSystem;
using CoreSystem.EffectSystem;
using Player;
using Tower;

public static class TowerCombatStats
{
    public const float MaxDetectRadiusReference = 40f;

    public readonly struct Snapshot
    {
        public Snapshot(
            float damage,
            float detectRadius,
            float effectRadius,
            float attacksPerSecond,
            bool isAreaAttack)
        {
            Damage = damage;
            DetectRadius = detectRadius;
            EffectRadius = effectRadius;
            AttacksPerSecond = attacksPerSecond;
            IsAreaAttack = isAreaAttack;
        }

        public float Damage { get; }
        public float DetectRadius { get; }
        public float EffectRadius { get; }
        public float AttacksPerSecond { get; }
        public bool IsAreaAttack { get; }

        public string AttackTypeLabel => IsAreaAttack ? "범위" : "단일";
    }

    public static bool TryGet(AbstractPlayer tower, out Snapshot snapshot)
    {
        snapshot = default;

        if (tower == null)
            return false;

        PlayerAttackSkill attackSkill = GetPrimaryAttackSkill(tower);
        SkillDataSO skillData = attackSkill != null ? attackSkill.SkillData : null;
        TowerVariantSO variant = tower.TowerVariant;

        float damageMultiplier = skillData != null ? skillData.damageMultiplier : 1f;
        float cooldown = skillData != null ? skillData.cooldown : 0f;
        float detectRadius = tower.PlayerData != null ? tower.PlayerData.DetectRadius : 0f;
        float effectRadius = variant?.attackVfx != null ? variant.attackVfx.damageRadius : 0f;

        bool isAreaAttack = tower.Archetype != TowerArchetype.Bow;
        float attacksPerSecond = cooldown > 0f ? 1f / cooldown : 0f;

        snapshot = new Snapshot(
            tower.EffectiveAttack * damageMultiplier,
            detectRadius,
            effectRadius,
            attacksPerSecond,
            isAreaAttack);

        return true;
    }

    private static PlayerAttackSkill GetPrimaryAttackSkill(AbstractPlayer tower)
    {
        PlayerAttackSkill[] skills = tower.GetComponentsInChildren<PlayerAttackSkill>(true);
        if (skills == null || skills.Length == 0)
            return null;

        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i]?.SkillData != null)
                return skills[i];
        }

        return skills[0];
    }
}
