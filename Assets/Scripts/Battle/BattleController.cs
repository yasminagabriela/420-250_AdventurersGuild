using System.Collections;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    [SerializeField] private BattleMenu menu;
    [SerializeField] private HealthManager player;
    [SerializeField] private HealthManager enemy;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private HealthManager playerHealth;
    [SerializeField] private HealthManager enemyHealth;
    [SerializeField] private int playerDamage = 2;
    [SerializeField] private int enemyDamage = 1;
    [SerializeField] private int healAmount = 2;
    [SerializeField] private float enemyDelay = 1.5f; // secondes avant que l'ennemi attaque

    [Header("Attaque")]
    [SerializeField] private int attackDamage = 3;
    [SerializeField] private float damageDelay = 0.4f;      // moment du coup dans l'animation
    [SerializeField] private float attackDuration = 0.85f;  // durée totale de l'animation

    [Header("Soin")]
    [SerializeField] private int healAmount = 4;

    private bool _busy;

    private void OnEnable() { menu.Confirmed += OnChoice; }
    private void OnDisable() { menu.Confirmed -= OnChoice; }
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
        if (_busy) return;
        if (_busy) return; // le tour est déjà en cours
        if (playerHealth.currentHealth == 0 || enemyHealth.currentHealth == 0) return;

        if (index == 0) StartCoroutine(AttackTurn());
        else if (index == 1) StartCoroutine(HealTurn());
        else if (index == 2) Flee();
    }

    private IEnumerator AttackTurn()
    {
        _busy = true;

        enemyHealth.TakeDamage(playerDamage);

        switch (index)
        if (enemyHealth.currentHealth > 0)
        {
            case 0: StartCoroutine(PlayerAttack()); break;
            case 1:
                player.Heal(healAmount);
                Debug.Log($"Joueur : {player.currentHealth}/{player.maxHealth}");
                break;
            case 2: Debug.Log("Fuite (pas encore codée)"); break;
            yield return new WaitForSeconds(enemyDelay);
            playerHealth.TakeDamage(enemyDamage);
        }

        _busy = false;
    }

    private IEnumerator PlayerAttack()
    private IEnumerator HealTurn()
    {
        if (enemy == null) yield break;
        if (playerHealth.currentHealth >= playerHealth.maxHealth) yield break;

        _busy = true;

        if (playerAnimator != null && playerAnimator.enabled
            && playerAnimator.runtimeAnimatorController != null)
            playerAnimator.SetTrigger("Attack");
        playerHealth.Heal(healAmount);

        yield return new WaitForSeconds(enemyDelay);
        playerHealth.TakeDamage(enemyDamage);

        yield return new WaitForSeconds(damageDelay);
        _busy = false;
    }

        if (enemy != null)
    private void Flee()
    {
        if (string.IsNullOrEmpty(BattleContext.ReturnSceneName))
        {
            enemy.TakeDamage(attackDamage);
            Debug.Log(enemy != null
                ? $"Ennemi : {enemy.currentHealth}/{enemy.maxHealth}"
                : "Ennemi vaincu !");
            Debug.LogWarning("Aucun niveau de retour : le combat a été lancé seul.");
            return;
        }

        yield return new WaitForSeconds(attackDuration - damageDelay);
        _busy = false;
        BattleContext.IgnoreBattlesUntil = Time.time + 2f;
        SceneManager.LoadScene(BattleContext.ReturnSceneName);
    }
}