using UnityEngine;

namespace CombatSystem
{
    [CreateAssetMenu(fileName = "Skill data", menuName = "Agent/Skill data", order = 25)]
    public class SkillDataSO : ScriptableObject
    {
        public int skillIndex;
        public string skillName;
        public float cooldown;
        public float damageMultiplier = 1f;
    }
}