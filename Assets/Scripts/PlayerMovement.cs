using System;
using Fusion;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : NetworkBehaviour
{
    private Vector3 _velocity;
    private bool _jumpPressed;

    private CharacterController _controller;

    public float PlayerSpeed = 2f;

    public float JumpForce = 5f;
    public float GravityValue = -9.81f;

    public NavMeshAgent Agent;
    private Vector3 _movePosition;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            var cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            cinemachineCamera.LookAt = transform;
            cinemachineCamera.Follow = transform;
        }
    }
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (HasStateAuthority == false)
        {
            return;
        }
        
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

    public override void FixedUpdateNetwork()
    {
        // Only move own player and not every other player. Each player controls its own player object.
       
        
        // Check for left mouse button press
 
    }

    private void MoveCharacter()
    {
        // Vector3 moveValue = clickPosition * Runner.DeltaTime * PlayerSpeed;

        Agent.SetDestination(_movePosition);

        // _controller.Move(moveValue);
    }
}