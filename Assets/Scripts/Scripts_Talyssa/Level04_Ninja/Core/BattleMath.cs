using UnityEngine;

namespace AdventurersGuild
{
    /// <summary>
    /// Formules de combat (pures, sans MonoBehaviour) -> faciles à tester en EditMode.
    /// Les paramètres "roll" sont des nombres aléatoires entre 0 et 1.
    /// </summary>
    public static class BattleMath
    {
        public const float CritMultiplier = 1.5f;

        /// <summary>Dégâts physiques façon FF1 : attaque x (1 à 1.5) - défense, minimum 1.</summary>
        public static int PhysicalDamage(int attack, int defense, float roll)
        {
            float raw = attack * (1f + 0.5f * Mathf.Clamp01(roll)) - defense;
            return Mathf.Max(1, Mathf.RoundToInt(raw));
        }

        /// <summary>Dégâts magiques : ignorent la défense.</summary>
        public static int MagicDamage(int power, float roll)
        {
            return Mathf.Max(1, Mathf.RoundToInt(power * (1f + 0.35f * Mathf.Clamp01(roll))));
        }

        /// <summary>Chance de toucher (0..1) selon l'agilité des deux combattants.</summary>
        public static float HitChance(int attackerAgility, int defenderAgility)
        {
            return Mathf.Clamp(0.9f + (attackerAgility - defenderAgility) * 0.02f, 0.6f, 0.99f);
        }

        /// <summary>Chance de fuir (0..1). Impossible contre un boss.</summary>
        public static float FleeChance(int heroAgility, int strongestEnemyAgility, bool boss)
        {
            if (boss) return 0f;
            return Mathf.Clamp(0.5f + (heroAgility - strongestEnemyAgility) * 0.05f, 0.2f, 0.95f);
        }

        /// <summary>XP totale à atteindre pour passer au niveau suivant.</summary>
        public static int XpToNextLevel(int level)
        {
            return Mathf.RoundToInt(15f * Mathf.Pow(level, 1.6f));
        }

        /// <summary>Dégâts de poison en fin de tour (1/16 des PV max, minimum 1).</summary>
        public static int PoisonDamage(int maxHp)
        {
            return Mathf.Max(1, maxHp / 16);
        }
    }
}
