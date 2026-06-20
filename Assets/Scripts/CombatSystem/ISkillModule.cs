using System;
using GGMLib.ModuleSystem;
using UnityEngine;

namespace CombatSystem
{
    public interface ISkillModule
    {
        ModuleOwner Owner { get; }

        event Action OnCurrentSkillEnd;
    
        //GameObject를 매개변수로 넣은 이유는 타겟팅 스킬을 구현할 때 사용하고자 함이다.
        bool CanUseSkill(int skillIndex, GameObject target = null); //인덱스 스킬이 사용가능한지 체크
        void UseSkill(int skillIndex, GameObject target = null); // 인덱스 스킬을 사용해라.
        void StopCurrentSkill();
        void ForceStopAllSkills();
        void InvokeSkillEnd(); //스킬을 종료시킨다.
    }
}