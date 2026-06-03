using GGMLib.ModuleSystem;
using UnityEngine;

namespace CombatSystem
{
    public class ActionDataModule : MonoBehaviour, IModule
    {
        public Vector3 HitPoint { get; set; }
        public Vector3 HitNormal { get; set; }
        public ModuleOwner Attacker { get; set; }
        //지금은 이 3개지만 차츰 메모장에 적힐 내용이 많아 진다.

        private ModuleOwner _owner;
        public void Initialize(ModuleOwner owner)
        {
            _owner = owner;
        }
    }
}