using System.Collections.Generic;
using System.Linq;
using GGMLib.ModuleSystem;
using UnityEngine;

namespace CoreSystem.EffectSystem
{
    public class VfxModule : MonoBehaviour, IModule
    {
        private ModuleOwner _owner;
        private Dictionary<int, IPlayableVFX> _vfxDict;
        
        public void Initialize(ModuleOwner owner)
        {
            _owner = owner;
            _vfxDict = GetComponentsInChildren<IPlayableVFX>().ToDictionary(vfx => vfx.VfxName.AssetHash);
        }

        public void PlayVfx(int vfxHash, Vector3 position, Quaternion rotation)
        {
            if (_vfxDict.TryGetValue(vfxHash, out IPlayableVFX vfx))
            {
                vfx.PlayVfx(position, rotation);
            }
            else
            {
                Debug.LogWarning($"VFX with hash {vfxHash} not found");
            }
        }

        public void PlayVfx(int vfxHash)
        {
            if (_vfxDict.TryGetValue(vfxHash, out IPlayableVFX vfx))
            {
                vfx.PlayVfx(); //제자리에서 재생하는 vfx
            } else
            {
                Debug.LogWarning($"VFX with hash {vfxHash} not found");
            }
        }

        public void StopVfx(int vfxHash)
        {
            if (_vfxDict.TryGetValue(vfxHash, out IPlayableVFX vfx))
            {
                vfx.StopVfx();
            }
        }
    }
}