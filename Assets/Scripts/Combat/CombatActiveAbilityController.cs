using Agents;
using CombatSystem;
using UnityEngine;

public class CombatActiveAbilityController : MonoBehaviour
{
    public static CombatActiveAbilityController Instance { get; private set; }

    [SerializeField] private CombatActiveAbilityConfigSO config;

    private float _freezeReadyAtUnscaled;
    private float _damageReadyAtUnscaled;

    public event System.Action Changed;

    public float FreezeCooldownRemaining =>
        Mathf.Max(0f, _freezeReadyAtUnscaled - Time.unscaledTime);

    public float GlobalDamageCooldownRemaining =>
        Mathf.Max(0f, _damageReadyAtUnscaled - Time.unscaledTime);

    private CombatActiveAbilityConfigSO _runtimeFallback;

    public float FreezeCooldownDuration => Config.freezeCooldownSeconds;
    public float GlobalDamageCooldownDuration => Config.globalDamageCooldownSeconds;

    private CombatActiveAbilityConfigSO Config
    {
        get
        {
            if (config != null)
                return config;

            config = Resources.Load<CombatActiveAbilityConfigSO>("CombatActiveAbilityConfig");
            if (config != null)
                return config;

            _runtimeFallback ??= ScriptableObject.CreateInstance<CombatActiveAbilityConfigSO>();
            return _runtimeFallback;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ResetSession()
    {
        _freezeReadyAtUnscaled = 0f;
        _damageReadyAtUnscaled = 0f;
        NotifyChanged();
    }

    public bool CanUseFreeze(out string reason)
    {
        if (!CanAttemptUse(out reason))
            return false;

        if (FreezeCooldownRemaining > 0f)
        {
            reason = FormatCooldownReason(FreezeCooldownRemaining);
            return false;
        }

        if (!HasLiveEnemiesOnField())
        {
            reason = "필드에 적이 없습니다";
            return false;
        }

        reason = null;
        return true;
    }

    public bool CanUseGlobalDamage(out string reason)
    {
        if (!CanAttemptUse(out reason))
            return false;

        if (GlobalDamageCooldownRemaining > 0f)
        {
            reason = FormatCooldownReason(GlobalDamageCooldownRemaining);
            return false;
        }

        if (!HasLiveEnemiesOnField())
        {
            reason = "필드에 적이 없습니다";
            return false;
        }

        reason = null;
        return true;
    }

    public bool CanAttemptUse(out string reason) => IsGameplayAvailable(out reason);

    public bool TryUseFreeze(out string failureReason)
    {
        if (!CanUseFreeze(out failureReason))
            return false;

        CombatActiveAbilityConfigSO settings = Config;
        int frozenCount = 0;
        Vector3 centerSum = Vector3.zero;

        foreach (Enemy enemy in Object.FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy)
                continue;

            EnemyFreezeState freeze = enemy.GetComponent<EnemyFreezeState>();
            if (freeze == null)
                freeze = enemy.gameObject.AddComponent<EnemyFreezeState>();

            freeze.ApplyFreeze(settings.freezeDurationSeconds);
            CombatActiveAbilityVfx.PlayFreeze(enemy, settings);
            centerSum += enemy.transform.position;
            frozenCount++;
        }

        if (frozenCount <= 0)
        {
            failureReason = "필드에 적이 없습니다";
            return false;
        }

        Vector3 fieldCenter = centerSum / frozenCount;
        Object.FindFirstObjectByType<WaveManager>()?.PauseWaveTime(settings.freezeDurationSeconds);
        CombatActiveAbilityVfx.PlayFreezeFieldBurst(fieldCenter, settings);
        CombatActiveAbilityVfx.PlayFreezeFeedback(frozenCount, settings);

        _freezeReadyAtUnscaled = Time.unscaledTime + settings.freezeCooldownSeconds;
        WarningMessageUi.Instance?.Show($"적 {frozenCount}마리와 웨이브 시간 {settings.freezeDurationSeconds:0.#}초 정지");
        NotifyChanged();
        failureReason = null;
        return true;
    }

    public bool TryUseGlobalDamage(out string failureReason)
    {
        if (!CanUseGlobalDamage(out failureReason))
            return false;

        CombatActiveAbilityConfigSO settings = Config;
        int hitCount = 0;
        float damageRatio = settings.damagePercentOfMaxHealth;
        float minimumDamage = settings.minimumDamagePerEnemy;
        Vector3 centerSum = Vector3.zero;

        foreach (Enemy enemy in Object.FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy || enemy.Health == null)
                continue;

            float damage = Mathf.Max(minimumDamage, enemy.Health.maxHealth * damageRatio);
            Vector3 hitPoint = enemy.transform.position + Vector3.up * 0.5f;
            enemy.ApplyDamage(new DamageData(damage, hitPoint, null));
            CombatActiveAbilityVfx.PlayGlobalDamageHit(enemy, settings);
            centerSum += enemy.transform.position;
            hitCount++;
        }

        if (hitCount <= 0)
        {
            failureReason = "필드에 적이 없습니다";
            return false;
        }

        Vector3 fieldCenter = centerSum / hitCount;
        CombatActiveAbilityVfx.PlayGlobalDamageBurst(fieldCenter, settings);
        CombatActiveAbilityVfx.PlayGlobalDamageFeedback(settings);

        _damageReadyAtUnscaled = Time.unscaledTime + settings.globalDamageCooldownSeconds;
        WarningMessageUi.Instance?.Show($"적 {hitCount}마리에게 피해");
        NotifyChanged();
        failureReason = null;
        return true;
    }

    private bool IsGameplayAvailable(out string reason)
    {
        if (TitlePreviewMode.Active)
        {
            reason = "미리보기";
            return false;
        }

        if (FieldEnemyTracker.Instance != null && FieldEnemyTracker.Instance.IsGameOver)
        {
            reason = "게임 종료";
            return false;
        }

        reason = null;
        return true;
    }

    private static string FormatCooldownReason(float remainingSeconds)
    {
        if (remainingSeconds >= 10f)
            return $"쿨타임 {Mathf.CeilToInt(remainingSeconds)}초";

        return $"쿨타임 {remainingSeconds:0.#}초";
    }

    private static bool HasLiveEnemiesOnField()
    {
        if (FieldEnemyTracker.Instance != null && FieldEnemyTracker.Instance.AliveCount > 0)
            return true;

        foreach (Enemy enemy in Object.FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy && !enemy.IsDead)
                return true;
        }

        return false;
    }

    private void NotifyChanged() => Changed?.Invoke();
}
