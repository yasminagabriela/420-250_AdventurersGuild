using System.Collections;
using UnityEngine;

public class BattleController : MonoBehaviour
{
    [SerializeField] private BattleMenu menu;
    [SerializeField] private HealthManager player;
    [SerializeField] private HealthManager enemy;
    [SerializeField] private Animator playerAnimator;

    [Header("Attaque")]
    [SerializeField] private int attackDamage = 3;
    [SerializeField] private float damageDelay = 0.4f;      // moment du coup dans l'animation
    [SerializeField] private float attackDuration = 0.85f;  // durée totale de l'animation

    [Header("Soin")]
    [SerializeField] private int healAmount = 4;

    private bool _busy;

    private void OnEnable() { menu.Confirmed += OnChoice; }
    private void OnDisable() { menu.Confirmed -= OnChoice; }

    private void OnChoice(int index)
    {
        if (_busy) return;

        switch (index)
        {
            case 0: StartCoroutine(PlayerAttack()); break;
            case 1:
                player.Heal(healAmount);
                Debug.Log($"Joueur : {player.currentHealth}/{player.maxHealth}");
                break;
            case 2: Debug.Log("Fuite (pas encore codée)"); break;
        }
    }

    private IEnumerator PlayerAttack()
    {
        if (enemy == null) yield break;
        _busy = true;

        if (playerAnimator != null && playerAnimator.enabled
            && playerAnimator.runtimeAnimatorController != null)
            playerAnimator.SetTrigger("Attack");

        yield return new WaitForSeconds(damageDelay);

        if (enemy != null)
        {
            enemy.TakeDamage(attackDamage);
            Debug.Log(enemy != null
                ? $"Ennemi : {enemy.currentHealth}/{enemy.maxHealth}"
                : "Ennemi vaincu !");
        }

        yield return new WaitForSeconds(attackDuration - damageDelay);
        _busy = false;
    }
}