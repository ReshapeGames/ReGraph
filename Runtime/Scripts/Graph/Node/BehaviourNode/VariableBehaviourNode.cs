using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class VariableBehaviourNode : BehaviourNode
    {
        [SerializeField]
        [OnValueChanged("OnChangeInfo")]
        [HideLabel]
        [OnInspectorGUI("CheckVariableDirty")]
        private VariableBehaviourInfo variableBehaviour;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (variableBehaviour.type == VariableBehaviourInfo.Type.None || variableBehaviour.variable == null)
            {
                ReDebug.LogWarning("Graph Warning", "Found an empty Variable Behaviour node in " + context.gameObject.name);
            }
            else
            {
                bool result = variableBehaviour.Activate();
                for (var i = 0; i < children.Count; ++i)
                {
                    if (children[i] is YesConditionNode)
                    {
                        var cNode = children[i] as YesConditionNode;
                        cNode.MarkExecute(execution, updateId, result);
                    }
                    else if (children[i] is NoConditionNode)
                    {
                        var cNode = children[i] as NoConditionNode;
                        cNode.MarkExecute(execution, updateId, result);
                    }
                }
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private void OnChangeInfo ()
        {
            MarkDirty();
            if (variableBehaviour.typeChanged)
            {
                variableBehaviour.typeChanged = false;
                MarkRepaint();
            }
        }

        private void CheckVariableDirty ()
        {
            MarkPropertyDirty(variableBehaviour.number);
            MarkPropertyDirty(variableBehaviour.message);
        }

        public static string displayName = "Variable Behaviour Node";
        public static string nodeName = "Variable";

        public override bool IsPortReachable (GraphNode node)
        {
            if (node is YesConditionNode or NoConditionNode)
            {
                if (variableBehaviour.type != VariableBehaviourInfo.Type.CheckCondition)
                    return false;
            }
            else if (node is ChoiceConditionNode)
            {
                return false;
            }

            return true;
        }

        public bool AcceptConditionNode ()
        {
            if (variableBehaviour.type == VariableBehaviourInfo.Type.CheckCondition)
                return true;
            return false;
        }

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
            if (variableBehaviour.type != VariableBehaviourInfo.Type.None && variableBehaviour.variable != null)
            {
                var message = variableBehaviour.variable.name + " being " + variableBehaviour.type.ToString().SplitCamelCase().ToLower();
                if (variableBehaviour.type == VariableBehaviourInfo.Type.CheckCondition)
                {
                    if (variableBehaviour.variable is NumberVariable)
                    {
                        message += " : " + variableBehaviour.condition.ToString().SplitCamelCase().ToLower() + " " + variableBehaviour.number;
                    }
                    else if (variableBehaviour.variable is WordVariable)
                    {
                        message += " : " + variableBehaviour.condition.ToString().SplitCamelCase().ToLower() + " " + variableBehaviour.message;
                    }
                }
                else if (variableBehaviour.type is VariableBehaviourInfo.Type.SetValue or VariableBehaviourInfo.Type.AddValue or VariableBehaviourInfo.Type.MinusValue or
                         VariableBehaviourInfo.Type.MultiplyValue or VariableBehaviourInfo.Type.DivideValue)
                {
                    if (variableBehaviour.variable is NumberVariable)
                    {
                        message += " : " + variableBehaviour.number;
                    }
                    else if (variableBehaviour.variable is WordVariable)
                    {
                        message += " : " + variableBehaviour.message;
                    }
                }

                return message;
            }

            return string.Empty;
        }
#endif
    }
}