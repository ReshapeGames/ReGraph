using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class YesConditionNode : ConditionNode
    {
        public override void MarkExecute (GraphExecution execution, int updateId, bool condition)
        {
            if (condition)
                MarkExecute(execution, updateId);
        }
        
#if UNITY_EDITOR
        public static string displayName = "Yes Condition Node";
        public static string nodeName = "Yes";

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
            return "<color=#FFF600>Continue if condition is true</color>";
        }
#endif
    }
}