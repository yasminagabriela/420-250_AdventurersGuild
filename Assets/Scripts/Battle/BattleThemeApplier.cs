using UnityEngine;

public class BattleThemeApplier : MonoBehaviour
{
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private SpriteRenderer enemyRenderer;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private BattleTheme fallbackTheme;

    private void Awake()
    {
        BattleTheme theme = BattleContext.Theme != null ? BattleContext.Theme : fallbackTheme;
        if (theme == null) return;

        if (theme.background != null)
            background.sprite = theme.background;

        if (theme.enemySprite != null && enemyRenderer != null)
            enemyRenderer.sprite = theme.enemySprite;

        if (theme.enemyFlipX && enemyRenderer != null)
            enemyRenderer.flipX = !enemyRenderer.flipX;

        if (theme.enemyScale != 1f && enemyRenderer != null)
            enemyRenderer.transform.localScale = new Vector3(theme.enemyScale, theme.enemyScale, 1f);

        if (theme.enemyOffset != Vector2.zero && enemyRenderer != null)
            enemyRenderer.transform.position += (Vector3)theme.enemyOffset;

        if (theme.playerAnimator != null)
        {
            playerAnimator.runtimeAnimatorController = theme.playerAnimator;
        }
        else if (theme.playerSprite != null)
        {
            playerAnimator.enabled = false;
            playerAnimator.GetComponent<SpriteRenderer>().sprite = theme.playerSprite;
        }
    }
}