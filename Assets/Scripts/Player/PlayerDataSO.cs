using System;
using UnityEngine;

namespace Player
{
    public enum EnemyDataField
    {
        DetectRadius,
        ViewAngle,
        StopDistance
    }
    
    [CreateAssetMenu(fileName = "Player data", menuName = "Agent/Player data", order = 35)]
    public class PlayerDataSO : ScriptableObject
    {
        [field: SerializeField] public float DetectRadius { get; set; } = 5f;
        [field: SerializeField] public float ViewAngle { get; set; } = 360;
        [field: SerializeField] public float StopDistance { get; set; } = 1.2f;
        [field: SerializeField] public float Attack { get; set; } = 5f;
        public float GetFieldValue(EnemyDataField fieldEnum) => fieldEnum switch
        {
            EnemyDataField.DetectRadius => DetectRadius,
            EnemyDataField.ViewAngle => ViewAngle,
            EnemyDataField.StopDistance => StopDistance,
            _ => 0
        };

    }
}