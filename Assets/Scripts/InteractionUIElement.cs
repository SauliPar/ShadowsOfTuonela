using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InteractionUIElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button button;
    public InteractionUIMenu.InteractionType InteractionType;
    
    public void InitializeElement(string elementText, UnityAction<InteractionUIElement> callback, InteractionUIMenu.InteractionType interactionType)
    {
        text.text = elementText;
        InteractionType = interactionType;
  
        button.onClick.AddListener(() =>
        {
            callback.Invoke(this);
        });
    }

    public void RemoveElement()
    {
        button.onClick.RemoveAllListeners();
        Destroy(gameObject);
    }
}
