using System;
using GGMLib.ModuleSystem;
using UnityEngine;

namespace Agents
{
    public class AgentTrigger : MonoBehaviour, IModule
    {
        public event Action OnAnimationEnd;
        public event Action OnDamageCast;
        
        public void Initialize(ModuleOwner owner)
        {
            //여기서는 안한다.    
        }
        
        private void AnimationEndTrigger() => OnAnimationEnd?.Invoke();
        private void DamageCastTrigger() => OnDamageCast?.Invoke();
    }
}