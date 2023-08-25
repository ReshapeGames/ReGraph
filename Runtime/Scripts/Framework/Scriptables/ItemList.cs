using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Reshape.Unity;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif


namespace Reshape.ReFramework
{
    [CreateAssetMenu(menuName = "Reshape/Item List", fileName = "ReshapeItemList", order = 101)]
    [Serializable]
    public class ItemList : ScriptableObject
    {
        [LabelText("@DisplayName()")]
        [ListDrawerSettings(CustomAddFunction = "CreateNew", DraggableItems = false, Expanded = true, CustomRemoveElementFunction = "RemoveExisting")]
        [InlineEditor(InlineEditorModes.GUIOnly, InlineEditorObjectFieldModes.Hidden, Expanded = true)]
        public List<ItemData> items;

#if UNITY_EDITOR
        public void CreateNew ()
        {
            var listPath = AssetDatabase.GetAssetPath(this);
            var listFolderPath = Path.GetDirectoryName(listPath);
#if UNITY_EDITOR_WIN
            var choicesPath = listFolderPath + "\\ItemData\\";
#elif UNITY_EDITOR_OSX
            var choicesPath = listFolderPath + "/ItemData/";
#endif
            if (!Directory.Exists(choicesPath))
                Directory.CreateDirectory(choicesPath);

            ItemData itemData = ScriptableObject.CreateInstance<ItemData>();
            itemData.id = Guid.NewGuid().ToString();
            AssetDatabase.CreateAsset(itemData, choicesPath + itemData.id + ".asset");
            AssetDatabase.SaveAssets();
            items.Add(itemData);
            AssetDatabase.Refresh();
        }

        public void RemoveExisting (List<ItemData> list, ItemData elementToBeRemoved)
        {
            items.Remove(elementToBeRemoved);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(elementToBeRemoved));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public string DisplayName ()
        {
            return ReExtensions.SplitCamelCase(this.name);
        }
#endif
    }
}