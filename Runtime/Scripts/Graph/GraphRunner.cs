using System;
using System.Runtime.CompilerServices;
using Reshape.ReFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using Reshape.Unity;

namespace Reshape.ReGraph
{
    [AddComponentMenu("ReGraph/Graph Runner", 1)]
    [HideMonoScript]
    [DisallowMultipleComponent]
    public class GraphRunner : BaseBehaviour
    {
        private const string TickName = "GraphRunner";

        [FoldoutGroup("Settings", expanded: false)]
        [ShowIf("ShowSettings")]
        [DisableIf("DisableSettings")]
        public bool runEvenInactive;

        [FoldoutGroup("Settings")]
        [ShowIf("ShowSettings")]
        [DisableIf("DisableSettings")]
        public bool runEvenDisabled;

        [FoldoutGroup("Settings")]
        [ShowIf("ShowSettings")]
        [DisableIf("DisableSettings")]
        public bool stopOnDeactivate;

        [HideLabel]
        public Graph graph;

        private GraphContext context;
        private bool disabled;

        public bool activated
        {
            get
            {
                if (this == null)
                    return false;
                if (!enabled)
                    if (!runEvenDisabled)
                        return false;
                if (!gameObject.activeInHierarchy)
                    if (!runEvenInactive)
                        return false;
                return true;
            }
        }

        //-----------------------------------------------------------------
        //-- static methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- public methods
        //-----------------------------------------------------------------

        public void TriggerAction (ActionNameChoice type)
        {
            if (type != null)
                Activate(TriggerNode.Type.ActionTrigger, actionName: type);
        }

        public void TriggerCollision (TriggerNode.Type type, GraphRunner runner)
        {
            OnTrigger(type, runner.gameObject);
        }

        [SpecialName]
        public void TriggerSpawn (ActionNameChoice type)
        {
            Activate(TriggerNode.Type.GameObjectSpawn, actionName: type);
        }

        public void TriggerInput (TriggerNode.Type type, string triggerId)
        {
            Activate(type, actionName: triggerId);
        }

        public void TriggerVariable (TriggerNode.Type type, string triggerId)
        {
            Activate(type, actionName: triggerId);
        }

        public void TriggerRay (TriggerNode.Type type, string triggerId)
        {
            Activate(type, actionName: triggerId);
        }

        public void ResumeTrigger (long executionId, int updateId)
        {
            var execution = graph?.FindExecute(executionId);
            if (execution == null)
            {
                ReDebug.LogWarning("Graph Warning", "Trigger " + executionId + " re-activation have not found in " + gameObject.name);
                return;
            }

            if (!activated)
            {
                graph?.StopExecute(execution, ReTime.frameCount);
                ReDebug.LogWarning("Graph Warning", "Trigger " + executionId + " re-activation being ignored in " + gameObject.name);
                return;
            }

            graph?.ResumeExecute(execution, ReTime.frameCount);
        }

        [SpecialName]
        public void InternalTrigger (string type)
        {
            Activate(TriggerNode.Type.All, actionName: type);
        }

        //-----------------------------------------------------------------
        //-- BaseBehaviour methods
        //-----------------------------------------------------------------

        [SpecialName]
        public override void Init ()
        {
            if (graph.HaveRequireInit())
                Activate();
            if (graph.HaveRequireUpdate())
                PlanTick(TickName);
            PlanUninit();
            DoneInit();
        }

        public override void Begin ()
        {
            Activate(TriggerNode.Type.OnStart);
            DoneBegin();
        }

        [SpecialName]
        public override void Tick ()
        {
            if (!activated)
                return;
            graph?.Update(ReTime.frameCount);
        }

        [SpecialName]
        public override void Uninit ()
        {
            if (graph.HaveRequireUpdate())
                OmitTick();
            if (graph.HaveRequireInit())
                Deactivate();
            DoneUninit();
        }

        [SpecialName]
        public override bool ReceivedRayCast (ReMonoBehaviour mono, string rayName, RaycastHit? hit)
        {
            if (!activated)
                return false;
            GraphExecution execute = Activate(TriggerNode.Type.RayReceived, actionName: rayName, interactedMono: mono);
            if (execute == null)
                return false;
            if (execute.isFailed)
                return false;
            return true;
        }

        //-----------------------------------------------------------------
        //-- mono methods
        //-----------------------------------------------------------------

        protected void Awake ()
        {
            if (graph != null)
            {
                context = new GraphContext(this);
                graph.Bind(context);
                if (graph.HaveRequireUpdate() || graph.HaveRequireInit())
                {
                    PlanInit();
                }
                if (graph.HaveRequireBegin())
                {
                    PlanBegin();
                }
            }
        }

