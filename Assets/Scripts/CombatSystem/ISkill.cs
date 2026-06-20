using System;
using UnityEngine;

namespace CombatSystem
{
    public interface ISkill
    {
        event Action OnSkillEnd;
        SkillDataSO SkillData { get; }
        bool IsUsing { get; }
        float NormalizedCooldown { get; } //0~1로 표현되는 쿨다운. 1일때 사용가능
        
        void InitializeSkill(ISkillModule skillModule);
        bool CanUseSkill(GameObject target = null); //타겟팅 스킬을 위해 게임오브젝트를 받는다.
        void UseSkill(GameObject target = null);
        void StopSkill(); //스킬 강제 종료.
    }
}