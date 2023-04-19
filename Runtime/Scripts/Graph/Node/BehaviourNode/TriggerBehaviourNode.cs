using System;
using System.Collections;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class TriggerBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            EnableIt = 10,
            DisableIt = 11,
            RunIt = 50
        }

        [SerializeReference]
        [OnValueChanged("MarkDirty")]
        [ValueDropdown("DrawTriggerListDropdown", ExpandAllMenuItems = true)]
        private string triggerNode;

        [SerializeField]
        [LabelText("Execution")]
        [OnValueChanged("MarkDirty")]
        private ExecutionType executionType;

        public string triggerNodeId => triggerNode;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (string.IsNullOrEmpty(triggerNode) || executionType == ExecutionType.None)
            {
                ReDebug.LogWarning("Graph Warning", "Found an empty Trigger Behaviour node in " + context.gameObject.name);
            }
            else
            {
                var triggers = Graph.GetChildren(context.graph.RootNode);
                for (int i = 0; i < triggers.Count; i++)
                {
                    TriggerNode trigger = (TriggerNode)triggers[i];
                    if (trigger.TriggerId == triggerNode)
                    {
                        if (executionType == ExecutionType.EnableIt)
                            trigger.enabled = true;
                        else if (executionType == ExecutionType.DisableIt)
                            trigger.enabled = false;
                        else if (executionType == ExecutionType.RunIt)
                            context.runner.InternalTrigger(trigger.TriggerId);
#if UNITY_EDITOR
                        triggers[i].OnEnableChange();
#endif
                        break;
                    }
                }
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private IEnumerable DrawTriggerListDropdown ()
        {
            var actionNameListDropdown = new ValueDropdownList<string>();
            if (runner != null && runner.graph != null && runner.graph.nodes != null)
            {
                for (int i = 0; i < runner.graph.nodes.Count; i++)
                {
                    if (runner.graph.nodes[i] is TriggerNode)
                    {
                        TriggerNode trigger = (TriggerNode)runner.graph.nodes[i];
                        actionNameListDropdown.Add(runner.graph.nodes[i].GetNodeViewTitle() + " (" + trigger.TriggerId + ")", trigger.TriggerId);
                    }
                }
            }

            return actionNameListDropdown;
        }

        public static string displayName = "Trigger Behaviour Node";
        public static string nodeName = "Trigger";

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
            if (!string.IsNullOrEmpty(triggerNode))
            {
                string triggerNodeName = String.Empty;
                if (runner != null && runner.graph != null && runner.graph.nodes != null)
                {
                    for (int i = 0; i < runner.graph.nodes.Count; i++)
                    {
                        if (runner.graph.nodes[i] is TriggerNode)
                        {
                            TriggerNode trigger = (TriggerNode)runner.graph.nodes[i];
                            if (trigger.guid == triggerNode)
                            {
                                triggerNodeName = trigger.GetNodeViewTitle() + " (" + trigger.TriggerId + ")";
                            }
                        }
                    }
                }
                if (executionType == ExecutionType.EnableIt)
                    return "Enable " + triggerNodeName;
                else if (executionType == ExecutionType.DisableIt)
                    return "Disable " + triggerNodeName;
                else if (executionType == ExecutionType.RunIt)
                    return "Run " + triggerNodeName;
            }

            return string.Empty;
        }
#endif
    }
}