        protected void LateUpdate ()
        {
            if (!activated)
                return;
            graph?.CleanExecutes();
        }

        protected void OnDisable ()
        {
            Disable();
        }

        protected void OnEnable ()
        {
            Enable();
        }

        protected void OnDestroy ()
        {
            if (graph?.Terminated == false)
            {
                OmitUninit();
                if (graph.HaveRequireUpdate())
                    OmitTick();
                if (graph.HaveRequireInit())
                    Deactivate();
            }
        }

        protected void OnTriggerEnter (Collider other)
        {
            OnTrigger(TriggerNode.Type.CollisionEnter, other.gameObject);
        }

        protected void OnTriggerExit (Collider other)
        {
            OnTrigger(TriggerNode.Type.CollisionExit, other.gameObject);
        }

        protected void OnTriggerEnter2D (Collider2D other)
        {
            OnTrigger(TriggerNode.Type.CollisionEnter, other.gameObject);
        }

        protected void OnTriggerExit2D (Collider2D other)
        {
            OnTrigger(TriggerNode.Type.CollisionExit, other.gameObject);
        }

        //-----------------------------------------------------------------
        //-- internal methods
        //-----------------------------------------------------------------

        private GraphExecution Activate (TriggerNode.Type type, string actionName = null, long executeId = 0, GameObject interactedGo = null, ReMonoBehaviour interactedMono = null)
        {
            if (!activated)
            {
                ReDebug.LogWarning("Graph Warning", type + " activation being ignored in " + gameObject.name);
                return null;
            }

            if (executeId == 0)
                executeId = ReTime.currentUtc;
            var execute = graph?.InitExecute(executeId, type);
            if (execute != null)
            {
                switch (type)
                {
                    case TriggerNode.Type.ActionTrigger:
                    case TriggerNode.Type.GameObjectSpawn:
                    case TriggerNode.Type.InputPress:
                    case TriggerNode.Type.InputRelease:
                    case TriggerNode.Type.VariableChange:
                    case TriggerNode.Type.RayAccepted:
                    case TriggerNode.Type.RayMissed:
                    case TriggerNode.Type.RayHit:
                    case TriggerNode.Type.RayLeave:
                    case TriggerNode.Type.All:
                        execute.parameters.actionName = actionName;
                        graph?.RunExecute(execute, ReTime.frameCount);
                        break;
                    case TriggerNode.Type.RayReceived:
                        execute.parameters.actionName = actionName;
                        execute.parameters.interactedMono = interactedMono;
                        graph?.RunExecute(execute, ReTime.frameCount);
                        break;
                    case TriggerNode.Type.CollisionEnter:
                    case TriggerNode.Type.CollisionExit:
                    case TriggerNode.Type.CollisionStepIn:
                    case TriggerNode.Type.CollisionStepOut:
                        execute.parameters.interactedGo = interactedGo;
                        graph?.RunExecute(execute, ReTime.frameCount);
                        break;
                    case TriggerNode.Type.OnStart:
                        graph?.RunExecute(execute, ReTime.frameCount);
                        break;
                }
            }

            return execute;
        }

        private void OnTrigger (TriggerNode.Type t, GameObject go)
        {
            Activate(t, interactedGo: go);
        }

        private void Enable ()
        {
            if (disabled && !stopOnDeactivate)
                graph?.UnpauseExecutes();
            disabled = false;
        }

        private void Disable ()
        {
            if (disabled) return;
            disabled = true;
            if (stopOnDeactivate)
                graph?.StopExecutes();
            if (activated) return;
            if (!stopOnDeactivate)
                graph?.PauseExecutes();
        }

        private void Activate ()
        {
            graph?.Start();
        }

        private void Deactivate ()
        {
            graph?.Stop();
        }

#if UNITY_EDITOR
        [Button]
        [ShowIf("ShowExecuteButton")]
        private void Execute (string actionName)
        {
            Activate(TriggerNode.Type.ActionTrigger, actionName: actionName);
        }

        private bool ShowExecuteButton ()
        {
            return Application.isPlaying && graph?.HavePreviewNode() == false;
        }

        private bool ShowSettings ()
        {
            return graph?.HavePreviewNode() == false && graph.Created;
        }

        private bool DisableSettings ()
        {
            return Application.isPlaying;
        }

        private void OnDrawGizmosSelected ()
        {
            if (graph == null)
                return;
            Graph.Traverse(graph.RootNode, (n) =>
            {
                if (n.drawGizmos)
                    n.OnDrawGizmos();
            });
        }
        
        public bool ContainNode (Type nodeType)
        {
            if (graph == null)
                return false;
            return graph.IsNodeTypeInUse(nodeType);
        }
#endif
    }
}