using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent Agent;
    [SerializeField] private Animator animator;
    
    
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
                    // Print the position in world space
                    // Debug.Log("World position: " + hit.point);
                    // _movePosition = hit.point;        
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
                // Print the position in world space
                // Debug.Log("Click position: " + hit.point);
                
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

    public void MoveCharacter(Vector3 clickPosition)
    {
        if (!IsOwner) return;

        // Debug.Log("painoit: " + clickPosition);
        Agent.SetDestination(clickPosition);
    }
}
