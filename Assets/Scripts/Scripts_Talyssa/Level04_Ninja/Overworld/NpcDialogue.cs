using UnityEngine;
using UnityEngine.SceneManagement;

namespace AdventurersGuild
{
    /// <summary>
    /// PNJ qui parle quand on appuie sur E à côté de lui.
    /// Peut changer de texte une fois un ennemi vaincu (ex. la sirène),
    /// et donner une récompense une seule fois.
    /// </summary>
    public class NpcDialogue : Interactable
    {
        public enum Reward { None, HealFull, TwoPotions, EtherAndHeal }

        [SerializeField] private string speakerName = "Ermite";
        [TextArea(2, 4)] [SerializeField] private string[] lines = { "Bonjour, voyageur." };

        [Header("Après une victoire (optionnel)")]
        [Tooltip("enemyId du BattleTrigger à vaincre (ex. sirene_boss). Vide = jamais.")]
        [SerializeField] private string requiredDefeatedEnemy = "";
        [TextArea(2, 4)] [SerializeField] private string[] linesAfter;

        [Header("Récompense unique")]
        [SerializeField] private Reward reward = Reward.None;
        [SerializeField] private bool rewardOnlyAfterVictory;
        [SerializeField] private string rewardMessage = "";

        [Header("Fin du niveau (optionnel)")]
        [Tooltip("Si rempli, charge cette scène après le dialogue de victoire (issue #18).")]
        [SerializeField] private string nextSceneName = "";

        private string RewardFlag => $"reward:{SceneManager.GetActiveScene().name}:{name}";

        private bool VictoryDone => !string.IsNullOrEmpty(requiredDefeatedEnemy)
                                    && BattleContext.DefeatedEnemies.Contains(requiredDefeatedEnemy);

        protected override void Interact()
        {
            bool after = VictoryDone && linesAfter != null && linesAfter.Length > 0;
            string[] toSay = after ? linesAfter : lines;
            DialogueBox.Show(speakerName, toSay, () =>
            {
                GiveRewardIfAny();
                if (after && !string.IsNullOrEmpty(nextSceneName))
                {
                    BattleContext.HasReturnPosition = false;
                    SceneManager.LoadScene(nextSceneName);
                }
            });
        }

        private void GiveRewardIfAny()
        {
            if (reward == Reward.None || UsedFlags.Contains(RewardFlag)) return;
            if (rewardOnlyAfterVictory && !VictoryDone) return;
            UsedFlags.Add(RewardFlag);

            // Toutes les récompenses soignent le joueur (HealthManager de l'équipe).
            HealthManager health = PlayerHealth();
            if (health != null) health.Heal(health.maxHealth);

            if (!string.IsNullOrEmpty(rewardMessage)) DialogueBox.Show("", new[] { rewardMessage });
        }
    }
}
