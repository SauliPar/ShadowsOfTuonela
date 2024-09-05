using UnityEngine;

public class InteractionUIMenu : Singleton<InteractionUIMenu>
{
    public CanvasGroup CanvasGroup;
    public RectTransform RectTransform;
    
    public void ShowInteractionUIMenu(Vector2 clickPosition)
    {
        CanvasGroup.alpha = 1f;

        RectTransform.position = clickPosition;
    }

    public void HideInteractionUIMenu()
    {
        CanvasGroup.alpha = 0f;
    }
}
