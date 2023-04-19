using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using Reshape.ReFramework;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class ActionTriggerNode : TriggerNode
    {
        [SerializeField]
        [ValueDropdown("DrawActionName1ListDropdown", ExpandAllMenuItems = true)]
        [OnValueChanged("MarkDirty")]
        private ActionNameChoice actionName;

        protected override State OnUpdate (GraphExecution execution, int updateId)
        {
            State state = execution.variables.GetState(guid, State.Running);
            if (state == State.Running)
            {
                if (execution.type == Type.ActionTrigger)
                {
                    if (actionName != null && execution.parameters.actionName != null && execution.parameters.actionName.Equals(actionName))
                    {
                        execution.variables.SetState(guid, State.Success);
                        state = State.Success;
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
        private static IEnumerable DrawActionName1ListDropdown ()
        {
            return ActionNameChoice.GetActionNameListDropdown();
        }

        public static string displayName = "Action Trigger Node";
        public static string nodeName = "Action";

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
            return actionName == null ? string.Empty : "Action Name : "+actionName;
        }
#endif
    }
}