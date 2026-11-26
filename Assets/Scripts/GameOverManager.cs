using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Écran Game Over (scène "GameOver"), chargé par le BattleController quand le joueur perd.
/// Brancher les boutons : Rejouer -> Retry(), Recommencer -> RestartFromBeginning(), Quitter -> Quit().
/// Chaque niveau peut choisir son fond et son son dans son BattleTheme
/// (Game Over Background / Game Over Clip). Sinon on utilise ceux par défaut ci-dessous.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Tooltip("Niveau chargé par le bouton Recommencer (et par Rejouer si on ne sait pas d'où on vient).")]
    [SerializeField] private string firstLevelName = "Level_01";

    [Header("Fond")]
    [Tooltip("SpriteRenderer qui affiche le fond du niveau où le joueur est mort.")]
    [SerializeField] private SpriteRenderer background;
    [Tooltip("Fond utilisé si le niveau n'en a pas (ex. Play directement dans GameOver).")]
    [SerializeField] private Sprite fallbackBackground;
    [Tooltip("Assombrit le fond pour que le texte GAME OVER ressorte.")]
    [SerializeField] private Color backgroundTint = new Color(0.45f, 0.45f, 0.45f, 1f);

    [Header("Son")]
    [Tooltip("Son joué si le niveau n'a pas son propre Game Over Clip dans son BattleTheme.")]
    [SerializeField] private AudioClip defaultGameOverClip;
    [Range(0f, 1f)][SerializeField] private float volume = 0.8f;
    [Tooltip("Coché = le son recommence en boucle (pour une musique).")]
    [SerializeField] private bool loop = false;

    private void Start()
    {
        BattleTheme theme = BattleContext.Theme;
        ShowBackground(theme);
        PlaySound(theme);
    }

    // 1) fond Game Over du niveau  2) sinon fond de combat du niveau  3) sinon fond par défaut
    private void ShowBackground(BattleTheme theme)
    {
        if (background == null) return;

        Sprite sprite = fallbackBackground;
        if (theme != null)
        {
            if (theme.gameOverBackground != null) sprite = theme.gameOverBackground;
            else if (theme.background != null) sprite = theme.background;
        }
        if (sprite == null) return;

        background.sprite = sprite;
        background.color = backgroundTint;
        FillCamera(background);
    }

    // 1) son Game Over du niveau  2) sinon son par défaut
    private void PlaySound(BattleTheme theme)
    {
        AudioClip clip = theme != null && theme.gameOverClip != null ? theme.gameOverClip : defaultGameOverClip;
        if (clip == null) return;

        AudioListener.pause = false;   // au cas où le jeu était en pause
        AudioSource source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.loop = loop;
        source.playOnAwake = false;
        source.Play();
    }

    // Étire l'image pour qu'elle remplisse tout l'écran de la caméra
    private static void FillCamera(SpriteRenderer sr)
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;
        Vector2 size = sr.sprite.bounds.size;
        float scale = Mathf.Max(width / size.x, height / size.y);

        sr.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
        sr.transform.localScale = new Vector3(scale, scale, 1f);
    }

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