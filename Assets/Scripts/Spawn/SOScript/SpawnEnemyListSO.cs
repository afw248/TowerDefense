using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnEnemyListSO", menuName = "Scriptable Objects/SpawnEnemyList")]
public class SpawnEnemyListSO : ScriptableObject
{
   public List<GameObject> enemy = new(); 
}
