using System.Collections;
using UnityEngine;

public class EnemySounds : MonoBehaviour
{
    public enum EnemySoundType
    {
        CombatStart, Attack, Hurt, LowHealth, Taunt, Death
    }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] defaultClips;
    [SerializeField] private float reactionDelay = 0.3f;

    private AudioClip[] clips;
    private float volume = 0.5f;

    private void Awake()
    {
        clips = defaultClips;

        if (BattleContext.Enemy != null)
        {
            volume = BattleContext.Enemy.volume;
            
            if (BattleContext.Enemy.clips != null && BattleContext.Enemy.clips.Length > 0)
                clips = BattleContext.Enemy.clips;
        }
    }

    public void Play(EnemySoundType type)
    {
        bool isReaction = type == EnemySoundType.Hurt
                      || type == EnemySoundType.LowHealth
                      || type == EnemySoundType.Death;

        if (isReaction)
            StartCoroutine(PlayAfterDelay(type, reactionDelay));
        else
            PlayNow(type);
    }

    private IEnumerator PlayAfterDelay(EnemySoundType type, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayNow(type);
    }

    private void PlayNow(EnemySoundType type)
    {
        int index = (int)type;

        if (index >= clips.Length || clips[index] == null)
        {
            Debug.LogWarning($"Aucun clip pour {type} sur {name}");
            return;
        }

        if (type == EnemySoundType.Death)
        {
            AudioSource.PlayClipAtPoint(clips[index], Camera.main.transform.position, volume);
            return;
        }

        audioSource.PlayOneShot(clips[index], volume);
    }
}