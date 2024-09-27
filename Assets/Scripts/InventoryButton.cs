using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventoryButton : MonoBehaviour
{
    [SerializeField] private Button button;
    
    public void Initialize(Inventory inventory)
    {
        button.onClick.AddListener(inventory.ToggleInventory);
    }
}
