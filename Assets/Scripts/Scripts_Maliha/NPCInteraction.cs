using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField, TextArea(2, 5)] private string[] lines;

    private bool playerNearby = false;
    private bool isTalking = false;
    private int lineIndex = 0;

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (isTalking)
                StopTalking();
        }
    }


    private void Update()
    {
        Debug.Log("nearby: " + playerNearby);
        if (!playerNearby || !Keyboard.current.eKey.wasPressedThisFrame) return;

        if (!isTalking)
            StartTalking();
        else
            NextLine();
    }

    private void StartTalking()
    {
        if (lines.Length == 0) 
            return;
        isTalking = true;
        lineIndex = 0;
        animator.SetBool("IsTalking", true);
        dialoguePanel.SetActive(true);
        dialogueText.text = lines[lineIndex];
    }

    private void NextLine()
    {
        lineIndex++;
        if (lineIndex < lines.Length)
            dialogueText.text = lines[lineIndex];
        else
            StopTalking();
    }

    private void StopTalking()
    {
        isTalking = false;

        if (animator != null)
            animator.SetBool("IsTalking", false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
}