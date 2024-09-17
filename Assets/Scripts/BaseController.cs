using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;

public class BaseController : NetworkBehaviour
{
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Animator animator;
    [SerializeField] protected NetworkTransform networkTransform;
    [SerializeField] protected PlayerState playerState;

    // public ControllerState CharacterState;

    protected virtual void Start()
    {
        // CharacterState = playerState.CharacterState.Value;
        
        agent.updateRotation = false;
        agent.angularSpeed = 0;
    }
    
    public void StartFight(Vector3 fightPosition, int faceIndex)
    {
        // if (!IsServer) return;

        // if (IsServer)
        // {
        //     CharacterState = ControllerState.Combat;
        // }
        
        ForcePosition(fightPosition);
        ForceRotation(faceIndex);
    }
    
    protected void ForceRotation(int faceIndex)
    {
        animator.SetInteger("Direction", faceIndex);
    }

    protected void ForcePosition(Vector3 fightPosition)
    {
        // if (CharacterState != ControllerState.Combat) return;
        
        Move(fightPosition);
    }

    protected void Move(Vector3 clickPosition)
    {
        agent.SetDestination(clickPosition);
    }
    
    public void OnDeath()
    {
        // if (IsOwner)
        // {
            agent.ResetPath();
            Invoke(nameof(ResetPlayerController), 1f);
        // }
    }
    public void OnVictory()
    {
        // Debug.Log("tultiin onvictoryyn");
        
        ResetPlayerController();
    }

    protected void ResetPlayerController()
    {
        // CharacterState = ControllerState.Default;
    }

    public void TeleportCharacter(Vector3 teleportPosition)
    {
        if (IsOwner)
        {
            networkTransform.Teleport(teleportPosition, Quaternion.identity, Vector3.one);
        }
    }
}
