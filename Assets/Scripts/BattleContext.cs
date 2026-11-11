using System.Collections.Generic;
using UnityEngine;

public static class BattleContext
{
    public static string EnemyId;
    public static Vector3 PlayerReturnPosition;
    public static bool HasReturnPosition;
    public static readonly HashSet<string> DefeatedEnemies = new HashSet<string>();

    // Ajouts niveau 4 (Talyssa)
    public static string ReturnSceneName;
    public static float TriggerGraceSeconds;
}