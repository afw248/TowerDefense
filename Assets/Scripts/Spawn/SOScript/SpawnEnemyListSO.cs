using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnEnemyListSO", menuName = "Scriptable Objects/SpawnEnemyList")]
public class SpawnEnemyListSO : ScriptableObject
{
    public List<GameObject> enemy = new();
    public List<GameObject> bossEnemy = new();

    public GameObject GetBossPrefab(int index)
    {
        if (bossEnemy != null && index >= 0 && index < bossEnemy.Count && bossEnemy[index] != null)
            return bossEnemy[index];

        if (enemy == null || enemy.Count == 0)
            return null;

        index = Mathf.Clamp(index, 0, enemy.Count - 1);
        return enemy[index];
    }

    public bool HasBossPrefabOverride(int index)
    {
        return bossEnemy != null
            && index >= 0
            && index < bossEnemy.Count
            && bossEnemy[index] != null;
    }
}
