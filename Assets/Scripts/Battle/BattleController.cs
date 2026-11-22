using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    [SerializeField] private BattleMenu menu;
    [SerializeField] private HealthManager playerHealth;
    [SerializeField] private HealthManager enemyHealth;
    [SerializeField] private int playerDamage = 2;
    [SerializeField] private int enemyDamage = 1;
    [SerializeField] private int healAmount = 2;
    [SerializeField] private float enemyDelay = 1.5f; // secondes avant que l'ennemi attaque

    private bool _busy;

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

        enemyHealth.TakeDamage(playerDamage);

        if (enemyHealth.currentHealth > 0)
        {
            yield return new WaitForSeconds(enemyDelay);
            playerHealth.TakeDamage(enemyDamage);
        }

        _busy = false;
    }

    private IEnumerator HealTurn()
    {
        if (playerHealth.currentHealth >= playerHealth.maxHealth) yield break;

        _busy = true;

        playerHealth.Heal(healAmount);

        yield return new WaitForSeconds(enemyDelay);
        playerHealth.TakeDamage(enemyDamage);

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
}
