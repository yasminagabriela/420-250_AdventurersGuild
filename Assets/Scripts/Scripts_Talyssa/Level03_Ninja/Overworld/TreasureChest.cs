using UnityEngine;
using UnityEngine.SceneManagement;

namespace AdventurersGuild
{
    /// <summary>Coffre qui s'ouvre une seule fois (issues #5 et #12).</summary>
    public class TreasureChest : Interactable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private int potions = 1;
        [SerializeField] private int antidotes = 1;
        [SerializeField] private int ethers = 1;
        [SerializeField] private int gold = 25;

        private string Flag => $"chest:{SceneManager.GetActiveScene().name}:{name}";
        private bool IsOpened => PartyState.Flags.Contains(Flag);

        protected override void Start()
        {
            base.Start();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = IsOpened ? openSprite : closedSprite;
        }

        protected override bool CanInteract => !IsOpened;

        protected override void Interact()
        {
            PartyState.Flags.Add(Flag);
            spriteRenderer.sprite = openSprite;
            PartyState.Potions += potions;
            PartyState.Antidotes += antidotes;
            PartyState.Ethers += ethers;
            PartyState.Gold += gold;

            var parts = new System.Collections.Generic.List<string>();
            if (potions > 0) parts.Add($"{potions} Potion(s)");
            if (antidotes > 0) parts.Add($"{antidotes} Antidote(s)");
            if (ethers > 0) parts.Add($"{ethers} Éther(s)");
            if (gold > 0) parts.Add($"{gold} pièces d'or");
            DialogueBox.Show("", new[] { "Tu ouvres le coffre...", "Tu obtiens : " + string.Join(", ", parts) + " !" });
        }
    }
}
