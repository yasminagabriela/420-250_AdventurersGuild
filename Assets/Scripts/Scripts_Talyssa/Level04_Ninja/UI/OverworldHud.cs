using UnityEngine;

namespace AdventurersGuild
{
    /// <summary>
    /// Titre du niveau + objectif pendant l'exploration (issue #6).
    /// La barre de vie est enlevée : les PV sont affichés par le HealthBar de l'équipe.
    /// </summary>
    public class OverworldHud : MonoBehaviour
    {
        [SerializeField] private string levelTitle = "Niveau 4 — La Forêt Lumineuse";
        [SerializeField] private string objective = "Objectif : vaincre la sirène de l'étang et rejoindre la fée.";
        [SerializeField] private string bossEnemyId = "sirene_boss";
        [SerializeField] private string objectiveAfterBoss = "Objectif : parler à la fée sur l'île de l'arbre rose.";
        [SerializeField] private float titleDuration = 4f;

        private void OnGUI()
        {
            if (DialogueBox.IsOpen) return;

            float vw = PixelGUI.VirtualWidth;
            if (Time.timeSinceLevelLoad < titleDuration && !BattleContext.HasReturnPosition)
                PixelGUI.Text(new Rect(0, 120, vw, 30), levelTitle, 20, PixelGUI.Highlight, TextAnchor.MiddleCenter);

            string obj = BattleContext.DefeatedEnemies.Contains(bossEnemyId) ? objectiveAfterBoss : objective;
            PixelGUI.Text(new Rect(10, PixelGUI.VirtualHeight - 22, vw - 20, 16), obj, 10, PixelGUI.Disabled, TextAnchor.LowerLeft);
        }
    }
}
