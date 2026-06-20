using GGMLib.CoreLib;
using UnityEngine;

namespace CoreSystem.EffectSystem
{
    public interface IPlayableVFX
    {
        AssetNameSO VfxName { get; }
        void PlayVfx(Vector3 position, Quaternion rotation);
        void PlayVfx();
        void StopVfx();
    }
}