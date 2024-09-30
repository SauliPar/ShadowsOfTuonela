using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CombatManager : Singleton<CombatManager>
{
    private HashSet<Combat> _combatList = new HashSet<Combat>();
   
    public void CheckCombatEligibility(NetworkObject player1, NetworkObject player2)
    {
        if (Vector3.Distance(player1.transform.position, player2.transform.position) > GlobalSettings.MaximumDuelInitiateDistance) return;
        if (player2.transform.position.x > GlobalSettings.SafeZoneXValue) return;
        
        // first we get playerState components
        var player1State = player1.GetComponent<PlayerState>();
        var player2State = player2.GetComponent<PlayerState>();
        
        // then we check if the combat states are "fightable"
        if (player1State.CombatState.Value != CombatState.Default) return;
        if (player2State.CombatState.Value != CombatState.Default) return;

        Debug.Log("combat seems eligible");
        
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
                combat.EndCombat();

                combatToRemove = combat;
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
    public PlayerStatistics player1Statistics;
    public PlayerStatistics player2Statistics;
    
    private bool _combatIsOn = false;
    private int _hitcounter;

    public Combat(NetworkObject inputPlayer1, NetworkObject inputPlayer2, PlayerState inputPlayer1State, PlayerState inputPlayer2State)
    {
        player1 = inputPlayer1;
        player2 = inputPlayer2;

        player1State = inputPlayer1State;
        player2State = inputPlayer2State;

        player1Statistics = inputPlayer1.GetComponent<PlayerStatistics>();
        player2Statistics = inputPlayer2.GetComponent<PlayerStatistics>();
    }

    public void EndCombat()
    {
        Debug.Log("ending combat");

        _combatIsOn = false;
        player1.StopCoroutine(StartChangingNetworkVariable());
        
        player1State.CombatState.Value = CombatState.Default;
        player2State.CombatState.Value = CombatState.Default;
    }

    public void StartCombat()
    {
        // Debug.Log("Starting combat...");

        player1.StartCoroutine(StartChangingNetworkVariable());
    }
    
    private IEnumerator StartChangingNetworkVariable()
    {
        int playerIndex = 0;

        PlayerState[] players = { player1State, player2State };

        _combatIsOn = true;

        while (_combatIsOn)
        {
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
        int defenseValue = 1;
        int attackValue = 1;
        int strengthValue = 1;
        
        if (playerIndex == 0)
        {
            defenseValue = player1Statistics.Defense;
            attackValue = player2Statistics.Attack;
            strengthValue = player2Statistics.Strength;
        }
        else if (playerIndex == 1)
        {
            defenseValue = player2Statistics.Defense;
            attackValue = player1Statistics.Attack;
            strengthValue = player1Statistics.Strength;
        }

        // Debug.Log("str_att_def: " + strengthValue + ", " + attackValue + ", " + defenseValue );

        float hitChance = (float)attackValue / defenseValue;
        hitChance = Mathf.Clamp(hitChance, 0.1f, 1f);
        
        // Debug.Log("hitchance: " + hitChance);


        var randomNumber = Random.Range(0f, 1f);
        // Debug.Log("randomNumber: " + randomNumber);

        if (randomNumber <= hitChance)
        {
            var maxDamageNumber = 5 + (strengthValue / 5);
            
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
        InventoryManager.Instance.HandleDroppedItemData(winningPlayerState.GetComponent<NetworkObject>(), winningPlayerState);

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
