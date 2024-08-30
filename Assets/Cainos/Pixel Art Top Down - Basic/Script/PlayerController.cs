using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent Agent;
    
    private Vector3 _movePosition;

    public void Start()
    {
        if (!IsOwner) return;

        var cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        cinemachineCamera.LookAt = transform;
        cinemachineCamera.Follow = transform;
    }
    
    private void Update()
    {
        if (!IsOwner) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray from the mouse cursor on screen in the direction of the camera
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            // Check if the ray hits anything in the game world
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // Print the position in world space
                Debug.Log("World position: " + hit.point);
                _movePosition = hit.point;        
                MoveCharacter();
            }
        }
    }
    
    private void MoveCharacter()
    {
        Agent.SetDestination(_movePosition);
    }
}
