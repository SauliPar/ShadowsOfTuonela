using UnityEngine;

[CreateAssetMenu(fileName = "New Equippable", menuName = "ScriptableObjects/Equippable")]
public class Equippable : Item
{
   public EquipType EquipType;
   
   [Header("Input stats values")]
   public int StrengthValue;
   public int AttackValue;
   public int DefenseValue;
}

public enum EquipType
{
   Weapon,
   Armor
}