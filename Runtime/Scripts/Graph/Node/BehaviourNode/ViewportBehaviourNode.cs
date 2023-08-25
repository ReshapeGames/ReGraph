using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class ViewportBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            CursorLocked = 10,
            CursorConfined = 11,
            CursorNormal = 12
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (executionType == ExecutionType.CursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (executionType == ExecutionType.CursorConfined)
            {
                Cursor.lockState = CursorLockMode.Confined;
            } 
            else if (executionType == ExecutionType.CursorNormal)
            {
                Cursor.lockState = CursorLockMode.None;
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Normal Cursor State", ExecutionType.CursorNormal},
            {"Confined Cursor State", ExecutionType.CursorConfined},
            {"Lock and Hide Cursor", ExecutionType.CursorLocked}
        };
        
        public static string displayName = "Viewport Behaviour Node";
        public static string nodeName = "Viewport";

        public override string GetNodeInspectorTitle ()
        {
            return displayName;
        }

        public override string GetNodeViewTitle ()
        {
            return nodeName;
        }

        public override string GetNodeMenuDisplayName ()
        {
            return $"Audio & Visual/{nodeName}";
        }

        public override string GetNodeViewDescription ()
        {
            if (executionType == ExecutionType.CursorNormal)
                return "Set cursor to normal state";
            if (executionType == ExecutionType.CursorConfined)
                return "Set cursor to confined state";
            if (executionType == ExecutionType.CursorLocked)
                return "Hide and lock cursor to center";
            return string.Empty;
        }
#endif
    }
}