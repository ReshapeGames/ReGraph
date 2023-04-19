using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class ChoiceConditionNode : ConditionNode
    {
        [SerializeField]
        [OnInspectorGUI("@MarkPropertyDirty(choice)")]
        [InlineProperty]
        public StringProperty choice;

        public override void MarkExecute (GraphExecution execution, int updateId, bool condition)
        {
            if (condition)
                MarkExecute(execution, updateId);
        }

#if UNITY_EDITOR
        public static string displayName = "Choice Condition Node";
        public static string nodeName = "Choice";

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
            if (!string.IsNullOrEmpty(choice.ToString()))
                return choice + "\\n<color=#FFF600>Continue if chosen</color>";
            return string.Empty;
        }
#endif
    }
}