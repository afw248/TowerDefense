using Player;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "GradeList",
    menuName = "Scriptable Objects/GradeList")]
public class GradeList : ScriptableObject
{
    public string gradeName;

    [Range(0, 100)]
    public int weight = 10;

    public List<AbstractPlayer> tower = new();
}