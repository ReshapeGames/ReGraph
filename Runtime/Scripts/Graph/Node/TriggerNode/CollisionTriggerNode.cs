using System;
using System.Collections;
using Reshape.ReFramework;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class CollisionTriggerNode : TriggerNode
    {
        [ValueDropdown("TriggerTypeChoice")]
        [OnValueChanged("MarkDirty")]
        [PropertyOrder(1)]
        public Type triggerType;

        [HideLabel]
        [FoldoutGroup("Settings")]
        [OnInspectorGUI("MarkDirtyCollisionMatchInfo")]
        [PropertyOrder(3)]
        public CollisionMatchInfo matchInfo;

        protected override State OnUpdate (GraphExecution execution, int updateId)
        {
            State state = execution.variables.GetState(guid, State.Running);
            if (state == State.Running)
            {
                if (execution.type is Type.CollisionEnter or Type.CollisionExit or Type.CollisionStepIn or Type.CollisionStepOut)
                {
                    if (execution.type == triggerType)
                    {
                        GameObject inGo = execution.parameters.interactedGo;
                        if (inGo != null && context.runner.IsGameObjectMatch(inGo, matchInfo.excludeTags, matchInfo.excludeLayers, matchInfo.onlyTags, matchInfo.onlyLayers, matchInfo.specificNames))
                        {
                            if (!matchInfo.inOutDetection)
                            {
                                if (execution.type is Type.CollisionEnter or Type.CollisionExit)
                                {
                                    var behave = inGo.GetComponent<GraphRunner>();
                                    if (behave != null)
                                    {
                                        if (execution.type is Type.CollisionEnter)
                                            behave.TriggerCollision(Type.CollisionStepIn, context.runner);
                                        else if (execution.type is Type.CollisionExit)
                                            behave.TriggerCollision(Type.CollisionStepOut, context.runner);
                                    }
                                }

                                execution.variables.SetState(guid, State.Success);
                                state = State.Success;
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
        
        // [PropertyOrder(2)]
        // [OnValueChanged("MarkDirty")]
        // [Tooltip("Only use in Graph Editor")]
        // public string devNote;

#if UNITY_EDITOR
        private void MarkDirtyCollisionMatchInfo ()
        {
            if (matchInfo.dirty)
            {
                matchInfo.dirty = false;
                MarkDirty();
            }
        }
        
        private IEnumerable TriggerTypeChoice ()
        {
            ValueDropdownList<Type> menu = new ValueDropdownList<Type>();
            menu.Add("Being Enter", Type.CollisionEnter);
            menu.Add("Being Exit", Type.CollisionExit);
            menu.Add("Step In", Type.CollisionStepIn);
            menu.Add("Step Out", Type.CollisionStepOut);
            return menu;
        }

        public static string displayName = "Collision Trigger Node";
        public static string nodeName = "Collision";

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
            if (triggerType == Type.CollisionEnter)
                desc += "Being Enter";
            else if (triggerType == Type.CollisionExit)
                desc += "Being Exit";
            else if (triggerType == Type.CollisionStepIn)
                desc += "Step In";
            else if (triggerType == Type.CollisionStepOut)
                desc += "Step Out";
            // if (!string.IsNullOrEmpty(devNote))
            //     desc += "\\n" + devNote;
            return desc;
        }
#endif
    }
}