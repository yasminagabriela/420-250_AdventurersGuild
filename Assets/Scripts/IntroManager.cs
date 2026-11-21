using UnityEngine;
using UnityEngine.InputSystem;

public class IntroManager : MonoBehaviour
{
    public GameObject introPanel;
    private bool started;

    void Start()
    {
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
        introPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}