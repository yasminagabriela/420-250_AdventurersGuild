using UnityEngine;
using UnityEngine.SceneManagement;

namespace AdventurersGuild
{
    /// <summary>Coffre qui s'ouvre une seule fois, joue un son et soigne le joueur.</summary>
    public class TreasureChest : Interactable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite openSprite;
        [Tooltip("Points de vie redonnés au joueur (HealthManager de l'équipe).")]
        [SerializeField] private int healAmount = 3;

        [Header("Son (BF-17)")]
        [SerializeField] private AudioClip openSound;
        [Range(0f, 1f)][SerializeField] private float volume = 0.8f;

        private string Flag => $"chest:{SceneManager.GetActiveScene().name}:{name}";
        private bool IsOpened => UsedFlags.Contains(Flag);

        protected override void Start()
        {
            base.Start();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = IsOpened ? openSprite : closedSprite;
        }

        protected override bool CanInteract => !IsOpened;

        protected override void Interact()
        {
            UsedFlags.Add(Flag);
            spriteRenderer.sprite = openSprite;
            PlayOpenSound();

            HealthManager health = PlayerHealth();
            if (health != null) health.Heal(healAmount);

            DialogueBox.Show("", new[] { "Tu ouvres le coffre...", "Une potion ! Tu récupères " + healAmount + " PV." });
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
