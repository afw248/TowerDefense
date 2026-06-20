using UnityEngine;

namespace CoreSystem.EffectSystem
{
    [CreateAssetMenu(fileName = "HitEffect", menuName = "Effects/Hit Effect", order = 0)]
    public class HitEffectDataSO : ScriptableObject
    {
        [Tooltip("SpecialSkillsEffectsPack 등 이펙트 프리팹")]
        public GameObject effectPrefab;

        [Min(0.05f)]
        public float scale = 0.5f;

        [Min(0.1f)]
        public float lifetime = 2f;

        public Vector3 positionOffset;
    }
}
