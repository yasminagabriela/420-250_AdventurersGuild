public enum BattleState { PlayerTurn, EnemyTurn, Won, Lost }

public static class BattleRules
{
    public static BattleState NextState(BattleState current, int playerHealth, int enemyHealth)
    {
        if (enemyHealth <= 0) return BattleState.Won;
        if (playerHealth <= 0) return BattleState.Lost;

        switch (current)
        {
            case BattleState.PlayerTurn: return BattleState.EnemyTurn;
            case BattleState.EnemyTurn: return BattleState.PlayerTurn;
            default: return current;
        }
    }
}