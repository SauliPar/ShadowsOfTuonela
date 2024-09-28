using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PlayerState : NetworkBehaviour
{
    /// <summary>
    /// Default Permissions: Everyone can read, server can only write
    /// Player health is typically something determined (updated/written to) on the server
    ///  side, but a value everyone should be synchronized with (that is, read permissions).
    /// </summary>
    ///
    /// 
    public NetworkVariable<int> Health = new NetworkVariable<int>(GlobalSettings.DefaultHealth);
    // public NetworkVariable<int> Damage = new NetworkVariable<int>(0);
    // public NetworkVariable<bool> IsDead = new NetworkVariable<bool>(false);
    
    public NetworkVariable<CombatState> CombatState = new();

    public NetworkList<int> InventoryList = new();

    public HealthBarScript HealthBarScript;
    public DamageTakenScript DamageTakenScript;
    public BaseController BaseController;
    public GameObject DroppedItemPrefab;
    public Item DroppedItem;
    public NetworkObject MyNetworkObject;

    public Inventory Inventory;
    
    private Dictionary<string, Item> itemDictionary;

    private async void Start()
    {
        if (IsServer)
        {
            // load inventory from cloud, and initialize it to the list
            itemDictionary = await LoadPlayerInventory();
            foreach (var item in itemDictionary.Values)
            {
                InventoryList.Add(item.Id);
            }
        }
        else
        {
            // Debug.Log("Tultiin PlayerStaten client-osioon");
        }
        
        CombatState.OnValueChanged += OnCharacterStateChanged;
        Health.OnValueChanged += OnHealthValueChanged;
        InventoryList.OnListChanged += OnInventoryListChanged;
    }

    public override void OnDestroy()
    {
        CombatState.OnValueChanged -= OnCharacterStateChanged;
        Health.OnValueChanged -= OnHealthValueChanged;
        InventoryList.OnListChanged -= OnInventoryListChanged;

        if (IsServer) InventoryList = null;
    }

    private void OnInventoryListChanged(NetworkListEvent<int> changeevent)
    {
        Debug.Log("oninventorylistchanged");
        
        Inventory.SetupInventorySlots(InventoryList);
    }

    private Task<Dictionary<string, Item>> LoadPlayerInventory()
    {
        Dictionary<string, Item> newDictionary = new Dictionary<string, Item>();
        newDictionary.Add("1", ItemCatalogManager.Instance.GetItemById(1));
        newDictionary.Add("2", ItemCatalogManager.Instance.GetItemById(2));
        return Task.FromResult(newDictionary);
    }

    private void OnCharacterStateChanged(CombatState previousvalue, CombatState newvalue)
    {
        // BaseController.CharacterState = newvalue;
    }
    
    // private void OnInventoryListChanged(List<int> previousvalue, List<int> newvalue)
    // {
    //     // List<Item> itemList = new List<Item>();
    //     // foreach (var itemId in newvalue)
    //     // {
    //     //     itemList.Add(ItemCatalogManager.Instance.GetItemById(itemId));
    //     // }
    //     //
    //     // Inventory.SetupInventorySlots(itemList);
    // }


    private void OnHealthValueChanged(int previousvalue, int newvalue)
    {
        // if (IsOwner)
        // {
            var substractValue = previousvalue - newvalue;
            HealthBarScript.SetHealthBarValue(newvalue);
            
            if (substractValue < 0) return;
           
            DamageTakenScript.ShowDamage(substractValue);
        // }
       
        
        // // Debug.Log("onhealthvaluechanged");
        // HealthBarScript.SetHealthBarValue(newvalue);
        //
        // // Debug.Log("damagenumber 1: " + (previousvalue - newvalue));
        //
        // DamageTakenScript.ShowDamage(previousvalue - newvalue);
    }

    public bool DecreaseHealthPoints(int damageValue)
    {
        Health.Value -= damageValue;
        // Damage.Value = damageValue;

        if (Health.Value <= 0)
        {
            // should combat continue?
            return false;
        }

        // should combat continue?
        return true;
    }

    public void ResetHealth()
    {
        Health.Value = GlobalSettings.DefaultHealth;
        HealthBarScript.SetHealthBarValue(Health.Value);
    }
    
    [Rpc(SendTo.Everyone)]
    public void DeathRpc()
    {
        if (IsOwner)
        {
            Debug.Log("oltiin owner ja toistettiin RPC");
            BaseController.OnDeath();
            BaseController.TeleportCharacter(Vector3.zero);
        }
    }

    [Rpc(SendTo.Owner)]
    public void StartCombatRpc(Vector3 fightPosition, int faceIndex)
    {
        BaseController.StartFight(fightPosition, faceIndex);
    }

    [Rpc(SendTo.Server)]
    public void UseItemRpc(int index)
    {
        if (index >= 0 && index < InventoryList.Count)
        {
            var item = ItemCatalogManager.Instance.GetItemById(InventoryList[index]);

            switch (item.ItemType)
            {
                case ItemType.Consumable:
                    Health.Value += 10;
                    InventoryList.RemoveAt(index);
                    break;
                case ItemType.Equipment:
                    ItemIsEligibleToUseRpc(index);
                    break;
            }
        }
        else
        {
            // ban player for cheating inventory :D
        }
    }

    [Rpc(SendTo.Owner)]
    private void ItemIsEligibleToUseRpc(int index)
    {
        Inventory.EquipItem(index);
    }

    [Rpc(SendTo.Owner)]
    public void DropItemToPlayerRpc(int droppedItemId)
    {
        var drop = Instantiate(DroppedItemPrefab, transform.position - new Vector3(0, 0.5f, 0), Quaternion.identity);
        // drop.GetComponent<NetworkObject>().SpawnWithOwnership(MyNetworkObject.NetworkObjectId);
        drop.GetComponent<DroppedItem>().SetupDroppedItem(ItemCatalogManager.Instance.GetItemById(droppedItemId));
    }
}
