using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    private List<DropData> droppedItems = new List<DropData>();

    public GameObject DroppedItemPrefab;
    
    // public List<Transform> groundLootList = new List<Transform>();

    public List<GroundLootPair> GroundLootPairs = new List<GroundLootPair>();

    private void Start ()
    {
        NetworkManager.Singleton.OnServerStarted += ServerStarted;
        
    }

    private void ServerStarted()
    {
        foreach (var groundLootPair in GroundLootPairs)
        {
            SpawnOnTheGround(groundLootPair.LootSpawnTransform.position, groundLootPair.Loot.Id);
        }    
    }

    public void SpawnOnTheGround(Vector3 spawnPosition, int itemToDrop)
    {
        var dropPosition = spawnPosition + new Vector3(0, .2f, 0);
        
        var instance = Instantiate(DroppedItemPrefab, dropPosition, Quaternion.identity);
        var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        
        instance.GetComponent<DroppedItem>().SetupDroppedItem(itemToDrop);

        instanceNetworkObject.Spawn();
        
        CreateDropData(itemToDrop, instanceNetworkObject.NetworkObjectId, instanceNetworkObject);
    }

    public void HandleDroppedItemData(PlayerState inputWinningPlayerState, PlayerState inputLoserPlayerState)
    {
        var winnerNetworkObject = inputWinningPlayerState.GetComponent<NetworkObject>();
        var dropPosition = winnerNetworkObject.transform.position;
        dropPosition -= new Vector3(0, 0.5f, 0);
        
        // we get the inventoryList from loser
        var dropList = inputLoserPlayerState.InventoryList;
        
        // Debug.Log("häviäjän inventoryssa oli näin monta itemiä: " + dropList.Count);
        // if (dropList.Count > 0)
        // {
        //     Debug.Log("yritettiin tiputtaa: " + ItemCatalogManager.Instance.GetItemById(dropList[0]));
        // }
        // else
        // {
        //     Debug.Log("vihun inventory oli tyhjä D: ");
        //
        // }

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
        
                CreateDropData(drop, instanceNetworkObject.NetworkObjectId, instanceNetworkObject, winnerNetworkObject);
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

                CreateDropData(drop, instanceNetworkObject.NetworkObjectId, instanceNetworkObject);
            }
        }
        
        // we clear the loser's inventory :D
        inputLoserPlayerState.InventoryList?.Clear();
        inputLoserPlayerState.EquippedItems?.Clear();
        
        // ToDo: add a timer to change the ownership
    }
    
    public void CreateDropData(int inputItemIdThatDropped, ulong inputDroppedItemNetworkId, NetworkObject droppedItemNetworkObject, NetworkObject inputPlayerNetworkObject = null)
    {
        var dropData = new DropData(
            playerNetworkObject: inputPlayerNetworkObject, 
            itemIdThatDropped: inputItemIdThatDropped, 
            droppedItemNetworkId: inputDroppedItemNetworkId, 
            droppedItemNetworkObject: droppedItemNetworkObject, 
            isCommunistic: true);
        
        droppedItems.Add(dropData);
    }

    public void TryToPickUpItem(ulong droppedItemId, NetworkObject playerPickingUp)
    {
        var dropData = droppedItems.FirstOrDefault(x => x.DroppedItemNetworkId == droppedItemId);

        if (dropData != null)
        {
            var playerState = playerPickingUp.GetComponent<PlayerState>();
            
            if (playerState)
            {
                if (playerState.InventoryList.Count < GlobalSettings.InventoryMaxSize)
                {
                    playerState.InventoryList.Add(dropData.ItemIdThatDropped);
                    dropData.DroppedItemNetworkObject.Despawn();
                    droppedItems.Remove(dropData);
                }
            }
        }
    }
}

public class DropData
{
    public NetworkObject PlayerNetworkObject;
    public int ItemIdThatDropped;
    public ulong DroppedItemNetworkId;
    public NetworkObject DroppedItemNetworkObject;
    public bool IsCommunistic;
    
    public DropData(NetworkObject playerNetworkObject, int itemIdThatDropped, ulong droppedItemNetworkId, 
        NetworkObject droppedItemNetworkObject, bool isCommunistic)
    {
        PlayerNetworkObject = playerNetworkObject;
        ItemIdThatDropped = itemIdThatDropped;
        DroppedItemNetworkId = droppedItemNetworkId;
        DroppedItemNetworkObject = droppedItemNetworkObject;
        IsCommunistic = isCommunistic;
    }
}

[Serializable]
public class GroundLootPair
{
    public Transform LootSpawnTransform;
    public Item Loot;
}
