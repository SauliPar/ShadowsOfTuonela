using System;using System.Collections.Generic;
using UnityEngine;

public class InteractionUIMenu : Singleton<InteractionUIMenu>
{
    public CanvasGroup CanvasGroup;
    public RectTransform RectTransform;
    public Transform parentContainer;

    [SerializeField] private GameObject interactionUIElement;
    private List<InteractionUIElement> uiElements = new List<InteractionUIElement>();

    private Vector3 _lastClickPosition;

    public InteractionType MyInteractionType;
    private PlayerController _controller;

    public enum InteractionType
    {
        Walk,
        Fight,
        PickUp,
        Examine,
    }
    
    public void ShowInteractionUIMenu(Vector2 clickPosition, Vector3 worldClickPosition, PlayerController controller)
    {
        ClearUIElements();
        
        CanvasGroup.alpha = 1f;

        _lastClickPosition = worldClickPosition;
        RectTransform.position = clickPosition;

        _controller = controller;

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
        
        interactionUIScript.InitializeTheElement("Walk here", ButtonPressed, InteractionType.Walk);


        
        // string randomWord = "Kalijjaa";
        //
        // var uiElement = Instantiate(interactionUIElement, parentContainer);
        //
        // uiElements.Add(interactionUIScript);
    }

    public void ButtonPressed(InteractionUIElement interactionUIElement)
    {
        Debug.Log("painoit nabbia :D");

        InteractionType interactionType = interactionUIElement.InteractionType;
        
        switch (interactionType)
        {
            case InteractionType.Walk:
                _controller.MoveCharacter(_lastClickPosition);
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