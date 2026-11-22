using UnityEngine;

namespace AdventurersGuild
{
    public enum EnemySkillType
    {
        None,
        PoisonBite,   // dégâts physiques + chance d'empoisonner
        Sleep,        // aucun dégât, endort le héros
        Magic         // dégâts magiques qui ignorent la défense
    }

    /// <summary>
    /// Fiche d'un type d'ennemi (Serpent, Sirène, Champignon...).
    /// Créer via clic droit > Create > Adventurers Guild > Ennemi.
    /// L'id doit correspondre au début du enemyId du BattleTrigger
    /// (ex. id "serpent" pour "serpent_1", "serpent_2"...).
    /// </summary>
    [CreateAssetMenu(menuName = "Adventurers Guild/Ennemi", fileName = "Enemy_")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identité")]
        public string id = "serpent";
        public string displayName = "Serpent";
        public bool isBoss;

        [Header("Statistiques")]
        public int maxHp = 18;
        public int attack = 5;
        public int defense = 2;
        public int agility = 8;

        [Header("Compétence spéciale")]
        public string skillName = "Morsure venimeuse";
        public EnemySkillType skillType = EnemySkillType.PoisonBite;
        [Range(0f, 1f)] public float skillChance = 0.3f;
        [Range(0f, 1f)] public float statusChance = 0.4f;
        public int skillPower = 6;

        [Header("Récompenses")]
        public int xpReward = 8;
        public int goldReward = 6;

        [Header("Groupe (combat)")]
        [Min(1)] public int groupMin = 1;
        [Min(1)] public int groupMax = 2;

        [Header("Visuel en combat")]
        public Sprite[] idleFrames;
        public Sprite attackFrame;
        public float battleScale = 3f;
        public Color tint = Color.white;
    }
}
