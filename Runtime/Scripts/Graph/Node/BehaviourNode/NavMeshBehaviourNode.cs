using System.Collections;
using System.Globalization;
using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class NavMeshBehaviourNode : BehaviourNode
    {
        public const string VAR_PROCEED = "_proceed";
        public const string VAR_DEST_X = "_dest_x";
        public const string VAR_DEST_Y = "_dest_y";
        public const string VAR_DEST_Z = "_dest_z";

        public enum ExecutionType
        {
            None,
            AgentWalk = 10,
            AgentStop = 50,
            AgentResume = 60,
            AgentSpeed = 100,
            AgentChase = 200,
            AgentGiveUp = 210,
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [ShowIf("@executionType != ExecutionType.None")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(agent)")]
        [InlineButton("@agent.SetObjectValue(AssignComponent<NavMeshAgent>())", "♺", ShowIf = "@agent.IsObjectValueType()")]
        private SceneObjectProperty agent = new SceneObjectProperty(SceneObject.ObjectType.NavMeshAgent);

        [SerializeField]
        [ShowIf("@executionType == ExecutionType.AgentWalk || executionType == ExecutionType.AgentChase || executionType == ExecutionType.AgentGiveUp")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(transform)")]
        [InlineButton("@transform.SetObjectValue(AssignComponent<UnityEngine.Transform>())", "♺", ShowIf = "@transform.IsObjectValueType()")]
        private SceneObjectProperty transform = new SceneObjectProperty(SceneObject.ObjectType.Transform);

        [SerializeField]
        [OnInspectorGUI("@MarkPropertyDirty(speed)")]
        [InlineProperty]
        [ShowIf("@executionType == ExecutionType.AgentSpeed")]
        private FloatProperty speed = new FloatProperty(0);

        private string proceedKey;
        private string destXKey;
        private string destYKey;
        private string destZKey;

        private void InitVariables ()
        {
            if (string.IsNullOrEmpty(proceedKey))
                proceedKey = guid + VAR_PROCEED;
            if (string.IsNullOrEmpty(destXKey))
                destXKey = guid + VAR_DEST_X;
            if (string.IsNullOrEmpty(destYKey))
                destYKey = guid + VAR_DEST_Y;
            if (string.IsNullOrEmpty(destZKey))
                destZKey = guid + VAR_DEST_Z;
        }

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (executionType == ExecutionType.None || agent.IsNull)
            {
                ReDebug.LogWarning("Graph Warning", "Found an empty NavMesh Behaviour node in " + context.gameObject.name);
            }
            else if (executionType == ExecutionType.AgentWalk)
            {
                if (transform.IsNull)
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty NavMesh Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    InitVariables();
                    execution.variables.SetInt(proceedKey, 0);
                    
                    var pos = ((Transform)transform).position;
                    ((NavMeshAgent)agent).destination = pos;
                    execution.variables.SetFloat(destXKey, pos.x);
                    execution.variables.SetFloat(destYKey, pos.y);
                    execution.variables.SetFloat(destZKey, pos.z);
                }
            }
            else if (executionType == ExecutionType.AgentStop)
            {
                ((NavMeshAgent)agent).isStopped = true;
            }
            else if (executionType == ExecutionType.AgentResume)
            {
                ((NavMeshAgent)agent).isStopped = false;
            }
            else if (executionType == ExecutionType.AgentSpeed)
            {
                ((NavMeshAgent)agent).speed = speed;
            }
            else if (executionType == ExecutionType.AgentChase)
            {
                var agt = (NavMeshAgent) agent;
                var chase = agt.GetComponent<AgentChaseController>();
                if (chase == null)
                    chase = agt.gameObject.AddComponent<AgentChaseController>();
                chase.Initial(agt, (Transform)transform);
            }
            else if (executionType == ExecutionType.AgentGiveUp)
            {
                var chase = ((NavMeshAgent)agent).GetComponent<AgentChaseController>();
                if (chase != null)
                    chase.Terminate();
            }

            base.OnStart(execution, updateId);
        }

        protected override State OnUpdate (GraphExecution execution, int updateId)
        {
            if (executionType is ExecutionType.AgentWalk)
            {
                int key = execution.variables.GetInt(proceedKey);
                if (key == 0)
                {
                    if (agent.IsNull)
                        return State.Failure;
                    var agt = (NavMeshAgent) agent;
                    if (!agt.pathPending)
                    {
                        var setDest = new Vector3();
                        setDest.x = execution.variables.GetFloat(destXKey);
                        setDest.y = execution.variables.GetFloat(destYKey);
                        setDest.z = execution.variables.GetFloat(destZKey);
                        if (Vector3.Distance(agt.destination, setDest) >= 1)
                        {
                            //~~ TODO Handle agent destination have been changed, some where in the script have commanding the same agent move elsewhere. 
                            return State.Failure;
                        }

                        if (agt.remainingDistance <= agt.stoppingDistance)
                        {
                            execution.variables.SetInt(proceedKey, 1);
                            key = 1;
                        }
                    }
                }

                if (key > 0)
                    return base.OnUpdate(execution, updateId);
                return State.Running;
            }

            return base.OnUpdate(execution, updateId);
        }

        public override bool IsRequireUpdate ()
        {
            return enabled;
        }

#if UNITY_EDITOR
        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Agent Walk", ExecutionType.AgentWalk},
            {"Agent Stop Walk", ExecutionType.AgentStop},
            {"Agent Resume Walk", ExecutionType.AgentResume},
            {"Agent Chase", ExecutionType.AgentChase},
            {"Agent Give Up Chase", ExecutionType.AgentGiveUp},
            {"Agent Speed", ExecutionType.AgentSpeed},
        };

        public static string displayName = "NavMesh Behaviour Node";
        public static string nodeName = "NavMesh";

        public override string GetNodeInspectorTitle ()
        {
            return displayName;
        }

        public override string GetNodeViewTitle ()
        {
            return nodeName;
        }

        public override string GetNodeViewDescription ()
        {
            if (executionType != ExecutionType.None && !agent.IsNull)
            {
                if (executionType == ExecutionType.AgentWalk && !transform.IsNull)
                {
                    var pos = ((Transform)transform).position;
                    return agent.name + " walk to " + transform.name + " " + pos + "\n<color=#FFF600>Continue at arrival";
                }

                if (executionType == ExecutionType.AgentChase && !transform.IsNull)
                {
                    return agent.name + " chase " + transform.name;
                }

                if (executionType == ExecutionType.AgentGiveUp && !transform.IsNull)
                {
                    return agent.name + " give up chase " + transform.name;
                }

                if (executionType == ExecutionType.AgentStop)
                    return agent.name + " stop walking";
                if (executionType == ExecutionType.AgentResume)
                    return agent.name + " resume walking";
                if (executionType == ExecutionType.AgentSpeed)
                    return agent.name + "'s agent speed change to " + speed;
            }

            return string.Empty;
        }
#endif
    }
}