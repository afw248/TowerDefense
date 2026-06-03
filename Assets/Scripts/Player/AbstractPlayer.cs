using System;
using Agents;
using Agents.FSM;
using CombatSystem;
using FSM;
using UnityEngine;

namespace Player
{
    public abstract class AbstractPlayer : Agent
    {
        [field: SerializeField] public PlayerDataSO PlayerData { get; private set; } 
        public IRenderer Renderer { get; private set; }
        public ISensor Sensor { get; private set; }
        public ISkillModule SkillModule { get; private set; }
        public AgentTrigger Trigger { get; private set; } 
        [field:SerializeField]public StateListSO playerStates { get; private set; }

        public StateMachine _stateMachine;

        [SerializeField] private bool isDebugMode;

        
        protected override void InitializeModules()
        {
            base.InitializeModules();
            _stateMachine = new StateMachine(this, playerStates.states);
            Renderer = GetModule<IRenderer>();
            Sensor = GetModule<ISensor>();
            SkillModule = GetModule<ISkillModule>();
            Trigger = GetModule<AgentTrigger>();
        }

        protected override void AfterInitModules()
        {
            base.AfterInitModules();
        }
        public void OnEnable()
        {
            if (_stateMachine != null)
            {
                ChangeState(PlayerState.INSTALL, transitionDuration: 0);
            }
        }
        public void Update()
        {
            _stateMachine.UpdateMachine();
        }
        private void OnDrawGizmos()
        {
            if (!isDebugMode) return;
            if (PlayerData == null) return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, PlayerData.DetectRadius);
        }
        public void ChangeState(PlayerState newState, float transitionDuration)
    => _stateMachine.ChangeState((int)newState, transitionDuration);
    }
}