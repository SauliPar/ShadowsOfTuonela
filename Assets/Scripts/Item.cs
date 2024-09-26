using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Items", order = 1)]
public class Item : ScriptableObject
{
    public string ItemName;
    public Sprite ItemIcon;
    public string Id;
    public ItemType ItemType;
    public int StackSize = 1;
}

public enum ItemType
{
    Equipment,
    Consumable
}
