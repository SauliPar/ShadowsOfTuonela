using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
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
    [Header("Network Variables")]
    public NetworkVariable<int> Health = new NetworkVariable<int>(GlobalSettings.DefaultHealth);
    public NetworkVariable<CombatState> CombatState = new();
    public NetworkList<int> InventoryList = new();
    public NetworkList<int> EquippedItems = new();
    public NetworkVariable<FixedString128Bytes> PlayerTag = new NetworkVariable<FixedString128Bytes>();

    [Header("Components")]
    public HealthBarScript HealthBarScript;
    public DamageTakenScript DamageTakenScript;
    public BaseController BaseController;
    public Animator Animator;
    public Inventory Inventory;
    public TextMeshProUGUI PlayerTagComponent;
    public RespawnHandler RespawnHandler;
    
    private Dictionary<int, int> itemDictionary;
    public bool IsBot;

    protected override async void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();
   
        // Debug.Log("tultiin playerstaten startiin");
        if (IsServer)
        {
            var fixedString = new FixedString128Bytes("wiedenluch");
            PlayerTag.Value = fixedString;
            
            if (IsBot)
            {
                itemDictionary = await LoadBotInventory();
                foreach (var item in itemDictionary.Values)
                {
                    InventoryList.Add(item);
                }
            }
            
            // load inventory from cloud, and initialize it to the list
            // ToDo: Player inventory
        }
        else
        {
            if (!IsBot)
            {
                PlayerTagComponent.text = PlayerTag.Value.ToString();
            }
        }
        
        CombatState.OnValueChanged += OnCharacterStateChanged;
        Health.OnValueChanged += OnHealthValueChanged;
        
        if (IsBot) return;
       
        PlayerTag.OnValueChanged += OnPlayerTagChanged;
        InventoryList.OnListChanged += OnInventoryListChanged;
    }

    private void OnPlayerTagChanged(FixedString128Bytes previousvalue, FixedString128Bytes newvalue)
    {
        PlayerTagComponent.text = newvalue.ToString();
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

    private Task<Dictionary<int, int>> LoadBotInventory()
    {
        Dictionary<int, int> newDictionary = new Dictionary<int, int>();

        var randomDropAmount = 1;
        for (int i = 0; i < randomDropAmount; i++)
        {
            var randomItemIndex =
                ItemCatalogManager.Instance.GetRandomItemWithDropChances();

            // we do this if we returned nothing
            if (randomItemIndex < 1)
            {
                return Task.FromResult(newDictionary);
            }
            
            newDictionary.Add(i, randomItemIndex);
            // Debug.Log("lisättiin botille itemi: " + ItemCatalogManager.Instance.GetItemById(newDictionary[i]).ItemName);
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

    private void HealPlayer(Consumable healItem)
    {
        if (!IsServer) return;
        if (Health.Value >= GlobalSettings.DefaultHealth) return;

        Health.Value = Math.Min(Health.Value + healItem.HealValue, GlobalSettings.DefaultHealth);
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
                RespawnHandler.ShowRespawnCanvas();
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
                    HealPlayer(consumableItem);
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

    [Rpc(SendTo.Server)]
    public void ChangeNameTagServerRPC(FixedString128Bytes inputString)
    {
        PlayerTag.Value = inputString;
    }
}
