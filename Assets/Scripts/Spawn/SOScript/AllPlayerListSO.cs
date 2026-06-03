using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AllPlayerListSO", menuName = "Scriptable Objects/AllPlayerListSO")]
public class AllPlayerListSO : ScriptableObject
{
    public List<GradeList> towerList = new();
}
