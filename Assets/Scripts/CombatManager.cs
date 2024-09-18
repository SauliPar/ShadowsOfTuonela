using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CombatManager : Singleton<CombatManager>
{
    private List<Combat> _combatList = new List<Combat>();
    public bool CheckCombatEligibility(NetworkObject player1, NetworkObject player2)
    {
        // Debug.Log("tultiin checkcombateligibilityyn");

        if (Vector3.Distance(player1.transform.position, player2.transform.position) > GlobalSettings.MaximumDuelInitiateDistance) return false;
        
        // first we get playerState components
        var player1State = player1.GetComponent<PlayerState>();
        var player2State = player2.GetComponent<PlayerState>();
        
        // then we check if the combat states are "fightable"
        if (player1State.CombatState.Value != CombatState.Default) return false;
        if (player2State.CombatState.Value != CombatState.Default) return false;

        Debug.Log("combat seems eligible");
        
        // we can use null-coalescing operator (??) to simplify conditional logic
        ForcePlayerMovement(player1State, player2State, player2.transform.position);
        
        // we create instance of combat
        var combat = new Combat(player1, player2, player1State, player2State);
        // we store the combat into a list for future use
        _combatList.Add(combat);
        // we start the combat
        combat.StartCombat();
        
        return true;
    }

    public void RequestFlee(NetworkObject playerTryingToFlee)
    {
        foreach (var combat in _combatList)
        {
            if (combat.player1 == playerTryingToFlee || combat.player2 == playerTryingToFlee)
            {
                // we found a matching fight
                combat.EndCombat();

                _combatList.Remove(combat);
                break;
            }
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

    public Combat(NetworkObject inputPlayer1, NetworkObject inputPlayer2, PlayerState inputPlayer1State, PlayerState inputPlayer2State)
    {
        player1 = inputPlayer1;
        player2 = inputPlayer2;

        player1State = inputPlayer1State;
        player2State = inputPlayer2State;
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
            
            _combatIsOn = players[playerIndex].DecreaseHealthPoints(Random.Range(0, 10));

            if (!_combatIsOn)
            {
                HandlePlayerDeath(playerIndex, players);
                yield break;
            }

            HitCounter();
            yield return new WaitForSeconds(1);
        }
    }

    private void HandlePlayerDeath(int playerIndex, PlayerState[] players)
    {
        PlayerState losingPlayerState = players[playerIndex];
        PlayerState winningPlayerState = players[playerIndex==0?1:0];

        // what happens to losing side
        losingPlayerState.DeathRpc();
        losingPlayerState.ResetHealth();
        
        // what happens to winning side

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
