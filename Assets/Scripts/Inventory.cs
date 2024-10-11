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
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }
        else
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
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

        // check if an item was equipped, we need to store it

        bool itemWasEquipped = false;
        int itemIndexWasEquipped = 0;
        inventoryItems.RemoveAll(x => x == null);
        foreach (var inventoryItem in inventoryItems)
        {
            if (inventoryItem.ItemIsEquipped)
            {
                itemWasEquipped = true;
                itemIndexWasEquipped = inventoryItem.index;
            }
          
            inventoryItem.RemoveElement();
        }
        
        int index = 0;

        foreach (Item item in itemList)
        {
            var itemSlot = Instantiate(inventorySlotPrefab, inventoryContainer);
            var component = itemSlot.GetComponent<InventoryItem>();
            
            component.InitializeElement(item, playerState, index);

            if (index == itemIndexWasEquipped && itemWasEquipped)
            {
                component.EquipItem();
            }
            
            inventoryItems.Add(component);

            index++;
        }
    }

    private InventoryItem FindItemWithIndex(int index)
    {
        foreach (var inventoryItem in inventoryItems)
        {
            if (inventoryItem.index == index)
            {
                return inventoryItem;
            }
        }

        return null;
    }

    public void EquipItem(int index)
    {
        inventoryItems.RemoveAll(x => x == null);
        foreach (var inventoryItem in inventoryItems)
        {
            inventoryItem.UnequipItem();
        }
        
        var inventorySlot = FindItemWithIndex(index);
        
        inventorySlot.EquipItem();
        Debug.Log("you equipped: " + FindItemWithIndex(index) + ", that was on slot: " + index);
    }
}
