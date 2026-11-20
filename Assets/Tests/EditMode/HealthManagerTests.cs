using NUnit.Framework;
using UnityEngine;

public class HealthManagerTests
{
    [Test]
    public void TestTakeDamageReducesCurrentHealth()
    {
        // Arrange
        var gameObject = new GameObject("TestHealthManager");

        try
        {
            var manager = gameObject.AddComponent<HealthManager>();
            manager.maxHealth = 10;
            manager.currentHealth = 7;
            const int damage = 4;
            const int expectedHealth = 3;

            // Act
            manager.TakeDamage(damage);

            // Assert
            Assert.AreEqual(expectedHealth, manager.currentHealth,
                $"TakeDamage({damage}) devrait laisser {expectedHealth} de vie mais laisse {manager.currentHealth}.");
        }
        finally
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void TestOnDiedIsNotInvokedWhenHealthRemains()
    {
        // Arrange
        var gameObject = new GameObject("TestHealthManager");

        try
        {
            var manager = gameObject.AddComponent<HealthManager>();
            manager.maxHealth = 10;
            manager.currentHealth = 10;
            int diedCount = 0;
            manager.OnDied += (m) => diedCount++;
            const int expectedCount = 0;

            // Act
            manager.TakeDamage(4);

            // Assert
            Assert.AreEqual(expectedCount, diedCount,
                $"OnDied ne devrait pas être appelé quand il reste de la vie, mais a été appelé {diedCount} fois.");
        }
        finally
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void TestOnDiedIsInvokedOnlyOnce()
    {
        // Arrange
        var gameObject = new GameObject("TestHealthManager");

        try
        {
            var manager = gameObject.AddComponent<HealthManager>();
            manager.maxHealth = 10;
            manager.currentHealth = 10;
            int diedCount = 0;
            manager.OnDied += (m) => diedCount++;
            const int expectedCount = 1;

            // Act
            manager.TakeDamage(10);
            manager.TakeDamage(5);

            // Assert
            Assert.AreEqual(expectedCount, diedCount,
                $"OnDied devrait être appelé {expectedCount} fois mais a été appelé {diedCount} fois.");
        }
        finally
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void TestHealIncreasesCurrentHealth()
    {
        // Arrange
        var gameObject = new GameObject("TestHealthManager");

        try
        {
            var manager = gameObject.AddComponent<HealthManager>();
            manager.maxHealth = 10;
            manager.currentHealth = 5;
            const int amount = 3;
            const int expectedHealth = 8;

            // Act
            manager.Heal(amount);

            // Assert
            Assert.AreEqual(expectedHealth, manager.currentHealth,
                $"Heal({amount}) devrait donner {expectedHealth} mais donne {manager.currentHealth}.");
        }
        finally
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void TestHealDoesNotExceedMaxHealth()
    {
        // Arrange
        var gameObject = new GameObject("TestHealthManager");

        try
        {
            var manager = gameObject.AddComponent<HealthManager>();
            manager.maxHealth = 10;
            manager.currentHealth = 5;
            const int amount = 20;
            const int expectedHealth = 10;

            // Act
            manager.Heal(amount);

            // Assert
            Assert.AreEqual(expectedHealth, manager.currentHealth,
                $"Heal({amount}) devrait s'arrêter à {expectedHealth} mais donne {manager.currentHealth}.");
        }
        finally
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void TestHealWhenDeadDoesNotResurrect()
    {
        // Arrange
        var gameObject = new GameObject("TestHealthManager");

        try
        {
            var manager = gameObject.AddComponent<HealthManager>();
            manager.currentHealth = 0;
            const int expectedHealth = 0;

            // Act
            manager.Heal(5);

            // Assert
            Assert.AreEqual(expectedHealth, manager.currentHealth,
                $"Heal sur un personnage mort devrait garder {expectedHealth} mais donne {manager.currentHealth}.");
        }
        finally
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }
}