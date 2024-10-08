using System.Collections.Generic;
using UnityEngine;

public static class GlobalSettings
{
    public static int RespawnTimer => 10;
    public static int DefaultHealth => 20;
    public static float MaximumDuelInitiateDistance => 2f;
    public static float MaximumLootDistance => 2f;
    public static float PVPZoneZValue => 25f;

    public static Vector3 FightInitiatorPosition => new Vector3(1, 0, 0);

    public static Vector3 FightReceiverPosition => new Vector3(-1, 0, 0);

    public enum AnimationTriggers
    {
        None,
        DirChange,
        FistFightTrigger,
        BronzeScimitarFightTrigger,
        Rune2hFightTrigger
    }
}
