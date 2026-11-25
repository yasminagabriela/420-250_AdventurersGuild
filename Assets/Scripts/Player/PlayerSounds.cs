using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip debutAttackClip;
    [SerializeField] private AudioClip endAttackClip;
    [SerializeField] private AudioClip healClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float volumeEndClip = 1f;

    private void Awake()
    {
        BattleTheme theme = BattleContext.Theme;
        if (theme == null) return;

        if (theme.attackStartClip != null) debutAttackClip = theme.attackStartClip;
        if (theme.attackEndClip != null) endAttackClip = theme.attackEndClip;
        if (theme.healClip != null) healClip = theme.healClip;
    }

    public void PlayAttackStart() => PlayClip(debutAttackClip, volume);
    public void PlayAttackEnd() => PlayClip(endAttackClip, volume);
    public void PlayHeal() => PlayClip(healClip, volume);

    private void PlayClip(AudioClip clip, float volumeEndClip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip, volumeEndClip);
    }
}
