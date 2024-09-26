using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [HideInInspector]
    public Item item;
    
    public void SetupDroppedItem(Item inputItem)
    {
        item = inputItem;
        spriteRenderer.sprite = inputItem.ItemIcon;
    }

    public Item PickUpItem()
    {
        return item;
    }
}
