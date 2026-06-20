using Player;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "GradeList",
    menuName = "Scriptable Objects/GradeList")]
public class GradeList : ScriptableObject
{
    public string gradeName;

    [Tooltip("소환 가중치 (0~100, 등급별 상대 비율)")]
    [Range(0f, 100f)]
    public float weight = 10f;

    public List<AbstractPlayer> tower = new();
}
