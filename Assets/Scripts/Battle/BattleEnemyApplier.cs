using UnityEngine;

public class BattleEnemyApplier : MonoBehaviour
{
    [SerializeField] private SpriteRenderer enemyRenderer;
    [SerializeField] private HealthManager enemyHealth;
    [SerializeField] private EnemyData fallbackEnemy;
    [SerializeField] private Animator enemyAnimator;

    public EnemyData Data { get; private set; }

    private void Awake()
    {
        Data = BattleContext.Enemy != null ? BattleContext.Enemy : fallbackEnemy;
        if (Data == null) return;

        if (Data.battleSprite != null)
            enemyRenderer.sprite = Data.battleSprite;

        if (Data.animatorController != null && enemyAnimator != null)
            enemyAnimator.runtimeAnimatorController = Data.animatorController;

        enemyHealth.maxHealth = Data.maxHealth;
    }
}