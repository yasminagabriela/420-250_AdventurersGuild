using System;
using System.Collections.Generic;
using UnityEngine;

// Une case de l'inventaire : un type d'objet + combien le joueur en a
public class InventorySlot
{
    public ItemData Item;
    public int Quantite;

    public InventorySlot(ItemData item, int quantite)
    {
        Item = item;
        Quantite = quantite;
    }
}

// Static, comme BattleContext : le contenu survit aux changements de scène (carte ↔ combat).
public static class Inventory
{
    public const int Capacite = 5;

    // 5 cases fixes. null = case vide. Les objets gardent leur case (comme Minecraft).
    private static readonly InventorySlot[] slots = new InventorySlot[Capacite];

    // Objets déjà ramassés sur la carte, pour qu'ils ne réapparaissent pas après un combat
    public static readonly HashSet<string> RamassesIds = new HashSet<string>();

    // Prévient la barre d'inventaire qu'elle doit se redessiner
    public static event Action Changed;

    public static InventorySlot GetSlot(int index) =>
        (index >= 0 && index < Capacite) ? slots[index] : null;

    // Ajoute des objets : d'abord dans une case du même type, sinon dans la 1re case vide.
    // Renvoie false si rien n'a pu être ajouté (inventaire plein).
    public static bool AjouterItem(ItemData item, int quantite = 1)
    {
        if (item == null || quantite <= 0) return false;
        int restant = quantite;

        // 1) compléter les piles existantes
        for (int i = 0; i < Capacite && restant > 0; i++)
        {
            InventorySlot s = slots[i];
            if (s != null && s.Item == item && s.Quantite < item.maxPile)
            {
                int ajout = Mathf.Min(restant, item.maxPile - s.Quantite);
                s.Quantite += ajout;
                restant -= ajout;
            }
        }
        // 2) nouvelles cases
        for (int i = 0; i < Capacite && restant > 0; i++)
        {
            if (slots[i] == null)
            {
                int ajout = Mathf.Min(restant, item.maxPile);
                slots[i] = new InventorySlot(item, ajout);
                restant -= ajout;
            }
        }

        bool ok = restant < quantite;
        if (ok) Changed?.Invoke();
        return ok;
    }

    // Utilise 1 objet de la case index. Renvoie l'objet utilisé (null si la case est vide).
    public static ItemData UtiliserItem(int index)
    {
        InventorySlot s = GetSlot(index);
        if (s == null) return null;
        ItemData item = s.Item;
        s.Quantite--;
        if (s.Quantite <= 0) slots[index] = null;
        Changed?.Invoke();
        return item;
    }

    // Retire des objets d'un type (ex. : une potion utilisée en combat)
    public static bool RetirerItem(ItemData item, int quantite = 1)
    {
        if (Compter(item) < quantite) return false;
        for (int i = Capacite - 1; i >= 0 && quantite > 0; i--)
        {
            InventorySlot s = slots[i];
            if (s == null || s.Item != item) continue;
            int retrait = Mathf.Min(quantite, s.Quantite);
            s.Quantite -= retrait;
            quantite -= retrait;
            if (s.Quantite <= 0) slots[i] = null;
        }
        Changed?.Invoke();
        return true;
    }

    public static int Compter(ItemData item)
    {
        int total = 0;
        foreach (InventorySlot s in slots)
            if (s != null && s.Item == item) total += s.Quantite;
        return total;
    }

    // Inventaire vide (nouvelle partie / recommencer)
    public static void Vider()
    {
        for (int i = 0; i < Capacite; i++) slots[i] = null;
        RamassesIds.Clear();
        Changed?.Invoke();
    }

    // Le joueur commence chaque Play avec un inventaire vide
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        for (int i = 0; i < Capacite; i++) slots[i] = null;
        RamassesIds.Clear();
        Changed = null;
    }
}