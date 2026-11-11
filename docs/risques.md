# Registre des risques — Adventurers Guild

Échelles : P et I de 1 à 3. Criticité = P × I
(1-2 faible, 3-4 moyenne, 6-9 élevée). Revu à chaque sprint planning.

## Synthèse (de la criticité la plus haute à la plus basse)

| ID | Risque | Criticité | Responsable |
|----|--------|-----------|-------------|
| R3 | Écran de victoire qui ne s'affiche pas | 6 (élevée) | Maliha |
| R9 | Deux remises la même semaine | 6 (élevée) | Talyssa |
| R1 | Conflits dans la scène de bataille | 4 (moyenne) | Yasmina |
| R4 | Synchronisation des particules de griffes | 4 (moyenne) | Maliha |
| R7 | Dépendance coffre → inventaire | 4 (moyenne) | Yasmina |
| R8 | Son du coffre dépendant de BF-05 | 4 (moyenne) | Talyssa |
| R2 | Case vide dans la barre d'inventaire | 2 (faible) | Yasmina |
| R5 | Réception des items des coffres | 2 (faible) | Jaynelle |
| R6 | Animation de soin | 2 (faible) | Jaynelle |

## R3 — Écran de victoire qui ne s'affiche pas

- **Risque :** Parce que l'écran de victoire dépend de la détection de l'arrivée du joueur à la fin de la carte, il est possible que l'écran de victoire ne s'affiche pas lorsque le joueur atteint la fin du niveau, ce qui entraînerait un jeu incomplet et le non-respect d'un besoin fonctionnel essentiel.
- **BF touchés :** BF-08
- **Catégorie :** Technique
- **P :** 2 · **I :** 3 · **Criticité :** 6 (élevée)
- **Stratégie :** Réduire
- **Mitigation :** L'issue de BF-08 contient la case « Le joueur atteint la fin de la carte et l'écran de victoire s'affiche », cochée par une autre membre que Maliha après un test sur main, avant le 20 novembre.
- **Déclencheur :** Lors du test sur main, le joueur atteint la fin de la carte et l'écran de victoire ne s'affiche pas.
- **Contingence :** Remplacer la détection par une zone d'arrivée avec un collider « Is Trigger » qui charge directement la scène de victoire, puis refaire le test.
- **Responsable :** Maliha
- **Statut :** Ouvert

## R9 — Deux remises la même semaine

- **Risque :** Parce que le TP UML (24 novembre) et le projet Unity (29 novembre) tombent la même semaine, il est possible que l'équipe manque de temps pour terminer et tester les BF du jeu, ce qui entraînerait des BF retirés ou une remise compromise.
- **BF touchés :** Tout le projet
- **Catégorie :** Planification
- **P :** 2 · **I :** 3 · **Criticité :** 6 (élevée)
- **Stratégie :** Réduire
- **Mitigation :** Les issues de BF-05 et BF-08 (Essentiels) ont une date d'échéance au plus tard le 20 novembre dans le Board, et une issue « Gel des fonctionnalités — 22 novembre » existe dans le Board.
- **Déclencheur :** Le 20 novembre, l'issue de BF-05 ou de BF-08 est encore ouverte dans le Board.
- **Contingence :** Retirer les BF Optionnels (BF-09, BF-10, BF-14, BF-17) du sprint en cours et réassigner leur responsable au BF Essentiel en retard jusqu'à la fermeture de son issue.
- **Responsable :** Talyssa
- **Statut :** Ouvert

## R1 — Conflits dans la scène de bataille

- **Risque :** Parce que les membres ne s'annoncent pas avant de modifier la scène de bataille et que les fichiers .unity se fusionnent mal avec Git, il est possible que deux personnes la modifient en même temps et que le merge produise un conflit dans le fichier de la scène, ce qui entraînerait la perte d'une partie du niveau et du temps de travail à refaire pour la reconstruire.
- **BF touchés :** BF-03, BF-04, BF-09, BF-14, BF-15
- **Catégorie :** Communication
- **P :** 2 · **I :** 2 · **Criticité :** 4 (moyenne)
- **Stratégie :** Réduire
- **Mitigation :** Une seule personne modifie la scène de bataille à la fois (annoncé au daily); chaque élément de combat est enregistré comme prefab dans Assets/Prefabs avant d'être placé dans la scène.
- **Déclencheur :** —
- **Contingence :** —
- **Responsable :** Yasmina
- **Statut :** Ouvert

## R4 — Synchronisation des particules de griffes

