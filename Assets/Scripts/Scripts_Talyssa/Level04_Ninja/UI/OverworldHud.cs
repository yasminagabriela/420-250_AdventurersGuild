using UnityEngine;

namespace AdventurersGuild
{
    /// <summary>Petit statut du ninja en haut à gauche pendant l'exploration (issue #6).</summary>
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
            const float w = 170f;
            PixelGUI.Window(new Rect(6, 6, w, 58));
            string status = PartyState.Poisoned ? "  <color=#9cff5a>Poison</color>" : "";
            PixelGUI.Text(new Rect(14, 9, w - 16, 16), $"{PartyState.HeroName}  Nv {PartyState.Level}{status}", 11);
            float ratio = (float)PartyState.Hp / PartyState.MaxHp;
            PixelGUI.Text(new Rect(14, 25, 70, 14), $"PV {PartyState.Hp}/{PartyState.MaxHp}", 10);
            PixelGUI.Bar(new Rect(86, 29, w - 92, 6), ratio, ratio < 0.3f ? PixelGUI.HpLow : PixelGUI.HpColor);
            PixelGUI.Text(new Rect(14, 40, 70, 14), $"PM {PartyState.Mp}/{PartyState.MaxMp}", 10);
            PixelGUI.Bar(new Rect(86, 44, w - 92, 6), (float)PartyState.Mp / PartyState.MaxMp, PixelGUI.MpColor);

            float vw = PixelGUI.VirtualWidth;
            if (Time.timeSinceLevelLoad < titleDuration && !BattleContext.HasReturnPosition)
                PixelGUI.Text(new Rect(0, 120, vw, 30), levelTitle, 20, PixelGUI.Highlight, TextAnchor.MiddleCenter);

            string obj = BattleContext.DefeatedEnemies.Contains(bossEnemyId) ? objectiveAfterBoss : objective;
            PixelGUI.Text(new Rect(10, PixelGUI.VirtualHeight - 22, vw - 20, 16), obj, 10, PixelGUI.Disabled, TextAnchor.LowerLeft);
        }
    }
}
