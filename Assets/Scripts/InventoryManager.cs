using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    private List<DropData> droppedItems = new List<DropData>();
    
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
        var dropData = droppedItems.FirstOrDefault(x => x.PlayerWhoGotTheDrop == playerNetworkObject && x.ItemIdThatDropped == itemId);

        if (dropData != null)
        {
            playerNetworkObject.GetComponent<PlayerState>()?.InventoryList.Add(itemId);
        }
    }
}

public class DropData
{
    public NetworkObject PlayerWhoGotTheDrop;
    public int ItemIdThatDropped;
    public Vector3 DropWorldPosition;
}
