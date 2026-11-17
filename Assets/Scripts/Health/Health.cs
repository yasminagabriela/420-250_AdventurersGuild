
public class Health
{
    public static int ApplyDamage(int currentHealth, int damage)
    {
        int newHealth = currentHealth - damage;
        if (newHealth < 0)
        {
            return 0;
        }
        return newHealth;
    }

    public static int Heal(int currentHealth, int amount, int maxHealth)
    {
        int newHealth = currentHealth + amount;
        if (newHealth > maxHealth)
        {
            return maxHealth;
        }
        return newHealth;
    }
}
