using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverText;
    public GameObject restartButton;
    public GameObject nextButton;

    public void GameOver()
    {
        gameOverText.SetActive(true);
        restartButton.SetActive(true);
        nextButton.SetActive(true);
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
