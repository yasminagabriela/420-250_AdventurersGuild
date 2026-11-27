using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Affiche les 5 cases de Inventory : l'icône + la quantité en bas à droite (comme Minecraft)
public class InventoryBar : MonoBehaviour
{
    [SerializeField] private Image[] slotIcons;      // l'Image "Icon" de chaque case, dans l'ordre
    [SerializeField] private TMP_Text[] slotCounts;  // le texte "Count" de chaque case, dans l'ordre

    private void OnEnable()
    {
        Inventory.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        Inventory.Changed -= Refresh;
    }

    private void Refresh()
    {
        for (int i = 0; i < slotIcons.Length; i++)
        {
            InventorySlot s = Inventory.GetSlot(i);
            bool rempli = s != null;

            slotIcons[i].enabled = rempli;
            if (rempli) slotIcons[i].sprite = s.Item.icon;

            // Comme Minecraft : on n'écrit le nombre que s'il y en a plus d'un
            slotCounts[i].text = (rempli && s.Quantite > 1) ? s.Quantite.ToString() : "";
        }
    }
}