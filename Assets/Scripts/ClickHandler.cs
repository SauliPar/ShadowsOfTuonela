using System;
using UnityEngine;

public class ClickHandler : MonoBehaviour
{
    [SerializeField] private Transform clickParent;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Animator animator;
    private void Start()
    {
        EventManager.StartListening(Events.Click, HandleClickEvent);
    }

    private void HandleClickEvent(object data)
    {
        if (data == null) return;
       
        var worldPosition = (Vector3)data;

        clickParent.position = worldPosition;
        
        animator.Play("ClickMoveAnimation");
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.StopListening(Events.Click, HandleClickEvent);
        }
    }

    private void Update()
    {
        // clickParent.LookAt(mainCamera.transform);
    }
}
