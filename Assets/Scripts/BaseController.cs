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
    
    // Declare a delegate
    public delegate void NpcDeathDelegate();

    // Declare an event of type NpcDeathDelegate
    public event NpcDeathDelegate OnNpcDeath;
    
    
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
        agent.SetDestination(clickPosition);
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
