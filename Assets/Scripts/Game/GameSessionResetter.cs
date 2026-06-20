using Player;
using UnityEngine;

public static class GameSessionResetter
{
    public static void ResetForNewSession()
    {
        ClearEnemies();
        ClearTowers();
        ResetTrackers();
        ResetEconomy();
        ResetUpgrades();
        HideGameOver();
        HideTransientUi();
        ResetGameSpeed();
    }

    public static void ClearEnemiesOnly()
    {
        ClearEnemies();
        ResetTrackers();
    }

    private static void ClearTowers()
    {
        TileManager tileManager = Object.FindFirstObjectByType<TileManager>();
        if (tileManager == null)
            return;

        tileManager.ClearAllTowersImmediate();
    }

    private static void ClearEnemies()
    {
        SpawnManager spawner = Object.FindFirstObjectByType<SpawnManager>();
        if (spawner != null && spawner.spawnPoint != null)
        {
            Transform spawnRoot = spawner.spawnPoint;
            for (int i = spawnRoot.childCount - 1; i >= 0; i--)
                Object.Destroy(spawnRoot.GetChild(i).gameObject);
        }

        foreach (Enemy enemy in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            if (enemy != null)
                Object.Destroy(enemy.gameObject);
        }

        foreach (BossEnemy boss in Object.FindObjectsByType<BossEnemy>(FindObjectsSortMode.None))
        {
            if (boss != null)
                Object.Destroy(boss.gameObject);
        }
    }

    private static void ResetTrackers()
    {
        FieldEnemyTracker.Instance?.ResetState();
        LeakTracker.Instance?.ResetState();
    }

    private static void ResetEconomy()
    {
        EconomyManager.Instance?.ResetSession();
    }

    private static void ResetUpgrades()
    {
        ArchetypeUpgradeManager.Instance?.ResetUpgrades();
        CombatActiveAbilityController.Instance?.ResetSession();
    }

    private static void HideGameOver()
    {
        if (GameOverUi.Instance != null)
            GameOverUi.Instance.Hide();
    }

    private static void HideTransientUi()
    {
        TowerInfoPanelUi towerInfoPanel = Object.FindFirstObjectByType<TowerInfoPanelUi>(FindObjectsInactive.Include);
        towerInfoPanel?.Hide();

        TowerInspectRangeIndicator.Instance?.Hide();
        BossHealthBarUi.Instance?.Hide();
        SettingsPanelUi settingsPanel = Object.FindFirstObjectByType<SettingsPanelUi>(FindObjectsInactive.Include);
        settingsPanel?.PrepareHidden();

        ArchetypeUpgradePanelUi upgradePanel = Object.FindFirstObjectByType<ArchetypeUpgradePanelUi>(FindObjectsInactive.Include);
        upgradePanel?.PrepareHidden();
    }

    private static void ResetGameSpeed()
    {
        Time.timeScale = 1f;
        GameSpeedController.Instance?.SetNormalSpeed();
    }
}
