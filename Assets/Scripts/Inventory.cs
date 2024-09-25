using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    private bool _inventoryOn;
    
    public void ToggleInventory()
    {
        _inventoryOn = !_inventoryOn;

        if (_inventoryOn)     
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            canvasGroup.alpha = 0f;
        }
    }
}
