using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public int maxHealth = 12;
    public int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Health.ApplyDamage(currentHealth, damage);

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Health.Heal(currentHealth, amount, maxHealth);
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
