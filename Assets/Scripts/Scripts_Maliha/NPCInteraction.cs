using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private bool playerNearby = false;
    private bool isTalking = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Player is nearby!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            StopTalking();
        }
    }

    private void Update()
    {
        if (playerNearby && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isTalking)
                StopTalking();
            else
                StartTalking();
        }
    }

    private void StartTalking()
    {
        isTalking = true;
        animator.SetBool("IsTalking", true);
        Debug.Log("NPC is talking!");
    }

    private void StopTalking()
    {
        isTalking = false;
        animator.SetBool("IsTalking", false);
        Debug.Log("NPC stopped talking!");
    }
}