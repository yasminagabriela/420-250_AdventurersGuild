using UnityEngine;

public class BattleEnemyApplier : MonoBehaviour
{
    [SerializeField] private SpriteRenderer enemyRenderer;
    [SerializeField] private HealthManager enemyHealth;
    [SerializeField] private EnemyData fallbackEnemy;

    private void Awake()
    {
        EnemyData data = BattleContext.Enemy != null ? BattleContext.Enemy : fallbackEnemy;
        if (data == null) return;

        if (data.battleSprite != null)
            enemyRenderer.sprite = data.battleSprite;

        enemyHealth.maxHealth = data.maxHealth;
    }
}