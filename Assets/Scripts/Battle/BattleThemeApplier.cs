using UnityEngine;

public class BattleThemeApplier : MonoBehaviour
{
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private BattleTheme fallbackTheme;

    private void Awake()
    {
        BattleTheme theme = BattleContext.Theme != null ? BattleContext.Theme : fallbackTheme;
        if (theme == null) return;

        if (theme.background != null)
            background.sprite = theme.background;

        if (theme.playerAnimator != null)
        {
            playerAnimator.runtimeAnimatorController = theme.playerAnimator;
        }
        else if (theme.playerSprite != null)
        {
            playerAnimator.enabled = false;
            playerAnimator.GetComponent<SpriteRenderer>().sprite = theme.playerSprite;
        }

        playerAnimator.transform.position =
            new Vector3(theme.playerPosition.x, theme.playerPosition.y, 0f);

        if (enemyTransform != null)
            enemyTransform.position =
                new Vector3(theme.enemyPosition.x, theme.enemyPosition.y, 0f);
    }
}