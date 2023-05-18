using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Reshape.ReGraph
{
    [CustomEditor(typeof(GraphRunner))]
    public class GraphRunnerEditor : OdinEditor
    {
        private Graph graphCache;

        public override void OnInspectorGUI ()
        {
            if (!Application.isPlaying)
            {
                if (Tree != null && Tree.UnitySerializedObject != null)
                {
                    if (graphCache == null)
                    {
                        GraphRunner runner = Tree.UnitySerializedObject.targetObject as GraphRunner;
                        if (runner != null)
                            graphCache = runner.graph;
                    }

                    if (graphCache != null)
                    {
                        if (graphCache.Created)
                        {
                            if (!graphCache.HavePreviewNode())
                            {
                                EditorGUILayout.HelpBox("All graph data in saved in the scene file.", MessageType.Info);
                                if (GUILayout.Button("Edit Graph"))
                                {
                                    GraphRunner runner = Tree.UnitySerializedObject.targetObject as GraphRunner;
                                    GraphEditorWindow.OpenWindow(runner);
                                }

                                base.OnInspectorGUI();
                                if (IsShowCleanGraphButton())
                                    DisplayCleanGraphButton();
                                return;
                            }

                            if (!GraphEditorWindow.HasFocus())
                            {
                                graphCache.InitPreviewNode();
                            }
                        }
                    }
                }
            }

            base.OnInspectorGUI();

            void DisplayCleanGraphButton ()
            {
                if (GUILayout.Button("Clean Graph"))
                {
                    if (EditorUtility.DisplayDialog("Clean Graph", "Are you sure you would like to clean up the graph?", "YES", "NO"))
                    {
                        for (var i = 0; i < graphCache.nodes.Count; i++)
                        {
                            var node = graphCache.nodes[i];
                            if (node == null)
                            {
                                graphCache.nodes.RemoveAt(i);
                                i--;
                                continue;
                            }

                            for (var j = 0; j < node.children.Count; j++)
                            {
                                if (node.children[j] == null)
                                {
                                    node.children.RemoveAt(j);
                                    j--;
                                }
                            }
                        }

                        InspectorUtilities.RegisterUnityObjectDirty(Tree.UnitySerializedObject.targetObject);
                        GraphEditorWindow.RefreshCurrentGraph();
                    }
                }
            }

            bool IsShowCleanGraphButton ()
            {
                for (var i = 0; i < graphCache.nodes.Count; i++)
                {
                    if (graphCache.nodes[i] == null)
                        return true;
                    var children = new List<GraphNode>();
                    graphCache.nodes[i].GetChildren(ref children);
                    for (var j = 0; j < children.Count; j++)
                    {
                        if (children[j] == null)
                            return true;
                    }
                }

                return false;
            }
        }
    }
}