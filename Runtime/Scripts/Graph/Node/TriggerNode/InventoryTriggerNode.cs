using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using Reshape.ReFramework;
using Reshape.Unity;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class InventoryTriggerNode : TriggerNode
    {
        [ValueDropdown("TriggerTypeChoice")]
        [OnValueChanged("MarkDirty")]
        public Type triggerType;

        [SerializeField]
        [OnInspectorGUI("@MarkPropertyDirty(inventoryName)")]
        [InlineProperty]
        [LabelText("Inv Name")]
        [Tooltip("Inventory Name")]
        private StringProperty inventoryName;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ValueDropdown("ItemChoice")]
        [InlineButton("SwitchToItemCache", "▼")]
        [InlineButton("@ItemManager.OpenWindow()", "✚")]
        [ShowIf("ShowItemData")]
        private ItemData item;

        [SerializeField]
        [HideInInspector]
        private int itemType = 0;

        [SerializeReference]
        [OnValueChanged("MarkDirty")]
        [ValueDropdown("ItemCacheChoice")]
        [InlineButton("SwitchToItemData", "▼")]
        [ShowIf("ShowItemCache")]
        private string itemCache;

        protected override State OnUpdate (GraphExecution execution, int updateId)
        {
            State state = execution.variables.GetState(guid, State.Running);
            if (state == State.Running)
            {
                if ((execution.type is Type.InventoryQuantityChange && execution.type == triggerType) || execution.type == Type.All)
                {
                    if (execution.parameters.actionName != null && execution.parameters.actionName.Equals(TriggerId))
                    {
                        execution.variables.SetState(guid, State.Success);
                        state = State.Success;
                    }
                }

                if (state != State.Success)
                {
                    execution.variables.SetState(guid, State.Failure);
                    state = State.Failure;
                }
            }

            if (state == State.Success)
                return base.OnUpdate(execution, updateId);
            return State.Failure;
        }
        
        protected override void OnInit ()
        {
            if (triggerType == Type.InventoryQuantityChange && !string.IsNullOrEmpty(inventoryName))
            {
                if ((itemType == 0 && item != null) || (itemType == 1 && !string.IsNullOrEmpty(itemCache)))
                {
                    InventoryData inv = ReInventoryController.GetInventory(inventoryName);
                    if (inv != null)
                    {
                        inv.OnChange -= OnInventoryChange;
                        inv.OnChange += OnInventoryChange;
                    }
                    else
                    {
                        ReInventoryController.OnInvCreated += OnInventoryCreated;
                    }
                }
                else
                {
                    LogWarning("Found an empty Inventory Trigger node in " + context.gameObject.name);
                }
            }
            else
            {
                LogWarning("Found an empty Inventory Trigger node in " + context.gameObject.name);
            }
        }
        
        private void OnInventoryCreated (string invName)
        {
            if (string.Equals(inventoryName, invName))
            {
                InventoryData inv = ReInventoryController.GetInventory(inventoryName);
                if (inv != null)
                {
                    inv.OnChange -= OnInventoryChange;
                    inv.OnChange += OnInventoryChange;
                    ReInventoryController.OnInvCreated -= OnInventoryCreated;
                }
                else
                {
                    LogWarning($"{inventoryName} inventory not found when trigger inventory quantity changed in {context.gameObject.name}.");
                }
            }
        }
        
        private void OnInventoryChange (string itemId)
        {
            if (itemType == 1 && !string.IsNullOrEmpty(itemCache))
            {
                item = null;
                CacheBehaviourNode cacheNode = (CacheBehaviourNode) context.graph.GetNode(itemCache);
                if (cacheNode != null)
                {
                    object cacheObj = context.GetCache(cacheNode.GetItemCacheName());
                    if (cacheObj != null)
                        item = (ItemData) cacheObj;
                }
            }
            
            if (string.IsNullOrEmpty(inventoryName) || item == null)
            {
                LogWarning($"Found invalid item ({item.id}) quantity change trigger in {context.gameObject.name}");
            }
            else
            {
                if (itemId == item.id)
                {
                    context.runner.TriggerInventory(Type.InventoryQuantityChange, TriggerId);
                }
            }
        }
        
        protected override void OnReset ()
        {
            if (triggerType == Type.InventoryQuantityChange && !string.IsNullOrEmpty(inventoryName))
            {
                InventoryData inv = ReInventoryController.GetInventory(inventoryName);
                if (inv != null)
                    inv.OnChange -= OnInventoryChange;
            }
        }

        public override bool IsRequireInit ()
        {
            if (triggerType == Type.InventoryQuantityChange)
            {
                if (!string.IsNullOrEmpty(inventoryName))
                {
                    if (itemType == 0 && item != null)
                    {
                        return true;
                    }
                    else if (itemType == 1 && !string.IsNullOrEmpty(itemCache))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

#if UNITY_EDITOR
        private void SwitchToItemCache ()
        {
            itemType = 1;
            MarkDirty();
        }

        private void SwitchToItemData ()
        {
            itemType = 0;
            MarkDirty();
        }

        private bool ShowItemData ()
        {
            switch (triggerType)
            {
                case Type.InventoryQuantityChange:
                    if (itemType == 0)
                        return true;
                    else
                        return false;
                default:
                    return false;
            }
        }

        private bool ShowItemCache ()
        {
            switch (triggerType)
            {
                case Type.InventoryQuantityChange:
                    if (itemType == 1)
                        return true;
                    else
                        return false;
                default:
                    return false;
            }
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
                    var tempItem = itemList.items[j];
                    itemListDropdown.Add(itemList.name + "/" + tempItem.displayName, tempItem);
                }
            }

            return itemListDropdown;
        }

        private IEnumerable ItemCacheChoice ()
        {
            var itemListDropdown = new ValueDropdownList<string>();
            for (int i = 0; i < runner.graph.nodes.Count; i++)
            {
                if (runner.graph.nodes[i] is CacheBehaviourNode)
                {
                    CacheBehaviourNode cacheNode = (CacheBehaviourNode) runner.graph.nodes[i];
                    var cache = cacheNode.GetItemCacheChoice();
                    if (cache != null)
                        itemListDropdown.Add(cacheNode.GetCacheName(), cacheNode.guid);
                }
            }

            if (!string.IsNullOrEmpty(itemCache))
            {
                bool found = false;
                for (int i = 0; i < itemListDropdown.Count; i++)
                {
                    if (itemListDropdown[i].Value == itemCache)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    itemCache = string.Empty;
                    MarkDirty();
                }
            }

            return itemListDropdown;
        }

        private string GetItemCacheName (string cacheNodeId)
        {
            if (runner != null && runner.graph != null && runner.graph.nodes != null)
            {
                for (int i = 0; i < runner.graph.nodes.Count; i++)
                {
                    if (runner.graph.nodes[i] is CacheBehaviourNode)
                    {
                        CacheBehaviourNode cacheNode = (CacheBehaviourNode) runner.graph.nodes[i];
                        if (cacheNode.guid == cacheNodeId)
                        {
                            return cacheNode.GetItemCacheName();
                        }
                    }
                }
            }

            return string.Empty;
        }

        private IEnumerable TriggerTypeChoice ()
        {
            ValueDropdownList<Type> menu = new ValueDropdownList<Type>();
            menu.Add("Quantity Changed", Type.InventoryQuantityChange);
            return menu;
        }

        public static string displayName = "Inventory Trigger Node";
        public static string nodeName = "Inventory";

        public override string GetNodeInspectorTitle ()
        {
            return displayName;
        }

        public override string GetNodeViewTitle ()
        {
            return nodeName;
        }

        public override string GetNodeViewDescription ()
        {
            string desc = String.Empty;
            if (triggerType == Type.InventoryQuantityChange)
            {
                if (!string.IsNullOrEmpty(inventoryName))
                {
                    if (itemType == 0 && item != null)
                    {
                        desc = $"{item.displayName}'s quantity changed in {inventoryName}";
                    }
                    else if (itemType == 1 && !string.IsNullOrEmpty(itemCache))
                    {
                        desc = $"{GetItemCacheName(itemCache)}'s quantity changed in {inventoryName}";
                    }
                }
            }

            return desc;
        }
#endif
    }
}