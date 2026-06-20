using CoreSystem.EffectSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "CombatActiveAbilityConfig", menuName = "TowerDefense/Combat Active Ability Config")]
public class CombatActiveAbilityConfigSO : ScriptableObject
{
    [Header("Freeze")]
    public float freezeDurationSeconds = 4f;
    public float freezeCooldownSeconds = 75f;
    public HitEffectDataSO freezeHitVfx;
    [Range(0f, 1f)]
    public float freezeShakeIntensity = 0.22f;

    [Header("Global Damage")]
    [Range(0.05f, 0.5f)]
    public float damagePercentOfMaxHealth = 0.18f;
    public float minimumDamagePerEnemy = 1f;
    public float globalDamageCooldownSeconds = 90f;
    public HitEffectDataSO damageHitVfx;
    public HitEffectDataSO damageBurstVfx;
    [Range(0f, 1f)]
    public float globalDamageShakeIntensity = 0.4f;

    [Header("Tutorial")]
    public int tutorialUnlockWave = 1;
}