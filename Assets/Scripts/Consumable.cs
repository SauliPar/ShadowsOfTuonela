using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "ScriptableObjects/Consumable")]
public class Consumable : Item
{
    public int UseDuration = 0;
    public int HealValue;
}
