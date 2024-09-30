using System.Collections.Generic;
using System.Linq;
using Cainos.PixelArtTopDown_Basic;
using Unity.Netcode;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    private List<DropData> droppedItems = new List<DropData>();

    public GameObject DroppedItemPrefab;
    
    public void HandleDroppedItemData(NetworkObject networkObject, PlayerState playerState)
    {
        // playerState.DropItemToPlayerRpc(dropData.ItemIdThatDropped);

        var randomItem = ItemCatalogManager.Instance.GetRandomItemFromDatabase();
        
        var instance = Instantiate(DroppedItemPrefab, networkObject.transform.position - new Vector3(0, 0.5f, 0), Quaternion.identity);
        var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        
        instance.GetComponent<DroppedItem>().SetupDroppedItem(randomItem);

        instanceNetworkObject.SpawnWithOwnership(networkObject.OwnerClientId);
        
        Debug.Log(networkObject.NetworkObjectId);
        
        
        DropData dropData = new DropData();
        dropData.PlayerWhoGotTheDrop = networkObject;
        dropData.ItemIdThatDropped = randomItem;
        dropData.NetworkId = instanceNetworkObject.NetworkObjectId;
        dropData.NetworkObject = instanceNetworkObject;
        
        droppedItems.Add(dropData);
        
        // ToDo: add a timer to change the ownership
    }

    public void TryToPickUpItem(ulong droppedItemId)
    {
        var dropData = droppedItems.FirstOrDefault(x => x.NetworkId == droppedItemId);

        if (dropData != null)
        {
            dropData.PlayerWhoGotTheDrop.GetComponent<PlayerState>()?.InventoryList.Add(dropData.ItemIdThatDropped);
            dropData.NetworkObject.Despawn();
            droppedItems.Remove(dropData);
        }
    }
}

public class DropData
{
    public NetworkObject PlayerWhoGotTheDrop;
    public int ItemIdThatDropped;
    public ulong NetworkId;
    public NetworkObject NetworkObject;

}
