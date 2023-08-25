using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class PhysicsBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            ClearVelocity = 10
        }
        
        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        private ExecutionType executionType;

        [SerializeField]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(rigidbody)")]
        [InlineButton("@rigidbody.SetObjectValue(AssignComponent<Rigidbody>())", "♺", ShowIf = "@rigidbody.IsObjectValueType()")]
        private SceneObjectProperty rigidbody = new SceneObjectProperty(SceneObject.ObjectType.Rigidbody);

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (rigidbody.IsEmpty || executionType == ExecutionType.None)
            {
                LogWarning("Found an empty Physics Behaviour node in " + context.gameObject.name);
            }
            else if (executionType == ExecutionType.ClearVelocity)
            {
                var rb = (Rigidbody) rigidbody; 
                rb.velocity = Vector3.zero;
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        public static string displayName = "Physics Behaviour Node";
        public static string nodeName = "Physics";

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
            return nodeName;
        }

        public override string GetNodeViewDescription ()
        {
            if (!rigidbody.IsEmpty)
            {
                if (executionType == ExecutionType.ClearVelocity)
                    return "Clear velocity on " + rigidbody.name;
            }

            return string.Empty;
        }
#endif
    }
}