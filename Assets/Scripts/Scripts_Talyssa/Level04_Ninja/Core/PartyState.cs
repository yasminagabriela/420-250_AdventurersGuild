using System.Collections.Generic;
using UnityEngine;

namespace AdventurersGuild
{
    /// <summary>
    /// État du ninja qui persiste entre les scènes (exploration <-> combat).
    /// Statique comme BattleContext : simple et suffisant pour le projet.
    /// </summary>
    public static class PartyState
    {
        public const string HeroName = "Ninja";

        public static int Level { get; private set; } = 1;
        public static int Xp { get; private set; }
        public static int Gold;

        public static int MaxHp { get; private set; }
        public static int Hp;
        public static int MaxMp { get; private set; }
        public static int Mp;
        public static int Attack { get; private set; }
        public static int Defense { get; private set; }
        public static int Agility { get; private set; }
        public static int Magic { get; private set; }

        public static int Potions = 3;
        public static int Antidotes = 1;
        public static int Ethers = 1;

        public static bool Poisoned;

        /// <summary>Drapeaux d'histoire : coffres ouverts, cadeaux reçus...</summary>
        public static readonly HashSet<string> Flags = new HashSet<string>();

        public const int PotionHeal = 30;
        public const int EtherRestore = 12;

        static PartyState() { ResetAll(); }

        public static void ResetAll()
        {
            Level = 1; Xp = 0; Gold = 0;
            Potions = 3; Antidotes = 1; Ethers = 1;
            Poisoned = false;
            Flags.Clear();
            RecalculateStats();
            Hp = MaxHp; Mp = MaxMp;
        }

        private static void RecalculateStats()
        {
            int l = Level - 1;
            MaxHp = 40 + l * 8;
            MaxMp = 12 + l * 2;
            Attack = 9 + l * 2;
            Defense = 3 + l;
            Agility = 8 + l;
            Magic = 7 + l * 2;
        }

        public static int XpForNext => BattleMath.XpToNextLevel(Level);

        /// <summary>Ajoute de l'XP. Retourne le nombre de niveaux gagnés.</summary>
        public static int GainXp(int amount)
        {
            Xp += Mathf.Max(0, amount);
            int gained = 0;
            while (Xp >= XpForNext)
            {
                Xp -= XpForNext;
                int oldMaxHp = MaxHp, oldMaxMp = MaxMp;
                Level++;
                gained++;
                RecalculateStats();
                Hp += MaxHp - oldMaxHp;
                Mp += MaxMp - oldMaxMp;
            }
            return gained;
        }

        public static void Heal(int amount) => Hp = Mathf.Clamp(Hp + amount, 0, MaxHp);
        public static void RestoreMp(int amount) => Mp = Mathf.Clamp(Mp + amount, 0, MaxMp);

        public static void RestoreFull()
        {
            Hp = MaxHp; Mp = MaxMp; Poisoned = false;
        }
    }
}
