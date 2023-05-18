using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Reshape.ReGraph
{
    public class GraphFinder : OdinEditorWindow
    {
        [PropertyOrder(-10)]
        [ValueDropdown("TypeChoice")]
        [OnValueChanged("OnChangeType")]
        [InlineButton("OnChangeType", "↺")]
        public Type searchType;

        [PropertyOrder(-9)]
        [LabelText("Found GameObject")]
        [OnInspectorGUI("DisableGUIAfter")]
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, IsReadOnly = true, ShowPaging = false)]
        public List<GameObject> matchGo;

        [MenuItem("Tools/Reshape/Graph Finder", priority = 11002)]
        public static void OpenWindow ()
        {
            var window = GetWindow<GraphFinder>();
            window.Show();
        }

        public void Reset ()
        {
            if (matchGo != null)
                matchGo.Clear();
            searchType = default;
        }

        private void OnChangeType ()
        {
            if (searchType == null)
                return;
            matchGo = new List<GameObject>();
            EditorUtility.DisplayProgressBar("Graph Finder", "Search " + searchType.Name, 0);
            var found = FindObjectsOfType(typeof(GraphRunner));
            int searchLength = found.Length;
            for (var i = 0; i < searchLength; i++)
            {
                EditorUtility.DisplayProgressBar("Graph Finder", $"Search {searchType.Name} ({(i + 1).ToString()}/{searchLength.ToString()})", (i + 1f) / searchLength);
                GraphRunner runner = (GraphRunner) found[i];
                if (runner.ContainNode(searchType))
                    matchGo.Add(runner.gameObject);
            }

            EditorUtility.ClearProgressBar();
        }

        public ValueDropdownList<Type> TypeChoice ()
        {
            var listDropdown = new ValueDropdownList<Type>();
            var types = TypeCache.GetTypesDerivedFrom<GraphNode>();
            foreach (var type in types)
            {
                if (type == typeof(BehaviourNode) || type == typeof(ConditionNode) || type == typeof(TriggerNode) || type == typeof(RootNode))
                    continue;
                listDropdown.Add($"{type.Name.Substring(0, type.Name.IndexOf("Node", StringComparison.Ordinal))}", type);
            }

            return listDropdown;
        }

        private void DisableGUIAfter ()
        {
            GUI.enabled = false;
        }
        
#if UNITY_EDITOR
        [InitializeOnLoad]
        public static class GraphFinderResetOnPlay
        {
            static GraphFinderResetOnPlay ()
            {
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                EditorApplication.playModeStateChanged += OnPlayModeChanged;
            }

            private static void OnPlayModeChanged (PlayModeStateChange state)
            {
                bool update = false;
                if ( state == PlayModeStateChange.EnteredEditMode )
                {
                    if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                        update = true;
                }

                if (update)
                {
                    if (HasOpenInstances<GraphFinder>())
                    {
                        var window = GetWindow<GraphFinder>();
                        if (window != null)
                            window.Reset();
                    }
                }
            }
        }
#endif
    }
}