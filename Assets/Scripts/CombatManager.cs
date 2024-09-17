using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CombatManager : Singleton<CombatManager>
{
    private const float MaximumDuelInitiateDistance = 2f;
    private Vector3 _fightInitiatorPosition = new Vector3(1, 0 ,0);
    private Vector3 _fightReceiverPosition = new Vector3(-1, 0 ,0);

    public bool CheckCombatEligibility(NetworkObject player1, NetworkObject player2)
    {
        Debug.Log("tultiin checkcombateligibilityyn");

        if (Vector3.Distance(player1.transform.position, player2.transform.position) > MaximumDuelInitiateDistance) return false;
        
        // player1 components
        var player1Controller = player1.GetComponent<PlayerController>();
        if (player1Controller.CharacterState != ControllerState.Default) return false;

        // player2 components
        var player2Controller = player2.GetComponent<BaseController>();
        if (player2Controller.CharacterState != ControllerState.Default) return false;

        Debug.Log("combat seems eligible");
        
        // we can use null-coalescing operator (??) to simplify conditional logic
        ForceControllers(player1Controller, player2Controller, player2.transform.position);
        
        // we create instance of combat
        var combat = new Combat(player1, player2);
        combat.StartCombat();
        
        return true;
    }

    private void ForceControllers(BaseController player1Controller, BaseController player2Controller, Vector3 fightPosition)
    {
        // Debug.Log("player1 posi paikassa 1: " + fightPosition + _fightInitiatorPosition);
        // Debug.Log("player2 posi paikassa 1: " + fightPosition + _fightReceiverPosition);
        
        player1Controller.StartFight(fightPosition + _fightInitiatorPosition, 3);
        player2Controller.StartFight(fightPosition + _fightReceiverPosition, 2);
    }
    private void ForcePlayerAndNPCControllers(PlayerController player1Controller, BotMovementScript botMovementScript, Vector3 fightPosition)
    {
        // Debug.Log("player1 posi paikassa 1: " + fightPosition + _fightInitiatorPosition);
        // Debug.Log("player2 posi paikassa 1: " + fightPosition + _fightReceiverPosition);
        
        player1Controller.StartFight(fightPosition + _fightInitiatorPosition, 3);
        botMovementScript.StartFight(fightPosition + _fightReceiverPosition, 2);
    }
}

public class Combat
{
    public NetworkObject player1;
    public NetworkObject player2;
    public PlayerState player1State;
    public PlayerState player2State;
    
    private bool didAnyPlayerDie = false;

    public Combat(NetworkObject inputPlayer1, NetworkObject inputPlayer2)
    {
        // Debug.Log("tehtiin uusi combat");

        player1 = inputPlayer1;
        player2 = inputPlayer2;

        player1State = inputPlayer1.GetComponent<PlayerState>();
        player2State = inputPlayer2.GetComponent<PlayerState>();
    }

    public void StartCombat()
    {
        Debug.Log("Starting combat...");

        player1.StartCoroutine(StartChangingNetworkVariable());
    }
    
    private IEnumerator StartChangingNetworkVariable()
    {
        Debug.Log("Starting combat coroutine");
        bool playerDied = false;

        List<PlayerState> players = new List<PlayerState>();
        players.Add(player1State);
        players.Add(player2State);
        int playerIndex = 0;

        while (!playerDied)
        {
            playerIndex++;
            if (playerIndex > 1) playerIndex = 0;
            
            playerDied = players[playerIndex].DecreaseHealthPoints(Random.Range(0, 10));
        
            yield return new WaitForSeconds(1);
        }
        
        // Debug.Log("häviäjän playerindex oli " + playerIndex);


        if (playerIndex == 0)
        {
            player1.GetComponent<BaseController>().OnDeath();
            player1State.DeathRpc();
            player1State.ResetHealth();
            player2.GetComponent<BaseController>().OnVictory();
        }
        else if (playerIndex == 1)
        {
            player1.GetComponent<BaseController>().OnVictory();
            player2State.ResetHealth();
            player2State.DeathRpc();
            player2.GetComponent<BaseController>().OnDeath();
            // player2.GetComponent<BaseController>().TeleportCharacter(Vector3.zero);
        }

        // player1State.IsDead.Value = false;
        // player2State.IsDead.Value = false;
    }
}
