using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData_Nom", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "Ennemi";
    public Sprite battleSprite;
    public int maxHealth = 12;
    public int attackDamage = 2;
}