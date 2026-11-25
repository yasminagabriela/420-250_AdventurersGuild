using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    public int maxHealth = 7;
    public int currentHealth;
    public bool isPlayer = false;
    public event Action<HealthManager> OnDamaged;
    public event Action<HealthManager> OnDied;

    static int savedHealth;
    static string savedLevel = "";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSaved()
    {
        savedLevel = "";
        savedHealth = 0;
    }

    void Awake()
    {
        currentHealth = maxHealth;
        if (!isPlayer) return;

        string level = SceneManager.GetActiveScene().name;
        if (level == "BattleScene") level = BattleContext.ReturnSceneName;

        if (level == savedLevel && savedHealth > 0) currentHealth = savedHealth;
        else { savedLevel = level; savedHealth = currentHealth; }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth == 0) return;
        currentHealth = Health.ApplyDamage(currentHealth, damage);
        if (isPlayer) savedHealth = currentHealth;

        if (currentHealth == 0) OnDied?.Invoke(this);
        else OnDamaged?.Invoke(this);
    }

    public void Heal(int amount)
    {
        if (currentHealth == 0) return;
        currentHealth = Health.Heal(currentHealth, amount, maxHealth);
        if (isPlayer) savedHealth = currentHealth;
    }
}
