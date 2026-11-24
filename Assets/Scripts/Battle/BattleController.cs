using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    [SerializeField] private BattleMenu menu;
    [SerializeField] private HealthManager playerHealth;
    [SerializeField] private HealthManager enemyHealth;
    [SerializeField] private Animator playerAnimator;

    [Header("Attaque")]
    [SerializeField] private int playerDamage = 2;
    [SerializeField] private float attackDuration = 0.85f;
    [SerializeField] private PlayerSounds playerSounds;

    [Header("Ennemi")]
    [SerializeField] private int enemyDamage = 1;
    [SerializeField] private float enemyDelay = 1.3f;
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private EnemySounds enemySounds;

    [Header("Soin")]
    [SerializeField] private int healAmount = 4;
    [SerializeField] private int lowHealth = 3;

    private bool _busy;
    private bool _lowHealthPlayed;
    private bool _tauntPlayed;

    private void Start()
    {
        enemySounds.Play(EnemySounds.EnemySoundType.CombatStart);
    }

    private void OnEnable()
    {
        menu.Confirmed += OnChoice;
    }

    private void OnDisable()
    {
        menu.Confirmed -= OnChoice;
    }

    private void OnChoice(int index)
    {
        if (_busy) return; // le tour est déjà en cours
        if (playerHealth.currentHealth == 0 || enemyHealth.currentHealth == 0) return;

        if (index == 0) StartCoroutine(AttackTurn());
        else if (index == 1) StartCoroutine(HealTurn());
        else if (index == 2) Flee();
    }

    private IEnumerator AttackTurn()
    {
        _busy = true;

        if (playerAnimator != null && playerAnimator.enabled
            && playerAnimator.runtimeAnimatorController != null)
            playerAnimator.SetTrigger("Attack");

        playerSounds?.PlayAttackStart();
        playerSounds?.PlayAttackEnd();

        enemyHealth.TakeDamage(playerDamage);

        if (enemyHealth.currentHealth == 0)
        {
            enemySounds.Play(EnemySounds.EnemySoundType.Death);
        }
        else if (enemyHealth.currentHealth <= lowHealth && !_lowHealthPlayed)
        {
            _lowHealthPlayed = true;
            enemySounds.Play(EnemySounds.EnemySoundType.LowHealth);
        }
        else
            enemySounds.Play(EnemySounds.EnemySoundType.Hurt);

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration));

        if (enemyHealth.currentHealth > 0)
        {
            yield return new WaitForSeconds(enemyDelay);
            EnemyAttacks();
        }

        _busy = false;
    }

    private IEnumerator HealTurn()
    {
        if (playerHealth.currentHealth >= playerHealth.maxHealth) yield break;

        _busy = true;

        playerHealth.Heal(healAmount);
        playerSounds?.PlayHeal();
        if (playerHealth.currentHealth > lowHealth) _tauntPlayed = false;

        yield return new WaitForSeconds(enemyDelay);
        EnemyAttacks();

        _busy = false;
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

    private void EnemyAttacks()
    {
        if (enemyAnimator != null && enemyAnimator.enabled
            && enemyAnimator.runtimeAnimatorController != null)
            enemyAnimator.SetTrigger("Attack");

        playerHealth.TakeDamage(enemyDamage);

        bool playerIsLow = playerHealth.currentHealth > 0
            && playerHealth.currentHealth <= lowHealth
            && !_tauntPlayed;

        if (playerIsLow)
        {
            _tauntPlayed = true;
            enemySounds.Play(EnemySounds.EnemySoundType.Taunt);
        }
        else
        {
            enemySounds.Play(EnemySounds.EnemySoundType.Attack);
        }
    }
}