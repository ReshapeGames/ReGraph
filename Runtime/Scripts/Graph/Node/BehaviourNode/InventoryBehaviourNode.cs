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
        public enum ExecutionType
        {
            None,
            Create = 11,
            Destroy = 21,
            Clear = 31,
            AddItem = 41,
            RemoveItem = 51,
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
        private FloatProperty size = new FloatProperty(1000);

        [SerializeField]
        [ShowIf("@executionType == ExecutionType.Create")]
        [ValidateInput("ValidateStack", "Value must be more than 0 and smaller than 10,000,000!", InfoMessageType.Warning)]
        [ReadOnly]
        [OnInspectorGUI("@MarkPropertyDirty(stack)")]
        [InlineProperty]
        private FloatProperty stack = new FloatProperty(9999999);

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ShowIf("@executionType == ExecutionType.GetItemTotalQuantity || executionType == ExecutionType.LinkItemTotalQuantity")]
        [LabelText("Variable")]
        private NumberVariable paramVariable;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ShowIf("@executionType == ExecutionType.AddItem")]
        private bool autoCreate;

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
                    ReInventoryController.CreateInventory(inventoryName, size, stack);
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
                            ReInventoryController.CreateInventory(inventoryName, size, stack);
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