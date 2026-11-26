using UnityEngine;

[CreateAssetMenu(fileName = "BattleTheme_Niveau", menuName = "Game/Battle Theme")]
public class BattleTheme : ScriptableObject
{
    [Header("Décor")]
    public Sprite background;
    [Tooltip("Fond de l'écran Game Over pour ce niveau. Vide = même fond que le combat.")]
    public Sprite gameOverBackground;

    [Header("Joueur en combat")]
    public RuntimeAnimatorController playerAnimator;
    public Sprite playerSprite;   // utilisé si playerAnimator est vide
    public float playerScale = 1f;

    [Header("Positions (1 unité = 16 pixels)")]
    public Vector2 playerOffset;
    public Vector2 enemyOffset;

    [Header("Monstre en combat")]
    public RuntimeAnimatorController enemyAnimator;
    public Sprite enemySprite;
    public bool enemyFlipX;
    public float enemyScale = 1f;

    [Header("Audio en combat")]
    public AudioClip battleMusic;
    public AudioClip attackStartClip;
    public AudioClip attackEndClip;
    public AudioClip healClip;
    [Tooltip("Son de l'écran Game Over pour ce niveau. Vide = son par défaut de la scène GameOver.")]
    public AudioClip gameOverClip;
}