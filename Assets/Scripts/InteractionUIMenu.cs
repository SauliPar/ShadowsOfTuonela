using System;using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InteractionUIMenu : MonoBehaviour
{
    public CanvasGroup CanvasGroup;
    public RectTransform RectTransform;
    public Transform parentContainer;

    [SerializeField] private GameObject interactionUIElement;
    private List<InteractionUIElement> uiElements = new List<InteractionUIElement>();

    private Vector3 _lastClickPosition;

    public PlayerController PlayerController;
    private Transform _enemyPlayerTransform;
    private DroppedItem _droppedItem;

    public enum InteractionType
    {
        Walk,
        Fight,
        PickUp,
        Examine,
    }
    
    public void ShowInteractionUIMenu(
        Vector2 clickPosition, 
        Vector3 worldClickPosition, 
        Transform enemyPlayerTransform = null,
        DroppedItem droppedItem = null)
    {
        ClearUIElements();
        
        CanvasGroup.alpha = 1f;

        _lastClickPosition = worldClickPosition;
        RectTransform.position = clickPosition;
        
        if (enemyPlayerTransform != null)
        {
            _enemyPlayerTransform = enemyPlayerTransform;
        }
        else
        {
            _enemyPlayerTransform = null;
        }

        if (droppedItem != null)
        {
            _droppedItem = droppedItem;
        }
        else
        {
            _droppedItem = null;
        }

        InitializeInteractionUIMenu();
    }

    private void ClearUIElements()
    {
        foreach (var uiElement in uiElements)
        {
            uiElement.RemoveElement();
        }
        
        uiElements.Clear();
    }

    private void InitializeInteractionUIMenu()
    {
        // we gotta come up with a way to do the instantiates in an order
        // we do Movetron first
        var walkUIElement = Instantiate(interactionUIElement, parentContainer);
        var interactionUIScript = walkUIElement.GetComponent<InteractionUIElement>();
        uiElements.Add(interactionUIScript);
        
        interactionUIScript.InitializeElement("Walk here", ButtonPressed, InteractionType.Walk);

        // then we fite, for the horde!
        if (_enemyPlayerTransform)
        {
            var fightUIElement = Instantiate(interactionUIElement, parentContainer);
            var interactionUIScript2 = fightUIElement.GetComponent<InteractionUIElement>();
            uiElements.Add(interactionUIScript2);
            interactionUIScript2.InitializeElement("Fight", ButtonPressed,
                InteractionType.Fight);
        }

        if (_droppedItem)
        {
            var droppedItemUIElement = Instantiate(interactionUIElement, parentContainer);
            var interactionUIScript3 = droppedItemUIElement.GetComponent<InteractionUIElement>();
            uiElements.Add(interactionUIScript3);
            interactionUIScript3.InitializeElement("Pick up", ButtonPressed, InteractionType.PickUp);
        }
        
        // string randomWord = "Kalijjaa";
        //
        // var uiElement = Instantiate(interactionUIElement, parentContainer);
        //
        // uiElements.Add(interactionUIScript);
    }

    public void ButtonPressed(InteractionUIElement interactionUIElement)
    {
        // Debug.Log("painoit nabbia :D");

        InteractionType interactionType = interactionUIElement.InteractionType;
        
        switch (interactionType)
        {
            case InteractionType.Walk:
                PlayerController.Move(_lastClickPosition, false);
                break;
            case InteractionType.Fight:
                Debug.Log("painoit tappelua :D");
                PlayerController.SendPlayerCombatRequestServerRPC(PlayerController.PlayerNetworkObject, _enemyPlayerTransform.GetComponent<NetworkObject>());
                break;
            case InteractionType.PickUp:
                PlayerController.Move(_lastClickPosition, false);
                // PlayerController.TryToPickUpItemServerRpc(droppedItem.NetworkObject.NetworkObjectId);
                break;
            default:
                Debug.Log("ei tämmösiä oo vielä devattu hölmö :D");
                break;
        }
    }

    public void HideInteractionUIMenu()
    {
        CanvasGroup.alpha = 0f;
    }
}