using System;
using System.Collections.Generic;
using UnityEngine;

namespace Agents.FSM
{
    public class StateMachine
    {
        public AgentState CurrentState { get; private set; }

        private Dictionary<int, AgentState> _stateDict;

        public StateMachine(Agent agent, StateSO[] stateList)
        {
            _stateDict = new Dictionary<int, AgentState>();

            foreach (StateSO stateData in stateList)
            {
                Type type = Type.GetType(stateData.className);
                Debug.Assert(type != null, $"찾고자 하는 타입이 존재하지 않습니다. : {stateData.className}");

                int paramHash = stateData.stateParam != null ? stateData.stateParam.ParamHash : 0;
                AgentState agentState = (AgentState)Activator.CreateInstance(type, agent, paramHash);
                
                _stateDict.Add(stateData.assetIndex, agentState);
            }
        }

        public void ChangeState(int newStateIndex, float transitionDuration = 0.1f)
        {
            CurrentState?.Exit();
            AgentState newState = _stateDict.GetValueOrDefault(newStateIndex);
            Debug.Assert(newState != null, $"실행하려는 상태가 존재하지 않습니다. : {newStateIndex}");
            CurrentState = newState;
            CurrentState.Enter(transitionDuration);
        }
        
        public void UpdateMachine() => CurrentState?.Update();
    }
}