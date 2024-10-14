using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class CombatManager : Singleton<CombatManager>
{
    private HashSet<Combat> _combatList = new HashSet<Combat>();

    private Dictionary<ulong, float> cooldownDictionary = new Dictionary<ulong, float>();

    private void Update()
    {
        var keysToRemove = new List<ulong>();
        var keysToUpdate = new List<ulong>();

        foreach (var valuePair in cooldownDictionary)
        {
            if (valuePair.Value - Time.deltaTime <= 0)
            {
                keysToRemove.Add(valuePair.Key);
            }
            else
            {
                keysToUpdate.Add(valuePair.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            cooldownDictionary.Remove(key);
        }

        foreach (var key in keysToUpdate)
        {
            cooldownDictionary[key] -= Time.deltaTime;
        }
    }

    public void CheckCombatEligibility(NetworkObject player1, NetworkObject player2)
    {
        if (Vector3.Distance(player1.transform.position, player2.transform.position) > GlobalSettings.MaximumDuelInitiateDistance) return;
        if(cooldownDictionary.ContainsKey(player1.NetworkObjectId) || cooldownDictionary.ContainsKey(player2.NetworkObjectId)) return;
        
        // first we get playerState components
        var player1State = player1.GetComponent<PlayerState>();
        var player2State = player2.GetComponent<PlayerState>(); 
        
        // are we fighting against bot?
        if (player2State.IsBot)
        {
            // is the bot on the north side of gate?
            if(player2.transform.position.z > GlobalSettings.PVPZoneZValue) return;
        }
        // if not, then are we in the pvp zone?
        else
        {
            // is the enemy player south side of the gate?
            if (player2.transform.position.z < GlobalSettings.PVPZoneZValue) return;
        }
        
        // then we check if the combat states are "fightable"
        if (player1State.CombatState.Value != CombatState.Default) return;
        if (player2State.CombatState.Value != CombatState.Default) return;

        // Debug.Log("combat seems eligible");
        
        ForcePlayerMovement(player1State, player2State, player2.transform.position);
        
        // we create instance of combat
        var combat = new Combat(player1, player2, player1State, player2State);
        // we store the combat into a list for future use
        _combatList.Add(combat);
        // we start the combat
        combat.StartCombat();
    }

    public void RequestFlee(NetworkObject playerTryingToFlee)
    {
        Combat combatToRemove = null;

        foreach (var combat in _combatList)
        {
            if (combat.player1 == playerTryingToFlee || combat.player2 == playerTryingToFlee)
            {
                // we found a matching fight
                if (combat.CombatCoroutine != null)
                {
                    CombatManager.Instance.StopCoroutine(combat.CombatCoroutine);
                }                
                
                combat.EndCombat();

                combatToRemove = combat;
                
                cooldownDictionary.Add(combat.player1.NetworkObjectId, GlobalSettings.CombatCooldown);
                cooldownDictionary.Add(combat.player2.NetworkObjectId, GlobalSettings.CombatCooldown);
                break;
            }
        }

        if (combatToRemove != null)
        {
            _combatList.Remove(combatToRemove);
        }
    }

    private void ForcePlayerMovement(PlayerState player1State, PlayerState player2State, Vector3 fightPosition)
    {
        player1State.CombatState.Value = CombatState.Combat;
        player2State.CombatState.Value = CombatState.Combat;
        
        player1State.StartCombatRpc(fightPosition + GlobalSettings.FightInitiatorPosition, 3);
        player2State.StartCombatRpc(fightPosition + GlobalSettings.FightReceiverPosition, 2);
    }
}

public class Combat
{
    public NetworkObject player1;
    public NetworkObject player2;
    public PlayerState player1State;
    public PlayerState player2State;

    private bool _combatIsOn = false;
    private int _hitcounter;

    private int[] _player1CombatStats, _player2CombatStats;
    public Coroutine CombatCoroutine;

    public Combat(NetworkObject inputPlayer1, NetworkObject inputPlayer2, PlayerState inputPlayer1State, PlayerState inputPlayer2State)
    {
        player1 = inputPlayer1;
        player2 = inputPlayer2;

        player1State = inputPlayer1State;
        player2State = inputPlayer2State;

        _player1CombatStats = ComputePlayerCombatStats(player1State.EquippedItems);
        _player2CombatStats = ComputePlayerCombatStats(player2State.EquippedItems);
    }

    private int[] ComputePlayerCombatStats(NetworkList<int> equippedItemIds)
    {
        List<Equippable> equippedItems = new List<Equippable>();
        foreach (var itemId in equippedItemIds)
        {
            var item = (Equippable)ItemCatalogManager.Instance.GetItemById(itemId);
            equippedItems.Add(item);
        }

        int playerStr = 1, playerAtt = 1, playerDef = 1;
        foreach (var equippable in equippedItems)
        {
            playerStr += equippable.StrengthValue;
            playerAtt += equippable.AttackValue;
            playerDef += equippable.DefenseValue;
        }

        return new[] { playerStr, playerAtt, playerDef };
    }

    public void EndCombat()
    {
        // Debug.Log("ending combat");

        _combatIsOn = false;
        // CombatManager.Instance.StopCoroutine(_combatCoroutine);

        if (player1State != null)
        {
            player1State.CombatState.Value = CombatState.Default;
        }

        if (player2State != null)
        {
            player2State.CombatState.Value = CombatState.Default;
        }
    }

    public void StartCombat()
    {
        // Debug.Log("Starting combat...");

        // Debug.Log("------------------------");
        // Debug.Log("player1 stats: " + _player1CombatStats[0] + ", " + _player1CombatStats[1] + ", " + _player1CombatStats[2]);
        // Debug.Log("player2 stats: " + _player2CombatStats[0] + ", " + _player2CombatStats[1] + ", " + _player2CombatStats[2]);
        // Debug.Log("------------------------");

        CombatCoroutine = CombatManager.Instance.StartCoroutine(StartChangingNetworkVariable());
    }
    
    private IEnumerator StartChangingNetworkVariable()
    {
        int playerIndex = 0;

        PlayerState[] players = { player1State, player2State };

        _combatIsOn = true;

        while (_combatIsOn)
        {
            if (players[0].CombatState.Value == CombatState.Default ||
                players[1].CombatState.Value == CombatState.Default
                )
            {
                _combatIsOn = false;
                EndCombat();
                break;
            }
       
            playerIndex++;
            if (playerIndex >= players.Length) playerIndex = 0;

            int calculatedDamage = CalculateDamage(playerIndex);
            _combatIsOn = players[playerIndex].DecreaseHealthPoints(calculatedDamage);

            if (!_combatIsOn)
            {
                HandlePlayerDeath(playerIndex, players);
                yield break;
            }

            HitCounter();
            yield return new WaitForSeconds(1);
        }
    }

    private int CalculateDamage(int playerIndex)
    {
        int[] damageDealer = _player1CombatStats;
        int[] damageReceiver = _player2CombatStats;

        if (playerIndex == 1)
        {
            damageDealer = _player2CombatStats;
            damageReceiver = _player1CombatStats;
        }
        
        // we take str and att stats from attacker, and def value from defender
        int strengthValue = damageReceiver[0];
        int attackValue = damageReceiver[1];
        int defenseValue = damageDealer[2];
        
        float hitChance = (float)attackValue / defenseValue;
        hitChance = Mathf.Clamp(hitChance, 0.1f, 1f);
        
        // Debug.Log("hitchance: " + hitChance);
        
        var randomNumber = Random.Range(0f, 1f);
        // Debug.Log("randomNumber: " + randomNumber);

        if (randomNumber <= hitChance)
        {
            var maxDamageNumber = 3 + (strengthValue / 5);
            
            return Random.Range(1, maxDamageNumber);
        }
        
        return 0;
    }

    private void HandlePlayerDeath(int playerIndex, PlayerState[] players)
    {
        PlayerState losingPlayerState = players[playerIndex];
        PlayerState winningPlayerState = players[playerIndex==0?1:0];

        // what happens to losing side
        losingPlayerState.DeathRpc();
        losingPlayerState.ResetHealth();
        
        // what happens to winning side
        InventoryManager.Instance.HandleDroppedItemData(winningPlayerState, losingPlayerState);

        EndCombat();
    }

    private void HitCounter()
    {
        _hitcounter++;
        if (_hitcounter >= 6)
        {
            player1State.CombatState.Value = CombatState.Flee;
            player2State.CombatState.Value = CombatState.Flee;
        }
    }
}
