using UnityEngine;

public static class GlobalSettings
{
    public static int DefaultHealth => 20;
    public static float MaximumDuelInitiateDistance => 2f;

    public static Vector3 FightInitiatorPosition => new Vector3(1, 0, 0);

    public static Vector3 FightReceiverPosition => new Vector3(-1, 0, 0);
}
