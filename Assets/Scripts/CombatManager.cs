using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CombatManager : Singleton<CombatManager>
{
    private const float maximumDuelInitiateDistance = 2f;
    
    public bool CheckCombatEligibility(NetworkObject player1, NetworkObject player2)
    {
        Debug.Log("tultiin checkcombateligibilityyn");

        if (Vector3.Distance(player1.transform.position, player2.transform.position) >
            maximumDuelInitiateDistance) return false;

        var player1ControllerState = player1.GetComponent<PlayerController>().MyPlayerState;
        var player2ControllerState = PlayerController.ControllerState.Default;

        if (player1ControllerState != PlayerController.ControllerState.Default ||
            player2ControllerState != PlayerController.ControllerState.Default) return false;

        Debug.Log("combat seems eligible");
        
        // we create instance of combat
        Combat combat = new Combat(player1, player2);
        combat.StartCombat();

        return true;
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
