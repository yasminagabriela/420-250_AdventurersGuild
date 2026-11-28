using UnityEngine;

// Sur le Joueur. Le ramassage se fait maintenant avec ItemPickup (sur chaque clé),
// qui met la clé dans l'inventaire. Ce script sert à savoir si le joueur a une clé et à l'utiliser.
public class KeyPickup : MonoBehaviour
{
    [SerializeField] private ItemData keyItem;   // glisser Item_Cle

    public bool HasKey => Inventory.Compter(keyItem) > 0;

    public bool UseKey()
    {
        return Inventory.RetirerItem(keyItem);   // retire 1 clé de la barre, false s'il n'y en a pas
    }
}