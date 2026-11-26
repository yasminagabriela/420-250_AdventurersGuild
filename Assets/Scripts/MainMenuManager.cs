using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu principal (scène "MainMenu").
/// Brancher les boutons : Jouer -> Play(), Quitter -> Quit(),
/// et (optionnel) un bouton par niveau -> LoadLevel("Level_02"), etc.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Tooltip("Niveau lancé par le bouton Jouer.")]
    [SerializeField] private string firstLevelName = "Level_01";

    [Header("Musique du menu (optionnel)")]
    [SerializeField] private AudioClip menuMusic;
    [Range(0f, 1f)][SerializeField] private float volume = 0.6f;

    private void Start()
    {
        // On peut arriver ici depuis la pause : tout remettre en marche
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (menuMusic != null)
        {
            AudioSource source = GetComponent<AudioSource>();
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            source.clip = menuMusic;
            source.volume = volume;
            source.loop = true;
            source.Play();
        }
    }

    /// <summary>Commence une nouvelle partie au premier niveau.</summary>
    public void Play()
    {
        LoadLevel(firstLevelName);
    }

    /// <summary>Lance un niveau précis (pour des boutons Niveau 1, Niveau 2, ...).</summary>
    public void LoadLevel(string levelName)
    {
        // Nouvelle partie : ennemis réapparaissent, joueur au point de départ
        BattleContext.DefeatedEnemies.Clear();
        BattleContext.HasReturnPosition = false;
        BattleContext.IgnoreBattlesUntil = 0f;
        SceneManager.LoadScene(levelName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}