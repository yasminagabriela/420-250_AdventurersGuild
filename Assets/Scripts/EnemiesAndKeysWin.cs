using UnityEngine;

public class EnemiesAndKeysWin : MonoBehaviour
{
    public VictoryManager victoryManager;
    private bool hasWon;

    void Update()
    {
        if (hasWon) 
            return;

        bool noEnemies = GameObject.FindGameObjectsWithTag("Ennemi").Length == 0;
        bool noKeys = GameObject.FindGameObjectsWithTag("Key").Length == 0;

        if (noEnemies && noKeys)
        {
            hasWon = true;
            victoryManager.Victory();
        }
    }
}