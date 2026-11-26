using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    [SerializeField] private BattleMenu menu;
    [SerializeField] private HealthManager playerHealth;
    [SerializeField] private HealthManager enemyHealth;
    [SerializeField] private Animator playerAnimator;

    [Header("Joueur")]
    [SerializeField] private int attackDamage = 3;
    [SerializeField] private int healAmount = 4;
    [SerializeField] private float damageDelay = 0.4f;           // moment du coup dans l'animation
    [SerializeField] private float attackDuration = 0.85f;
    [SerializeField] private PlayerSounds playerSounds;

    [Header("Ennemi")]
    [SerializeField] private BattleEnemyApplier enemyApplier;
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private SpriteRenderer effectRenderer;
    [SerializeField] private EnemySounds enemySounds;
    [SerializeField] private int fallbackEnemyDamage = 2;
    [SerializeField] private float enemyTurnDelay = 0.8f;
    [SerializeField] private float enemyHitDelay = 0.35f;        // moment du coup dans l'animation
    [SerializeField] private float enemyAttackDuration = 0.85f;  // durée du clip
    [SerializeField] private float lungeDistance = 0.75f;        // si pas d'animation
    [SerializeField] private float effectDuration = 0.4f;

    [Header("Vie basse")]
    [SerializeField] private int lowHealth = 3;

    [Header("Fin du combat")]
    [SerializeField] private float endDelay = 1.5f;
    [Tooltip("Scène chargée quand le joueur perd le combat.")]
    [SerializeField] private string gameOverSceneName = "GameOver";

    private BattleState _state = BattleState.PlayerTurn;
    private bool _busy;
    private bool _lowHealthPlayed;
    private bool _tauntPlayed;
    private SpriteRenderer _enemyRenderer;
    private SpriteRenderer _playerRenderer;
    private Sprite _idleSprite;

    // Un objet détruit (vie à 0) compte comme 0 de vie
    private int PlayerHealth => playerHealth != null ? playerHealth.currentHealth : 0;
    private int EnemyHealth => enemyHealth != null ? enemyHealth.currentHealth : 0;

    private void OnEnable()
    {
        menu.Confirmed += OnChoice;
    }

    private void OnDisable()
    {
        menu.Confirmed -= OnChoice;
    }

    private void Start()
    {
        if (enemyHealth != null)
        {
            _enemyRenderer = enemyHealth.GetComponent<SpriteRenderer>();
            if (_enemyRenderer != null) _idleSprite = _enemyRenderer.sprite;
        }
        if (playerHealth != null) _playerRenderer = playerHealth.GetComponent<SpriteRenderer>();
        if (effectRenderer != null) effectRenderer.enabled = false;

        PlayEnemySound(EnemySounds.EnemySoundType.CombatStart);
        Debug.Log("Votre tour");
    }

    private void OnChoice(int index)
    {
        if (_state != BattleState.PlayerTurn || _busy) return;
        if (PlayerHealth == 0 || EnemyHealth == 0) return;

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
                if (PlayerHealth >= playerHealth.maxHealth)
                {
                    _busy = false;
                    yield break;            // vie pleine : le tour n'est pas consommé
                }
                playerHealth.Heal(healAmount);
                if (playerSounds != null) playerSounds.PlayHeal();
                if (PlayerHealth > lowHealth) _tauntPlayed = false;
                Debug.Log($"Joueur : {PlayerHealth}/{playerHealth.maxHealth}");
                yield return new WaitForSeconds(0.5f);
                break;

            case 2:
                Flee();
                _busy = false;
                yield break;                // la fuite ne consomme pas le tour
        }

        yield return StartCoroutine(AdvanceTurn());
        _busy = false;
    }

    private IEnumerator PlayerAttack()
    {
        if (playerAnimator != null && playerAnimator.enabled
            && playerAnimator.runtimeAnimatorController != null)
            playerAnimator.SetTrigger("Attack");

        if (playerSounds != null)
        {
            playerSounds.PlayAttackStart();
            playerSounds.PlayAttackEnd();
        }

        yield return new WaitForSeconds(damageDelay);

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(attackDamage);
            Debug.Log($"Ennemi : {EnemyHealth}");
        }

        if (EnemyHealth == 0)
        {
            PlayEnemySound(EnemySounds.EnemySoundType.Death);
            enemyAnimator.SetTrigger("Die");
        }
        else if (EnemyHealth <= lowHealth && !_lowHealthPlayed)
        {
            _lowHealthPlayed = true;
            PlayEnemySound(EnemySounds.EnemySoundType.LowHealth);
        }
        else
        {
            PlayEnemySound(EnemySounds.EnemySoundType.Hurt);
            enemyAnimator.SetTrigger("Hurt");
        }

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - damageDelay));
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
        bool animated = enemyAnimator != null && enemyAnimator.enabled
                        && enemyAnimator.runtimeAnimatorController != null;
        float startTime = Time.time;

        // direction : l'ennemi avance vers le joueur, où qu'ils soient placés
        float dir = -1f;
        if (playerHealth != null && enemyHealth != null)
            dir = Mathf.Sign(playerHealth.transform.position.x - enemyHealth.transform.position.x);
        float lunge = dir * lungeDistance;

        if (animated)
            enemyAnimator.SetTrigger("Attack");
        else if (_enemyRenderer != null && data != null && data.attackSprite != null)
            _enemyRenderer.sprite = data.attackSprite;

        yield return StartCoroutine(MoveEnemy(lunge, 0.15f));

        if (animated)
        {
            float wait = enemyHitDelay - (Time.time - startTime);
            if (wait > 0f) yield return new WaitForSeconds(wait);
        }

        // effet de slime + dégâts + son + le joueur clignote
        if (effectRenderer != null && data != null && data.attackEffectSprite != null)
        {
            effectRenderer.sprite = data.attackEffectSprite;
            effectRenderer.enabled = true;
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        
            if (playerHealth.currentHealth == 0)
                playerAnimator.SetTrigger("Die");
            else
                playerAnimator.SetTrigger("Hurt");
        }

        PlayEnemyAttackSound();
        yield return StartCoroutine(FlashPlayer());

        yield return new WaitForSeconds(effectDuration);
        if (effectRenderer != null) effectRenderer.enabled = false;

        // retour à la place de départ
        yield return StartCoroutine(MoveEnemy(-lunge, 0.15f));

        if (animated)
        {
            float remaining = enemyAttackDuration - (Time.time - startTime);
            if (remaining > 0f) yield return new WaitForSeconds(remaining);
        }
        else if (_enemyRenderer != null && _idleSprite != null)
        {
            _enemyRenderer.sprite = _idleSprite;
        }

        yield return new WaitForSeconds(0.3f);

        _state = BattleRules.NextState(_state, PlayerHealth, EnemyHealth);
        if (_state == BattleState.Lost)
            yield return StartCoroutine(EndBattle());
        else
            Debug.Log("Votre tour");
    }

    private void PlayEnemyAttackSound()
    {
        bool playerIsLow = PlayerHealth > 0 && PlayerHealth <= lowHealth && !_tauntPlayed;

        if (playerIsLow)
        {
            _tauntPlayed = true;
            PlayEnemySound(EnemySounds.EnemySoundType.Taunt);
        }
        else
        {
            PlayEnemySound(EnemySounds.EnemySoundType.Attack);
        }
    }

    private void PlayEnemySound(EnemySounds.EnemySoundType type)
    {
        if (enemySounds != null) enemySounds.Play(type);
    }

    private IEnumerator MoveEnemy(float dx, float duration)
    {
        if (enemyHealth == null) yield break;

        Transform enemyTransform = enemyHealth.transform;
        Vector3 start = enemyTransform.position;
        Vector3 end = start + new Vector3(dx, 0f, 0f);
        float t = 0f;

        while (t < duration)
        {
            if (enemyHealth == null) yield break;
            t += Time.deltaTime;
            enemyTransform.position = Vector3.Lerp(start, end, t / duration);
            yield return null;
        }
        enemyTransform.position = end;
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
        {
            BattleContext.IgnoreBattlesUntil = Time.time + 2f;
            SceneManager.LoadScene(BattleContext.ReturnSceneName);
        }
        else if (_state == BattleState.Lost && !string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
    }

    private void Flee()
    {
        if (string.IsNullOrEmpty(BattleContext.ReturnSceneName))
        {
            Debug.LogWarning("Aucun niveau de retour : le combat a été lancé seul.");
            return;
        }

        BattleContext.IgnoreBattlesUntil = Time.time + 2f;
        SceneManager.LoadScene(BattleContext.ReturnSceneName);
    }
}