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
    private int _previousDirectionValue;

    // Declare a delegate
    public delegate void NpcDeathDelegate();

    // Declare an event of type NpcDeathDelegate
    public event NpcDeathDelegate OnNpcDeath;

    protected virtual void Update()
    {
        animator.SetFloat("Speed", Mathf.Clamp(agent.velocity.magnitude, 0, 1f));
    }

    protected virtual void Start()
    {
        agent.updateRotation = false;
        agent.angularSpeed = 0;
    }
    
    public void StartFight(Vector3 fightPosition, int faceIndex)
    {
        ForcePosition(fightPosition);
        StartCombatAnimation(faceIndex);
    }
    
    protected void StartCombatAnimation(int faceIndex)
    {
        animator.SetInteger("Direction", faceIndex);

        var equippedItems = playerState.EquippedItems;
        Equippable equippedWeapon = null;
        foreach (var item in equippedItems)
        {
            Equippable myItem = (Equippable)ItemCatalogManager.Instance.GetItemById(item);
            if (myItem.EquipType == EquipType.Weapon)
            {
                equippedWeapon = myItem;
                break;
            }
        }

        if (equippedWeapon == null) animator.SetTrigger(GlobalSettings.AnimationTriggers.FistFightTrigger.ToString());
        else animator.SetTrigger(equippedWeapon.AnimationTrigger.ToString());
    }

    protected void ForcePosition(Vector3 fightPosition)
    {
        Move(fightPosition, true);
    }

    public virtual void Move(Vector3 clickPosition, bool forceMovement = false)
    {
        UpdateAnimator(clickPosition);
        agent.SetDestination(clickPosition);
    }
    
    public virtual void UpdateAnimator(Vector3 clickPosition)
    {
        Vector3 moveVector = clickPosition - transform.position;
        int directionValue = 0;
    
        // Normalize the moveVector to obtain the direction
        Vector3 moveDirection = moveVector.normalized;

        // check if we're going vertical
        if (Mathf.Abs(moveDirection.z) > Mathf.Abs(moveDirection.x))
        {
            // going north
            if (moveDirection.z > 0)
            {
                directionValue = 1;
            }

            // going south
            if (moveDirection.z < 0)
            {
                directionValue = 0;
            }
        }
        else
        {
            // going east
            if (moveDirection.x > 0)
            {
                directionValue = 2;
            }

            // going west
            if (moveDirection.x < 0)
            {
                directionValue = 3;
            }
        }

        if (directionValue != _previousDirectionValue)
        {
            
            animator.SetInteger("Direction", directionValue);
            _previousDirectionValue = directionValue;
            
            animator.SetTrigger("DirChange");
        }
    }
    
    public void OnDeath()
    {
        agent.ResetPath();
    }

    public void OnDeathNpc()
    {
        if (IsServer)
        {
            OnNpcDeath?.Invoke();

            PlayerNetworkObject.Despawn();
        }
    }

    public void TeleportCharacter(Vector3 teleportPosition)
    {
        if (IsOwner)
        {
            networkTransform.Teleport(teleportPosition, Quaternion.identity, Vector3.one);
        }
    }
}
