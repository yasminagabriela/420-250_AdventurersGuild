# Registre des risques — Adventurers Guild

Échelles : P et I de 1 à 3. Criticité = P × I
(1-2 faible, 3-4 moyenne, 6-9 élevée). Revu à chaque sprint planning.



## Synthèse (de la criticité la plus haute à la plus basse)

| ID | Risque | Criticité | Responsable |
|----|--------|-----------|-------------|
| R1 | Conflits dans la scène de bataille | 4 (moyenne) | [@yasminagabriela] |
| R2 | Case vide dans la barre d'inventaire | 2 (faible) | [@yasminagabriela] |



## R1 — Conflits dans la scène de bataille

- **Risque :** Parce que plusieurs membres ajoutent leurs éléments de combat (attaque, animations, particules, sons) dans la même scène de bataille (fichier .unity), il est possible qu'un merge produise un conflit qui efface une partie du niveau, ce qui entraînerait la perte de plusieurs jours de travail pour reconstruire la scène.
- **BF touchés :** BF-03, BF-04, BF-09, BF-14, BF-15
- **Catégorie :** Technique
- **P :** 2 · **I :** 2 · **Criticité :** 4 (moyenne)
- **Stratégie :** Réduire
- **Mitigation :** Chaque élément de combat est créé comme prefab dans Assets/Prefabs, et une seule personne, assignée à l'issue de la scène de bataille sur le Board, modifie le fichier .unity de la scène.
- **Déclencheur :** —
- **Contingence :** —
- **Responsable :** [@yasminagabriela]
- **Statut :** Ouvert


## R2 — Case vide dans la barre d'inventaire

- **Risque :** Parce que le code de la barre d'inventaire pourrait ne pas vérifier si une case contient un objet, il est possible que le joueur sélectionne une case vide et déclenche une référence nulle, ce qui entraînerait un plantage du jeu ou une action sans aucun retour pour le joueur.
- **BF touchés :** BF-01
- **Catégorie :** Technique
- **P :** 2 · **I :** 1 · **Criticité :** 2 (faible)
- **Stratégie :** Réduire
- **Mitigation :** L'issue BF-01 contient le critère « Sélectionner une case vide ne fait rien et ne cause aucune erreur dans la console », coché après un test dans Unity.
- **Déclencheur :** —
- **Contingence :** —
- **Responsable :** [@yasminagabriela]
- **Statut :** Ouvert
