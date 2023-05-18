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
            GetItemTotalQuantity = 1001
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [OnInspectorGUI("@MarkPropertyDirty(inventoryName)")]
        [InlineProperty]
        [ShowIf("@executionType != ExecutionType.None")]
        [LabelText("Name")]
        private StringProperty inventoryName;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ValueDropdown("ItemChoice")]
        [InlineButton("@ItemManager.OpenWindow()", "✚")]
        [ShowIf(
            "@executionType == ExecutionType.AddItem || executionType == ExecutionType.RemoveItem || executionType == ExecutionType.GetItemTotalQuantity || executionType == ExecutionType.LinkItemTotalQuantity")]
        private ItemData item;

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
        [ShowIf("@executionType == ExecutionType.Save || executionType == ExecutionType.Load")]
        [LabelText("Save File")]
        private StringProperty paramStr1;
        
        [SerializeField]
        [OnInspectorGUI("@MarkPropertyDirty(paramStr2)")]
        [InlineProperty]
        [ShowIf("@executionType == ExecutionType.Save || executionType == ExecutionType.Load")]
        [LabelText("Password")]
        private StringProperty paramStr2;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (executionType == ExecutionType.Create)
            {
                if (string.IsNullOrEmpty(inventoryName) || size <= 0 || stack <= 0)
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Inventory Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    if (!ReInventoryController.CreateInventory(inventoryName, size, stack))
                    {
                        ReDebug.LogWarning("Graph Warning", $"{inventoryName} inventory not success create in {context.gameObject.name}.");
                    }
                }
            }
            else if (executionType == ExecutionType.Destroy)
            {
                if (string.IsNullOrEmpty(inventoryName))
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Inventory Behaviour node in " + context.gameObject.name);
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
                    ReDebug.LogWarning("Graph Warning", "Found an empty Inventory Behaviour node in " + context.gameObject.name);
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
                        ReDebug.LogWarning("Graph Warning", $"{inventoryName} inventory not found when doing clear item in {context.gameObject.name}.");
                    }
                }
            }
            else if (executionType == ExecutionType.Save)
            {
                if (string.IsNullOrEmpty(inventoryName))
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Inventory Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    InventoryData inv = ReInventoryController.GetInventory(inventoryName);
                    if (inv != null)
                    {
                        SaveOperation op = ReSave.Save(paramStr1 + SAVE_NAME_MIDDLE + inventoryName, ReJson.ObjectToCustomJson(inv), paramStr2);
                        if (!op.success)
                            ReDebug.LogWarning("Graph Warning", $"{inventoryName} inventory not success save in {context.gameObject.name}.");
                    }
                    else
                    {
                        ReDebug.LogWarning("Graph Warning", $"{inventoryName} inventory not found when doing save in {context.gameObject.name}.");
                    }
                }
            }
            else if (executionType == ExecutionType.Load)
            {
                if (string.IsNullOrEmpty(inventoryName))
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Inventory Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    SaveOperation op = ReSave.Load(paramStr1 + SAVE_NAME_MIDDLE + inventoryName, paramStr2);
                    Debug.Log(op.savedString);
                    if (!op.success)
                    {
                        ReDebug.LogWarning("Graph Warning", $"{inventoryName} inventory not success load in {context.gameObject.name}.");
                    }
                    else
                    {
                        InventoryData inv = (InventoryData) ReJson.ObjectFromCustomJson<InventoryData>(op.savedString);
                        if (autoCreate)
                            ReInventoryController.DestroyInventory(inventoryName);
                        if (!ReInventoryController.CreateInventory(inv))
                            ReDebug.LogWarning("Graph Warning", $"{inventoryName} inventory not success create in {context.gameObject.name}.");
                    }
                }
            }
            else if (executionType == ExecutionType.AddItem)
            {
                if (string.IsNullOrEmpty(inventoryName) || item == null || size <= 0)
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Inventory Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    InventoryData inv = ReInventoryController.GetInventory(inventoryName);
                    if (inv == null)
                    {
                        if (!autoCreate)
                        {
                            ReDebug.LogWarning("Graph Warning", $"{inventoryName} inventory not found when doing add item in {context.gameObject.name}.");
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
                    ReDebug.LogWarning("Graph Warning", "Found an empty Inventory Behaviour node in " + context.gameObject.name);
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
                        ReDebug.LogWarning("Graph Warning", $"{inventoryName} inventory not found when doing remove item in {context.gameObject.name}.");
                    }
                }
            }
            else if (executionType == ExecutionType.GetItemTotalQuantity)
            {
                if (string.IsNullOrEmpty(inventoryName) || item == null || paramVariable == null)
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Inventory Behaviour node in " + context.gameObject.name);
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
                        ReDebug.LogWarning("Graph Warning", $"{inventoryName} inventory not found when get item quantity in {context.gameObject.name}.");
                    }
                }
            }
            else if (executionType == ExecutionType.LinkItemTotalQuantity)
            {
                if (string.IsNullOrEmpty(inventoryName) || item == null || paramVariable == null)
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Inventory Behaviour node in " + context.gameObject.name);
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
                        ReDebug.LogWarning("Graph Warning", $"{inventoryName} inventory not found when doing link item's quantity in {context.gameObject.name}.");
                    }
                }
            }

            base.OnStart(execution, updateId);
        }

        private void OnInventoryChange (string itemId)
        {
            if (string.IsNullOrEmpty(inventoryName) || item == null || paramVariable == null)
            {
                ReDebug.LogWarning("Graph Warning", $"Found invalid item ({item.id}) quantity change event in {context.gameObject.name}");
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
                        ReDebug.LogWarning("Graph Warning", $"{inventoryName} inventory not found when get item quantity in {context.gameObject.name}.");
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
                if (!string.IsNullOrEmpty(inventoryName) && item != null && size > 0)
                    return $"Add {size} {item.displayName} into {inventoryName}";
            }
            else if (executionType == ExecutionType.RemoveItem)
            {
                if (!string.IsNullOrEmpty(inventoryName) && item != null && size > 0)
                    return $"Remove {size} {item.displayName} from {inventoryName}";
            }
            else if (executionType == ExecutionType.GetItemTotalQuantity)
            {
                if (!string.IsNullOrEmpty(inventoryName) && item != null && paramVariable != null)
                    return $"Set {item.displayName}'s quantity into {paramVariable.name}";
            }
            else if (executionType == ExecutionType.LinkItemTotalQuantity)
            {
                if (!string.IsNullOrEmpty(inventoryName) && item != null && paramVariable != null)
                    return $"Link {item.displayName}'s quantity to {paramVariable.name}";
            }

            return string.Empty;
        }
#endif
    }
}