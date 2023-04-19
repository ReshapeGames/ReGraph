using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace Reshape.ReFramework
{
    [HideMonoScript]
    public class AgentChaseController : BaseBehaviour
    {
        private Transform targetTransform;
        private NavMeshAgent agent;

        public void Initial (NavMeshAgent agent, Transform trans)
        {
            targetTransform = trans;
            this.agent = agent;
        }

        public void Terminate ()
        {
            Destroy(this);
        }

        protected void Update ()
        {
            if (agent == null || targetTransform == null)
                Terminate();
            agent.destination = targetTransform.position;
        }
    }
}