using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public Image itemIcon;
    public Button button;

    [HideInInspector]
    public Item item;

    public void InitializeElement(Item inputItem, UnityAction<InventorySlot> callback)
    {
        item = inputItem;
        
        textMesh.text = "1";
        itemIcon.sprite = item.ItemIcon;
        
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
