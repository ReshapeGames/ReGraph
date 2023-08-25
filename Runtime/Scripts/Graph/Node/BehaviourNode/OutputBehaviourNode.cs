using System;
using System.Collections.Generic;
using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using Unity.VisualScripting.YamlDotNet.Core;
using UnityEditor;
#endif

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class OutputBehaviourNode : BehaviourNode
    {
        [Serializable]
        private struct OutputElementData
        {
            [HorizontalGroup("Split", width:110f)]
            [VerticalGroup("Split/Left")]
            [HideLabel]
            [DisplayAsString]
            public string name;
            [HideLabel]
            [VerticalGroup("Split/Right")]
            public VariableScriptableObject variable;

            public OutputElementData (string n, VariableScriptableObject v)
            {
                name = n;
                variable = v;
            }
        }

        [InfoBox("This node is ready for any usage", InfoMessageType.Error)]
        [SerializeField]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(camera)")]
        [InlineButton("@camera.objectValue.TrySetValue(AssignCamera())", "♺", ShowIf = "@camera.type == 0")]
        private SceneObjectProperty camera = new SceneObjectProperty(SceneObject.ObjectType.Camera);
        
        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ShowIf("ShowObjectListProperty")]
        [OnInspectorGUI("OnChangeObjectList")]
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, Expanded = true)]
        private List<OutputElementData> variableList;
        
        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (parent == null)
            {
                LogWarning("Found an empty Output Behaviour node in " + context.gameObject.name);
            }
            base.OnStart(execution, updateId);
        }
        
        public void ExportProperty (GraphNode node)
        {
            if (parent == null)
                return;
            if (parent is GameObjectBehaviourNode)
            {
                var behaviour = (GameObjectBehaviourNode) node;
                //~~ TODO export all useful property to objectList 
            }
        }
        
        [HideInInspector]
        public GraphNode previousParent;

#if UNITY_EDITOR
        private void OnChangeObjectList ()
        {
            if (parent != null && parent != previousParent)
            {
                if (parent is GameObjectBehaviourNode)
                {
                    //~~ TODO not create new list everytime 
                    variableList = new List<OutputElementData>();
                    variableList.Add(new OutputElementData("Rigidbody",null));
                    variableList.Add(new OutputElementData("Camera",null));
                    variableList.Add(new OutputElementData("Game Object",null));
                }
                previousParent = parent;
                MarkDirty();
            }
        }
        
        private bool ShowObjectListProperty ()
        {
            if (parent != null)
                return true;
            return false;
        }
        
        public static string displayName = "Output Behaviour Node";
        public static string nodeName = "Output (WIP)";

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
            return $"Logic/{nodeName}";
        }

        public override string GetNodeViewDescription ()
        {
            if (parent!= null)
                return $"Output parent node's property : {parent.GetNodeViewTitle()}";
            return string.Empty;
        }
#endif
    }
}