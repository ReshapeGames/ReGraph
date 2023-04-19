using System;
using System.Collections;
using Reshape.ReFramework;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class RayTriggerNode : TriggerNode
    {
        [ValueDropdown("TriggerTypeChoice")]
        [OnValueChanged("MarkDirty")]
        public Type triggerType;

        [SerializeField]
        [ValueDropdown("DrawActionName1ListDropdown", ExpandAllMenuItems = true)]
        [OnValueChanged("MarkDirty")]
        private ActionNameChoice actionName;

        protected override State OnUpdate (GraphExecution execution, int updateId)
        {
            State state = execution.variables.GetState(guid, State.Running);
            if (state == State.Running)
            {
                if (execution.type == triggerType && execution.type is Type.RayAccepted or Type.RayMissed or Type.RayHit or Type.RayReceived or Type.RayLeave)
                {
                    if (actionName != null && execution.parameters.actionName != null && execution.parameters.actionName.Equals(actionName))
                    {
                        execution.variables.SetState(guid, State.Success);
                        state = State.Success;
                        
                        if (execution.type is Type.RayReceived)
                        {
                            RayCastingController controller = execution.parameters.interactedMono.Remember<RayCastingController>(RayCastingController.INSTANCE);
                            if (controller != null)
                            {
                                controller.AddReceiver(context.runner, execution.parameters.actionName);
                            }
                        }
                    }
                }
                else if (execution.type == Type.All)
                {
                    if (execution.parameters.actionName != null && execution.parameters.actionName.Equals(TriggerId))
                    {
                        execution.variables.SetState(guid, State.Success);
                        state = State.Success;
                    }
                }

                if (state != State.Success)
                {
                    execution.variables.SetState(guid, State.Failure);
                    state = State.Failure;
                }
            }

            if (state == State.Success)
                return base.OnUpdate(execution, updateId);
            return State.Failure;
        }

#if UNITY_EDITOR
        private IEnumerable TriggerTypeChoice ()
        {
            ValueDropdownList<Type> menu = new ValueDropdownList<Type>();
            menu.Add("Being Accepted", Type.RayAccepted);
            menu.Add("Being Missed", Type.RayMissed);
            menu.Add("Being Hit Yet Accept", Type.RayHit);
            menu.Add("Receiving", Type.RayReceived);
            menu.Add("Leaving", Type.RayLeave);
            return menu;
        }

        private static IEnumerable DrawActionName1ListDropdown ()
        {
            return ActionNameChoice.GetActionNameListDropdown();
        }

        public static string displayName = "Ray Trigger Node";
        public static string nodeName = "Ray";

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
            string desc = String.Empty;
            if (triggerType == Type.RayAccepted && actionName != null)
                desc = "Ray Being Accepted at " + actionName + " action";
            else if (triggerType == Type.RayMissed && actionName != null)
                desc = "Ray Being Missed at " + actionName + " action";
            else if (triggerType == Type.RayHit && actionName != null)
                desc = "Ray Being Hit Yet Accept at " + actionName + " action";
            else if (triggerType == Type.RayReceived && actionName != null)
                desc = "Ray Receiving at " + actionName + " action";
            else if (triggerType == Type.RayLeave && actionName != null)
                desc = "Ray Leaving at " + actionName + " action";
            return desc;
        }
#endif
    }
}