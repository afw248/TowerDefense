using GGMLib.CoreLib;
using UnityEngine;

namespace CoreSystem.EffectSystem
{
    public class PlayParticleVfx : MonoBehaviour, IPlayableVFX
    {
        [field: SerializeField] public AssetNameSO VfxName { get; private set; }
        [SerializeField] private ParticleSystem[] particles;
        
        public void PlayVfx(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            PlayVfx();
        }

        public void PlayVfx()
        {
            foreach(ParticleSystem ps in particles)
                ps.Play();
        }

        public void StopVfx()
        {
            foreach(ParticleSystem ps in particles)
                ps.Stop();
        }
    }
}