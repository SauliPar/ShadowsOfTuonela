using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent Agent;
    [SerializeField] private Animator animator;
    
    [SerializeField] private HealthBarScript healthBarScript;
    
    // private Vector3 _movePosition;
    private bool _menuIsOn;

    public void Start()
    {
        if (!IsOwner) return;
        
        var cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        cinemachineCamera.LookAt = transform;
        cinemachineCamera.Follow = transform;
        
        Agent.updateRotation = false;
        Agent.angularSpeed = 0;
        
        EventManager.StartListening(Events.DamageEvent, OnDamageTaken);
    }
    
    private void Update()
    {
        if (!IsOwner) return;

        CheckMouseButtons();

        UpdateAnimator();
    }

    private void CheckMouseButtons()
    {
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
                            StartFight();
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
                        
                        // hit.transform.GetComponent<DoorScript>().ToggleServerRpc();
                    }
                    
                    MoveCharacter(hit.point);
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
    }

    private void UpdateAnimator()
    {
        Vector3 desiredVelocity = Agent.desiredVelocity;
        
        // check if we're going vertical
        if (Mathf.Abs(desiredVelocity.z) > Mathf.Abs(desiredVelocity.x))
        {
            // going north xd
            if (desiredVelocity.z > desiredVelocity.x)
            {
                animator.SetInteger("Direction", 1);
            }

            // going south xd
            if (desiredVelocity.z < desiredVelocity.x)
            {
                animator.SetInteger("Direction", 0);
            }
        }
        else
        {
            // going east xd
            if (desiredVelocity.x > desiredVelocity.z)
            {
                animator.SetInteger("Direction", 2);
            }

            // going west xd
            if (desiredVelocity.x < desiredVelocity.z)
            {
                animator.SetInteger("Direction", 3);
            }
        }
    }

    public void StartFight()
    {
        Debug.Log("you tried to fight someone else :D good for you");
    }

    public void MoveCharacter(Vector3 clickPosition)
    {
        if (!IsOwner) return;

        Agent.SetDestination(transform.position);

        // Debug.Log("painoit: " + clickPosition);
        Agent.SetDestination(clickPosition);
    }

    public void OnDamageTaken(object data)
    {
        var damageNumber = (int)data;
        
        healthBarScript.SubtractHealth(damageNumber);
    }
}
