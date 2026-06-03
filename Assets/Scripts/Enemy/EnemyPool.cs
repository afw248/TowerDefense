using JWLib.ObjectPool.Runtime;
using UnityEngine;

public class EnemyPool : MonoBehaviour, IPoolable
{
    public PoolItemSO Item { get; set; }

    public GameObject GameObject {  get; set; }

    public void ResetItem()
    {
       
    }
}
