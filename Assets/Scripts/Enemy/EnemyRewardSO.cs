using UnityEngine;

[CreateAssetMenu(fileName = "EnemyReward", menuName = "TowerDefense/Enemy Reward")]
public class EnemyRewardSO : ScriptableObject
{
    [Min(0)]
    public int killReward = 5;
}
