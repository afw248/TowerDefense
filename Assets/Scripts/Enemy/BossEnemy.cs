using Agents;
using CombatSystem;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class BossEnemy : MonoBehaviour
{
    private Enemy _enemy;
    private HealthModule _health;
    private bool _defeatHandled;

    public bool IsDead => _enemy != null && _enemy.IsDead;
    public HealthModule Health => _health;
    public string DisplayName { get; private set; } = "보스";

    public void Initialize(string sourcePrefabName)
    {
        _enemy = GetComponent<Enemy>();
        _health = _enemy != null ? _enemy.Health : GetComponentInChildren<HealthModule>();
        DisplayName = BuildDisplayName(sourcePrefabName);

        if (_health != null)
            _health.OnDeath += HandleDeath;

        if (_enemy != null)
            _enemy.OnDeath.AddListener(HandleDeath);
    }

    private static string BuildDisplayName(string sourcePrefabName)
    {
        if (string.IsNullOrWhiteSpace(sourcePrefabName))
            return "보스";

        string cleaned = sourcePrefabName
            .Replace("Enemy_Boss_", string.Empty)
            .Replace("Enemy_", string.Empty)
            .Replace('_', ' ')
            .Trim();

        return string.IsNullOrEmpty(cleaned) ? "보스" : $"보스 {cleaned}";
    }

    private void HandleDeath()
    {
        if (_defeatHandled)
            return;

        _defeatHandled = true;

        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        waveManager?.HandleBossDefeated(this);
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDeath -= HandleDeath;

        if (_enemy != null)
            _enemy.OnDeath.RemoveListener(HandleDeath);
    }
}
