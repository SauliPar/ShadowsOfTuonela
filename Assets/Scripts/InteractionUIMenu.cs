using UnityEngine;

public class InteractionUIMenu : Singleton<InteractionUIMenu>
{
    public CanvasGroup CanvasGroup;
    public RectTransform RectTransform;
    public Transform parentContainer;

    [SerializeField] private GameObject interactionUIElement;
    public void ShowInteractionUIMenu(Vector2 clickPosition)
    {
        CanvasGroup.alpha = 1f;

        RectTransform.position = clickPosition;

        InitializeInteractionUIMenu();
    }

    private void InitializeInteractionUIMenu()
    {
        var uiElement = Instantiate(interactionUIElement, parentContainer);
        uiElement.GetComponent<InteractionUIElement>().InitializeTheElement("Olutta", ButtonPressed);
    }

    public void ButtonPressed()
    {
        Debug.Log("painoit olutta :D");
    }

    public void HideInteractionUIMenu()
    {
        CanvasGroup.alpha = 0f;
    }
}
