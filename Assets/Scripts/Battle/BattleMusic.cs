using UnityEngine;

public class BattleMusic : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip defaultMusic;
    [SerializeField, Range(0f, 1f)] private float volume = 0.05f;

    private void Start()
    {
        AudioClip clip = defaultMusic;

        if (BattleContext.Theme != null && BattleContext.Theme.battleMusic != null)
            clip = BattleContext.Theme.battleMusic;

        if (clip == null) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = volume;
        musicSource.Play();
    }
}
