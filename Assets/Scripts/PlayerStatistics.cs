using System;
using UnityEngine;

public class PlayerStatistics: MonoBehaviour
{
    public int Strength { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }

    private int MaxPoints => 30;

    private void Start()
    {
        Strength = Attack = Defense = 1;
    }

    private bool PointCheck()
    {
        if (Strength + Attack + Defense > MaxPoints)
        {
            return false;
        }

        return true;
    }

    public int AddStrength()
    {
        if (PointCheck()) Strength++;
        return Strength;
    }
    public int SubtractStrength()
    {
        Strength--;
        return Strength;
    }
    public int AddAttack()
    {
        if (PointCheck()) Attack++;
        return Attack;
    }
    public int SubtractAttack()
    {
        Attack--;
        return Attack;
    }
    public int AddDefense()
    {
        if (PointCheck()) Defense++;
        return Defense;
    }
    public int SubtractDefense()
    {
        Defense--;
        return Defense;
    }
}
