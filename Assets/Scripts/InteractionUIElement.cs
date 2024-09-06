using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InteractionUIElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button button;
    
    public void InitializeTheElement(string elementText, UnityAction callback)
    {
        text.text = elementText;
  
        button.onClick.AddListener(() =>
        {
            callback.Invoke();
        });
    }

    public void RemoveElement()
    {
        button.onClick.RemoveAllListeners();
        Destroy(gameObject);
    }
}
