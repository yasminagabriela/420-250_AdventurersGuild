using System.Collections.Generic;
using UnityEngine;

public static class BattleContext
{
    public static string EnemyId;
    public static Vector3 PlayerReturnPosition;
    public static bool HasReturnPosition;
    public static readonly HashSet<string> DefeatedEnemies = new HashSet<string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        EnemyId = null;
        HasReturnPosition = false;
        PlayerReturnPosition = Vector3.zero;
        DefeatedEnemies.Clear();
    }
}