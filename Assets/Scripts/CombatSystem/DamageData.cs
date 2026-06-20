using GGMLib.ModuleSystem;
using UnityEngine;

namespace CombatSystem
{
    public struct DamageData
    {
        public float DamageAmount;
        public Vector3 HitPoint;
        public ModuleOwner Attacker;
        public DamageData(float damageAmount, Vector3 hitPoint, ModuleOwner attacker)
        {
            DamageAmount = damageAmount;
            HitPoint = hitPoint;
            Attacker = attacker;
        }
    }
}