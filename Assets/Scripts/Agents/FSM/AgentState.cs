namespace Agents.FSM
{
    public abstract class AgentState
    {
        protected readonly Agent _agent;
        protected readonly int _stateClipHash;
        protected readonly IRenderer _renderer;

        public AgentState(Agent agent, int clipHash)
        {
            _agent = agent;
            _stateClipHash = clipHash;
            _renderer = agent.GetModule<IRenderer>();
        }

        public virtual void Enter(float transitionDuration, int layerIndex = 0)
        {

        }
        
        public virtual void Update(){}
        public virtual void Exit(){}
    }
}