- **Risque :** Parce que BF-09 dépend des animations d'attaque du joueur et du déclenchement des effets de particules, il est possible que les particules de griffes ne s'affichent pas au bon moment ou ne se déclenchent pas correctement, ce qui entraînerait un manque de synchronisation entre les effets visuels et les attaques du joueur.
- **BF touchés :** BF-09, BF-15
- **Catégorie :** Technique
- **P :** 2 · **I :** 2 · **Criticité :** 4 (moyenne)
- **Stratégie :** Réduire
- **Mitigation :** Les particules sont déclenchées par un Animation Event dans le clip d'attaque, et l'issue de BF-09 contient la case « Particules visibles au moment du coup, testé en attaque et en réception d'attaque », cochée par une autre membre que Maliha.
- **Déclencheur :** —
- **Contingence :** —
- **Responsable :** Maliha
- **Statut :** Ouvert

## R7 — Dépendance coffre → inventaire

- **Risque :** Parce que BF-01 (barre d'inventaire, Yasmina) dépend de BF-12 (recevoir des items des coffres, Jaynelle), il est possible que la barre d'inventaire ne puisse pas être terminée ni testée avec de vrais items si BF-12 prend du retard, ce qui entraînerait le retard ou le retrait de BF-01.
- **BF touchés :** BF-01, BF-12
- **Catégorie :** Humain
- **P :** 2 · **I :** 2 · **Criticité :** 4 (moyenne)
- **Stratégie :** Réduire
- **Mitigation :** L'issue de BF-12 a une date d'échéance au 17 novembre dans le Board, et BF-01 est développé avec des items de test ajoutés sans coffre en attendant BF-12.
- **Déclencheur :** —
- **Contingence :** —
- **Responsable :** Yasmina
- **Statut :** Ouvert

## R8 — Son du coffre dépendant de BF-05

- **Risque :** Parce que BF-17 (effet sonore du coffre) dépend de BF-05 (ouvrir un coffre) de Yasmina, il est possible que le son ne puisse pas être ajouté ou testé si BF-05 prend du retard, ce qui entraînerait le retard ou le retrait de l'effet sonore du coffre.
- **BF touchés :** BF-17, BF-05
- **Catégorie :** Humain
- **P :** 2 · **I :** 2 · **Criticité :** 4 (moyenne)
- **Stratégie :** Réduire
- **Mitigation :** L'issue de BF-05 a une date d'échéance au 17 novembre dans le Board, et le son du coffre est importé et testé avec une touche temporaire avant que BF-05 soit terminé.
- **Déclencheur :** —
- **Contingence :** —
- **Responsable :** Talyssa
- **Statut :** Ouvert

## R2 — Case vide dans la barre d'inventaire

- **Risque :** Parce que le code de la barre d'inventaire ne vérifie pas si une case contient un objet avant de l'utiliser, il est possible que le joueur appuie sur une touche ou clique sur une case vide et déclenche une référence nulle, ce qui entraînerait un plantage du jeu ou une action qui ne fait rien sans aucun retour pour le joueur.
- **BF touchés :** BF-01
- **Catégorie :** Technique
- **P :** 2 · **I :** 1 · **Criticité :** 2 (faible)
- **Stratégie :** Accepter
- **Mitigation :** —
- **Déclencheur :** —
- **Contingence :** —
- **Responsable :** Yasmina
- **Statut :** Ouvert

## R5 — Réception des items des coffres

- **Risque :** Parce que BF-12 (recevoir des items venant des coffres) dépend du fonctionnement du système de récompenses des coffres, il est possible que les items ne soient pas correctement ajoutés à l'inventaire du joueur après l'ouverture d'un coffre, ce qui entraînerait une perte des récompenses prévues.
- **BF touchés :** BF-12
- **Catégorie :** Technique
- **P :** 1 · **I :** 2 · **Criticité :** 2 (faible)
- **Stratégie :** Réduire
- **Mitigation :** L'issue de BF-12 contient la liste des items que les coffres peuvent donner et le nom de la méthode qui les ajoute à l'inventaire.
- **Déclencheur :** —
- **Contingence :** —
- **Responsable :** Jaynelle
- **Statut :** Ouvert

## R6 — Animation de soin

- **Risque :** Parce que BF-13 (animation lorsque le joueur se soigne) dépend de BF-11 (option soigner), il est possible que l'animation ne se déclenche pas correctement lorsque le joueur utilise l'option soigner, ce qui entraînerait un manque de synchronisation entre l'action de soin et son animation.
- **BF touchés :** BF-13, BF-11
- **Catégorie :** Technique
- **P :** 1 · **I :** 2 · **Criticité :** 2 (faible)
- **Stratégie :** Réduire
- **Mitigation :** Le paramètre Trigger de l'animation de soin est activé dans la même méthode que l'augmentation de vie de BF-11, et l'issue de BF-13 contient la case « L'animation joue à chaque soin », cochée après un test.
- **Déclencheur :** —
- **Contingence :** —
- **Responsable :** Jaynelle
- **Statut :** Ouvert