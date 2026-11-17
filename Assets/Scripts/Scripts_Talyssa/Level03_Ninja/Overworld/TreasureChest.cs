using UnityEngine;
using UnityEngine.SceneManagement;

namespace AdventurersGuild
{
    /// Coffre qui s'ouvre une seule fois et joue un son 
    public class TreasureChest : Interactable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private int potions = 1;
        [SerializeField] private int antidotes = 1;
        [SerializeField] private int ethers = 1;
        [SerializeField] private int gold = 25;

        [Header("Son (BF-17)")]
        [SerializeField] private AudioClip openSound;
        [Range(0f, 1f)][SerializeField] private float volume = 0.8f;

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
            PlayOpenSound();

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

        private void PlayOpenSound()
        {
            if (openSound == null) return;
            // joué à la position de la caméra (là où est l'Audio Listener) -> volume plein
            Vector3 where = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(openSound, where, volume);
        }
    }
}