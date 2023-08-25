using System.Collections;
using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class ActionBehaviourNode : BehaviourNode
    {
        [SerializeField]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(graph)")]
        [InlineButton("@graph.SetObjectValue(AssignComponent<GraphRunner>())", "♺", ShowIf = "@graph.IsObjectValueType()")]
        private SceneObjectProperty graph = new SceneObjectProperty(SceneObject.ObjectType.GraphRunner);

        [SerializeField]
        [ValueDropdown("DrawActionNameListDropdown", ExpandAllMenuItems = true)]
        [OnValueChanged("MarkDirty")]
        private ActionNameChoice actionName;
        
        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (graph.IsEmpty || actionName == null)
                LogWarning("Found an empty Action Behaviour node in "+context.gameObject.name);
            else
                ((GraphRunner)graph)?.TriggerAction(actionName);
            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private static IEnumerable DrawActionNameListDropdown ()
        {
            return ActionNameChoice.GetActionNameListDropdown();
        }
        
        public static string displayName = "Action Behaviour Node";
        public static string nodeName = "Action";

        public override string GetNodeInspectorTitle()
        {
            return displayName;
        }

        public override string GetNodeViewTitle()
        {
            return nodeName;
        }

        public override string GetNodeMenuDisplayName ()
        {
            return $"Logic/{nodeName}";
        }
        
        public override string GetNodeViewDescription ()
        {
            if (!graph.IsEmpty && actionName != null)
                return "Execute "+actionName+" in graph of "+graph.name;
            return string.Empty;
        }
#endif
    }
}