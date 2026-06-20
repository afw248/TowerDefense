using GGMLib.ModuleSystem;
using TMPro;
using UnityEngine;

namespace Agents
{
    [RequireComponent(typeof(Animator))]
    public class AgentRenderer : MonoBehaviour, IModule, IRenderer
    {
        public Animator Animator { get; private set; }
        protected ModuleOwner _owner;


        public virtual void Initialize(ModuleOwner owner)
        {
            _owner = owner;
            Animator = GetComponent<Animator>();
        }

        public void PlayClip(int clipHash, float normalizedTime, float crossFadeDuration, int layerIndex = 0)
        {
            if (Animator == null)
                return;

            Animator.CrossFadeInFixedTime(clipHash, crossFadeDuration, layerIndex, normalizedTime);
            Animator.Update(0f);
        }

        public void SetFloat(int idHash, float value)
        {
            Animator.SetFloat(idHash, value);
        }
    }
}