using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Reshape.ReFramework
{
    public class ActionNameListWindow : OdinEditorWindow
    {
        [InlineEditor(Expanded = true)]
        [ListDrawerSettings(HideRemoveButton = true, HideAddButton = true, DraggableItems = false)]
        [PropertyOrder(-3)]
        [OnInspectorGUI("DrawGenerateAllButton")]
        public List<ActionNameList> customActionNameList;

        [InlineEditor(InlineEditorModes.GUIOnly, InlineEditorObjectFieldModes.Hidden, Expanded = true)]
        [PropertyOrder(-1)]
        [BoxGroup("Default Action Name List")]
        public ActionNameList defaultActionNameList;

        [MenuItem("Tools/Reshape/Edit Action Name")]
        public static void OpenWindow ()
        {
            var window = GetWindow<ActionNameListWindow>();
            window.customActionNameList = new List<ActionNameList>();

            var assets = AssetDatabase.FindAssets("t:ActionNameList");
            for (var i = 0; i < assets.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assets[i]);
                var nameList = AssetDatabase.LoadAssetAtPath<ActionNameList>(path);
                if (path.Contains("Runtime/Datas/DefaultActionNameList.asset"))
                    window.defaultActionNameList = nameList;
                else
                    window.customActionNameList.Add(nameList);
            }

            window.Show();
        }

        private void DrawGenerateAllButton ()
        {
            if(GUILayout.Button("Generate All Action Name", GUILayout.Height(30)))
            {
                defaultActionNameList.GenerateActionNameChoice();
                for (var i = 0; i < customActionNameList.Count; i++)
                {
                    customActionNameList[i].GenerateActionNameChoice();
                }
            }

            GUI.enabled = false;
        }
    }
}