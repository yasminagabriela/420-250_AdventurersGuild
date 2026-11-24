using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    [SerializeField] private BattleMenu menu;
    [SerializeField] private HealthManager player;
    [SerializeField] private HealthManager enemy;
    [SerializeField] private Animator playerAnimator;

    [Header("Joueur")]
    [SerializeField] private int attackDamage = 3;
    [SerializeField] private int healAmount = 4;
    [SerializeField] private float damageDelay = 0.4f;
    [SerializeField] private float attackDuration = 0.85f;

    [Header("Ennemi")]
    [SerializeField] private BattleEnemyApplier enemyApplier;
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private SpriteRenderer effectRenderer;
    [SerializeField] private int fallbackEnemyDamage = 2;
    [SerializeField] private float enemyTurnDelay = 0.8f;
    [SerializeField] private float enemyHitDelay = 0.35f;        // moment du coup dans l'animation
    [SerializeField] private float enemyAttackDuration = 0.85f;  // durée du clip
    [SerializeField] private float lungeDistance = 0.75f;        // si pas d'animation
    [SerializeField] private float effectDuration = 0.4f;

    [Header("Fin du combat")]
    [SerializeField] private float endDelay = 1.5f;

    private BattleState _state = BattleState.PlayerTurn;
    private bool _busy;
    private SpriteRenderer _enemyRenderer;
    private SpriteRenderer _playerRenderer;
    private Sprite _idleSprite;

    // Un objet détruit (vie à 0) compte comme 0 de vie
    private int PlayerHealth => player != null ? player.currentHealth : 0;
    private int EnemyHealth => enemy != null ? enemy.currentHealth : 0;

    private void OnEnable() { menu.Confirmed += OnChoice; }
    private void OnDisable() { menu.Confirmed -= OnChoice; }

    private void Start()
    {
        if (enemy != null)
        {
            _enemyRenderer = enemy.GetComponent<SpriteRenderer>();
            if (_enemyRenderer != null) _idleSprite = _enemyRenderer.sprite;
        }
        if (player != null) _playerRenderer = player.GetComponent<SpriteRenderer>();
        if (effectRenderer != null) effectRenderer.enabled = false;

        Debug.Log("Votre tour");
    }

    private void OnChoice(int index)
    {
        if (_state != BattleState.PlayerTurn || _busy) return;
        StartCoroutine(PlayerTurn(index));
    }

    private IEnumerator PlayerTurn(int index)
    {
        _busy = true;

        switch (index)
        {
            case 0:
                yield return StartCoroutine(PlayerAttack());
                break;
            case 1:
                player.Heal(healAmount);
                Debug.Log($"Joueur : {PlayerHealth}/{player.maxHealth}");
                yield return new WaitForSeconds(0.5f);
                break;
            case 2:
                Debug.Log("Fuite (pas encore codée)");
                _busy = false;
                yield break;            // la fuite ne consomme pas le tour
        }

        yield return StartCoroutine(AdvanceTurn());
        _busy = false;
    }

    private IEnumerator PlayerAttack()
    {
        if (playerAnimator != null && playerAnimator.enabled
            && playerAnimator.runtimeAnimatorController != null)
            playerAnimator.SetTrigger("Attack");

        yield return new WaitForSeconds(damageDelay);

        if (enemy != null)
        {
            enemy.TakeDamage(attackDamage);
            Debug.Log($"Ennemi : {EnemyHealth}");
        }

        yield return new WaitForSeconds(attackDuration - damageDelay);
    }

    private IEnumerator AdvanceTurn()
    {
        _state = BattleRules.NextState(_state, PlayerHealth, EnemyHealth);

        if (_state == BattleState.EnemyTurn)
            yield return StartCoroutine(EnemyTurn());
        else if (_state == BattleState.Won || _state == BattleState.Lost)
            yield return StartCoroutine(EndBattle());
    }

    private IEnumerator EnemyTurn()
    {
        Debug.Log("Tour de l'ennemi");
        yield return new WaitForSeconds(enemyTurnDelay);

        EnemyData data = enemyApplier != null ? enemyApplier.Data : BattleContext.Enemy;
        int damage = data != null ? data.attackDamage : fallbackEnemyDamage;
        bool animated = enemyAnimator != null && enemyAnimator.runtimeAnimatorController != null;
        float startTime = Time.time;

        if (animated)
        {
            enemyAnimator.SetTrigger("Attack");
            yield return new WaitForSeconds(enemyHitDelay);
        }
        else
        {
            if (_enemyRenderer != null && data != null && data.attackSprite != null)
                _enemyRenderer.sprite = data.attackSprite;
            yield return StartCoroutine(MoveEnemy(-lungeDistance, 0.15f));
        }

        // effet de slime + dégâts + le joueur clignote
        if (effectRenderer != null && data != null && data.attackEffectSprite != null)
        {
            effectRenderer.sprite = data.attackEffectSprite;
            effectRenderer.enabled = true;
        }

        if (player != null)
        {
            player.TakeDamage(damage);
            Debug.Log($"Joueur : {PlayerHealth}");
        }
        yield return StartCoroutine(FlashPlayer());

        yield return new WaitForSeconds(effectDuration);
        if (effectRenderer != null) effectRenderer.enabled = false;

        if (animated)
        {
            float remaining = enemyAttackDuration - (Time.time - startTime);
            if (remaining > 0f) yield return new WaitForSeconds(remaining);
        }
        else
        {
            yield return StartCoroutine(MoveEnemy(lungeDistance, 0.15f));
            if (_enemyRenderer != null && _idleSprite != null)
                _enemyRenderer.sprite = _idleSprite;
        }

        yield return new WaitForSeconds(0.3f);

        _state = BattleRules.NextState(_state, PlayerHealth, EnemyHealth);
        if (_state == BattleState.Lost)
            yield return StartCoroutine(EndBattle());
        else
            Debug.Log("Votre tour");
    }

    private IEnumerator MoveEnemy(float dx, float duration)
    {
        if (enemy == null) yield break;

        Vector3 start = enemy.transform.position;
        Vector3 end = start + new Vector3(dx, 0f, 0f);
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            enemy.transform.position = Vector3.Lerp(start, end, t / duration);
            yield return null;
        }
        enemy.transform.position = end;
    }

    private IEnumerator FlashPlayer()
    {
        for (int i = 0; i < 2; i++)
        {
            if (_playerRenderer == null) yield break;
            _playerRenderer.color = new Color(1f, 0.4f, 0.4f);
            yield return new WaitForSeconds(0.08f);

            if (_playerRenderer == null) yield break;
            _playerRenderer.color = Color.white;
            yield return new WaitForSeconds(0.08f);
        }
    }

    private IEnumerator EndBattle()
    {
        menu.enabled = false;               // plus de choix possible

        if (_state == BattleState.Won)
        {
            Debug.Log("Victoire !");
            if (!string.IsNullOrEmpty(BattleContext.EnemyId))
                BattleContext.DefeatedEnemies.Add(BattleContext.EnemyId);
        }
        else
        {
            Debug.Log("Défaite...");
        }

        yield return new WaitForSeconds(endDelay);

        if (_state == BattleState.Won && !string.IsNullOrEmpty(BattleContext.ReturnSceneName))
            SceneManager.LoadScene(BattleContext.ReturnSceneName);
    }
}