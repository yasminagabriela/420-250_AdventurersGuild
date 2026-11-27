using UnityEngine;

// Un type d'objet (clé, potion...). Créer un asset : clic droit → Create → AdventurersGuild → Item
[CreateAssetMenu(fileName = "NouvelItem", menuName = "AdventurersGuild/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int soin;           // 0 pour une clé, ex. 20 pour une potion
    public int maxPile = 99;   // combien on peut en empiler dans une seule case
}