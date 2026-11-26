using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Progression du joueur : quels niveaux sont débloqués (sauvegardé avec PlayerPrefs).
/// - Le niveau 1 est toujours débloqué.
/// - Arriver dans "Level_0N" (par n'importe quel moyen : NextLevel, PNJ, porte...) débloque le niveau N.
/// - LevelProgress.CompleteCurrentLevel() débloque le niveau suivant (appelé par VictoryManager.Victory()).
/// </summary>
public static class LevelProgress
{
    private const string Key = "UnlockedLevel";

    /// <summary>Plus haut niveau débloqué (1 au minimum).</summary>
    public static int Unlocked => Mathf.Max(1, PlayerPrefs.GetInt(Key, 1));

    public static bool IsUnlocked(int level) => level <= Unlocked;

    public static void Unlock(int level)
    {
        if (level <= Unlocked) return;
        PlayerPrefs.SetInt(Key, level);
        PlayerPrefs.Save();
        Debug.Log($"[LevelProgress] Niveau {level} débloqué");
    }

    /// <summary>À appeler quand le joueur réussit le niveau où il est.</summary>
    public static void CompleteCurrentLevel()
    {
        int n = LevelNumber(SceneManager.GetActiveScene().name);
        if (n > 0) Unlock(n + 1);
    }

    /// <summary>Remet la progression à zéro (seul le niveau 1 est ouvert).</summary>
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }

    /// <summary>"Level_03" -> 3, autre scène -> 0.</summary>
    public static int LevelNumber(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || !sceneName.StartsWith("Level_")) return 0;
        return int.TryParse(sceneName.Substring("Level_".Length), out int n) ? n : 0;
    }

    public static string SceneName(int level) => $"Level_{level:00}";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int n = LevelNumber(scene.name);
        if (n > 0) Unlock(n);
    }
}
