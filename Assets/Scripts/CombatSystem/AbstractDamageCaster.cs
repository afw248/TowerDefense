using GGMLib.ModuleSystem;
using UnityEngine;

namespace CombatSystem
{
    public abstract class AbstractDamageCaster : MonoBehaviour
    {
        [SerializeField] protected LayerMask whatIsTarget;
        
        public ModuleOwner CasterOwner { get; private set; }

        public virtual void InitCaster(ModuleOwner owner)
        {
            CasterOwner = owner;
        }
        
        public abstract void CastDamage(Vector3 position, Vector3 direction, SkillDataSO skillData);
    }
}