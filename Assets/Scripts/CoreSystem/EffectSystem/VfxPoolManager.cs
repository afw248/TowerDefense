using System.Collections.Generic;
using UnityEngine;

namespace CoreSystem.EffectSystem
{
    public class VfxPoolManager : MonoBehaviour
    {
        public static VfxPoolManager Instance { get; private set; }

        [SerializeField]
        private int prewarmCountPerPrefab = 2;

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
        private Transform _poolRoot;

        public static void EnsureExists()
        {
            if (Instance != null)
                return;

            VfxPoolManager existing = FindFirstObjectByType<VfxPoolManager>();
            if (existing != null)
            {
                Instance = existing;
                return;
            }

            var go = new GameObject(nameof(VfxPoolManager));
            go.AddComponent<VfxPoolManager>();
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
            EnsurePoolRoot();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            _pools.Clear();
        }

        public void PlayHitEffect(HitEffectDataSO data, Vector3 position, Quaternion rotation)
        {
            if (data == null || data.effectPrefab == null)
                return;

            GameObject instance = GetInstance(data.effectPrefab);
            if (instance == null)
                return;

            instance.transform.SetPositionAndRotation(
                position + data.positionOffset,
                rotation);

            PackParticleEffect player = instance.GetComponent<PackParticleEffect>();
            if (player == null)
                player = instance.AddComponent<PackParticleEffect>();

            player.Configure(data.effectPrefab, data.lifetime, data.scale);
            player.Play();
        }

        public void ReturnToPool(GameObject prefab, GameObject instance)
        {
            if (instance == null)
                return;

            StopAllParticles(instance);
            EnsurePoolRoot();

            instance.SetActive(false);
            instance.transform.SetParent(_poolRoot, false);

            if (prefab == null)
                return;

            if (!_pools.TryGetValue(prefab, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>();
                _pools[prefab] = queue;
            }

            queue.Enqueue(instance);
        }

        private static void StopAllParticles(GameObject instance)
        {
            ParticleSystem[] particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in particleSystems)
            {
                if (ps != null)
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public GameObject GetInstance(GameObject prefab)
        {
            if (prefab == null)
                return null;

            GameObject instance = GetFromPool(prefab);
            if (instance == null)
                return null;

            instance.transform.SetParent(null);
            instance.transform.localScale = Vector3.one;
            StopAllParticles(instance);

            instance.SetActive(true);
            return instance;
        }

        private void EnsurePoolRoot()
        {
            if (_poolRoot != null)
                return;

            _poolRoot = new GameObject("VfxPoolRoot").transform;
            _poolRoot.SetParent(transform);
        }

        private GameObject GetFromPool(GameObject prefab)
        {
            if (prefab == null)
                return null;

            EnsurePoolRoot();

            if (_pools.TryGetValue(prefab, out Queue<GameObject> queue))
            {
                while (queue.Count > 0)
                {
                    GameObject pooled = queue.Dequeue();
                    if (pooled != null)
                        return pooled;
                }
            }
            else
            {
                _pools[prefab] = new Queue<GameObject>();
            }

            GameObject created = Instantiate(prefab, _poolRoot);
            created.SetActive(false);
            return created;
        }

        public void Prewarm(HitEffectDataSO data)
        {
            if (data == null || data.effectPrefab == null)
                return;

            EnsurePoolRoot();

            if (!_pools.ContainsKey(data.effectPrefab))
                _pools[data.effectPrefab] = new Queue<GameObject>();

            for (int i = 0; i < prewarmCountPerPrefab; i++)
            {
                GameObject instance = Instantiate(data.effectPrefab, _poolRoot);
                instance.SetActive(false);
                _pools[data.effectPrefab].Enqueue(instance);
            }
        }
    }
}
