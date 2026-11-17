using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class HealthManagerTests
{
    [Test]
    public void TestApplyDamageWhenHealthRemains()
    {
        // Arrange
        const int currentHealth = 10;
        const int damage = 4;
        const int expectedHealth = 6;

        // Act
        int actualHealth = Health.ApplyDamage(currentHealth, damage);

        // Assert
        Assert.AreEqual(expectedHealth, actualHealth,
            $"ApplyDamage({currentHealth}, {damage}) devrait retourner {expectedHealth} mais retourne {actualHealth}.");
    }

    [Test]
    public void TestApplyDamageWhenDamageIsZero()
    {
        // Arrange
        const int currentHealth = 10;
        const int damage = 0;
        const int expectedHealth = 10;

        // Act
        int actualHealth = Health.ApplyDamage(currentHealth, damage);

        // Assert
        Assert.AreEqual(expectedHealth, actualHealth,
            $"ApplyDamage({currentHealth}, {damage}) devrait retourner {expectedHealth} mais retourne {actualHealth}.");
    }

    [Test]
    public void TestApplyDamageWhenDamageEqualsHealth()
    {
        // Arrange
        const int currentHealth = 7;
        const int damage = 7;
        const int expectedHealth = 0;

        // Act
        int actualHealth = Health.ApplyDamage(currentHealth, damage);

        // Assert
        Assert.AreEqual(expectedHealth, actualHealth,
            $"ApplyDamage({currentHealth}, {damage}) devrait retourner {expectedHealth} mais retourne {actualHealth}.");
    }

    [Test]
    public void TestApplyDamageWhenDamageExceedsHealth()
    {
        // Arrange
        const int currentHealth = 2;
        const int damage = 8;
        const int expectedHealth = 0;

        // Act
        int actualHealth = Health.ApplyDamage(currentHealth, damage);

        // Assert
        Assert.AreEqual(expectedHealth, actualHealth,
            $"ApplyDamage({currentHealth}, {damage}) devrait retourner {expectedHealth} mais retourne {actualHealth}.");
    }

    [Test]
    public void TestHealWhenHealthIsBelowMax()
    {
        // Arrange
        const int currentHealth = 5;
        const int amount = 1;
        const int maxHealth = 10;
        const int expectedHealth = 6;

        // Act
        int actualHealth = Health.Heal(currentHealth, amount, maxHealth);

        // Assert
        Assert.AreEqual(expectedHealth, actualHealth,
            $"Heal({currentHealth}, {amount}, {maxHealth}) devrait retourner {expectedHealth} mais retourne {actualHealth}.");
    }

    [Test]
    public void TestHealWhenHealthReachesMax()
    {
        // Arrange
        const int currentHealth = 9;
        const int amount = 1;
        const int maxHealth = 10;
        const int expectedHealth = 10;

        // Act
        int actualHealth = Health.Heal(currentHealth, amount, maxHealth);

        // Assert
        Assert.AreEqual(expectedHealth, actualHealth,
            $"Heal({currentHealth}, {amount}, {maxHealth}) devrait retourner {expectedHealth} mais retourne {actualHealth}.");
    }

    [Test]
    public void TestHealWhenHealingExceedsMax()
    {
        // Arrange
        const int currentHealth = 8;
        const int amount = 7;
        const int maxHealth = 10;
        const int expectedHealth = 10;

        // Act
        int actualHealth = Health.Heal(currentHealth, amount, maxHealth);

        // Assert
        Assert.AreEqual(expectedHealth, actualHealth,
            $"Heal({currentHealth}, {amount}, {maxHealth}) devrait retourner {expectedHealth} mais retourne {actualHealth}.");
    }
}
