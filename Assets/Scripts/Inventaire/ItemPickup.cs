using UnityEngine;

// À mettre sur un objet ramassable de la carte (clé, potion). Il faut un Collider2D en "Is Trigger".
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemData item;
    [SerializeField] private int quantite = 1;                        // ex. un paquet de 3 potions
    [SerializeField] private string pickupId = "level01_objet_a";   // UNIQUE pour chaque objet placé
    [SerializeField] private AudioClip pickupSound;

    private void Start()
    {
        // Déjà ramassé avant un combat ? Il ne revient pas.
        if (Inventory.RamassesIds.Contains(pickupId))
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!Inventory.AjouterItem(item, quantite)) return;   // inventaire plein : on le laisse par terre

        Inventory.RamassesIds.Add(pickupId);

        AudioSource source = other.GetComponent<AudioSource>();
        if (source != null && pickupSound != null) source.PlayOneShot(pickupSound);

        gameObject.SetActive(false);   // désactivé = n'est plus compté par EnemiesAndKeysWin
    }
}