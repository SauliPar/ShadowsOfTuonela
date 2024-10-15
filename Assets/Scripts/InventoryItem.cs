using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public Image itemIcon;
    public Button button;
    public Button deleteButton;
    public GameObject backgroundImage;
    public GameObject DeleteParent;

    private Color _defaultBackgroundColor;

    [HideInInspector]
    public Item item;

    public int index;
    private PlayerState playerState;

    public bool ItemIsEquipped;

    public void InitializeElement(Item inputItem, PlayerState inputPlayerState, int inputIndex)
    {
        // disable the red background by default
        backgroundImage.SetActive(false);
        
        item = inputItem;
        
        textMesh.text = "1";
        itemIcon.sprite = item.ItemIcon;
        index = inputIndex;

        playerState = inputPlayerState;

        button.onClick.AddListener(OnButtonPress);
        deleteButton.onClick.AddListener(OnDeleteButtonPress);
    }

    private void OnButtonPress()
    {
        if (playerState.CombatState.Value != CombatState.Default) return;

        if (ItemIsEquipped)
        {
            playerState.UnequipItemServerRpc(index);
        }
        else
        {
            playerState.UseItemRpc(index);
        }
    }
    private void OnDeleteButtonPress()
    {
        if (playerState.CombatState.Value != CombatState.Default) return;

        ItemIsEquipped = false;
        
        playerState.TryToDeleteItemRpc(index);
    }
    

    public void EquipItem()
    {
        ItemIsEquipped = true;
        
        backgroundImage.SetActive(true);
    }

    public void UnequipItem()
    {
        ItemIsEquipped = false;
        
        backgroundImage.SetActive(false);
    }


    public void RemoveElement()
    {
        button.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        Destroy(gameObject);
    }

    public void EnableDeleteButton()
    {
        DeleteParent.SetActive(true);
    }
    public void DisableDeleteButton()
    {
        DeleteParent.SetActive(false);
    }
}
