using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private PlayerState playerState;

    private bool _inventoryOn;
    private bool _initialRun = true;
    
    private List<InventoryItem> inventoryItems = new List<InventoryItem>();
    
    
    public void ToggleInventory()
    {
        if (_initialRun && !inventoryItems.Any())
        {
            SetupInventorySlots(playerState.InventoryList);
            _initialRun = false;
        }
        
        _inventoryOn = !_inventoryOn;

        if (_inventoryOn)     
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            canvasGroup.alpha = 0f;
        }
    }
    
    public void SetupInventorySlots(NetworkList<int> itemIdList)
    {
        List<Item> itemList = new List<Item>();
        foreach (var itemId in itemIdList)
        {
            itemList.Add(ItemCatalogManager.Instance.GetItemById(itemId));
        }

        inventoryItems.RemoveAll(x => x == null);
        foreach (var inventoryItem in inventoryItems)
        {
            inventoryItem.RemoveElement();
        }
        
        foreach (Item item in itemList)
        {
            var itemSlot = Instantiate(inventorySlotPrefab, inventoryContainer);
            var component = itemSlot.GetComponent<InventoryItem>();
            int index = 0;
            
            if (itemList.Count > 0)
            {
                index = itemList.Count - 1;
            }
            
            component.InitializeElement(item, playerState, index);
            
            inventoryItems.Add(component);
        }
    }

    private Item FindItemWithIndex(int index)
    {
        foreach (var inventoryItem in inventoryItems)
        {
            if (inventoryItem.index == index)
            {
                return inventoryItem.item;
            }
        }

        return null;
    }

    public void EquipItem(int index)
    {
        Debug.Log("you equipped: " + FindItemWithIndex(index) + ", that was on slot: " + index);
    }

    // [Rpc(SendTo.Server)]
    // private void OnButtonPressServerRpc(int index)
    // {
    //     Item item = null;
    //     InventoryItem selectedInventoryItem = null;
    //     
    //     foreach (var inventorySlot in inventoryItems)
    //     {
    //         if (inventorySlot.index == index)
    //         {
    //             selectedInventoryItem = inventorySlot;
    //             item = inventorySlot.item;
    //             break;
    //         }
    //     }
    //
    //     if (item == null || selectedInventoryItem == null) return;
    //     
    //     ItemType itemType = item.ItemType;
    //
    //     switch (itemType)
    //     {
    //         case ItemType.Consumable:
    //             Debug.Log("hiilasit xd");
    //             playerState.Health.Value += 10;
    //             inventoryItems.Remove(selectedInventoryItem);
    //             selectedInventoryItem.RemoveElement();
    //             break;
    //         case ItemType.Equipment:
    //             Debug.Log("yritit equippaa :D");
    //             break;
    //     }
    // }
    //
    // [ServerRpc]
    // public void OnItemPickupServerRpc(string itemId)
    // {
    //     Item item = ItemCatalogManager.Instance.GetItemById(itemId);
    //     var itemSlot = Instantiate(inventorySlotPrefab, inventoryContainer);
    //     var component = itemSlot.GetComponent<InventoryItem>();
    //     int index = 0;
    //         
    //     if (inventoryItems.Count > 0)
    //     {
    //         index = inventoryItems.Count - 1;
    //     }
    //         
    //     component.InitializeElement(item, OnButtonPressServerRpc, index);
    //         
    //     inventoryItems.Add(component);
    // }
}
