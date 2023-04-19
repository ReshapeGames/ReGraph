using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Reshape.ReGraph
{
    public class GraphInspector : OdinEditorWindow
    {
        [InlineEditor(Expanded = true)]
        [PropertyOrder(-3)]
        [OnInspectorGUI("DisableGUIAfter")]
        [OnValueChanged("SelectRunner")]
        public GraphRunner runner;

        [MenuItem("Tools/Reshape/Graph Inspector")]
        public static void OpenWindow ()
        {
            var window = GetWindow<GraphInspector>();
            Selection.selectionChanged = window.OnSelectionChanged;
            window.Show();
        }

        private void OnSelectionChanged ()
        {
            DetectRunner();
        }

        private void SelectRunner ()
        {
            Selection.activeObject = runner;
        }

        private void OnInspectorUpdate ()
        {
            if (Selection.selectionChanged != OnSelectionChanged)
            {
                Selection.selectionChanged = OnSelectionChanged;
            }

            DetectRunner();
            Repaint();
        }

        void OnFocus ()
        {
            if (DetectRunner())
                Repaint();
        }

        private bool DetectRunner ()
        {
            if (Selection.objects.Length == 1)
            {
                if (Selection.activeGameObject)
                {
                    GraphRunner temp = Selection.activeGameObject.GetComponent<GraphRunner>();
                    if (temp != null)
                    {
                        if (temp != runner)
                        {
                            runner = temp;
                            runner.graph.InitPreviewNode();
                            return true;
                        }
                    }
                    else
                    {
                        runner = null;
                        return true;
                    }
                }
                else
                {
                    runner = null;
                    return true;
                }
            }
            else
            {
                runner = null;
                return true;
            }

            return false;
        }

        private void DisableGUIAfter ()
        {
            GUI.enabled = false;
        }
    }
}