using System;
using System.Collections;
using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class CacheBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            SetItem = 101,
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType = ExecutionType.SetItem;

        [SerializeField]
        [OnInspectorGUI("OnUpdateName")]
        [InlineProperty]
        [LabelText("Cache Name")]
        private StringProperty name;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ValueDropdown("ItemChoice")]
        [InlineButton("@ItemManager.OpenWindow()", "✚")]
        [ShowIf("@executionType == ExecutionType.SetItem")]
        [OnInspectorGUI("OnUpdateItem")]
        private ItemData item;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (executionType == ExecutionType.SetItem)
            {
                if (string.IsNullOrEmpty(name) || item == null)
                {
                    LogWarning("Found an empty Cache Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    if (IsItemNotExist())
                    {
                        LogWarning("Found an empty Cache Behaviour node in " + context.gameObject.name);
                    }
                    else
                    {
                        context.SetCache(name, item);
                    }
                }
            }

            base.OnStart(execution, updateId);
        }

        public bool IsItemNotExist ()
        {
            if (item == null)
                return true;
            object tempObj = item;
            if (tempObj != null && tempObj.ToString() == "null")
                return true;
            return false;
        }
        
        public string GetItemCacheName ()
        {
            return name;
        }

#if UNITY_EDITOR
        public void OnUpdateName ()
        {
            if (MarkPropertyDirty(name))
            {
                if (runner != null && runner.graph != null && runner.graph.nodes != null)
                {
                    for (int i = 0; i < runner.graph.nodes.Count; i++)
                    {
                        if (runner.graph.nodes[i] is InventoryBehaviourNode)
                        {
                            InventoryBehaviourNode invNode = (InventoryBehaviourNode)runner.graph.nodes[i];
                            if (invNode.GetCacheItemType())
                            {
                                invNode.dirty = true;
                            }
                        }
                    }
                }
            }
        }

        public void OnUpdateItem ()
        {
            if (IsItemNotExist())
            {
                item = null;
                MarkDirty();
            }
        }

        public ItemData GetItemCacheChoice ()
        {
            if (executionType == ExecutionType.SetItem)
            {
                if (!string.IsNullOrEmpty(name) && item != null)
                    if (!IsItemNotExist())
                        return item;
            }

            return null;
        }

        public string GetCacheName ()
        {
            if (executionType == ExecutionType.SetItem)
            {
                if (!string.IsNullOrEmpty(name) && item != null)
                    return name;
            }

            return string.Empty;
        }

        private IEnumerable ItemChoice ()
        {
            var itemListDropdown = new ValueDropdownList<ItemData>();
            var guids = AssetDatabase.FindAssets("t:ItemList");
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var itemList = AssetDatabase.LoadAssetAtPath<ItemList>(path);
                for (int j = 0; j < itemList.items.Count; j++)
                {
                    var item = itemList.items[j];
                    itemListDropdown.Add(itemList.name + "/" + item.displayName, item);
                }
            }

            return itemListDropdown;
        }

        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Set Item", ExecutionType.SetItem},
        };

        public static string displayName = "Cache Behaviour Node";
        public static string nodeName = "Cache";

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
            if (executionType == ExecutionType.SetItem)
            {
                if (!string.IsNullOrEmpty(name) && item != null)
                    return $"Set {item.displayName} into cache {name}";
            }

            return string.Empty;
        }
#endif
    }
}