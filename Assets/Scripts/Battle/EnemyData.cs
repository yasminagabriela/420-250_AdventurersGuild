using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData_Nom", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "Ennemi";
    public Sprite battleSprite;
    public int maxHealth = 12;
    public int attackDamage = 2;
    public AudioClip[] clips;
    [Range(0f, 1f)] public float volume = 0.5f;

    [Header("Affichage en combat (optionnel)")]
    public float battleScale = 1f;      // 1 = taille normale, 2 = deux fois plus gros
    public Vector2 battleOffset;        // (0, 0) = on ne déplace pas
    public bool flipX = false;          // coche pour retourner l'image
}