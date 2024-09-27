using Unity.Netcode;
using UnityEngine;

public class DroppedItem : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private NetworkObject networkObject;
    
    [HideInInspector]
    public Item item;
    
    public void SetupDroppedItem(Item inputItem)
    {
        item = inputItem;
        spriteRenderer.sprite = inputItem.ItemIcon;
    }

    public void PickUpItem()
    {
        Destroy(gameObject);
    }
}
