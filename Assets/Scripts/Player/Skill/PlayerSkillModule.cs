using System;
using System.Collections.Generic;
using System.Linq;
using CombatSystem;
using GGMLib.ModuleSystem;
using UnityEngine;

namespace Player
{
    public class PlayerSkillModule : MonoBehaviour, IModule, ISkillModule
    {
        public ModuleOwner Owner { get; private set; }
        public AbstractPlayer Enemy { get; private set; }

        public event Action OnCurrentSkillEnd;

        private Dictionary<int, ISkill> _skillDict;

        // 현재 사용 중인 스킬
        private ISkill _currentSkill;

        public void Initialize(ModuleOwner owner)
        {
            Owner = owner;

            Enemy = Owner as AbstractPlayer;

            Debug.Assert(
                Enemy != null,
                $"적의 스킬 모듈은 반드시 AbstractEnemy의 자식이어야 합니다. : {gameObject}");

            _skillDict =
                GetComponentsInChildren<ISkill>()
                    .ToDictionary(skill => skill.SkillData.skillIndex);

            foreach (ISkill skill in _skillDict.Values)
            {
                skill.InitializeSkill(this);
            }
        }

        public bool CanUseSkill(int skillIndex, GameObject target = null)
        {
            // 이미 스킬 사용 중
            if (_currentSkill != null)
                return false;

            if (_skillDict.TryGetValue(skillIndex, out ISkill skill))
            {
                return skill.CanUseSkill(target);
            }

            return false;
        }

        public void UseSkill(int skillIndex, GameObject target = null)
        {
            // 이미 사용 중이면 무시
            if (_currentSkill != null)
                return;
            if (_skillDict.TryGetValue(skillIndex, out ISkill skill))
            {
                _currentSkill = skill;

                _currentSkill.OnSkillEnd += InvokeSkillEnd;

                skill.UseSkill(target);
            }
        }

        public void StopCurrentSkill()
        {
            ForceStopAllSkills();
        }

        public void ForceStopAllSkills()
        {
            if (_currentSkill != null)
            {
                _currentSkill.OnSkillEnd -= InvokeSkillEnd;
                _currentSkill = null;
            }

            foreach (ISkill skill in _skillDict.Values)
            {
                if (skill is PlayerAttackSkill attackSkill)
                    attackSkill.ForceStopForDrag();
                else if (skill.IsUsing)
                    skill.StopSkill();
            }
        }

        public void InvokeSkillEnd()
        {
            if (_currentSkill != null)
            {
                _currentSkill.OnSkillEnd -= InvokeSkillEnd;

                // 가장 중요
                _currentSkill = null;
            }

            OnCurrentSkillEnd?.Invoke();
        }

    }
}