using System.Collections.Generic;
using UnityEngine;

public class InteractionUIMenu : Singleton<InteractionUIMenu>
{
    public CanvasGroup CanvasGroup;
    public RectTransform RectTransform;
    public Transform parentContainer;

    [SerializeField] private GameObject interactionUIElement;
    private List<InteractionUIElement> uiElements = new List<InteractionUIElement>();
    public void ShowInteractionUIMenu(Vector2 clickPosition)
    {
        ClearUIElements();
        

        CanvasGroup.alpha = 1f;

        RectTransform.position = clickPosition;

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
        string randomWord = "Kalijjaa";

        var uiElement = Instantiate(interactionUIElement, parentContainer);
        var interactionUIScript = uiElement.GetComponent<InteractionUIElement>();
        interactionUIScript.InitializeTheElement(randomWord, ButtonPressed);
        
        uiElements.Add(interactionUIScript);
    }

    public void ButtonPressed()
    {
        Debug.Log("painoit nabbia :D");
    }

    public void HideInteractionUIMenu()
    {
        CanvasGroup.alpha = 0f;
    }
}
