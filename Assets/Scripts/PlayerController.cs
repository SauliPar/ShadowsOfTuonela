using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : BaseController
{
    private bool _menuIsOn;

    protected override void Start()
    {
        if (!IsOwner) return;
        
        base.Start();
        
        var cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        cinemachineCamera.LookAt = transform;
        cinemachineCamera.Follow = transform;
    }
    
    private void Update()
    {
        if (!IsOwner) return;

        CheckMouseButtons();
    }

    private void CheckMouseButtons()
    {
        if (playerState.CharacterState.Value != ControllerState.Default) return;
        
        // check left click
        if (Input.GetMouseButtonDown(0))
        {
            if (_menuIsOn)
            {
                InteractionUIMenu.Instance.HideInteractionUIMenu();
                _menuIsOn = false;
            }
            else
            {
                // Create a ray from the mouse cursor on screen in the direction of the camera
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
                // Check if the ray hits anything in the game world
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    // let's see where we hit
                    if (hit.collider.CompareTag("Player"))
                    {
                        // Is the player not us?
                        if (!hit.transform.GetComponent<NetworkObject>().IsLocalPlayer)
                        {
                            var playerNetworkObject = hit.transform.GetComponent<NetworkObject>();
                            SendPlayerCombatRequestServerRPC(GetComponent<NetworkObject>(), playerNetworkObject);
                            return;
                        }
                    }

                    if (hit.collider.CompareTag("Door"))
                    {
                        Debug.Log("osuit oveen :D");
                        
                        hit.transform.GetComponent<DoorScript>().ToggleServerRpc();
                    }
                    
                    if (hit.collider.CompareTag("DuelingNPC"))
                    {
                        Debug.Log("haastat riitaa NPC:n kanssa, aika tyylikästä :D");

                        var npcNetworkObject = hit.transform.GetComponent<NetworkObject>();
                        SendNPCCombatRequestServerRPC(GetComponent<NetworkObject>(), npcNetworkObject);

                        // hit.transform.GetComponent<DoorScript>().ToggleServerRpc();
                        return;
                    }
                    
                    Move(hit.point, false);
                }
            }
        }
        
        // check right click
        if (Input.GetMouseButtonDown(1))
        {
            // Create a ray from the mouse cursor on screen in the direction of the camera
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            // Check if the ray hits anything in the game world
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // let's see where we hit
                if (hit.collider.CompareTag("Player"))
                {
                    // Is the player not us?
                    if (!hit.transform.GetComponent<NetworkObject>().IsLocalPlayer)
                    {
              
                        Transform enemyPlayerTransform = hit.transform;
                        InteractionUIMenu.Instance.ShowInteractionUIMenu(Input.mousePosition, hit.point, this, enemyPlayerTransform);

                        _menuIsOn = true;
                        return;
                    }
                }
                
                InteractionUIMenu.Instance.ShowInteractionUIMenu(Input.mousePosition, hit.point, this);
                _menuIsOn = true;
            }
        }
        
        // UpdateAnimator();
    }

    private void UpdateAnimator(Vector3 clickPosition)
    {
        Vector3 moveVector = clickPosition - transform.position;
    
        // Normalize the moveVector to obtain the direction
        Vector3 moveDirection = moveVector.normalized;

        // check if we're going vertical
        if (Mathf.Abs(moveDirection.z) > Mathf.Abs(moveDirection.x))
        {
            // going north
            if (moveDirection.z > 0)
            {
                animator.SetInteger("Direction", 1);
            }

            // going south
            if (moveDirection.z < 0)
            {
                animator.SetInteger("Direction", 0);
            }
        }
        else
        {
            // going east
            if (moveDirection.x > 0)
            {
                animator.SetInteger("Direction", 2);
            }

            // going west
            if (moveDirection.x < 0)
            {
                animator.SetInteger("Direction", 3);
            }
        }
    }

    public override void Move(Vector3 clickPosition, bool forceMovement = false)
    {
        if (!IsOwner) return;

        if (!forceMovement)
        {
            UpdateAnimator(clickPosition);   
        }
        
        agent.SetDestination(clickPosition);
    }

    [Rpc(SendTo.Server)]
    public void SendNPCCombatRequestServerRPC(NetworkObjectReference  player1, NetworkObjectReference player2)
    {
        Debug.Log("tultiin npc sendcombatrequestiin");

        if (player1.TryGet(out NetworkObject player1NetworkObject) &&
            player2.TryGet(out NetworkObject player2NetworkObject))
        {
            if (CombatManager.Instance.CheckCombatEligibility(player1NetworkObject, player2NetworkObject))
            {
                // StartFight();
            }
        }
    }
    
    [Rpc(SendTo.Server)]
    public void SendPlayerCombatRequestServerRPC(NetworkObjectReference  player1, NetworkObjectReference player2)
    {
        Debug.Log("tultiin sendcombatrequestiin");

        if (player1.TryGet(out NetworkObject player1NetworkObject) &&
            player2.TryGet(out NetworkObject player2NetworkObject))
        {
            if (CombatManager.Instance.CheckCombatEligibility(player1NetworkObject, player2NetworkObject))
            {
                // StartFight();
            }
        }
    }
}

public enum ControllerState
{
    Default,
    Combat,
}
