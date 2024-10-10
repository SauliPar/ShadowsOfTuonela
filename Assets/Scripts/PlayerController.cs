using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : BaseController
{
    private InventoryManager inventoryManager;
    public InteractionUIMenu InteractionUIMenu;
    public Collider PlayerCollider;
    public GameObject PlayerCanvasTransform;

    private Transform _playerTransform;
    private bool _menuIsOn;
    
    private float _timer = .5f;
    private float _timeOut = .5f;

    public bool IsRespawning;
    
    protected override void Start()
    {
        PlayerCanvasTransform.SetActive(false);
        
        if (!IsOwner) return;
        
        base.Start();

        SetupComponents();
    }

    private void SetupComponents()
    {
        PlayerCanvasTransform.SetActive(true);

        PlayerCollider.enabled = false;
        _playerTransform = transform;
        var cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        cinemachineCamera.LookAt = _playerTransform;
        cinemachineCamera.Follow = _playerTransform;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (IsRespawning) return;
        
        HandleInputs();
        
        base.Update();
    }

    private void HandleInputs()
    {
        switch (playerState.CombatState.Value)
        {
            case CombatState.Combat:
                return;
            case CombatState.Default:
                DoDefaultStuff();
                break;
            case CombatState.Flee:
                UpdateTimeoutTimer();
                DoFleeStuff();
                break;
        }
    }

    private void UpdateTimeoutTimer()
    {
        _timer += Time.deltaTime;
    }

    private void DoFleeStuff()
    {
        if (Input.GetMouseButton(0))
        {
            if(_timer < _timeOut) return;

            var currentGO = CurrentInput.GameObjectUnderPointer();
            if (currentGO != null && currentGO.layer == LayerMask.NameToLayer("UI")) return;
            
            SendFleeRequestToServerRpc(PlayerNetworkObject);

            _timer = 0f;
        }
    }

    private void DoDefaultStuff()
    {
        HandleLeftClick();
        HandleRightClick();
    }
    
    private void HandleLeftClick()
    {
        // check left click
        if (!Input.GetMouseButtonDown(0)) return;
        
        if (_menuIsOn)
        {
            InteractionUIMenu.HideInteractionUIMenu();
            _menuIsOn = false;
            return;
        }
        
        // Create a ray from the mouse cursor on screen in the direction of the camera
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    
        var currentGO = CurrentInput.GameObjectUnderPointer();
        if (currentGO != null && currentGO.layer == LayerMask.NameToLayer("UI")) return;
        
        // Check if the ray hits anything in the game world
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // Debug.Log("collider peliobjectin nimi: " + hit.collider.gameObject.name);
            if (hit.collider.CompareTag("DroppedItem"))
            {
                if (Vector3.Distance(transform.position, hit.collider.transform.position) < GlobalSettings.MaximumLootDistance)
                {
                    var droppedItem = hit.collider.GetComponent<DroppedItem>();
                    if (droppedItem != null)
                    {
                        // Handle interaction with dropped item
                        TryToPickUpItemServerRpc(droppedItem.NetworkObject.NetworkObjectId, PlayerNetworkObject);
                    }
                }
            }
            
            // let's see where we hit
            if (hit.collider.CompareTag("Player"))
            {
                // Is the player not us?
                if (!hit.transform.GetComponent<NetworkObject>().IsLocalPlayer)
                {
                    var playerNetworkObject = hit.transform.GetComponent<NetworkObject>();
                    SendPlayerCombatRequestServerRPC(PlayerNetworkObject, playerNetworkObject);
                    return;
                }
            }

            if (hit.collider.CompareTag("Door"))
            {
                hit.transform.GetComponent<DoorScript>().ToggleServerRpc();
            }
            
            if (hit.collider.CompareTag("DuelingNPC"))
            {
                var npcNetworkObject = hit.transform.GetComponent<NetworkObject>();
                SendNPCCombatRequestServerRPC(GetComponent<NetworkObject>(), npcNetworkObject);

                return;
            }
            
            Move(hit.point, false);
        }
    }

    private void HandleRightClick()
    {
        // check right click
        if (!Input.GetMouseButtonDown(1)) return;
        
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
                    InteractionUIMenu.ShowInteractionUIMenu(Input.mousePosition, hit.point, enemyPlayerTransform);

                    _menuIsOn = true;
                    return;
                }
            }
            if (hit.collider.CompareTag("DuelingNPC"))
            {
                Transform enemyPlayerTransform = hit.transform;
                InteractionUIMenu.ShowInteractionUIMenu(Input.mousePosition, hit.point, enemyPlayerTransform);

                _menuIsOn = true;
                return;
            }
            if (hit.collider.CompareTag("DroppedItem"))
            {
                var droppedItem = hit.collider.GetComponent<DroppedItem>();
                if (droppedItem != null)
                {
                    // Handle interaction with dropped item
                    // For example, pick up the item
                    InteractionUIMenu.ShowInteractionUIMenu(Input.mousePosition, hit.point, null, droppedItem);
                    _menuIsOn = true;
                    return;
                }
            }
            
            InteractionUIMenu.ShowInteractionUIMenu(Input.mousePosition, hit.point);
            _menuIsOn = true;
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
        if (player1.TryGet(out NetworkObject player1NetworkObject) &&
            player2.TryGet(out NetworkObject player2NetworkObject))
        {
            CombatManager.Instance.CheckCombatEligibility(player1NetworkObject, player2NetworkObject);
        }
    }
    
    [Rpc(SendTo.Server)]
    public void SendPlayerCombatRequestServerRPC(NetworkObjectReference player1, NetworkObjectReference player2)
    {
        if (player1.TryGet(out NetworkObject player1NetworkObject) &&
            player2.TryGet(out NetworkObject player2NetworkObject))
        {
            CombatManager.Instance.CheckCombatEligibility(player1NetworkObject, player2NetworkObject);
        }
    }
    
    [Rpc(SendTo.Server)]
    private void SendFleeRequestToServerRpc(NetworkObjectReference playerTryingToFlee)
    {
        if (playerTryingToFlee.TryGet(out NetworkObject networkObjectTryingToFlee))
        {
            CombatManager.Instance.RequestFlee(networkObjectTryingToFlee);
        }
    }
    
    [Rpc(SendTo.Server)]
    public void TryToPickUpItemServerRpc(ulong droppedItemId, NetworkObjectReference playerPickingUp)
    {
        if (playerPickingUp.TryGet(out NetworkObject playerNetworkObject))
        {
            InventoryManager.Instance.TryToPickUpItem(droppedItemId, playerNetworkObject);
        }
    }
    
    private static StandaloneInputModuleV2 currentInput;

    private StandaloneInputModuleV2 CurrentInput
    {
        get
        {
            if (currentInput == null)
            {
                currentInput = EventSystem.current.currentInputModule as StandaloneInputModuleV2;
                if (currentInput == null)
                {
                    Debug.LogError("Missing StandaloneInputModuleV2.");
                }
            }

            return currentInput;
        }
    }
}
