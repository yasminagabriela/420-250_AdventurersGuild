using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menu principal (scène "MainMenu").
/// - Seuls les niveaux débloqués sont cliquables, les autres sont gris.
/// - Le niveau à jouer (le plus haut débloqué) clignote avec un contour.
/// Boutons : Start -> Play(), Quitter -> Quit(), Level N -> LoadLevel(N) (fait tout seul).
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelSlot
    {
        [Tooltip("Numéro du niveau (1 = Level_01).")]
        public int level = 1;
        public Button button;
        [Tooltip("Image du personnage au-dessus du bouton (optionnel).")]
        public Graphic character;
    }

    [Header("Niveaux")]
    [SerializeField] private LevelSlot[] levels;

    [Header("Look")]
    [SerializeField] private Color lockedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color blinkColor = new Color(1f, 0.85f, 0.1f, 1f);
    [SerializeField] private float blinkSpeed = 3f;
    [SerializeField] private Vector2 blinkThickness = new Vector2(8f, -8f);

    [Header("Musique du menu (optionnel)")]
    [SerializeField] private AudioClip menuMusic;
    [Range(0f, 1f)][SerializeField] private float volume = 0.6f;

    private Outline _blink;
    private readonly System.Collections.Generic.Dictionary<Graphic, Color> _originalColors =
        new System.Collections.Generic.Dictionary<Graphic, Color>();

    private void Start()
    {
        // On peut arriver ici depuis la pause : tout remettre en marche
        Time.timeScale = 1f;
        AudioListener.pause = false;

        RefreshLevels();
        PlayMusic();
    }

    private void Update()
    {
        if (_blink == null) return;
        Color c = blinkColor;
        c.a = Mathf.Abs(Mathf.Sin(Time.unscaledTime * blinkSpeed));   // 0 -> 1 -> 0 ...
        _blink.effectColor = c;
    }

    /// <summary>Met chaque bouton à jour : gris et bloqué, ou ouvert (et clignotant si c'est le niveau à jouer).</summary>
    public void RefreshLevels()
    {
        if (_blink != null) Destroy(_blink);
        _blink = null;
        if (levels == null) return;

        int current = CurrentLevel();
        foreach (LevelSlot slot in levels)
        {
            if (slot == null || slot.button == null) continue;

            bool open = LevelProgress.IsUnlocked(slot.level);
            int level = slot.level;
            slot.button.onClick.RemoveAllListeners();
            slot.button.onClick.AddListener(() => LoadLevel(level));
            slot.button.interactable = open;

            // Gris sur le bouton, son texte et le personnage quand c'est bloqué
            foreach (Graphic g in slot.button.GetComponentsInChildren<Graphic>(true))
                if (g != slot.button.targetGraphic) SetLocked(g, !open);
            ColorBlock colors = slot.button.colors;
            colors.disabledColor = lockedColor;
            slot.button.colors = colors;
            if (slot.character != null) SetLocked(slot.character, !open);

            if (slot.level == current)
            {
                _blink = slot.button.gameObject.AddComponent<Outline>();
                _blink.effectDistance = blinkThickness;
            }
        }
    }

    // Gris si bloqué, sinon on remet la couleur d'origine (choisie dans l'Inspector)
    private void SetLocked(Graphic g, bool locked)
    {
        if (!_originalColors.ContainsKey(g)) _originalColors[g] = g.color;
        g.color = locked ? lockedColor : _originalColors[g];
    }

    // Le niveau à jouer = le plus haut débloqué qui existe dans le menu
    private int CurrentLevel()
    {
        int best = 1;
        foreach (LevelSlot slot in levels)
            if (slot != null && LevelProgress.IsUnlocked(slot.level) && slot.level > best) best = slot.level;
        return best;
    }

    /// <summary>Bouton Start : lance le niveau à jouer (celui qui clignote).</summary>
    public void Play()
    {
        LoadLevel(levels != null && levels.Length > 0 ? CurrentLevel() : 1);
    }

    public void LoadLevel(int level)
    {
        if (!LevelProgress.IsUnlocked(level)) return;   // sécurité

        // Nouvelle partie : ennemis réapparaissent, joueur au point de départ
        BattleContext.DefeatedEnemies.Clear();
        BattleContext.HasReturnPosition = false;
        BattleContext.IgnoreBattlesUntil = 0f;
        SceneManager.LoadScene(LevelProgress.SceneName(level));
    }

    /// <summary>Pour tester (clic droit sur le composant > Réinitialiser la progression).</summary>
    [ContextMenu("Réinitialiser la progression")]
    public void ResetProgress()
    {
        LevelProgress.ResetProgress();
        if (Application.isPlaying) RefreshLevels();
        Debug.Log("[LevelProgress] Progression remise à zéro : seul le niveau 1 est ouvert");
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PlayMusic()
    {
        if (menuMusic == null) return;
        AudioSource source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.clip = menuMusic;
        source.volume = volume;
        source.loop = true;
        source.Play();
    }
}
