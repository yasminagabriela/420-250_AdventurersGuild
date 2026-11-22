using UnityEngine;

[CreateAssetMenu(fileName = "BattleTheme_Niveau", menuName = "Game/Battle Theme")]
public class BattleTheme : ScriptableObject
{
    [Header("Décor")]
    public Sprite background;

    [Header("Joueur en combat")]
    public RuntimeAnimatorController playerAnimator;
    public Sprite playerSprite;

    [Header("Monstre en combat")]
    public Sprite enemySprite;
    public bool enemyFlipX;
    public float enemyScale = 1f;
    public Vector2 enemyOffset;
}