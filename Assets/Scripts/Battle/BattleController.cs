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
    [SerializeField] private int fallbackEnemyDamage = 2;
    [SerializeField] private float enemyTurnDelay = 0.8f;

    [Header("Fin du combat")]
    [SerializeField] private float endDelay = 1.5f;

    private BattleState _state = BattleState.PlayerTurn;
    private bool _busy;

    // Un objet détruit (vie à 0) compte comme 0 de vie
    private int PlayerHealth => player != null ? player.currentHealth : 0;
    private int EnemyHealth => enemy != null ? enemy.currentHealth : 0;

    private void OnEnable() { menu.Confirmed += OnChoice; }
    private void OnDisable() { menu.Confirmed -= OnChoice; }
    private void Start() { Debug.Log("Votre tour"); }

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

        int damage = BattleContext.Enemy != null
            ? BattleContext.Enemy.attackDamage
            : fallbackEnemyDamage;

        if (player != null)
        {
            player.TakeDamage(damage);
            Debug.Log($"Joueur : {PlayerHealth}");
        }

        yield return new WaitForSeconds(0.5f);

        _state = BattleRules.NextState(_state, PlayerHealth, EnemyHealth);
        if (_state == BattleState.Lost)
            yield return StartCoroutine(EndBattle());
        else
            Debug.Log("Votre tour");
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