using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using Reshape.ReFramework;
using Reshape.Unity;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class RunnerStartTriggerNode : TriggerNode 
    {
        protected override State OnUpdate (GraphExecution execution, int updateId)
        {
            State state = execution.variables.GetState(guid, State.Running);
            if (state == State.Running)
            {
                if (execution.type is Type.OnStart)
                {
                    execution.variables.SetState(guid, State.Success);
                    state = State.Success;
                }
                else if (execution.type == Type.All)
                {
                    if (execution.parameters.actionName.Equals(TriggerId))
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

        public override bool IsRequireBegin ()
        {
            return true;
        }

#if UNITY_EDITOR
        public static string displayName = "Runner Start Trigger Node";
        public static string nodeName = "Runner Start";

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
            return "When graph runner first time enable";
        }
#endif
    }
}