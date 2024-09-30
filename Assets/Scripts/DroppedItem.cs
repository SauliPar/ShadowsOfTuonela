using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class DroppedItem : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private NetworkObject networkObject;
    [SerializeField] private Collider itemCollider;
    
    // [HideInInspector]
    public NetworkVariable<int> itemId = new NetworkVariable<int>();
    

    public void SetupDroppedItem(int inputItemId)
    {
        itemId.Value = inputItemId;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            spriteRenderer.sprite = ItemCatalogManager.Instance.GetItemById(itemId.Value).ItemIcon;
            itemCollider.enabled = true;
        }
    }
}
