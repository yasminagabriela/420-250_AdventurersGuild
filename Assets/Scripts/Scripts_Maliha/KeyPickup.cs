
using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private int keyCount = 0;

    public bool HasKey => keyCount > 0;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Key"))
            return;

        keyCount++;

        playerAudioSource.PlayOneShot(pickupSound);

        Debug.Log("Key detruit!");
        Destroy(other.gameObject);
    }

    public bool UseKey()
    {
        if (keyCount <= 0)
            return false;

        keyCount--;
        return true;
    }
}