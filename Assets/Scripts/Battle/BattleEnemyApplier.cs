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

        if (data.battleScale != 1f)
            enemyRenderer.transform.localScale *= data.battleScale;

        enemyRenderer.transform.position += (Vector3)data.battleOffset;

        if (data.flipX)
            enemyRenderer.flipX = !enemyRenderer.flipX;

        enemyHealth.maxHealth = data.maxHealth;
    }
}