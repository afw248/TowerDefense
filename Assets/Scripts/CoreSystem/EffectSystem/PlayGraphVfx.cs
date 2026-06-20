using GGMLib.CoreLib;
using UnityEngine;
using UnityEngine.VFX;

namespace CoreSystem.EffectSystem
{
    public class PlayGraphVfx : MonoBehaviour, IPlayableVFX
    {
        [field: SerializeField] public AssetNameSO VfxName { get; private set; }
        [SerializeField] private VisualEffect[] effects;
        
        public void PlayVfx(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            PlayVfx();
        }

        public void PlayVfx()
        {
            foreach (VisualEffect effect in effects)
            {
                effect.Play();
            }
        }

        public void StopVfx()
        {
            foreach (VisualEffect effect in effects)
            {
                effect.Stop();
            }
        }
    }
}