using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace AdventurersGuild.EditorTools
{
    /// <summary>
    /// Menu Unity : Adventurers Guild > Niveau 4 (Ninja) > Tout construire
    /// Construit automatiquement, à partir des PNG de Art_Talyssa :
    ///  - réglages d'import pixel art (16 PPU, Point, sans compression)
    ///  - Tiles, animations + Animator Controllers, fiches EnemyData
    ///  - prefabs (ninja, ennemis, PNJ, animaux, décors, unité de combat)
    ///  - Scene_Level04 (d'après Tiles/Level03_Map.txt) et BattleScene
    /// On peut le relancer après avoir modifié un sprite dans Pixelorama.
    /// </summary>
    public static class Level03Builder
    {
        private const string Art = "Assets/Art/Art_Talyssa";
        private const string TileDir = Art + "/Tiles";
        private const string DataDir = Art + "/Enemies";
        private const string AnimDir = "Assets/Animation/Animations_Talyssa";
        private const string PrefabDir = "Assets/Prefabs/Prefabs_Talyssa";
        private const string LevelScenePath = "Assets/Scenes/Scene_Level04.unity";
        private const string BattleScenePath = "Assets/Scenes/BattleScene.unity";
        private const string MapPath = Art + "/Tiles/Level03_Map.txt";
        private const int PixelsPerUnit = 16;

        // ================================================================== MENU
        [MenuItem("Adventurers Guild/Niveau 4 (Ninja)/Tout construire", priority = 0)]
        public static void BuildAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            try
            {
                ShowProgress("Réglages d'import des sprites", 0.05f);
                ConfigureTextures();
                ShowProgress("Tiles", 0.2f);
                TileSet tiles = CreateTiles();
                ShowProgress("Animations", 0.35f);
                Dictionary<string, AnimatorController> ctrls = CreateAnimators();
                ShowProgress("Fiches ennemis", 0.5f);
                EnemyData[] enemies = CreateEnemyData();
                ShowProgress("Prefabs", 0.6f);
                Prefabs prefabs = CreatePrefabs(ctrls);
                ShowProgress("BattleScene", 0.75f);
                BuildBattleScene(enemies, prefabs);
                ShowProgress("Scene_Level04", 0.85f);
                BuildLevelScene(tiles, prefabs);
                AddScenesToBuildSettings();
                AssetDatabase.SaveAssets();
                EditorUtility.ClearProgressBar();
                EditorSceneManager.OpenScene(LevelScenePath);
                EditorUtility.DisplayDialog("Niveau 4 prêt !",
                    "Scene_Level04 et BattleScene ont été créées.\n\nAppuie sur Play pour jouer :\n" +
                    "WASD/flèches = bouger, E/Espace = parler/ouvrir, Entrée = valider en combat, Échap = retour.", "Super !");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogException(e);
                EditorUtility.DisplayDialog("Erreur", "La construction a échoué :\n" + e.Message, "OK");
            }
        }

        [MenuItem("Adventurers Guild/Niveau 4 (Ninja)/Seulement réimporter les sprites", priority = 20)]
        public static void ReimportSpritesOnly()
        {
            ConfigureTextures();
            Debug.Log("[Niveau 4] Sprites réimportés en pixel art (16 PPU, Point).");
        }

        /// <summary>
        /// Peint la carte (Level03_Map.txt) dans le Grid sélectionné de TA scène :
        /// tilemaps "Ground" (sol, eau, buissons, gué) et "Decoration" (fleurs).
        /// Ne crée aucun objet (ennemis, PNJ...), seulement les tuiles.
        /// </summary>
        [MenuItem("Adventurers Guild/Niveau 4 (Ninja)/Peindre la carte dans le Grid sélectionné", priority = 10)]
        public static void PaintIntoSelectedGrid()
        {
            GameObject sel = Selection.activeGameObject;
            Grid grid = sel != null ? sel.GetComponentInParent<Grid>() : null;
            if (grid == null)
            {
                EditorUtility.DisplayDialog("Aucun Grid sélectionné",
                    "Sélectionne ton objet Grid (ou un de ses Tilemaps) dans la Hierarchy, puis relance ce menu.", "OK");
                return;
            }
            try
            {
                ShowProgress("Réglages d'import des sprites", 0.2f);
                ConfigureTextures();
                ShowProgress("Tiles", 0.5f);
                TileSet tiles = CreateTiles();
                ShowProgress("Peinture de la carte", 0.8f);

                Tilemap ground = FindOrCreateTilemap(grid.gameObject, "Ground", 0);
                Tilemap decor = FindOrCreateTilemap(grid.gameObject, "Decoration", 1);
                Undo.RegisterCompleteObjectUndo(new Object[] { ground, decor }, "Peindre la carte niveau 4");
                ground.ClearAllTiles();
                decor.ClearAllTiles();
                PaintTiles(tiles, ground, decor);

                // Le joueur et les animaux doivent savoir quelles tuiles sont marchables.
                WalkableMap wm = grid.GetComponent<WalkableMap>();
                if (wm == null) wm = Undo.AddComponent<WalkableMap>(grid.gameObject);
                new P(wm).Ref("groundTilemap", ground).Refs("walkableTiles", tiles.Walkable).Apply();
                foreach (PlayerController pc in Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
                    new P(pc).Ref("groundTilemap", ground).Refs("walkableTiles", tiles.Walkable).Apply();

                EditorSceneManager.MarkSceneDirty(grid.gameObject.scene);
                EditorUtility.ClearProgressBar();
                Debug.Log("[Niveau 4] Carte peinte dans " + grid.name + ". N'oublie pas d'enregistrer la scène (Ctrl+S).");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogException(e);
                EditorUtility.DisplayDialog("Erreur", e.Message, "OK");
            }
        }

        private static Tilemap FindOrCreateTilemap(GameObject grid, string name, int order)
        {
            Transform t = grid.transform.Find(name);
            if (t != null && t.GetComponent<Tilemap>() != null) return t.GetComponent<Tilemap>();
            Tilemap tm = NewTilemap(grid, name, order);
            Undo.RegisterCreatedObjectUndo(tm.gameObject, "Créer " + name);
            return tm;
        }

        /// <summary>Peint seulement les tuiles de Level03_Map.txt (sans objets ni lumières).</summary>
        private static void PaintTiles(TileSet tiles, Tilemap ground, Tilemap decor)
        {
            TextAsset mapAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(MapPath);
            if (mapAsset == null) throw new Exception("Carte introuvable : " + MapPath);
            string[] rows = mapAsset.text.Replace("\r", "").Split('\n').Where(r => r.Length > 0).ToArray();
            int height = rows.Length;
            char At(int col, int row) => (row < 0 || row >= height || col < 0 || col >= rows[row].Length) ? '#' : rows[row][col];

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < rows[row].Length; col++)
                {
                    char c = At(col, row);
                    var cell = new Vector3Int(col, height - 1 - row, 0);
                    TileBase t;
                    if (c == '~') t = tiles.Water[WaterMask(At, col, row)];
                    else if (c == '=' || c == 'R' || c == 'H') t = tiles.Stones;
                    else if (c == '#') t = tiles.Bush[(col * 7 + row * 3) % 2];
                    else if (c == 'x' || c == 'G' || c == 'T' || c == 'U' || c == 'C') t = tiles.GroundBlocked;
                    else t = tiles.Ground[(col * 13 + row * 7) % 4];
                    ground.SetTile(cell, t);
                    if (c == '*') decor.SetTile(cell, tiles.FlowerPink);
                    else if (c == '+') decor.SetTile(cell, tiles.FlowerBlue);
                    else if (c == ',') decor.SetTile(cell, tiles.FlowersSmall);
                }
            }
            ground.CompressBounds();
            decor.CompressBounds();
        }

        private static void ShowProgress(string step, float t) =>
            EditorUtility.DisplayProgressBar("Adventurers Guild - Niveau 4", step, t);

        // ================================================================== IMPORT
        private static void ConfigureTextures()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Art }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is TextureImporter imp)) continue;
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.spritePixelsPerUnit = PixelsPerUnit;
                imp.filterMode = FilterMode.Point;
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.mipmapEnabled = false;
                imp.alphaIsTransparency = true;
                imp.wrapMode = TextureWrapMode.Clamp;
                var s = new TextureImporterSettings();
                imp.ReadTextureSettings(s);
                s.spriteAlignment = (int)SpriteAlignment.Center;
                s.spriteMeshType = SpriteMeshType.FullRect;
                imp.SetTextureSettings(s);
                imp.SaveAndReimport();
            }
        }

        private static Sprite S(string rel)
        {
            string path = $"{Art}/{rel}.png";
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sp == null) throw new Exception($"Sprite introuvable : {path}");
            return sp;
        }

        private static Sprite[] Frames(string prefix, params string[] names) =>
            names.Select(n => S($"{prefix}_{n}")).ToArray();

        // ================================================================== TILES
        private class TileSet
        {
            public Tile[] Ground, Bush, Water;
            public Tile GroundBlocked, Stones, FlowerPink, FlowerBlue, FlowersSmall;
            public TileBase[] Walkable => Ground.Cast<TileBase>().Append(Stones).ToArray();
        }

        private static TileSet CreateTiles()
        {
            EnsureFolder(TileDir);
            var t = new TileSet
            {
                Ground = Enumerable.Range(0, 4).Select(i => MakeTile($"Sol_{i}", S($"Tiles/sol_{i}"))).ToArray(),
                Bush = Enumerable.Range(0, 2).Select(i => MakeTile($"Buisson_{i}", S($"Tiles/buisson_{i}"))).ToArray(),
                Water = Enumerable.Range(0, 16).Select(i => MakeTile($"Eau_{i:00}", S($"Tiles/eau_{i:00}"))).ToArray(),
                GroundBlocked = MakeTile("Sol_Bloque", S("Tiles/sol_0")),
                Stones = MakeTile("Pierres_Gue", S("Tiles/pierres_gue")),
                FlowerPink = MakeTile("Fleur_Rose", S("Decorations/fleur_rose")),
                FlowerBlue = MakeTile("Fleur_Bleue", S("Decorations/fleur_bleue")),
                FlowersSmall = MakeTile("Fleurs_Petites", S("Decorations/fleurs_petites")),
            };
            return t;
        }

        private static Tile MakeTile(string name, Sprite sprite)
        {
            string path = $"{TileDir}/{name}.asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        // ================================================================== ANIMATIONS
        private static Dictionary<string, AnimatorController> CreateAnimators()
        {
            var result = new Dictionary<string, AnimatorController>();

            // Ninja : Idle / Move (paramètre IsMoving, comme PlayerController)
            string nd = AnimDir + "/Ninja";
            EnsureFolder(nd);
            var nIdle = MakeClip($"{nd}/Ninja_Idle.anim", Frames("Player_level04_Ninja/ninja", "idle_0", "idle_1"), 3, true);
            var nWalk = MakeClip($"{nd}/Ninja_Walk.anim", Frames("Player_level04_Ninja/ninja", "walk_0", "walk_1", "walk_2", "walk_3"), 8, true);
            result["ninja"] = MakeController($"{nd}/ninja.controller", nIdle, nWalk, null);

            // Ennemis : Idle / Move / Attack (paramètres IsMoving + trigger Attack, comme EnemyPatrol et BattleTrigger)
            var enemies = new (string key, string[] idle)[]
            {
                ("serpent", new[] { "idle_0", "idle_1" }),
                ("champignon", new[] { "idle_0", "idle_1" }),
                ("feufollet", new[] { "idle_0", "idle_1", "idle_2" }),
                ("sirene", new[] { "idle_0", "idle_1" }),
            };
            foreach (var (key, idle) in enemies)
            {
                string dir = $"{AnimDir}/{Cap(key)}";
                EnsureFolder(dir);
                Sprite[] idleFrames = Frames($"Enemies/{key}", idle);
                var cIdle = MakeClip($"{dir}/{Cap(key)}_Idle.anim", idleFrames, 3, true);
                var cMove = MakeClip($"{dir}/{Cap(key)}_Move.anim", idleFrames, 7, true);
                var cAtk = MakeClip($"{dir}/{Cap(key)}_Attack.anim", Frames($"Enemies/{key}", "attack_0", "attack_0"), 4, false);
                result[key] = MakeController($"{dir}/{key}.controller", cIdle, cMove, cAtk);
            }
            return result;
        }

        private static string Cap(string s) => char.ToUpperInvariant(s[0]) + s.Substring(1);

        private static AnimationClip MakeClip(string path, Sprite[] frames, float fps, bool loop)
        {
            AssetDatabase.DeleteAsset(path);
            var clip = new AnimationClip { frameRate = fps };
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[frames.Length + 1];
            for (int i = 0; i < frames.Length; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
            keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / fps, value = frames[frames.Length - 1] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static AnimatorController MakeController(string path, AnimationClip idle, AnimationClip move, AnimationClip attack)
        {
            AssetDatabase.DeleteAsset(path);
            AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
            ctrl.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            AnimatorStateMachine sm = ctrl.layers[0].stateMachine;

            AnimatorState sIdle = sm.AddState("Idle");
            sIdle.motion = idle;
            sm.defaultState = sIdle;
            AnimatorState sMove = sm.AddState("Move");
            sMove.motion = move;

            AnimatorStateTransition toMove = sIdle.AddTransition(sMove);
            toMove.hasExitTime = false; toMove.duration = 0f;
            toMove.AddCondition(AnimatorConditionMode.If, 0f, "IsMoving");
            AnimatorStateTransition toIdle = sMove.AddTransition(sIdle);
            toIdle.hasExitTime = false; toIdle.duration = 0f;
            toIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsMoving");

            if (attack != null)
            {
                ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
                AnimatorState sAtk = sm.AddState("Attack");
                sAtk.motion = attack;
                AnimatorStateTransition any = sm.AddAnyStateTransition(sAtk);
                any.hasExitTime = false; any.duration = 0f; any.canTransitionToSelf = false;
                any.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
                AnimatorStateTransition back = sAtk.AddTransition(sIdle);
                back.hasExitTime = true; back.exitTime = 1f; back.duration = 0f;
            }
            EditorUtility.SetDirty(ctrl);
            return ctrl;
        }

        // ================================================================== DONNÉES ENNEMIS
        private static EnemyData[] CreateEnemyData()
        {
            EnsureFolder(DataDir);
            return new[]
            {
                Enemy("serpent", "Serpent", false, 18, 6, 2, 9, "Morsure venimeuse", EnemySkillType.PoisonBite, 0.3f, 0.5f, 7,
                      8, 6, 1, 3, 3f, new[] { "idle_0", "idle_1" }),
                Enemy("champignon", "Champignon toxique", false, 24, 5, 3, 4, "Spores toxiques", EnemySkillType.PoisonBite, 0.45f, 0.6f, 5,
                      10, 8, 1, 2, 3f, new[] { "idle_0", "idle_1" }),
                Enemy("feufollet", "Feu follet", false, 16, 6, 1, 13, "Flamme spectrale", EnemySkillType.Magic, 0.5f, 0f, 8,
                      12, 10, 1, 2, 3f, new[] { "idle_0", "idle_1", "idle_2" }),
                Enemy("sirene", "Sirène", true, 110, 10, 4, 10, "Chant envoûtant", EnemySkillType.Sleep, 0.3f, 0.45f, 0,
                      80, 100, 1, 1, 2.6f, new[] { "idle_0", "idle_1" }),
            };
        }

        private static EnemyData Enemy(string id, string display, bool boss, int hp, int atk, int def, int agi,
                                       string skill, EnemySkillType type, float skillChance, float statusChance, int power,
                                       int xp, int gold, int gMin, int gMax, float scale, string[] idle)
        {
            string path = $"{DataDir}/Enemy_{Cap(id)}.asset";
            EnemyData d = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (d == null)
            {
                d = ScriptableObject.CreateInstance<EnemyData>();
                AssetDatabase.CreateAsset(d, path);
            }
            d.id = id; d.displayName = display; d.isBoss = boss;
            d.maxHp = hp; d.attack = atk; d.defense = def; d.agility = agi;
            d.skillName = skill; d.skillType = type; d.skillChance = skillChance; d.statusChance = statusChance; d.skillPower = power;
            d.xpReward = xp; d.goldReward = gold; d.groupMin = gMin; d.groupMax = gMax;
            d.battleScale = scale;
            d.idleFrames = Frames($"Enemies/{id}", idle);
            d.attackFrame = S($"Enemies/{id}_attack_0");
            EditorUtility.SetDirty(d);
            return d;
        }

        // ================================================================== PREFABS
        private class Prefabs
        {
            public GameObject Player, Serpent, Champignon, FeuFollet, Sirene;
            public GameObject Ermite, Fee, Renard, Hibou, Papillon;
            public GameObject Coffre, Arbre, Kiosque, Ruines;
            public BattleUnit BattleUnit;
        }

        private static Prefabs CreatePrefabs(Dictionary<string, AnimatorController> ctrls)
        {
            EnsureFolder(PrefabDir);
            var p = new Prefabs();

            // ---- Joueur ninja (réutilise PlayerController de l'équipe)
            {
                GameObject go = NewSpriteObject("Player_Ninja", S("Player_level04_Ninja/ninja_idle_0"));
                go.tag = "Player";
                var anim = go.AddComponent<Animator>();
                anim.runtimeAnimatorController = ctrls["ninja"];
                Rigidbody2D rb = AddKinematicBody(go);
                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.7f, 0.7f);
                var pc = go.AddComponent<PlayerController>();
                new P(pc).Ref("body", rb).Ref("animator", anim).Ref("spriteRenderer", go.GetComponent<SpriteRenderer>())
                         .Float("moveSpeed", 5f).Vec3("feetOffset", Vector3.zero).Apply();
                AddYSort(go, 0f, false);
                p.Player = SavePrefab(go);
            }

            // ---- Ennemis (réutilisent EnemyPatrol + BattleTrigger de l'équipe)
            p.Serpent = EnemyPrefab("Enemy_Serpent", "serpent", ctrls, 0.42f, true, null);
            p.Champignon = EnemyPrefab("Enemy_Champignon", "champignon", ctrls, 0.42f, true, null);
            p.FeuFollet = EnemyPrefab("Enemy_FeuFollet", "feufollet", ctrls, 0.42f, true, new Color(0.4f, 1f, 0.95f));
            p.Sirene = EnemyPrefab("Enemy_Sirene", "sirene", ctrls, 0.55f, false, new Color(1f, 0.5f, 0.85f));

            // ---- PNJ
            {
                GameObject go = NewSpriteObject("NPC_Ermite", S("NPC/ermite_idle_0"));
                Flipbook(go, Frames("NPC/ermite", "idle_0", "idle_1"), null, 2f, Vector2.zero);
                go.AddComponent<NpcDialogue>();
                AddYSort(go, 0f, true);
                AddLight(go, new Color(1f, 0.8f, 0.4f), 2.2f, 0.9f, new Vector3(0.35f, 0f, 0f), true);
                p.Ermite = SavePrefab(go);
            }
            {
                GameObject go = NewSpriteObject("NPC_Fee", S("NPC/fee_idle_0"));
                Flipbook(go, Frames("NPC/fee", "idle_0", "idle_1"), null, 5f, Vector2.zero);
                go.AddComponent<NpcDialogue>();
                var fl = go.AddComponent<FloatAround>();
                new P(fl).Vec2("amplitude", new Vector2(0.15f, 0.12f)).Float("speed", 1.5f).Ref("spriteRenderer", go.GetComponent<SpriteRenderer>()).Apply();
                AddYSort(go, 0f, false);
                AddLight(go, new Color(1f, 0.55f, 0.9f), 2.5f, 1f, Vector3.zero, true);
                p.Fee = SavePrefab(go);
            }

            // ---- Animaux
            {
                GameObject go = NewSpriteObject("Animal_Renard", S("NPC/renard_idle_0"));
                SpriteFlipbook fb = Flipbook(go, Frames("NPC/renard", "idle_0", "idle_1"), Frames("NPC/renard", "walk_0", "walk_1"), 4f, Vector2.zero);
                var w = go.AddComponent<AnimalWander>();
                new P(w).Ref("spriteRenderer", go.GetComponent<SpriteRenderer>()).Ref("flipbook", fb).Apply();
                AddYSort(go, 0f, false);
                p.Renard = SavePrefab(go);
            }
            {
                GameObject go = NewSpriteObject("Animal_Hibou", S("NPC/hibou_idle_0"));
                Flipbook(go, Frames("NPC/hibou", "idle_0", "idle_1"), null, 6f, new Vector2(1.5f, 4f));
                AddYSort(go, 0f, true);
                p.Hibou = SavePrefab(go);
            }
            {
                GameObject go = NewSpriteObject("Animal_Papillon", S("NPC/papillon_idle_0"));
                Flipbook(go, Frames("NPC/papillon", "idle_0", "idle_1"), null, 6f, Vector2.zero);
                var fl = go.AddComponent<FloatAround>();
                new P(fl).Vec2("amplitude", new Vector2(0.9f, 0.35f)).Float("speed", 0.9f).Ref("spriteRenderer", go.GetComponent<SpriteRenderer>()).Apply();
                AddYSort(go, -0.5f, false);
                AddLight(go, new Color(0.6f, 0.85f, 1f), 1.2f, 0.7f, Vector3.zero, true);
                p.Papillon = SavePrefab(go);
            }

            // ---- Décors
            {
                GameObject go = NewSpriteObject("Prop_Coffre", S("Decorations/coffre_ferme"));
                var chest = go.AddComponent<TreasureChest>();
                new P(chest).Ref("spriteRenderer", go.GetComponent<SpriteRenderer>())
                            .Ref("closedSprite", S("Decorations/coffre_ferme")).Ref("openSprite", S("Decorations/coffre_ouvert")).Apply();
                AddYSort(go, 0f, true);
                AddLight(go, new Color(1f, 0.85f, 0.4f), 1.4f, 0.6f, Vector3.zero, true);
                p.Coffre = SavePrefab(go);
            }
            {
                GameObject go = NewSpriteObject("Prop_ArbreRose", S("Decorations/arbre_rose"));
                AddYSort(go, -1.2f, true);
                AddLight(go, new Color(1f, 0.55f, 0.95f), 4.5f, 1.1f, new Vector3(0f, 0.5f, 0f), true);
                p.Arbre = SavePrefab(go);
            }
            {
                GameObject go = NewSpriteObject("Prop_KiosqueDore", S("Decorations/kiosque_dore"));
                AddYSort(go, -0.8f, true);
                AddLight(go, new Color(1f, 0.85f, 0.45f), 3.5f, 0.9f, Vector3.zero, true);
                p.Kiosque = SavePrefab(go);
            }
            {
                GameObject go = NewSpriteObject("Prop_Ruines", S("Decorations/ruines"));
                AddYSort(go, -0.4f, true);
                p.Ruines = SavePrefab(go);
            }

            // ---- Unité de combat (instanciée par BattleManager)
            {
                GameObject go = NewSpriteObject("BattleUnit", S("Player_level04_Ninja/ninja_idle_0"));
                var bu = go.AddComponent<BattleUnit>();
                new P(bu).Ref("spriteRenderer", go.GetComponent<SpriteRenderer>()).Apply();
                go.GetComponent<SpriteRenderer>().sortingOrder = 20;
                p.BattleUnit = SavePrefab(go).GetComponent<BattleUnit>();
            }
            return p;
        }

        private static GameObject EnemyPrefab(string name, string key, Dictionary<string, AnimatorController> ctrls,
                                              float radius, bool patrols, Color? light)
        {
            GameObject go = NewSpriteObject(name, S($"Enemies/{key}_idle_0"));
            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = ctrls[key];
            Rigidbody2D rb = AddKinematicBody(go);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = radius;
            EnemyPatrol patrol = null;
            if (patrols)
            {
                patrol = go.AddComponent<EnemyPatrol>();
                new P(patrol).Ref("body", rb).Ref("animator", anim).Ref("spriteRenderer", go.GetComponent<SpriteRenderer>())
                             .Float("moveSpeed", 1.5f).Float("waitTime", 1.2f).Bool("pingPong", true).Apply();
            }
            var trig = go.AddComponent<BattleTrigger>();
            new P(trig).Str("enemyId", key + "_1").Str("battleSceneName", "BattleScene").Ref("animator", anim)
                       .Ref("patrol", patrol).Float("attackDelay", 0.6f).Apply();
            AddYSort(go, 0f, false);
            if (light.HasValue) AddLight(go, light.Value, 2.2f, 1f, Vector3.zero, true);
            return SavePrefab(go);
        }

        private static GameObject NewSpriteObject(string name, Sprite sprite)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            return go;
        }

        private static Rigidbody2D AddKinematicBody(GameObject go)
        {
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            return rb;
        }

        private static SpriteFlipbook Flipbook(GameObject go, Sprite[] idle, Sprite[] move, float fps, Vector2 hold)
        {
            var fb = go.AddComponent<SpriteFlipbook>();
            new P(fb).Refs("idleFrames", idle).Refs("moveFrames", move ?? new Sprite[0])
                     .Float("framesPerSecond", fps).Vec2("holdFirstFrame", hold).Apply();
            return fb;
        }

        private static void AddYSort(GameObject go, float yOffset, bool isStatic)
        {
            var ys = go.AddComponent<YSort>();
            new P(ys).Float("yOffset", yOffset).Bool("isStatic", isStatic).Apply();
        }

        private static Light2D AddLight(GameObject parent, Color color, float radius, float intensity, Vector3 offset, bool pulse)
        {
            var go = new GameObject("Lumiere");
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = offset;
            var l = go.AddComponent<Light2D>();
            l.lightType = Light2D.LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.pointLightInnerRadius = 0f;
            l.pointLightOuterRadius = radius;
            if (pulse)
            {
                var gp = go.AddComponent<GlowPulse>();
                new P(gp).Float("minIntensity", intensity * 0.6f).Float("maxIntensity", intensity * 1.15f)
                         .Float("speed", UnityEngine.Random.Range(1.0f, 2.2f)).Apply();
            }
            return l;
        }

        private static GameObject SavePrefab(GameObject go)
        {
            string path = $"{PrefabDir}/{go.name}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ================================================================== SCÈNE DE COMBAT
        private static void BuildBattleScene(EnemyData[] enemies, Prefabs p)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(5.625f, new Color(0.03f, 0.06f, 0.12f));
            CreateGlobalLight(Color.white, 1f);

            var bg = NewSpriteObject("Fond_Foret", S("Decorations/fond_combat_foret"));
            bg.GetComponent<SpriteRenderer>().sortingOrder = -100;

            var mgr = new GameObject("BattleManager").AddComponent<BattleManager>();
            new P(mgr).Refs("enemyDatabase", enemies).Ref("unitPrefab", p.BattleUnit)
                      .Refs("heroIdleFrames", Frames("Player_level04_Ninja/ninja", "idle_0", "idle_1"))
                      .Ref("heroAttackFrame", S("Player_level04_Ninja/ninja_attack_1"))
                      .Str("fallbackReturnScene", "Scene_Level04").Apply();

            EditorSceneManager.SaveScene(scene, BattleScenePath);
        }

        // ================================================================== SCÈNE NIVEAU 3
        private static readonly Dictionary<char, (string id, string kind)> EnemyMarkers = new Dictionary<char, (string, string)>
        {
            { 'S', ("serpent_1", "serpent") }, { 's', ("serpent_2", "serpent") },
            { 'M', ("champignon_1", "champignon") }, { 'm', ("champignon_2", "champignon") },
            { 'W', ("feufollet_1", "feufollet") }, { 'R', ("sirene_boss", "sirene") },
        };

        private const string WalkableChars = ".*+,=PSsMmWRHYFOBbC";

        private static void BuildLevelScene(TileSet tiles, Prefabs p)
        {
            TextAsset mapAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(MapPath);
            if (mapAsset == null) throw new Exception("Carte introuvable : " + MapPath);
            string[] rows = mapAsset.text.Replace("\r", "").Split('\n').Where(r => r.Length > 0).ToArray();
            int height = rows.Length, width = rows[0].Length;
            char At(int col, int row) => (row < 0 || row >= height || col < 0 || col >= rows[row].Length) ? '#' : rows[row][col];
            Vector3Int Cell(int col, int row) => new Vector3Int(col, height - 1 - row, 0);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera cam = CreateCamera(6f, new Color(0.02f, 0.05f, 0.09f));
            CreateGlobalLight(new Color(0.55f, 0.62f, 0.95f), 0.55f);

            // ---- Grid + tilemaps
            var gridGo = new GameObject("Grid");
            gridGo.AddComponent<Grid>().cellSize = new Vector3(1, 1, 0);
            Tilemap ground = NewTilemap(gridGo, "Ground", 0);
            Tilemap decor = NewTilemap(gridGo, "Decoration", 1);
            var walkMap = gridGo.AddComponent<WalkableMap>();
            new P(walkMap).Ref("groundTilemap", ground).Refs("walkableTiles", tiles.Walkable).Apply();

            var lights = new GameObject("Lumieres").transform;
            var entities = new GameObject("Objects").transform;
            GameObject player = null;
            bool flowerToggle = false;

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    char c = At(col, row);
                    Vector3Int cell = Cell(col, row);
                    Vector3 center = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);

                    // sol
                    TileBase t;
                    if (c == '~') t = tiles.Water[WaterMask(At, col, row)];
                    else if (c == '=' || c == 'R' || c == 'H') t = tiles.Stones;
                    else if (c == '#') t = tiles.Bush[(col * 7 + row * 3) % 2];
                    else if (c == 'x' || c == 'G' || c == 'T' || c == 'U' || c == 'C') t = tiles.GroundBlocked;
                    else t = tiles.Ground[(col * 13 + row * 7) % 4];
                    ground.SetTile(cell, t);

                    // lumière de l'eau (une sur quelques cases)
                    if (c == '~' && col % 4 == 0 && row % 3 == 0)
                    {
                        var go = new GameObject("Lumiere_Eau");
                        go.transform.SetParent(lights, false);
                        go.transform.position = center;
                        AddLightComponent(go, new Color(0.35f, 1f, 0.9f), 2.6f, 0.55f, 0.5f);
                    }

                    // fleurs
                    if (c == '*' || c == '+' || c == ',')
                    {
                        decor.SetTile(cell, c == '*' ? tiles.FlowerPink : c == '+' ? tiles.FlowerBlue : tiles.FlowersSmall);
                        flowerToggle = !flowerToggle;
                        if (c != ',' && flowerToggle)
                        {
                            var go = new GameObject("Lumiere_Fleur");
                            go.transform.SetParent(lights, false);
                            go.transform.position = center;
                            Color fc = c == '*' ? new Color(1f, 0.45f, 0.85f) : new Color(0.5f, 0.65f, 1f);
                            AddLightComponent(go, fc, 1.3f, 0.8f, 2f);
                        }
                    }

                    // entités
                    GameObject inst = null;
                    switch (c)
                    {
                        case 'P':
                            player = Place(p.Player, center, entities);
                            break;
                        case 'G': inst = Place(p.Kiosque, center, entities); break;
                        case 'T': inst = Place(p.Arbre, center, entities); break;
                        case 'U': inst = Place(p.Ruines, center, entities); break;
                        case 'C': inst = Place(p.Coffre, center, entities); inst.name = "Coffre_Ruines"; break;
                        case 'F':
                            inst = Place(p.Renard, center, entities);
                            break;
                        case 'O': inst = Place(p.Hibou, center, entities); break;
                        case 'B':
                        case 'b': inst = Place(p.Papillon, center, entities); break;
                        case 'H':
                            inst = Place(p.Ermite, center, entities);
                            inst.name = "Ermite";
                            ConfigureHermit(inst.GetComponent<NpcDialogue>());
                            break;
                        case 'Y':
                            inst = Place(p.Fee, center, entities);
                            inst.name = "Fee_Lumi";
                            ConfigureFairy(inst.GetComponent<NpcDialogue>());
                            break;
                    }

                    if (EnemyMarkers.TryGetValue(c, out var enemy))
                    {
                        GameObject prefab = enemy.kind == "serpent" ? p.Serpent : enemy.kind == "champignon" ? p.Champignon
                                          : enemy.kind == "feufollet" ? p.FeuFollet : p.Sirene;
                        Vector3 pos = enemy.kind == "sirene" ? center + new Vector3(0f, 0.4f, 0f) : center;
                        inst = Place(prefab, pos, entities);
                        inst.name = "Enemy_" + enemy.id;
                        new P(inst.GetComponent<BattleTrigger>()).Str("enemyId", enemy.id).Apply();
                        EnemyPatrol patrol = inst.GetComponent<EnemyPatrol>();
                        if (patrol != null && enemy.kind != "champignon")
                            CreateWaypoints(patrol, enemy.id, col, row, At, Cell, entities);
                    }
                }
            }
            ground.CompressBounds();
            decor.CompressBounds();

            if (player == null) throw new Exception("La carte n'a pas de 'P' (position de départ du joueur).");
            new P(player.GetComponent<PlayerController>()).Ref("groundTilemap", ground).Refs("walkableTiles", tiles.Walkable).Apply();

            // caméra qui suit le ninja (CameraFollow de l'équipe)
            cam.transform.position = player.transform.position + new Vector3(0, 0, -10);
            var follow = cam.gameObject.AddComponent<CameraFollow>();
            new P(follow).Ref("target", player.transform).Vec3("offset", new Vector3(0, 0, -10)).Apply();

            new GameObject("HUD").AddComponent<OverworldHud>();

            EditorSceneManager.SaveScene(scene, LevelScenePath);
        }

        private static int WaterMask(Func<int, int, char> at, int col, int row)
        {
            bool W(char ch) => ch == '~' || ch == '=' || ch == 'R' || ch == 'H';
            int m = 0;
            if (W(at(col, row - 1))) m |= 1;  // nord
            if (W(at(col + 1, row))) m |= 2;  // est
            if (W(at(col, row + 1))) m |= 4;  // sud
            if (W(at(col - 1, row))) m |= 8;  // ouest
            return m;
        }

        private static void CreateWaypoints(EnemyPatrol patrol, string id, int col, int row,
                                            Func<int, int, char> at, Func<int, int, Vector3Int> cell, Transform parent)
        {
            // cherche la direction (droite, gauche, haut, bas) avec le plus de cases libres (max 3)
            var dirs = new[] { (1, 0), (-1, 0), (0, -1), (0, 1) };
            (int dx, int dy) best = (0, 0);
            int bestLen = 0;
            foreach (var (dx, dy) in dirs)
            {
                int len = 0;
                while (len < 3 && WalkableChars.IndexOf(at(col + dx * (len + 1), row + dy * (len + 1))) >= 0) len++;
                if (len > bestLen) { bestLen = len; best = (dx, dy); }
            }
            if (bestLen < 1) return;

            var root = new GameObject("Waypoints_" + id).transform;
            root.SetParent(parent, false);
            Transform MakeWp(string n, int c, int r)
            {
                Vector3Int ce = cell(c, r);
                var wp = new GameObject(n).transform;
                wp.SetParent(root, false);
                wp.position = new Vector3(ce.x + 0.5f, ce.y + 0.5f, 0f);
                return wp;
            }
            Transform a = MakeWp("WP_1", col, row);
            Transform b = MakeWp("WP_2", col + best.dx * bestLen, row + best.dy * bestLen);
            new P(patrol).Refs("waypoints", new Object[] { a, b }).Apply();
        }

        private static void ConfigureHermit(NpcDialogue npc)
        {
            new P(npc).Str("speakerName", "Ermite du kiosque")
                .Strs("lines", new[]
                {
                    "Ah... un ninja ! Personne n'était venu jusqu'au kiosque doré depuis bien longtemps.",
                    "Une sirène s'est installée sur le gué de l'étang aux fleurs. Son chant endort les voyageurs.",
                    "Si tu tombes sous son charme, un coup peut te réveiller... Garde des Potions sous la main.",
                    "Les serpents et les champignons empoisonnent. La Purification ou un Antidote te sauveront.",
                    "Tiens, prends ceci. Que la lumière de la forêt te guide.",
                })
                .Str("requiredDefeatedEnemy", "sirene_boss")
                .Strs("linesAfter", new[]
                {
                    "Le chant s'est tu... Tu as vaincu la sirène !",
                    "La fée Lumi t'attend sur l'île de l'arbre rose. Va la voir.",
                })
                .Enum("reward", (int)NpcDialogue.Reward.TwoPotions)
                .Str("rewardMessage", "Tu reçois 2 Potions !")
                .Apply();
        }

        private static void ConfigureFairy(NpcDialogue npc)
        {
            new P(npc).Str("speakerName", "Fée Lumi")
                .Strs("lines", new[] { "Psst... La sirène garde le gué. Je ne peux pas quitter mon île tant qu'elle est là." })
                .Str("requiredDefeatedEnemy", "sirene_boss")
                .Strs("linesAfter", new[]
                {
                    "Tu as libéré l'étang ! Les lucioles dansent à nouveau.",
                    "Laisse-moi soigner tes blessures, brave ninja...",
                    "Le chemin vers le boss final s'ouvre devant toi. Bonne chance, aventurier !",
                })
                .Enum("reward", (int)NpcDialogue.Reward.EtherAndHeal)
                .Bool("rewardOnlyAfterVictory", true)
                .Str("rewardMessage", "Tes PV et PM sont restaurés ! Tu reçois 2 Éthers.")
                .Apply();
        }

        private static GameObject Place(GameObject prefab, Vector3 position, Transform parent)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            return go;
        }

        private static Tilemap NewTilemap(GameObject grid, string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(grid.transform, false);
            var tm = go.AddComponent<Tilemap>();
            go.AddComponent<TilemapRenderer>().sortingOrder = order;
            return tm;
        }

        private static Camera CreateCamera(float size, Color background)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0, 0, -10);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = size;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            go.AddComponent<AudioListener>();
            return cam;
        }

        private static void CreateGlobalLight(Color color, float intensity)
        {
            var go = new GameObject("Global Light 2D");
            var l = go.AddComponent<Light2D>();
            l.lightType = Light2D.LightType.Global;
            l.color = color;
            l.intensity = intensity;
        }

        private static void AddLightComponent(GameObject go, Color color, float radius, float intensity, float pulseSpeed)
        {
            var l = go.AddComponent<Light2D>();
            l.lightType = Light2D.LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.pointLightInnerRadius = 0f;
            l.pointLightOuterRadius = radius;
            var gp = go.AddComponent<GlowPulse>();
            new P(gp).Float("minIntensity", intensity * 0.55f).Float("maxIntensity", intensity * 1.1f).Float("speed", pulseSpeed).Apply();
        }

        // ================================================================== BUILD SETTINGS
        private static void AddScenesToBuildSettings()
        {
            List<EditorBuildSettingsScene> list = EditorBuildSettings.scenes.ToList();
            foreach (string path in new[] { LevelScenePath, BattleScenePath })
                if (list.All(s => s.path != path)) list.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        // ================================================================== OUTILS
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }

        /// <summary>Petit raccourci pour remplir les champs [SerializeField] privés des scripts.</summary>
        private class P
        {
            private readonly SerializedObject _so;
            public P(Object target) { _so = new SerializedObject(target); }

            private SerializedProperty F(string name)
            {
                SerializedProperty prop = _so.FindProperty(name);
                if (prop == null) throw new Exception($"Champ '{name}' introuvable sur {_so.targetObject.GetType().Name}");
                return prop;
            }

            public P Ref(string n, Object v) { F(n).objectReferenceValue = v; return this; }
            public P Str(string n, string v) { F(n).stringValue = v; return this; }
            public P Float(string n, float v) { F(n).floatValue = v; return this; }
            public P Bool(string n, bool v) { F(n).boolValue = v; return this; }
            public P Enum(string n, int v) { F(n).enumValueIndex = v; return this; }
            public P Vec2(string n, Vector2 v) { F(n).vector2Value = v; return this; }
            public P Vec3(string n, Vector3 v) { F(n).vector3Value = v; return this; }

            public P Refs(string n, IList<Object> values)
            {
                SerializedProperty arr = F(n);
                arr.arraySize = values.Count;
                for (int i = 0; i < values.Count; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
                return this;
            }
            public P Refs<T>(string n, T[] values) where T : Object => Refs(n, values.Cast<Object>().ToList());

            public P Strs(string n, string[] values)
            {
                SerializedProperty arr = F(n);
                arr.arraySize = values.Length;
                for (int i = 0; i < values.Length; i++) arr.GetArrayElementAtIndex(i).stringValue = values[i];
                return this;
            }

            public void Apply() => _so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
