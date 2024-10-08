using UnityEngine;

// [CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Items", order = 1)]
public class Item: ScriptableObject
{
    public string ItemName;
    public Sprite ItemIcon;
    public int Id;
    public ItemType ItemType;
    public float DropChance;
}

public enum ItemType
{
    Equipment,
    Consumable
}
