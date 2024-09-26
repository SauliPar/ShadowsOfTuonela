using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class InventoryManager : Singleton<InventoryManager>
{
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private Item rune2h;
    [SerializeField] private Item lobster;

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    private async void Start()
    {
        // load stuff from cloud
        var dictionary = await LoadFromCloud();
        // we get dictionary, which we parse into item list or something

        if (dictionary.Count > 0)
        {
            SetupInventorySlots(dictionary);
        }
    }

    private void SetupInventorySlots(Dictionary<int, Item> dictionary)
    {
        foreach (KeyValuePair<int, Item> entry in dictionary)
        {
            var itemSlot = Instantiate(inventorySlotPrefab, inventoryContainer);
            var component = itemSlot.GetComponent<InventorySlot>();
            component.InitializeElement(entry.Value, OnButtonPress);
            
            inventorySlots.Add(component);
        }
    }

    private void OnButtonPress(InventorySlot inventorySlot)
    {
        ItemType itemType = inventorySlot.item.ItemType;

        switch (itemType)
        {
            case ItemType.Consumable:
                Debug.Log("hiilasit xd");
                inventorySlots.Remove(inventorySlot);
                inventorySlot.RemoveElement();
                break;
            case ItemType.Equipment:
                Debug.Log("yritit equippaa :D");
                break;
        }
    }

    private Task<Dictionary<int, Item>> LoadFromCloud()
    {
        Dictionary<int, Item> newDictionary = new Dictionary<int, Item>();
        newDictionary.Add(1, rune2h);
        newDictionary.Add(2, lobster);
        return Task.FromResult(newDictionary);
    }

    public void OnItemPickup(Item item)
    {
        var itemSlot = Instantiate(inventorySlotPrefab, inventoryContainer);
        var component = itemSlot.GetComponent<InventorySlot>();
        component.InitializeElement(item, OnButtonPress);
            
        inventorySlots.Add(component);
    }
}
