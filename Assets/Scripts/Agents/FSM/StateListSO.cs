using UnityEngine;

namespace Agents.FSM
{
    [CreateAssetMenu(fileName = "State list", menuName = "Agent/State list", order = 10)]
    public class StateListSO : ScriptableObject
    {
        [HideInInspector] public string generatePath;
        public string enumName;
        public StateSO[] states;
    }
}