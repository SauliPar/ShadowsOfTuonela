using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public Image itemIcon;
    public Button button;

    [HideInInspector]
    public Item item;

    public int index;
    private PlayerState playerState;

    public void InitializeElement(Item inputItem, PlayerState inputPlayerState, int inputIndex)
    {
        item = inputItem;
        
        textMesh.text = "1";
        itemIcon.sprite = item.ItemIcon;
        index = inputIndex;

        playerState = inputPlayerState;

        button.onClick.AddListener(OnButtonPress);
    }

    private void OnButtonPress()
    {
        playerState.UseItemRpc(index);
    }


    public void RemoveElement()
    {
        button.onClick.RemoveAllListeners();
        Destroy(gameObject);
    }
}
