using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class NoConditionNode : ConditionNode
    {
        public override void MarkExecute (GraphExecution execution, int updateId, bool condition)
        {
            if (!condition)
                MarkExecute(execution, updateId);
        }
        
#if UNITY_EDITOR
        public static string displayName = "No Condition Node";
        public static string nodeName = "No";

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
            return "<color=#FFF600>Continue if condition is false</color>";
        }
#endif
    }
}