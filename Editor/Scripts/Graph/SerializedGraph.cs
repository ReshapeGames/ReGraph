using System;
using System.Collections.Generic;
using System.Reflection;
using Reshape.ReFramework;
using Reshape.Unity.Editor;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace Reshape.ReGraph
{
    public class SerializedGraph
    {
        readonly public SerializedObject serializedObject;
        readonly public GraphRunner runner;
        readonly public Graph graph;

        const string sPropRootNode = "graph.rootNode";
        const string sPropNodes = "graph.nodes";
        const string sPropBlackboard = "blackboard";
        const string sPropGuid = "guid";
        const string sPropChild = "child";
        const string sPropChildren = "children";
        const string sPropParent = "parent";
        const string sPropPosition = "position";
        const string sViewTransformPosition = "graph.viewPosition";
        const string sViewTransformScale = "graph.viewScale";

        private SerializedProperty nodes;

        public SerializedProperty RootNode
        {
            get { return serializedObject.FindProperty(sPropRootNode); }
        }

        public SerializedProperty Nodes
        {
            get
            {
                if (nodes == null)
                    nodes = serializedObject.FindProperty(sPropNodes);

                return nodes;
            }
        }

        public SerializedProperty Blackboard
        {
            get { return serializedObject.FindProperty(sPropBlackboard); }
        }

        // Start is called before the first frame update
        public SerializedGraph (SerializedObject tree)
        {
            serializedObject = tree;
            runner = serializedObject.targetObject as GraphRunner;
            graph = runner.graph;
            nodes = null;
        }

        public void SaveNode (GraphNode node)
        {
            var nodeProp = FindNode(Nodes, node);
            if (nodeProp != null)
            {
                nodeProp.serializedObject.Update();
                nodeProp.serializedObject.ApplyModifiedProperties();
            }
        }

        public SerializedProperty FindNode (SerializedProperty array, GraphNode node)
        {
            if (node == null) return null;
            if (array.serializedObject == null) return null;
            for (int i = 0; i < array.arraySize; ++i)
            {
                var current = array.GetArrayElementAtIndex(i);
                if (current.managedReferenceValue != null && current.FindPropertyRelative(sPropGuid).stringValue == node.guid)
                    return current;
            }

            return null;
        }

        public void SetViewTransform (Vector3 position, Vector3 scale)
        {
            serializedObject.FindProperty(sViewTransformPosition).vector3Value = position;
            serializedObject.FindProperty(sViewTransformScale).vector3Value = scale;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public void SetNodePosition (GraphNode node, Vector2 position)
        {
            var nodeProp = FindNode(Nodes, node);
            if (nodeProp != null)
            {
                nodeProp.serializedObject.Update();
                Vector2 ori = nodeProp.FindPropertyRelative(sPropPosition).vector2Value;
                if (Vector2.Distance(ori, position) > 2f)
                {
                    nodeProp.FindPropertyRelative(sPropPosition).vector2Value = position;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        public void SetViewPreviewNode (GraphNode node)
        {
            graph.previewNode = node;
            //serializedObject.FindProperty("graph.previewNode").managedReferenceValue = node;
            //serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public void SetViewPreviewSelected (bool selected)
        {
            graph.previewSelected = selected;
            //serializedObject.FindProperty("graph.previewSelected").boolValue = selected;
            //serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public void DeleteNode (SerializedProperty array, GraphNode node)
        {
            if (node == null)
            {
                for (int i = 0; i < array.arraySize; ++i)
                {
                    var current = array.GetArrayElementAtIndex(i);
                    if (current.managedReferenceValue == null)
                    {
                        array.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }

                return;
            }

            for (int i = 0; i < array.arraySize; ++i)
            {
                var current = array.GetArrayElementAtIndex(i);
                if (current.managedReferenceValue != null)
                {
                    if (current.FindPropertyRelative(sPropGuid).stringValue == node.guid)
                    {
                        array.DeleteArrayElementAtIndex(i);
                        return;
                    }
                }
            }
        }

        public GraphNode CreateNodeInstance (System.Type type)
        {
            GraphNode node = System.Activator.CreateInstance(type) as GraphNode;
            node.guid = GUID.Generate().ToString();
            return node;
        }

        SerializedProperty AppendArrayElement (SerializedProperty arrayProperty)
        {
            arrayProperty.InsertArrayElementAtIndex(arrayProperty.arraySize);
            return arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1);
        }

        public GraphNode CreateNode (System.Type type, Vector2 position)
        {
            GraphNode node = CreateNodeInstance(type);
            node.position = position;
            node.runner = runner;

            SerializedProperty newNode = AppendArrayElement(Nodes);
            newNode.managedReferenceValue = node;

            serializedObject.ApplyModifiedProperties();

            return node;
        }

        public GraphNode CloneNode (GraphNode selectedNode, Vector2 position)
        {
            System.Type type = selectedNode.GetType();
            GraphNode node = CreateNodeInstance(type);
            string cloneNodeId = node.guid;

            object nodeObj = Convert.ChangeType(node, type);
            object selectedNodeObj = Convert.ChangeType(selectedNode, type);
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                bool isClonable = false;
                foreach (Type cInterface in field.FieldType.GetInterfaces())
                    if (cInterface.ToString().Contains("Reshape.ReFramework.IClone"))
                        isClonable = true;
                if (isClonable)
                {
                    SetField(field, field.FieldType);
                }
                else if (field.FieldType == typeof(UnityEvent))
                {
                    var nodeProp = FindNode(Nodes, selectedNode);
                    EventBehaviourNode newEventNode = (EventBehaviourNode) node;
                    newEventNode.unityEvent = new UnityEvent();
                    ReEditorHelper.CopyUnityEvents(nodeProp.FindPropertyRelative("unityEvent"), newEventNode.unityEvent);
                }
                else
                {
                    field.SetValue(nodeObj, field.GetValue(selectedNodeObj));
                }
            }

            node.children = new List<GraphNode>();
            node.guid = cloneNodeId;

            node.position = position;
            node.runner = runner;
            node.dirty = false;
            node.forceRepaint = false;

            SerializedProperty serializedNode = AppendArrayElement(Nodes);
            serializedNode.managedReferenceValue = node;

            serializedObject.ApplyModifiedProperties();

            return node;

            void SetField (FieldInfo field, Type fieldType)
            {
                var method = fieldType.GetMethod("ShallowCopy");
                var fieldObj = field.GetValue(selectedNodeObj);
                var fieldInstance = Convert.ChangeType(fieldObj, fieldType);
                field.SetValue(nodeObj, method?.Invoke(fieldInstance, null));
            }
        }

        public void SetRootNode (RootNode node)
        {
            RootNode.managedReferenceValue = node;
            serializedObject.ApplyModifiedProperties();
        }

        public void DeleteNode (GraphNode node)
        {
            /*SerializedProperty nodesProperty = Nodes;
            for (int i = 0; i < nodesProperty.arraySize; ++i)
            {
                var prop = nodesProperty.GetArrayElementAtIndex(i);
                var guid = prop.FindPropertyRelative(sPropGuid).stringValue;*/
            DeleteNode(Nodes, node);
            serializedObject.ApplyModifiedProperties();
            /*}*/
        }

        public void SortChildren (GraphNode node, List<GraphNode> sorted)
        {
            var nodeProperty = FindNode(Nodes, node);
            for (var i = 0; i < sorted.Count; i++)
            {
                if (sorted[i] == null)
                    continue;
                var childrenProperty = nodeProperty.FindPropertyRelative(sPropChildren);
                if (childrenProperty != null)
                {
                    for (var j = 0; j < childrenProperty.arraySize; ++j)
                    {
                        var current = childrenProperty.GetArrayElementAtIndex(j);
                        if (current.managedReferenceValue != null)
                        {
                            if (current.FindPropertyRelative(sPropGuid).stringValue == sorted[i].guid)
                            {
                                childrenProperty.MoveArrayElement(j, i);
                                serializedObject.ApplyModifiedProperties();
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void AddChild (GraphNode parent, GraphNode child)
        {
            var parentProperty = FindNode(Nodes, parent);
            var childProperty = FindNode(Nodes, child);
            var parentChildrenProperty = parentProperty.FindPropertyRelative(sPropChildren);
            var childParentProperty = childProperty.FindPropertyRelative(sPropParent);
            
            //~~ NOTE clean up null in children property
            DeleteNode(parentChildrenProperty, null);

            SerializedProperty newChild = AppendArrayElement(parentChildrenProperty);
            newChild.managedReferenceValue = child;
            childParentProperty.managedReferenceValue = parent; 
                
            serializedObject.ApplyModifiedProperties();
        }

        public void RemoveChild (GraphNode parent, GraphNode child)
        {
            var parentProperty = FindNode(Nodes, parent);
            var childProperty = FindNode(Nodes, child);
            var parentChildrenProperty = parentProperty.FindPropertyRelative(sPropChildren);
            var childParentProperty = childProperty.FindPropertyRelative(sPropParent);
            
            DeleteNode(parentChildrenProperty, child);
            childParentProperty.managedReferenceValue = null; 
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}