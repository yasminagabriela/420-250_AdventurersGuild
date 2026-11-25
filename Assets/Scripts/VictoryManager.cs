using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    public GameObject victoryPanel;
    public GameObject victoryText;
    public GameObject restartButton;
    public GameObject nextButton;

    public void Victory()
    {
        victoryPanel.SetActive(true);
        victoryText.SetActive(true);
        restartButton.SetActive(true);
        nextButton.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;

        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            SceneManager.LoadScene(1);
    }
}