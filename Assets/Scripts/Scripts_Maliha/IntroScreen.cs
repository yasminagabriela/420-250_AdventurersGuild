using UnityEngine;
using UnityEngine.InputSystem;

public class IntroScreen : MonoBehaviour
{
    [SerializeField] private GameObject introPanel;

    private void Start()
    {
        introPanel.SetActive(true);
        Time.timeScale = 0f; // freeze the game
    }

    private void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            introPanel.SetActive(false);
            Time.timeScale = 1f; // unfreeze the game
            enabled = false;     // stop checking for Enter
        }
    }
}