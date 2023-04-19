using System;
using System.Collections.Generic;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reshape.ReFramework
{
    public class ItemManager : OdinEditorWindow
    {
        [PropertyOrder(-10)]
        [ShowIf("ShowItemDB")]
        [InlineEditor(InlineEditorModes.GUIOnly, InlineEditorObjectFieldModes.Hidden, Expanded = true)]
        [ListDrawerSettings(CustomAddFunction = "GenerateItemList", DraggableItems = false, Expanded = true, CustomRemoveElementFunction = "RemoveExisting")]
        [LabelText("Item DB")]
        public List<ItemList> itemDb;

        [MenuItem("Tools/Reshape/Item Manager")]
        public static void OpenWindow ()
        {
            var window = GetWindow<ItemManager>();
            window.itemDb = GetAll();
            window.Show();
        }

        public static void CreateNew ()
        {
            var window = GetWindow<ItemManager>();
            window.GenerateItemList();
        }

        [Button("Generate Item List")]
        [ShowIf("@ShowItemDB() == false")]
        [PropertyOrder(-2)]
        public void GenerateItemList ()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Item List", "ItemList.asset", "asset", "Please choose the a folder to save the item list");
            if (path.Length != 0)
            {
                int pathIndex = path.IndexOf("Assets", StringComparison.Ordinal);
                if (pathIndex < 0)
                {
                    ReDebug.LogError("Save Item List", "Please select a path that relative to the project Assets folder!");
                    return;
                }

                path = path.Substring(pathIndex);
                ItemList asset = ScriptableObject.CreateInstance<ItemList>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                itemDb.Add(asset);
                Selection.activeObject = asset;
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
            }
        }

        private bool ShowItemDB ()
        {
            if (itemDb != null && itemDb.Count > 0)
                return true;
            return false;
        }

        public void RemoveExisting (List<ItemList> db, ItemList elementToBeRemoved)
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorUtility.DisplayDialog("Delete Item List",
                        $"Are you sure you wants to delete {elementToBeRemoved.name} ? \n\n This action is not undoable, the list will be removed permanently once confirmed.", "Confirm",
                        "Cancel"))
                {
                    itemDb.Remove(elementToBeRemoved);
                    for (int i = 0; i < elementToBeRemoved.items.Count; i++)
                    {
                        var data = elementToBeRemoved.items[i];
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(data));
                    }
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(elementToBeRemoved));
                    AssetDatabase.Refresh();
                }
            };
        }

        [DisplayAsString]
        [PropertyOrder(-1)]
        [OnInspectorGUI("DisableGUIAfter")]
        [HideLabel]
        public string disableGUIAfter = "";

        private void DisableGUIAfter ()
        {
            GUI.enabled = false;
        }

        public static ItemList GetFirstFound ()
        {
            var guids = AssetDatabase.FindAssets("t:ItemList");
            if (guids.Length > 1)
            {
                ReDebug.LogWarning("Item List", $"Found multiple item list files, currently is using the first found settings file.", false);
            }

            switch (guids.Length)
            {
                case 0:
                    break;
                default:
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    return AssetDatabase.LoadAssetAtPath<ItemList>(path);
            }

            return null;
        }

        public static List<ItemList> GetAll ()
        {
            var list = new List<ItemList>();
            var guids = AssetDatabase.FindAssets("t:ItemList");
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                list.Add(AssetDatabase.LoadAssetAtPath<ItemList>(path));
            }

            return list;
        }
    }
}