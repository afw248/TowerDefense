using UnityEngine;

namespace CoreSystem.EffectSystem
{
    public static class HitVfxUtility
    {
        public static void Play(HitEffectDataSO data, GameObject target, Quaternion rotation)
        {
            if (data == null || target == null)
                return;

            Play(data, target.transform.position, rotation);
        }

        public static void Play(HitEffectDataSO data, GameObject target)
        {
            Play(data, target, Quaternion.identity);
        }

        public static void Play(HitEffectDataSO data, Vector3 position, Quaternion rotation)
        {
            if (data == null)
                return;

            VfxPoolManager.EnsureExists();
            VfxPoolManager.Instance.PlayHitEffect(data, position, rotation);
        }
    }
}
