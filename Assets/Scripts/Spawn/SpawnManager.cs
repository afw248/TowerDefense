using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class SpawnManager : MonoBehaviour
{
    [field:SerializeField] public SpawnEnemyListSO spawn { get;private set; }
    [field: SerializeField] public Transform spawnPoint { get; private set; }
    [field: SerializeField] public SplineContainer spline { get; private set; }
    public float spawnDelay { get; set; } = 1f;
    public GameObject spawnEnemy { get; set; }

    public void Spawn()
    {
        if (spawnEnemy == null) return;     

        Vector3 spawnpos = new Vector3(
            spawnPoint.position.x,
            spawnEnemy.transform.position.y,
            spawnPoint.position.z
        );

        GameObject Enemy = Instantiate(spawnEnemy, spawnpos, Quaternion.identity);
        if (Enemy.TryGetComponent<SplineMove>(out SplineMove spEnemy))
        {
            spEnemy.transform.SetParent(spawnPoint,true);
            spEnemy.Path(spline);
        }
    }
    public int EnemyCounting()
    {
        return transform.childCount;
    }
}