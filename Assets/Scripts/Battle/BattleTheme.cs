using UnityEngine;

[CreateAssetMenu(fileName = "BattleTheme_Niveau", menuName = "Game/Battle Theme")]
public class BattleTheme : ScriptableObject
{
    [Header("Décor")]
    public Sprite background;

    [Header("Joueur en combat")]
    public RuntimeAnimatorController playerAnimator;
    public Sprite playerSprite;   // utilisé si playerAnimator est vide

    [Header("Positions (1 unité = 16 pixels)")]
    public Vector2 playerPosition = Vector2.zero;
    public Vector2 enemyPosition = Vector2.zero;
}