using UnityEngine;
using UnityEngine.InputSystem;

public class IntroManager : MonoBehaviour
{
    public GameObject introPanel;
    private bool started;

    // Remains true throughout scene leads - ex: when battle ends 
    public static bool introOver;

    // Resets when pressed Play again in the editor 
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetIntro()
    {
        introOver = false;
    }

    void Start()
    {
        if (introOver)
        {
            // skip intro
            started = true;
            introPanel.SetActive(false);
            Time.timeScale = 1f;
            return;
        }
        introPanel.SetActive(true);
        // game freeze 
        Time.timeScale = 0f; 
    }

    void Update()
    {
        if (started) 
            return;

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            StartGame();
    }

    void StartGame()
    {
        started = true;
        introOver = true;
        introPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}