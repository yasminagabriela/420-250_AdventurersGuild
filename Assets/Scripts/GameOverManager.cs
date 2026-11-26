using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Écran Game Over (scène "GameOver"), chargé par le BattleController quand le joueur perd.
/// Brancher les boutons : Rejouer -> Retry(), Recommencer -> RestartFromBeginning(), Quitter -> Quit().
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Tooltip("Niveau chargé par le bouton Recommencer (et par Rejouer si on ne sait pas d'où on vient).")]
    [SerializeField] private string firstLevelName = "Level_01";

    /// <summary>Recommence le niveau où le joueur est mort, avec toute sa vie.</summary>
    public void Retry()
    {
        string level = string.IsNullOrEmpty(BattleContext.ReturnSceneName)
            ? firstLevelName
            : BattleContext.ReturnSceneName;
        ResetRun();
        SceneManager.LoadScene(level);
    }

    /// <summary>Recommence tout le jeu au premier niveau.</summary>
    public void RestartFromBeginning()
    {
        ResetRun();
        SceneManager.LoadScene(firstLevelName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Les ennemis réapparaissent, le joueur repart du début du niveau.
    // La vie revient au maximum toute seule (HealthManager ne garde pas une vie à 0).
    private static void ResetRun()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        BattleContext.DefeatedEnemies.Clear();
        BattleContext.HasReturnPosition = false;
        BattleContext.IgnoreBattlesUntil = 0f;
    }
}