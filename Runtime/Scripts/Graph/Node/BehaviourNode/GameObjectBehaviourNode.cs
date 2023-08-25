using System.Collections;
using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class GameObjectBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            Show = 10,
            Hide = 11,
            EnableComponent = 30,
            DisableComponent = 31,
            Spawn = 50,
            Expel = 51
        }

        private enum GoType
        {
            None,
            WithRunner,
            WithoutRunner
        }
        
        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        private ExecutionType executionType;

        [SerializeField]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(gameObject)")]
        [InlineButton("@gameObject.SetObjectValue(AssignGameObject())", "♺", ShowIf = "@gameObject.IsObjectValueType()")]
        private SceneObjectProperty gameObject = new SceneObjectProperty(SceneObject.ObjectType.GameObject);

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ShowIf("ShowComponent")]
        [ValueDropdown("DrawComponentListDropdown", ExpandAllMenuItems = true)]
        private Component component;

        [SerializeField]
        [ShowIf("ShowLocation")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(location)")]
        [InlineButton("@location.SetObjectValue(AssignComponent<UnityEngine.Transform>())", "♺", ShowIf = "@location.IsObjectValueType()")]
        private SceneObjectProperty location = new SceneObjectProperty(SceneObject.ObjectType.Transform, "GameObjectLocation");

        [SerializeField]
        [ValueDropdown("DrawActionNameListDropdown", ExpandAllMenuItems = true)]
        [OnValueChanged("MarkDirty")]
        [ShowIf("ShowLocation")]
        [LabelText("OnSpawn Action")]
        private ActionNameChoice actionName;

        private GoType spawnType;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (gameObject.IsEmpty || executionType == ExecutionType.None)
            {
                LogWarning("Found an empty GameObject Behaviour node in " + context.gameObject.name);
            }
            else if (executionType is ExecutionType.DisableComponent or ExecutionType.EnableComponent)
            {
                if (component == null)
                {
                    LogWarning("Found an empty GameObject Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    bool value = executionType == ExecutionType.EnableComponent;
                    if (component is Renderer)
                    {
                        var comp = (Renderer) component;
                        comp.enabled = value;
                    }
                    else if (component is Collider)
                    {
                        var comp = (Collider) component;
                        comp.enabled = value;
                    }
                    else if (component is Behaviour)
                    {
                        var comp = (Behaviour) component;
                        comp.enabled = value;
                    }
                }
            }
            else if (executionType == ExecutionType.Show)
            {
                ((GameObject)gameObject).SetActiveOpt(true);
            }
            else if (executionType == ExecutionType.Hide)
            {
                ((GameObject)gameObject).SetActiveOpt(false);
            }
            else if (executionType == ExecutionType.Spawn)
            {
                GameObject go = null;
                if (!location.IsEmpty)
                    go = context.runner.TakeFromPool(gameObject, (Transform)location);
                else
                    go = context.runner.TakeFromPool(gameObject, context.transform);
                if (go != null && actionName != null)
                {
                    if (spawnType == GoType.None)
                    {
                        GraphRunner gr = go.GetComponent<GraphRunner>();
                        if (gr != null)
                        {
                            spawnType = GoType.WithRunner;
                            gr.TriggerSpawn(actionName);
                        }
                        else
                        {
                            spawnType = GoType.WithoutRunner;
                        }
                    }
                    else if (spawnType == GoType.WithRunner)
                    {
                        GraphRunner gr = go.GetComponent<GraphRunner>();
                        gr.TriggerSpawn(actionName);
                    }
                }
            }
            else if (executionType == ExecutionType.Expel)
            {
                context.runner.InsertIntoPool(gameObject, true);
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private bool ShowComponent ()
        {
            if (executionType is ExecutionType.DisableComponent or ExecutionType.EnableComponent)
                return true;
            return false;
        }

        private bool ShowLocation ()
        {
            if (executionType is ExecutionType.Spawn)
                return true;
            return false;
        }

        private IEnumerable DrawComponentListDropdown ()
        {
            var actionNameListDropdown = new ValueDropdownList<Component>();
            if (!gameObject.IsEmpty)
            {
                var components = ((GameObject)gameObject).GetComponents<Component>();
                foreach (var comp in components)
                {
                    if (comp is Collider or Renderer or Behaviour)
                        actionNameListDropdown.Add(comp.GetType().ToString(), comp);
                }
            }

            return actionNameListDropdown;
        }

        private static IEnumerable DrawActionNameListDropdown ()
        {
            return ActionNameChoice.GetActionNameListDropdown();
        }

        public static string displayName = "GameObject Behaviour Node";
        public static string nodeName = "GameObject";

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
            if (!gameObject.IsEmpty)
            {
                if (executionType == ExecutionType.DisableComponent && component != null)
                {
                    return "Disable " + component;
                }
                else if (executionType == ExecutionType.EnableComponent && component != null)
                {
                    return "Enable " + component;
                }
                else if (executionType == ExecutionType.Show)
                {
                    return "Show " + gameObject.name;
                }
                else if (executionType == ExecutionType.Hide)
                {
                    return "Hide " + gameObject.name;
                }
                else if (executionType == ExecutionType.Spawn)
                {
                    if (actionName != null)
                        return "Spawn " + gameObject.name + " at " + actionName + " action";
                    else
                        return "Spawn " + gameObject.name;
                }
                else if (executionType == ExecutionType.Expel)
                {
                    return "Expel " + gameObject.name;
                }
            }

            return string.Empty;
        }
#endif
    }
}