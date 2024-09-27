using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    // data structure to load and save from cloud
    private Dictionary<string, Item> inventoryDictionary = new Dictionary<string, Item>();
    // actual inventory that is in the player's inventory
    private List<InventoryItem> inventoryItems = new List<InventoryItem>();

    private List<DropData> droppedItems = new List<DropData>();
    
    // private void Start()
    // {
    // }
    //
    // private async void SaveInventory()
    // {
    //     inventoryDictionary.Clear();
    //
    //     foreach (var item in inventoryItems)
    //     {
    //         inventoryDictionary.Add(item.item.Id, item.item);
    //     }
    //     
    //     await SaveToCloud(inventoryDictionary);
    // }
    //
    // private async Task SaveToCloud(Dictionary<string, Item> dictionary)
    // {
    //     Debug.Log("tallensit pilveen :D hyvä homma");
    // }

    public void StartDroppingItem(NetworkObject networkObject, PlayerState playerState)
    {
        DropData dropData = new DropData();
        dropData.PlayerWhoGotTheDrop = networkObject;
        dropData.ItemIdThatDropped = ItemCatalogManager.Instance.GetRandomItemFromDatabase();
        dropData.DropWorldPosition = networkObject.transform.position;
        
        droppedItems.Add(dropData);
        
        playerState.DropItemToPlayerRpc(dropData.ItemIdThatDropped);
    }

    public void TryToPickUpItem(NetworkObject playerNetworkObject, int itemId)
    {
        if (droppedItems.Any())
        {
            var dropData = droppedItems.FirstOrDefault(x => x.PlayerWhoGotTheDrop == playerNetworkObject);
            if (dropData != null)
            {
                if (dropData.ItemIdThatDropped == itemId)
                {
                    Debug.Log("kokeillaan lisätä itemiä :D");
                    playerNetworkObject.GetComponent<PlayerState>().InventoryList.Add(itemId);
                }
            }
        }
    }
}

public class DropData
{
    public NetworkObject PlayerWhoGotTheDrop;
    public int ItemIdThatDropped;
    public Vector3 DropWorldPosition;
}
