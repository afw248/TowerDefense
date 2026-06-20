using System.Collections.Generic;
using CoreSystem.EffectSystem;
using GGMLib.ModuleSystem;
using UnityEngine;

public class TowerProjectileSpawner : MonoBehaviour
{
    public static TowerProjectileSpawner Instance { get; private set; }

    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
    private readonly Dictionary<GameObject, HashSet<GameObject>> _pooledInstances = new();
    private Transform _poolRoot;

    public static void EnsureExists()
    {
        if (Instance != null)
            return;

        TowerProjectileSpawner existing = FindFirstObjectByType<TowerProjectileSpawner>();
        if (existing != null)
        {
            Instance = existing;
            return;
        }

        var go = new GameObject(nameof(TowerProjectileSpawner));
        go.AddComponent<TowerProjectileSpawner>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreateOnLoad() => EnsureExists();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _poolRoot = new GameObject("ProjectilePoolRoot").transform;
        _poolRoot.SetParent(transform);
    }

    public static void Launch(
        TowerProjectileDataSO data,
        Transform firePoint,
        Transform target,
        float effectDamage,
        TowerAttackVfxDataSO hitVfx,
        HitEffectDataSO impactHitEffect,
        ModuleOwner attacker,
        GameObject targetObject)
    {
        if (data == null || data.projectilePrefab == null || firePoint == null || target == null)
            return;

        EnsureExists();
        if (data.spawnHitGraceDuration > 0f)
            Instance.EnsurePoolCapacity(data.projectilePrefab, 3);

        Vector3 direction = (target.position - firePoint.position).normalized;
        if (direction.sqrMagnitude < 0.0001f)
            direction = firePoint.forward;

        Quaternion rotation = Quaternion.LookRotation(direction);
        GameObject instance = Instance.GetFromPool(data.projectilePrefab);

        HomingTowerProjectile projectile = instance.GetComponent<HomingTowerProjectile>();
        if (projectile == null)
            projectile = instance.AddComponent<HomingTowerProjectile>();

        projectile.Launch(
            data.projectilePrefab,
            data,
            firePoint.position,
            rotation,
            target,
            effectDamage,
            hitVfx,
            impactHitEffect,
            attacker,
            targetObject);
    }

    public static void ReturnToPool(GameObject prefab, GameObject instance)
    {
        if (Instance == null || prefab == null || instance == null)
            return;

        Instance.Release(prefab, instance);
    }

    private void EnsurePoolCapacity(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0)
            return;

        EnsurePoolRoot();

        if (!_pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            _pools[prefab] = queue;
        }

        int available = queue.Count;
        for (int i = available; i < count; i++)
        {
            GameObject created = Instantiate(prefab);
            created.SetActive(false);
            created.transform.SetParent(_poolRoot, false);
            queue.Enqueue(created);
        }
    }

    private void EnsurePoolRoot()
    {
        if (_poolRoot == null)
        {
            _poolRoot = new GameObject("ProjectilePoolRoot").transform;
            _poolRoot.SetParent(transform);
        }
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        EnsurePoolRoot();
        EnsurePoolTracking(prefab);

        if (_pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            while (queue.Count > 0)
            {
                GameObject pooled = queue.Dequeue();
                if (pooled == null)
                    continue;

                _pooledInstances[prefab].Remove(pooled);

                if (pooled.activeInHierarchy)
                    continue;

                pooled.transform.SetParent(null, true);
                return pooled;
            }
        }
        else
        {
            _pools[prefab] = new Queue<GameObject>();
        }

        GameObject created = Instantiate(prefab);
        created.SetActive(false);
        created.transform.SetParent(null, true);
        return created;
    }

    private void Release(GameObject prefab, GameObject instance)
    {
        if (prefab == null || instance == null)
            return;

        EnsurePoolRoot();
        EnsurePoolTracking(prefab);

        if (!_pooledInstances[prefab].Add(instance))
            return;

        instance.SetActive(false);
        instance.transform.SetParent(_poolRoot, false);

        if (!_pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            _pools[prefab] = queue;
        }

        queue.Enqueue(instance);
    }

    private void EnsurePoolTracking(GameObject prefab)
    {
        if (!_pooledInstances.ContainsKey(prefab))
            _pooledInstances[prefab] = new HashSet<GameObject>();
    }
}
