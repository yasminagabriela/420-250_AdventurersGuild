using System.Collections.Generic;
using UnityEngine;

public static class BattleContext
{
    public static string EnemyId;
    public static Vector3 PlayerReturnPosition;
    public static bool HasReturnPosition;
    public static readonly HashSet<string> DefeatedEnemies = new HashSet<string>();
    public static BattleTheme Theme;
    public static EnemyData Enemy;
    public static string ReturnSceneName;
    public static float IgnoreBattlesUntil;

    public static float TriggerGraceSeconds;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        ReturnSceneName = null;
        Enemy = null;
        EnemyId = null;
        Theme = null;
        HasReturnPosition = false;
        PlayerReturnPosition = Vector3.zero;
        IgnoreBattlesUntil = 0f;
        DefeatedEnemies.Clear();
    }
}