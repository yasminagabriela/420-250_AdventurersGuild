using System;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public int maxHealth = 12;
    public int currentHealth;
    public event Action<HealthManager> OnDied;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth == 0) return;

        currentHealth = Health.ApplyDamage(currentHealth, damage);

        if (currentHealth == 0) OnDied?.Invoke(this);
    }

    public void Heal(int amount)
    {
        if (currentHealth == 0) return;

        currentHealth = Health.Heal(currentHealth, amount, maxHealth);
    }
}
