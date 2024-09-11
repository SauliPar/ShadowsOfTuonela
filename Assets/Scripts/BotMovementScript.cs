using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class BotMovementScript : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent Agent;
    [SerializeField] private Animator animator;
    
    public ControllerState MyNPCState;

    private void Start()
    {
        MyNPCState = ControllerState.Default;
        
        Agent.updateRotation = false;
        Agent.angularSpeed = 0;
    }
    
    public void StartFight(Vector3 fightPosition, int faceIndex)
    {
        if (!IsServer) return;

        MyNPCState = ControllerState.Combat;
        
        ForceNPCPosition(fightPosition);
        ForceNPCRotation(faceIndex);
    }
    
    private void ForceNPCRotation(int faceIndex)
    {
        animator.SetInteger("Direction", faceIndex);
    }

    private void ForceNPCPosition(Vector3 fightPosition)
    {
        if (MyNPCState != ControllerState.Combat) return;
        
        MoveNPC(fightPosition);
    }

    public void MoveNPC(Vector3 clickPosition)
    {
        // Agent.SetDestination(transform.position);
        // Debug.Log("NPC painoi paikassa 2: " + clickPosition);
        
        Agent.SetDestination(clickPosition);
    }
}
