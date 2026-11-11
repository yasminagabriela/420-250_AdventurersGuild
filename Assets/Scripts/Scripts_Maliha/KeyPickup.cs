using UnityEngine;
public class KeyPickup : MonoBehaviour
{
    public int keyCount = 0;

    public bool HasKey => keyCount > 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Key")) return;

        keyCount++;
        Debug.Log("Key detruit!");
        Destroy(other.gameObject);
    }

    public bool UseKey()
    {
        if (keyCount <= 0) return false;

        keyCount--;
        return true;
    }
}