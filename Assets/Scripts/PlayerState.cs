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
    
    public NetworkList<int> EquippedItems = new();

    public HealthBarScript HealthBarScript;
    public DamageTakenScript DamageTakenScript;
    public BaseController BaseController;

    public Animator Animator;
    // public GameObject DroppedItemPrefab;
    // public Item DroppedItem;
    // public NetworkObject MyNetworkObject;

    public Inventory Inventory;
    
    private Dictionary<int, Item> itemDictionary;

    public bool IsBot;

    protected override async void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();
   
        Debug.Log("tultiin playerstaten startiin");
        if (IsServer)
        {
            if (IsBot)
            {
                Debug.Log("oltiin botti :D");
                itemDictionary = await LoadBotInventory();
                foreach (var item in itemDictionary.Values)
                {
                    InventoryList.Add(item.Id);
                }
            }
            
            // load inventory from cloud, and initialize it to the list
            // ToDo: Player inventory
        }
        else
        {
            // Debug.Log("Tultiin PlayerStaten client-osioon");
        }
        
        CombatState.OnValueChanged += OnCharacterStateChanged;
        Health.OnValueChanged += OnHealthValueChanged;

        if (IsBot) return;
       
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
        if (IsBot) return;
        
        Inventory.SetupInventorySlots(InventoryList);
    }

    private Task<Dictionary<int, Item>> LoadBotInventory()
    {
        Dictionary<int, Item> newDictionary = new Dictionary<int, Item>();

        var randomDropAmount = Random.Range(1, 3);
        for (int i = 0; i < randomDropAmount; i++)
        {
            newDictionary.Add(i,
                ItemCatalogManager.Instance.GetItemById(ItemCatalogManager.Instance.GetRandomItemFromDatabase()));
            Debug.Log("lisättiin botille itemi: " + ItemCatalogManager.Instance.GetItemById(newDictionary[i].Id).ItemName);
        }
        
        return Task.FromResult(newDictionary);
    }

    private void OnCharacterStateChanged(CombatState previousvalue, CombatState newvalue)
    {
        HealthBarScript.StopInvoking();
        
        if (newvalue == global::CombatState.Combat)
        {
            HealthBarScript.Show();
        }
        if (newvalue == global::CombatState.Default)
        {
            Animator.SetTrigger(GlobalSettings.AnimationTriggers.DirChange.ToString());
        }
    }


    private void OnHealthValueChanged(int previousvalue, int newvalue)
    {
        // HealthBarScript.StopInvoking();
        
        var substractValue = previousvalue - newvalue;
        HealthBarScript.SetHealthBarValue(newvalue);
        
        if (substractValue < 0) return;
       
        DamageTakenScript.ShowDamage(substractValue);

        // HealthBarScript.StartHiding();
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
    
    [Rpc(SendTo.Owner)]
    public void DeathRpc()
    {
        if (IsOwner)
        {
            if (IsBot)
            {
                BaseController.OnDeathNpc();
                // BaseController.TeleportCharacter(Vector3.zero);
            }
            else
            {
                BaseController.OnDeath();
                BaseController.TeleportCharacter(Vector3.zero);
            }
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
        if (CombatState.Value != global::CombatState.Default) return;
        
        if (index >= 0 && index < InventoryList.Count)
        {
            var item = ItemCatalogManager.Instance.GetItemById(InventoryList[index]);

            switch (item.ItemType)
            {
                case ItemType.Consumable:
                    var consumableItem = (Consumable)item;
                    Health.Value += consumableItem.HealValue;
                    InventoryList.RemoveAt(index);
                    break;
                case ItemType.Equipment:
                    EquipItemCheck(index);
                    // ItemIsEligibleToUseRpc(index);
                    break;
            }
        }
        else
        {
            // ban player for cheating inventory :D
        }
    }

    private void EquipItemCheck(int index)
    {
        // first we see what item we're trying to equip
        var itemToEquip = ItemCatalogManager.Instance.GetItemById(InventoryList[index]);

        if (EquippedItems.Count > 0)
        {
            EquippedItems.Clear();
        }   
    
        EquippedItems.Add(itemToEquip.Id);
        
        ItemIsEligibleToUseRpc(index);
    }

    [Rpc(SendTo.Owner)]
    private void ItemIsEligibleToUseRpc(int index)
    {
        Inventory.EquipItem(index);
    }

    [Rpc(SendTo.Owner)]
    public void DropItemToPlayerRpc(int droppedItemId)
    {
        // var drop = Instantiate(DroppedItemPrefab, transform.position - new Vector3(0, 0.5f, 0), Quaternion.identity);
        // // drop.GetComponent<NetworkObject>().SpawnWithOwnership(MyNetworkObject.NetworkObjectId);
        // drop.GetComponent<DroppedItem>().SetupDroppedItem(ItemCatalogManager.Instance.GetItemById(droppedItemId));
    }
}
