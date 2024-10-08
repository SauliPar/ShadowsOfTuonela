using System.Collections.Generic;
using System.Linq;
using Cainos.PixelArtTopDown_Basic;
using Unity.Netcode;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    private List<DropData> droppedItems = new List<DropData>();

    public GameObject DroppedItemPrefab;
    
    public void HandleDroppedItemData(PlayerState inputWinningPlayerState, PlayerState inputLoserPlayerState)
    {
        var winnerNetworkObject = inputWinningPlayerState.GetComponent<NetworkObject>();
        var dropPosition = winnerNetworkObject.transform.position;
        dropPosition -= new Vector3(0, 0.5f, 0);
        
        // we get the inventoryList from loser
        var dropList = inputLoserPlayerState.InventoryList;
        
        // Debug.Log("häviäjän inventoryssa oli näin monta itemiä: " + dropList.Count);

        // bots don't get loot, at least not now
        if (!inputWinningPlayerState.IsBot)
        {
            // then we instantiate and spawn droppeditems based on the list
            foreach (var drop in dropList)
            {
                var instance = Instantiate(DroppedItemPrefab, dropPosition, Quaternion.identity);
                var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        
                instance.GetComponent<DroppedItem>().SetupDroppedItem(drop);

                instanceNetworkObject.SpawnWithOwnership(winnerNetworkObject.OwnerClientId);
        
                DropData dropData = new DropData();
                dropData.PlayerWhoGotTheDrop = winnerNetworkObject;
                dropData.ItemIdThatDropped = drop;
                dropData.NetworkId = instanceNetworkObject.NetworkObjectId;
                dropData.NetworkObject = instanceNetworkObject;
                dropData.IsCommunistic = false;
        
                // Debug.Log("dropattiin: " + ItemCatalogManager.Instance.GetItemById(drop));

                droppedItems.Add(dropData);
            }
        }
        // if the losing player is a bot
        else
        {
            foreach (var drop in dropList)
            {
                var instance = Instantiate(DroppedItemPrefab, dropPosition, Quaternion.identity);
                var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        
                instance.GetComponent<DroppedItem>().SetupDroppedItem(drop);

                instanceNetworkObject.SpawnWithOwnership(winnerNetworkObject.OwnerClientId);
        
                DropData dropData = new DropData();
                dropData.PlayerWhoGotTheDrop = null;
                dropData.ItemIdThatDropped = drop;
                dropData.NetworkId = instanceNetworkObject.NetworkObjectId;
                dropData.NetworkObject = instanceNetworkObject;
                dropData.IsCommunistic = true;
        
                // Debug.Log("dropattiin: " + ItemCatalogManager.Instance.GetItemById(drop));

                droppedItems.Add(dropData);
            }
        }
        
        // we clear the loser's inventory :D
        inputLoserPlayerState.InventoryList?.Clear();
        inputLoserPlayerState.EquippedItems?.Clear();
        
        // ToDo: add a timer to change the ownership
    }

    public void TryToPickUpItem(ulong droppedItemId, NetworkObject playerPickingUp)
    {
        var dropData = droppedItems.FirstOrDefault(x => x.NetworkId == droppedItemId);

        if (dropData != null)
        {
            playerPickingUp.GetComponent<PlayerState>()?.InventoryList.Add(dropData.ItemIdThatDropped);
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
    public bool IsCommunistic;
}
