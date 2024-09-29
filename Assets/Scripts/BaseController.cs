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
    public NetworkObject PlayerNetworkObject;
    
    protected virtual void Start()
    {
        agent.updateRotation = false;
        agent.angularSpeed = 0;
    }
    
    public void StartFight(Vector3 fightPosition, int faceIndex)
    {
        ForcePosition(fightPosition);
        ForceRotation(faceIndex);
    }
    
    protected void ForceRotation(int faceIndex)
    {
        animator.SetInteger("Direction", faceIndex);
    }

    protected void ForcePosition(Vector3 fightPosition)
    {
        Move(fightPosition, true);
    }

    public virtual void Move(Vector3 clickPosition, bool forceMovement = false)
    {
        agent.SetDestination(clickPosition);
    }
    
    public void OnDeath()
    {
        agent.ResetPath();
    }

    public void TeleportCharacter(Vector3 teleportPosition)
    {
        if (IsOwner)
        {
            networkTransform.Teleport(teleportPosition, Quaternion.identity, Vector3.one);
        }
    }
}
