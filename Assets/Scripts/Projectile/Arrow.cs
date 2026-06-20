using JWLib.ObjectPool.Runtime;
using UnityEngine;

public class Arrow : AbstractBow , IPoolable
{
    public PoolItemSO Item { get; set; }

    public GameObject GameObject { get; set; }

    public override void Free()
    {
        gameObject.SetActive(false);
    }

    public override void New()
    {
        gameObject.SetActive(true);
    }

    public void ResetItem()
    {
        transform.localRotation = Quaternion.identity;
    }
}
