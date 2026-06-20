using CoreSystem.EffectSystem;
using UnityEngine;

public static class CombatActiveAbilityVfx
{
    private static HitEffectDataSO _defaultFreezeHit;
    private static HitEffectDataSO _defaultDamageHit;
    private static HitEffectDataSO _defaultDamageBurst;

    public static void PlayFreeze(Enemy enemy, CombatActiveAbilityConfigSO config)
    {
        if (enemy == null)
            return;

        Vector3 impactPoint = enemy.transform.position + Vector3.up * 0.45f;
        HitVfxUtility.Play(ResolveFreezeHit(config), impactPoint, Quaternion.identity);
        enemy.GetComponent<EnemyHitFlash>()?.PlayFlash();
    }

    public static void PlayFreezeFieldBurst(Vector3 center, CombatActiveAbilityConfigSO config)
    {
        HitEffectDataSO burst = ResolveFreezeHit(config);
        if (burst == null)
            return;

        HitEffectDataSO fieldBurst = ScriptableObject.CreateInstance<HitEffectDataSO>();
        fieldBurst.effectPrefab = burst.effectPrefab;
        fieldBurst.scale = burst.scale * 2.4f;
        fieldBurst.lifetime = burst.lifetime;
        fieldBurst.positionOffset = burst.positionOffset;

        HitVfxUtility.Play(fieldBurst, center + Vector3.up * 0.8f, Quaternion.identity);
        Object.Destroy(fieldBurst);
    }

    public static void PlayGlobalDamageHit(Enemy enemy, CombatActiveAbilityConfigSO config)
    {
        if (enemy == null)
            return;

        Vector3 impactPoint = enemy.transform.position + Vector3.up * 0.55f;
        HitVfxUtility.Play(ResolveDamageHit(config), impactPoint, Quaternion.identity);
        enemy.GetComponent<EnemyHitFlash>()?.PlayFlash();
    }

    public static void PlayGlobalDamageBurst(Vector3 center, CombatActiveAbilityConfigSO config)
    {
        HitVfxUtility.Play(ResolveDamageBurst(config), center + Vector3.up * 1f, Quaternion.identity);
    }

    public static void PlayFreezeFeedback(int enemyCount, CombatActiveAbilityConfigSO config)
    {
        if (config != null && config.freezeShakeIntensity > 0f)
            GameplayCameraShake.RequestShake(config.freezeShakeIntensity);

        GameAudioManager.Instance?.PlaySfx(GameAudioId.WaveStart, 0.75f);
    }

    public static void PlayGlobalDamageFeedback(CombatActiveAbilityConfigSO config)
    {
        if (config != null && config.globalDamageShakeIntensity > 0f)
            GameplayCameraShake.RequestShake(config.globalDamageShakeIntensity);

        GameAudioManager.Instance?.PlayExplosion(1f);
    }

    private static HitEffectDataSO ResolveFreezeHit(CombatActiveAbilityConfigSO config)
    {
        if (config != null && config.freezeHitVfx != null)
            return config.freezeHitVfx;

        _defaultFreezeHit ??= Resources.Load<HitEffectDataSO>("Effects/CombatFreezeHit");
        return _defaultFreezeHit;
    }

    private static HitEffectDataSO ResolveDamageHit(CombatActiveAbilityConfigSO config)
    {
        if (config != null && config.damageHitVfx != null)
            return config.damageHitVfx;

        _defaultDamageHit ??= Resources.Load<HitEffectDataSO>("Effects/CombatDamageHit");
        return _defaultDamageHit;
    }

    private static HitEffectDataSO ResolveDamageBurst(CombatActiveAbilityConfigSO config)
    {
        if (config != null && config.damageBurstVfx != null)
            return config.damageBurstVfx;

        _defaultDamageBurst ??= Resources.Load<HitEffectDataSO>("Effects/CombatDamageBurst");
        return _defaultDamageBurst;
    }
}
