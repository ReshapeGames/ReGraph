using System;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using Reshape.ReFramework;
using Reshape.Unity;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class InventoryBehaviourNode : BehaviourNode
    {
        private const string SAVE_NAME_MIDDLE = "_inv_";
        private const float DEFAULT_SIZE = 100;
        private const float DEFAULT_STACK = 9999999;

        public enum ExecutionType
        {
            None,
            Create = 11,
            Destroy = 21,
            Clear = 31,
            AddItem = 41,
            RemoveItem = 51,
            Save = 101,
            Load = 111,
            LinkItemTotalQuantity = 901,
            GetItemTotalQuantity = 1001,
            GetItemName = 2001,
            GetItemDesc = 2002,
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [OnInspectorGUI("@MarkPropertyDirty(inventoryName)")]
        [InlineProperty]
        [ShowIf("ShowInventoryName")]
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

        [SerializeField]
        [ShowIf("@executionType == ExecutionType.Create || executionType == ExecutionType.AddItem || executionType == ExecutionType.RemoveItem")]
        [ValidateInput("ValidateSize", "Value must be more than 0 and smaller than 10,000,000!", InfoMessageType.Warning)]
        [LabelText("@GetSizeLabel()")]
        [OnInspectorGUI("@MarkPropertyDirty(size)")]
        [InlineProperty]
        private FloatProperty size = new FloatProperty(DEFAULT_SIZE);

        [SerializeField]
        [ShowIf("@executionType == ExecutionType.Create")]
        [ValidateInput("ValidateStack", "Value must be more than 0 and smaller than 10,000,000!", InfoMessageType.Warning)]
        [ReadOnly]
        [OnInspectorGUI("@MarkPropertyDirty(stack)")]
        [InlineProperty]
        private FloatProperty stack = new FloatProperty(DEFAULT_STACK);

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ShowIf("@executionType == ExecutionType.GetItemTotalQuantity || executionType == ExecutionType.LinkItemTotalQuantity")]
        [LabelText("Variable")]
        private NumberVariable paramVariable;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ShowIf("@executionType == ExecutionType.AddItem || executionType == ExecutionType.Load")]
        [LabelText("@GetAutoCreateLabel()")]
        private bool autoCreate;

        [SerializeField]
        [OnInspectorGUI("@MarkPropertyDirty(paramStr1)")]
        [InlineProperty]
        [ShowIf("ShowParamStr1")]
        [LabelText("@GetParamStr1Label()")]
        private StringProperty paramStr1;

        [SerializeField]
        [OnInspectorGUI("@MarkPropertyDirty(paramStr2)")]
        [InlineProperty]
        [ShowIf("@executionType == ExecutionType.Save || executionType == ExecutionType.Load")]
        [LabelText("Password")]
        private StringProperty paramStr2;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ShowIf("@executionType == ExecutionType.GetItemName || executionType == ExecutionType.GetItemDesc")]
        [LabelText("Variable")]
        private WordVariable paramWord1;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (executionType is ExecutionType.AddItem or ExecutionType.RemoveItem or ExecutionType.GetItemTotalQuantity or ExecutionType.LinkItemTotalQuantity or ExecutionType.GetItemName
                or ExecutionType.GetItemDesc)
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
            }

            if (executionType == ExecutionType.Create)
            {
                if (string.IsNullOrEmpty(inventoryName) || size <= 0 || stack <= 0)
                {
                    LogWarning("Found an empty Inventory Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    if (!ReInventoryController.CreateInventory(inventoryName, size, stack))
                    {
                        LogWarning($"{inventoryName} inventory not success create in {context.gameObject.name}.");
                    }
                }
            }
            else if (executionType == ExecutionType.Destroy)
            {
                if (string.IsNullOrEmpty(inventoryName))
                {
                    LogWarning("Found an empty Inventory Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    ReInventoryController.DestroyInventory(inventoryName);
                }
            }
            else if (executionType == ExecutionType.Clear)
            {
                if (string.IsNullOrEmpty(inventoryName))
                {
                    LogWarning("Found an empty Inventory Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    InventoryData inv = ReInventoryController.GetInventory(inventoryName);
                    if (inv != null)
                    {
                        inv.ClearItem();
                    }
                    else
                    {
                        LogWarning($"{inventoryName} inventory not found when doing clear item in {context.gameObject.name}.");
                    }
                }
            }
            else if (executionType == ExecutionType.Save)
            {
                if (string.IsNullOrEmpty(inventoryName))
                {
                    LogWarning("Found an empty Inventory Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    InventoryData inv = ReInventoryController.GetInventory(inventoryName);
                    if (inv != null)
                    {
                        SaveOperation op = ReSave.Save(paramStr1 + SAVE_NAME_MIDDLE + inventoryName, ReJson.ObjectToCustomJson(inv), paramStr2);
                        if (!op.success)
                            LogWarning($"{inventoryName} inventory not success save in {context.gameObject.name}.");
                    }
                    else
                    {
                        LogWarning($"{inventoryName} inventory not found when doing save in {context.gameObject.name}.");
                    }
                }
            }
            else if (executionType == ExecutionType.Load)
            {
                if (string.IsNullOrEmpty(inventoryName))
                {
                    LogWarning("Found an empty Inventory Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    SaveOperation op = ReSave.Load(paramStr1 + SAVE_NAME_MIDDLE + inventoryName, paramStr2);
                    if (!op.success)
                    {
                        LogWarning($"{inventoryName} inventory not success load in {context.gameObject.name}.");
                    }
                    else
                    {
                        InventoryData inv = (InventoryData) ReJson.ObjectFromCustomJson<InventoryData>(op.savedString);
                        if (autoCreate)
                            ReInventoryController.DestroyInventory(inventoryName);
                        if (!ReInventoryController.CreateInventory(inv))
                            LogWarning($"{inventoryName} inventory not success create in {context.gameObject.name}.");
                    }
                }
            }
            else if (executionType == ExecutionType.AddItem)
            {
                if (string.IsNullOrEmpty(inventoryName) || item == null || size <= 0)
                {
                    LogWarning("Found an empty Inventory Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    InventoryData inv = ReInventoryController.GetInventory(inventoryName);
                    if (inv == null)
                    {
                        if (!autoCreate)
                        {
                            LogWarning($"{inventoryName} inventory not found when doing add item in {context.gameObject.name}.");
                        }
                        else
                        {
                            ReInventoryController.CreateInventory(inventoryName, (int) DEFAULT_SIZE, (int) DEFAULT_STACK);
                            inv = ReInventoryController.GetInventory(inventoryName);
                        }
                    }

                    if (inv != null)
                        inv.AddItem(item.id, size);
                }
            }
            else if (executionType == ExecutionType.RemoveItem)
            {
                if (string.IsNullOrEmpty(inventoryName) || item == null || size <= 0)
                {
                    LogWarning("Found an empty Inventory Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    InventoryData inv = ReInventoryController.GetInventory(inventoryName);
                    if (inv != null)
                    {
                        inv.RemoveItem(item.id, size);
                    }
                    else
                    {
                        LogWarning($"{inventoryName} inventory not found when doing remove item in {context.gameObject.name}.");
                    }
                }
            }
            else if (executionType == ExecutionType.GetItemTotalQuantity)
            {
                if (string.IsNullOrEmpty(inventoryName) || item == null || paramVariable == null)
                {
                    LogWarning("Found an empty Inventory Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    InventoryData inv = ReInventoryController.GetInventory(inventoryName);
                    if (inv != null)
                    {
                        paramVariable.SetValue(inv.GetItemTotalQuantity(item.id));
                    }
                    else
                    {
                        LogWarning($"{inventoryName} inventory not found when get item quantity in {context.gameObject.name}.");
                    }
                }
            }
            else if (executionType == ExecutionType.GetItemName)
            {
                if (item == null || paramWord1 == null)
                {
                    LogWarning("Found an empty Inventory Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    paramWord1.SetValue(item.displayName);
                }
            }
            else if (executionType == ExecutionType.GetItemDesc)
            {
                if (item == null || paramWord1 == null)
                {
                    LogWarning("Found an empty Inventory Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    paramWord1.SetValue(item.description);
                }
            }
            else if (executionType == ExecutionType.LinkItemTotalQuantity)
            {
                if (string.IsNullOrEmpty(inventoryName) || item == null || paramVariable == null)
                {
                    LogWarning("Found an empty Inventory Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    InventoryData inv = ReInventoryController.GetInventory(inventoryName);
                    if (inv != null)
                    {
                        inv.OnChange -= OnInventoryChange;
                        inv.OnChange += OnInventoryChange;
                        OnInventoryChange(item.id);
                    }
                    else
                    {
                        LogWarning($"{inventoryName} inventory not found when doing link item's quantity in {context.gameObject.name}.");
                    }
                }
            }

            base.OnStart(execution, updateId);
        }

        private void OnInventoryChange (string itemId)
        {
            if (string.IsNullOrEmpty(inventoryName) || item == null || paramVariable == null)
            {
                LogWarning($"Found invalid item ({item.id}) quantity change event in {context.gameObject.name}");
            }
            else
            {
                if (itemId == item.id)
                {
                    InventoryData inv = ReInventoryController.GetInventory(inventoryName);
                    if (inv != null)
                    {
                        paramVariable.SetValue(inv.GetItemTotalQuantity(item.id));
                    }
                    else
                    {
                        LogWarning($"{inventoryName} inventory not found when get item quantity in {context.gameObject.name}.");
                    }
                }
            }
        }

        protected override void OnReset ()
        {
            if (executionType == ExecutionType.LinkItemTotalQuantity)
            {
                if (!string.IsNullOrEmpty(inventoryName) && item != null && paramVariable != null)
                {
                    InventoryData inv = ReInventoryController.GetInventory(inventoryName);
                    if (inv != null)
                        inv.OnChange -= OnInventoryChange;
                }
            }

            base.OnReset();
        }

        public override bool IsRequireInit ()
        {
            if (executionType == ExecutionType.LinkItemTotalQuantity)
                if (!string.IsNullOrEmpty(inventoryName) && item != null && paramVariable != null)
                    return enabled;
            return false;
        }

#if UNITY_EDITOR
        private bool ShowInventoryName ()
        {
            switch (executionType)
            {
                case ExecutionType.None:
                    return false;
                case ExecutionType.GetItemName:
                case ExecutionType.GetItemDesc:
                    return false;
                default:
                    return true;
            }
        }

        private bool ShowParamStr1 ()
        {
            switch (executionType)
            {
                case ExecutionType.Save:
                case ExecutionType.Load:
                    return true;
                default:
                    return false;
            }
        }

        private string GetParamStr1Label ()
        {
            switch (executionType)
            {
                case ExecutionType.Save:
                case ExecutionType.Load:
                    return "Save File";
                default:
                    return string.Empty;
            }
        }

        private bool ShowItemData ()
        {
            switch (executionType)
            {
                case ExecutionType.AddItem:
                case ExecutionType.RemoveItem:
                case ExecutionType.GetItemTotalQuantity:
                case ExecutionType.LinkItemTotalQuantity:
                case ExecutionType.GetItemName:
                case ExecutionType.GetItemDesc:
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
            switch (executionType)
            {
                case ExecutionType.AddItem:
                case ExecutionType.RemoveItem:
                case ExecutionType.GetItemTotalQuantity:
                case ExecutionType.LinkItemTotalQuantity:
                case ExecutionType.GetItemName:
                case ExecutionType.GetItemDesc:
                    if (itemType == 1)
                        return true;
                    else
                        return false;
                default:
                    return false;
            }
        }

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

        public bool GetCacheItemType ()
        {
            if (itemType == 1)
                return true;
            return false;
        }

        private string GetSizeLabel ()
        {
            if (executionType == ExecutionType.Create)
                return "Size";
            if (executionType is ExecutionType.AddItem or ExecutionType.RemoveItem)
                return "Amount";
            return string.Empty;
        }

        private string GetAutoCreateLabel ()
        {
            if (executionType == ExecutionType.Load)
                return "Overwrite";
            if (executionType is ExecutionType.AddItem)
                return "Auto Create";
            return string.Empty;
        }

        private bool ValidateSize (int value)
        {
            return value > 0f && value < 10000000;
        }

        private bool ValidateStack (int value)
        {
            return value > 0f && value < int.MaxValue - 1;
        }

        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Add Item", ExecutionType.AddItem},
            {"Remove Item", ExecutionType.RemoveItem},
            {"Link Item Quantity", ExecutionType.LinkItemTotalQuantity},
            {"Create", ExecutionType.Create},
            {"Destroy", ExecutionType.Destroy},
            {"Clear", ExecutionType.Clear},
            {"Save", ExecutionType.Save},
            {"Load", ExecutionType.Load},
            {"Get Item Quantity", ExecutionType.GetItemTotalQuantity},
            {"Get Item Name", ExecutionType.GetItemName},
            {"Get Item Description", ExecutionType.GetItemDesc},
        };

        public static string displayName = "Inventory Behaviour Node";
        public static string nodeName = "Inventory";

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
            return $"Gameplay/{nodeName}";
        }

        public override string GetNodeViewDescription ()
        {
            if (executionType == ExecutionType.Create)
            {
                if (!string.IsNullOrEmpty(inventoryName) && size > 0 && stack > 0)
                    return $"Create {inventoryName} with size {size} and stack {stack}";
            }
            else if (executionType == ExecutionType.Destroy)
            {
                if (!string.IsNullOrEmpty(inventoryName))
                    return $"Destroy {inventoryName}";
            }
            else if (executionType == ExecutionType.Clear)
            {
                if (!string.IsNullOrEmpty(inventoryName))
                    return $"Clear {inventoryName}";
            }
            else if (executionType == ExecutionType.Save)
            {
                if (!string.IsNullOrEmpty(inventoryName))
                    return $"Save {inventoryName}";
            }
            else if (executionType == ExecutionType.Load)
            {
                if (!string.IsNullOrEmpty(inventoryName))
                    return $"Load {inventoryName}";
            }
            else if (executionType == ExecutionType.AddItem)
            {
                if (!string.IsNullOrEmpty(inventoryName) && size > 0)
                {
                    if (itemType == 0 && item != null)
                    {
                        return $"Add {size} {item.displayName} into {inventoryName}";
                    }
                    else if (itemType == 1 && !string.IsNullOrEmpty(itemCache))
                    {
                        return $"Add {size} {GetItemCacheName(itemCache)} into {inventoryName}";
                    }
                }
            }
            else if (executionType == ExecutionType.RemoveItem)
            {
                if (!string.IsNullOrEmpty(inventoryName) && size > 0)
                {
                    if (itemType == 0 && item != null)
                    {
                        return $"Remove {size} {item.displayName} from {inventoryName}";
                    }
                    else if (itemType == 1 && !string.IsNullOrEmpty(itemCache))
                    {
                        return $"Remove {size} {GetItemCacheName(itemCache)} from {inventoryName}";
                    }
                }
            }
            else if (executionType == ExecutionType.GetItemTotalQuantity)
            {
                if (!string.IsNullOrEmpty(inventoryName) && paramVariable != null)
                {
                    if (itemType == 0 && item != null)
                    {
                        return $"Set {item.displayName}'s quantity into {paramVariable.name}";
                    }
                    else if (itemType == 1 && !string.IsNullOrEmpty(itemCache))
                    {
                        return $"Set {GetItemCacheName(itemCache)}'s quantity into {paramVariable.name}";
                    }
                }
            }
            else if (executionType == ExecutionType.LinkItemTotalQuantity)
            {
                if (!string.IsNullOrEmpty(inventoryName) && paramVariable != null)
                {
                    if (itemType == 0 && item != null)
                    {
                        return $"Link {item.displayName}'s quantity to {paramVariable.name}";
                    }
                    else if (itemType == 1 && !string.IsNullOrEmpty(itemCache))
                    {
                        return $"Link {GetItemCacheName(itemCache)}'s quantity to {paramVariable.name}";
                    }
                }
            }
            else if (executionType == ExecutionType.GetItemName)
            {
                if (paramWord1 != null)
                {
                    if (itemType == 0 && item != null)
                    {
                        return $"Set {item.displayName}'s name into {paramWord1.name}";
                    }
                    else if (itemType == 1 && !string.IsNullOrEmpty(itemCache))
                    {
                        return $"Set {GetItemCacheName(itemCache)}'s name into {paramWord1.name}";
                    }
                }
            }
            else if (executionType == ExecutionType.GetItemDesc)
            {
                if (paramWord1 != null)
                {
                    if (itemType == 0 && item != null)
                    {
                        return $"Set {item.displayName}'s description into {paramWord1.name}";
                    }
                    else if (itemType == 1 && !string.IsNullOrEmpty(itemCache))
                    {
                        return $"Set {GetItemCacheName(itemCache)}'s description into {paramWord1.name}";
                    }
                }
            }

            return string.Empty;
        }
#endif
    }
}