using System.Collections;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class CombatManager : Singleton<CombatManager>
{
    private const float maximumDuelInitiateDistance = 2f;
    private Vector3 _fightInitiatorPosition = new Vector3(2, 0 ,0);
    private Vector3 _fightReceiverPosition = new Vector3(-2, 0 ,0);
    
    public bool CheckCombatEligibility(NetworkObject player1, NetworkObject player2, bool isPlayer)
    {
        Debug.Log("tultiin checkcombateligibilityyn");

        if (Vector3.Distance(player1.transform.position, player2.transform.position) >
            maximumDuelInitiateDistance) return false;

        // player1 components
        var player1Controller = player1.GetComponent<PlayerController>();
        var player1State = player1Controller.MyPlayerState;
        
        // check if player1 is ready for combat
        if (player1State != ControllerState.Default) return false;

        PlayerController player2Controller = null;
        BotMovementScript npcController = null;
        var player2State = ControllerState.Default;

        // player2 components
        if (isPlayer)
        {
            player2Controller = player2.GetComponent<PlayerController>();
            player2State = player2Controller.MyPlayerState;

            // check if player2 is ready for combat
        }
        else
        {
            npcController = player2.GetComponent<BotMovementScript>();
            player2State = npcController.MyNPCState;
        }
        
        if (player2State != ControllerState.Default) return false;
        
        Debug.Log("combat seems eligible");
        
        if (isPlayer)
        {
            ForcePlayerControllers(player1Controller, player2Controller, player2.transform.position);
        }
        else
        {
            ForcePlayerAndNPCControllers(player1Controller, npcController, player2.transform.position);
        }
        
        // we create instance of combat
        Combat combat = new Combat(player1, player2);
        combat.StartCombat();
        
        return true;
    }

    private void ForcePlayerControllers(PlayerController player1Controller, PlayerController player2Controller, Vector3 fightPosition)
    {
        player1Controller.StartFight(fightPosition + _fightInitiatorPosition, 3);
        player2Controller.StartFight(fightPosition + _fightReceiverPosition, 2);
    }
    private void ForcePlayerAndNPCControllers(PlayerController player1Controller, BotMovementScript botMovementScript, Vector3 fightPosition)
    {
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

    public Combat(NetworkObject inputPlayer1, NetworkObject inputPlayer2)
    {
        Debug.Log("tehtiin uusi combat");

        player1 = inputPlayer1;
        player2 = inputPlayer2;

        player1State = inputPlayer1.GetComponent<PlayerState>();
        player2State = inputPlayer2.GetComponent<PlayerState>();;
    }

    public void StartCombat()
    {
        Debug.Log("starting combat");

        player1.StartCoroutine(StartChangingNetworkVariable());
    }
    
    private IEnumerator StartChangingNetworkVariable()
    {
        Debug.Log("aloitettiin combat coroutine");
        var count = 0;
        var updateFrequency = new WaitForSeconds(2f);
        while (count < 4)
        {
            player1State.Health.Value -= Random.Range(0, 10);
            player2State.Health.Value -= Random.Range(0, 10);
            yield return updateFrequency;
        }
    }
}
