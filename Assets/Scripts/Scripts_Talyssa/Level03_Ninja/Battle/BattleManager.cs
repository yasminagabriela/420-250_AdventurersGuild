using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AdventurersGuild
{
    /// <summary>
    /// Combat au tour par tour façon Final Fantasy 1.
    /// 1) Le joueur choisit une commande (Attaquer / Ninjutsu / Objets / Fuir).
    /// 2) Tout le monde agit dans l'ordre de l'agilité.
    /// 3) Victoire -> XP, or, retour à la carte. Défaite -> écran Game Over.
    /// L'ennemi rencontré vient de BattleContext.EnemyId (posé par BattleTrigger).
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        [Header("Données")]
        [SerializeField] private EnemyData[] enemyDatabase;
        [SerializeField] private BattleUnit unitPrefab;
        [SerializeField] private Sprite[] heroIdleFrames;
        [SerializeField] private Sprite heroAttackFrame;
        [Tooltip("Scène chargée après le combat si BattleContext.ReturnSceneName est vide.")]
        [SerializeField] private string fallbackReturnScene = "Level_04";
        [Tooltip("Pour tester BattleScene directement : ex. serpent_1. Laisser vide pour ne rien faire.")]
        [SerializeField] private string debugEnemyId = "";

        [Header("Placement")]
        [SerializeField] private Vector2 heroPosition = new Vector2(5.5f, -0.8f);
        [SerializeField] private Vector2 enemyAreaCenter = new Vector2(-4.5f, -0.6f);
        [SerializeField] private float heroScale = 3f;

        [Header("Scène de combat de l'équipe (optionnel)")]
        [Tooltip("Le menu Attaque / Sort / Fuite de l'équipe. Si rempli, il remplace le menu bleu.")]
        [SerializeField] private BattleMenu teamMenu;
        [Tooltip("L'objet Player de BattleScene (son sprite vient du BattleTheme).")]
        [SerializeField] private SpriteRenderer teamPlayer;
        [Tooltip("Hauteur du héros en unités (le ninja 16 px est agrandi pour faire cette taille).")]
        [SerializeField] private float teamHeroHeight = 3f;

        private bool TeamMode => teamMenu != null;

        private enum Phase { Intro, Command, Busy, Victory, Defeat }
        private enum Menu { Main, Ninjutsu, Items, Target }
        private enum ActionKind { Attack, FireShuriken, Heal, Purify, Potion, Antidote, Ether, Flee }

        private struct Popup
        {
            public Vector3 World; public string Text; public Color Color; public float Age;
        }

        private static readonly string[] MainOptions = { "Attaquer", "Ninjutsu", "Objets", "Fuir" };

        private const int FireCost = 4, HealCost = 3, PurifyCost = 2;

        private readonly List<BattleUnit> _enemies = new List<BattleUnit>();
        private readonly List<Popup> _popups = new List<Popup>();
        private BattleUnit _hero;
        private Phase _phase = Phase.Intro;
        private Menu _menu = Menu.Main;
        private int _cursor;
        private int _gameOverCursor;
        private ActionKind? _chosen;
        private ActionKind _pendingTargetAction;
        private BattleUnit _chosenTarget;
        private string _message = "";
        private int _sleepTurns;
        private bool _canFlee = true;
        private string _encounterId;

        // ------------------------------------------------------------------ Démarrage
        private void Start()
        {
            _encounterId = string.IsNullOrEmpty(BattleContext.EnemyId) ? debugEnemyId : BattleContext.EnemyId;
            EnemyData data = FindEnemyData(_encounterId);
            if (data == null)
            {
                // Ennemi d'un autre niveau (ex. Gloob) : on laisse la scène de l'équipe fonctionner seule.
                Debug.Log($"[BattleManager] '{_encounterId}' n'est pas un ennemi du niveau 4 : combat géré par l'équipe.");
                enabled = false;
                return;
            }

            if (PartyState.Hp <= 0) PartyState.RestoreFull();

            if (teamPlayer != null)
            {
                // On réutilise le Player de la scène (sprite / animator posés par BattleThemeApplier).
                _hero = teamPlayer.GetComponent<BattleUnit>();
                if (_hero == null) _hero = teamPlayer.gameObject.AddComponent<BattleUnit>();
                Sprite reference = BattleContext.Theme != null && BattleContext.Theme.playerSprite != null
                    ? BattleContext.Theme.playerSprite : teamPlayer.sprite;
                float unitHeight = reference != null ? reference.rect.height / reference.pixelsPerUnit : 1f;
                _hero.SetupHero(null, null, teamHeroHeight / Mathf.Max(0.01f, unitHeight));
            }
            else
            {
                _hero = Instantiate(unitPrefab, heroPosition, Quaternion.identity);
                _hero.name = "Hero_Ninja";
                _hero.SetupHero(heroIdleFrames, heroAttackFrame, heroScale);
            }

            if (TeamMode) teamMenu.Confirmed += OnTeamMenuConfirmed;

            int count = Random.Range(data.groupMin, data.groupMax + 1);
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = enemyAreaCenter + EnemyOffset(i, count, data.battleScale);
                BattleUnit u = Instantiate(unitPrefab, pos, Quaternion.identity);
                string label = count > 1 ? $"{data.displayName} {(char)('A' + i)}" : data.displayName;
                u.name = "Enemy_" + label;
                u.SetupEnemy(data, label);
                u.GetComponent<SpriteRenderer>().sortingOrder = 3 + i; // devant le fond (0) et le Player (2), derrière le menu (10-11)
                _enemies.Add(u);
            }
            _canFlee = !data.isBoss;
            StartCoroutine(BattleLoop(data, count));
        }

        private EnemyData FindEnemyData(string encounterId)
        {
            if (enemyDatabase == null || enemyDatabase.Length == 0 || string.IsNullOrEmpty(encounterId)) return null;
            string key = encounterId.ToLowerInvariant();
            // "serpent_2" -> cherche l'EnemyData dont l'id est le début ("serpent")
            EnemyData best = enemyDatabase.Where(e => e != null && key.StartsWith(e.id.ToLowerInvariant()))
                                          .OrderByDescending(e => e.id.Length).FirstOrDefault();
            if (best != null) return best;
            return null;
        }

        private void OnDestroy()
        {
            if (teamMenu != null) teamMenu.Confirmed -= OnTeamMenuConfirmed;
        }

        /// <summary>Menu de l'équipe : 0 = Attaque, 1 = Sort, 2 = Fuite.</summary>
        private void OnTeamMenuConfirmed(int index)
        {
            if (_phase != Phase.Command || _chosen != null) return;
            switch (index)
            {
                case 0:
                    _chosenTarget = _enemies.Where(e => e.IsAlive).OrderBy(e => e.Hp).FirstOrDefault();
                    _chosen = ActionKind.Attack;
                    break;
                case 1:
                    bool lowHp = PartyState.Hp < PartyState.MaxHp * 0.4f;
                    if (lowHp && PartyState.Mp >= HealCost) _chosen = ActionKind.Heal;
                    else if (PartyState.Mp >= FireCost) _chosen = ActionKind.FireShuriken;
                    else if (PartyState.Mp >= HealCost) _chosen = ActionKind.Heal;
                    else StartCoroutine(Say("Pas assez de PM !", 0.9f));
                    break;
                default:
                    if (_canFlee) _chosen = ActionKind.Flee;
                    else StartCoroutine(Say("Impossible de fuir un boss !", 0.9f));
                    break;
            }
        }

        private void SetTeamMenuActive(bool active)
        {
            if (teamMenu != null) teamMenu.enabled = active;
        }

        private static Vector2 EnemyOffset(int i, int count, float scale)
        {
            if (count == 1) return Vector2.zero;
            float spacing = Mathf.Max(1.6f, scale * 0.9f);
            // en quinconce, façon FF1
            float y = (i - (count - 1) / 2f) * spacing;
            float x = (i % 2 == 0) ? 0.6f : -0.6f;
            return new Vector2(x, -y * 0.75f);
        }

        // ------------------------------------------------------------------ Boucle principale
        private IEnumerator BattleLoop(EnemyData data, int count)
        {
            _phase = Phase.Intro;
            SetTeamMenuActive(false);
            yield return Say(count > 1 ? $"{count} {data.displayName}s surgissent !" : $"{data.displayName} apparaît !");
            if (data.isBoss) yield return Say("Une présence redoutable... Impossible de fuir !");

            while (true)
            {
                // ---- choix du joueur
                _chosen = null;
                _chosenTarget = null;
                if (_sleepTurns > 0)
                {
                    _sleepTurns--;
                    yield return Say("Le Ninja est envoûté et ne peut pas agir...");
                }
                else
                {
                    _menu = Menu.Main; _cursor = 0; _message = "";
                    _phase = Phase.Command;
                    SetTeamMenuActive(true);
                    while (_chosen == null) yield return null;
                }
                _phase = Phase.Busy;
                SetTeamMenuActive(false);

                // ---- fuite (résolue avant les autres actions)
                if (_chosen == ActionKind.Flee)
                {
                    int bestAgi = _enemies.Where(e => e.IsAlive).Select(e => e.Agility).DefaultIfEmpty(0).Max();
                    if (Random.value < BattleMath.FleeChance(_hero.Agility, bestAgi, !_canFlee))
                    {
                        yield return Say("Le Ninja disparaît dans un nuage de fumée !");
                        FinishAndReturn(fled: true);
                        yield break;
                    }
                    yield return Say("Impossible de fuir !");
                    _chosen = null;
                }

                // ---- ordre des tours selon l'agilité (+ un peu de hasard)
                var order = new List<(BattleUnit unit, float init)>();
                if (_chosen != null) order.Add((_hero, _hero.Agility + Random.Range(0f, 4f)));
                foreach (BattleUnit e in _enemies.Where(e => e.IsAlive))
                    order.Add((e, e.Agility + Random.Range(0f, 4f)));
                order.Sort((a, b) => b.init.CompareTo(a.init));

                foreach (var (unit, _) in order)
                {
                    if (!unit.IsAlive || !_hero.IsAlive) continue;
                    if (unit.IsHero) yield return HeroAction(_chosen.Value, _chosenTarget);
                    else yield return EnemyAction(unit);

                    if (_enemies.All(e => !e.IsAlive)) { yield return Victory(); yield break; }
                    if (!_hero.IsAlive) { yield return Defeat(); yield break; }
                }

                // ---- poison en fin de tour
                if (PartyState.Poisoned && _hero.IsAlive)
                {
                    int dmg = BattleMath.PoisonDamage(_hero.MaxHp);
                    DamageHero(dmg, new Color(0.6f, 1f, 0.4f));
                    yield return _hero.Flash(new Color(0.5f, 1f, 0.3f), 2);
                    yield return Say($"Le poison inflige {dmg} dégâts.");
                    if (!_hero.IsAlive) { yield return Defeat(); yield break; }
                }
            }
        }

        // ------------------------------------------------------------------ Actions du héros
        private IEnumerator HeroAction(ActionKind action, BattleUnit target)
        {
            switch (action)
            {
                case ActionKind.Attack:
                    {
                        if (target == null || !target.IsAlive) target = _enemies.FirstOrDefault(e => e.IsAlive);
                        if (target == null) yield break;
                        yield return _hero.Lunge();
                        if (Random.value > BattleMath.HitChance(_hero.Agility, target.Agility))
                        {
                            AddPopup(target.HeadPosition, "Raté", Color.white);
                            yield return Say($"Le Ninja rate {target.DisplayName} !");
                            yield break;
                        }
                        bool crit = Random.value < 0.15f;
                        int dmg = BattleMath.PhysicalDamage(_hero.Attack, target.Defense, Random.value);
                        if (crit) dmg = Mathf.RoundToInt(dmg * BattleMath.CritMultiplier);
                        yield return HitEnemy(target, dmg, crit ? new Color(1f, 0.85f, 0.2f) : Color.white);
                        yield return Say(crit ? $"Coup critique ! {target.DisplayName} subit {dmg} dégâts !"
                                              : $"{target.DisplayName} subit {dmg} dégâts.");
                        break;
                    }
                case ActionKind.FireShuriken:
                    {
                        PartyState.Mp -= FireCost;
                        yield return _hero.CastPose(new Color(1f, 0.6f, 0.2f));
                        yield return Say("Ninjutsu : Shuriken de feu !");
                        foreach (BattleUnit e in _enemies.Where(e => e.IsAlive).ToList())
                        {
                            int dmg = BattleMath.MagicDamage(Mathf.RoundToInt(PartyState.Magic * 1.5f), Random.value);
                            StartCoroutine(e.Flash(new Color(1f, 0.5f, 0.1f)));
                            yield return HitEnemy(e, dmg, new Color(1f, 0.6f, 0.2f));
                        }
                        break;
                    }
                case ActionKind.Heal:
                    {
                        PartyState.Mp -= HealCost;
                        int amount = PartyState.Magic * 2 + Random.Range(4, 10);
                        yield return _hero.CastPose(new Color(0.4f, 1f, 0.6f));
                        HealHero(amount);
                        yield return Say($"Ninjutsu : Soin ! +{amount} PV");
                        break;
                    }
                case ActionKind.Purify:
                    {
                        PartyState.Mp -= PurifyCost;
                        yield return _hero.CastPose(new Color(0.7f, 0.9f, 1f));
                        PartyState.Poisoned = false;
                        yield return Say("Ninjutsu : Purification ! Le poison disparaît.");
                        break;
                    }
                case ActionKind.Potion:
                    {
                        PartyState.Potions--;
                        HealHero(PartyState.PotionHeal);
                        yield return _hero.Flash(new Color(0.4f, 1f, 0.6f), 2);
                        yield return Say($"Potion ! +{PartyState.PotionHeal} PV");
                        break;
                    }
                case ActionKind.Antidote:
                    {
                        PartyState.Antidotes--;
                        PartyState.Poisoned = false;
                        yield return _hero.Flash(new Color(0.7f, 0.9f, 1f), 2);
                        yield return Say("Antidote ! Le poison est guéri.");
                        break;
                    }
                case ActionKind.Ether:
                    {
                        PartyState.Ethers--;
                        PartyState.RestoreMp(PartyState.EtherRestore);
                        yield return _hero.Flash(PixelGUI.MpColor, 2);
                        yield return Say($"Éther ! +{PartyState.EtherRestore} PM");
                        break;
                    }
            }
        }

        private IEnumerator HitEnemy(BattleUnit target, int dmg, Color popupColor)
        {
            target.Hp = Mathf.Max(0, target.Hp - dmg);
            AddPopup(target.HeadPosition, dmg.ToString(), popupColor);
            yield return target.Shake();
            if (!target.IsAlive)
            {
                yield return target.FadeOut();
                yield return Say($"{target.DisplayName} est vaincu !");
            }
        }

        // ------------------------------------------------------------------ IA ennemie
        private IEnumerator EnemyAction(BattleUnit enemy)
        {
            EnemyData d = enemy.Data;
            bool useSkill = d.skillType != EnemySkillType.None && Random.value < d.skillChance;

            if (useSkill && d.skillType == EnemySkillType.Sleep)
            {
                yield return enemy.CastPose(new Color(1f, 0.6f, 0.9f));
                yield return Say($"{enemy.DisplayName} : {d.skillName} !");
                if (_sleepTurns == 0 && Random.value < d.statusChance)
                {
                    _sleepTurns = Random.Range(1, 3);
                    AddPopup(_hero.HeadPosition, "Zzz", new Color(1f, 0.7f, 0.95f));
                    yield return Say("Le Ninja tombe sous le charme...");
                }
                else yield return Say("Le Ninja résiste au chant !");
                yield break;
            }

            if (useSkill && d.skillType == EnemySkillType.Magic)
            {
                yield return enemy.CastPose(new Color(0.6f, 1f, 1f));
                int mdmg = BattleMath.MagicDamage(d.skillPower, Random.value);
                yield return Say($"{enemy.DisplayName} : {d.skillName} !");
                DamageHero(mdmg, new Color(0.6f, 1f, 1f));
                yield return _hero.Shake();
                yield return Say($"Le Ninja subit {mdmg} dégâts.");
                yield break;
            }

            // attaque physique (éventuellement empoisonnée)
            yield return enemy.Lunge();
            if (Random.value > BattleMath.HitChance(enemy.Agility, _hero.Agility))
            {
                AddPopup(_hero.HeadPosition, "Esquive", Color.white);
                yield return Say($"Le Ninja esquive l'attaque de {enemy.DisplayName} !");
                yield break;
            }
            int power = useSkill ? Mathf.Max(enemy.Attack, d.skillPower) : enemy.Attack;
            int dmg = BattleMath.PhysicalDamage(power, _hero.Defense, Random.value);
            DamageHero(dmg, new Color(1f, 0.4f, 0.4f));
            if (_sleepTurns > 0 && Random.value < 0.5f) _sleepTurns = 0; // un coup peut réveiller
            yield return _hero.Shake();
            yield return Say(useSkill ? $"{enemy.DisplayName} : {d.skillName} ! {dmg} dégâts."
                                      : $"{enemy.DisplayName} attaque ! {dmg} dégâts.");
            if (useSkill && d.skillType == EnemySkillType.PoisonBite && !PartyState.Poisoned && Random.value < d.statusChance)
            {
                PartyState.Poisoned = true;
                AddPopup(_hero.HeadPosition + Vector3.up * 0.4f, "Poison", new Color(0.6f, 1f, 0.3f));
                yield return Say("Le Ninja est empoisonné !");
            }
        }

        // ------------------------------------------------------------------ Fin du combat
        private IEnumerator Victory()
        {
            _phase = Phase.Victory;
            int xp = _enemies.Sum(e => e.Data.xpReward);
            int gold = _enemies.Sum(e => e.Data.goldReward);
            PartyState.Gold += gold;
            int levels = PartyState.GainXp(xp);
            PartyState.Poisoned = false;
            yield return Say($"Victoire ! +{xp} XP, +{gold} pièces d'or.", 1.6f);
            if (levels > 0)
            {
                _hero.MaxHp = PartyState.MaxHp; _hero.Hp = PartyState.Hp;
                yield return _hero.Flash(PixelGUI.Highlight, 3);
                yield return Say($"Niveau supérieur ! Le Ninja passe au niveau {PartyState.Level} !", 1.8f);
            }
            if (!string.IsNullOrEmpty(BattleContext.EnemyId)) BattleContext.DefeatedEnemies.Add(BattleContext.EnemyId);
            FinishAndReturn(fled: false);
        }

        private IEnumerator Defeat()
        {
            yield return _hero.FadeOut(0.8f);
            yield return Say("Le Ninja est tombé au combat...", 1.4f);
            _phase = Phase.Defeat;
            _gameOverCursor = 0;
        }

        private void FinishAndReturn(bool fled)
        {
            string scene = string.IsNullOrEmpty(BattleContext.ReturnSceneName) ? fallbackReturnScene : BattleContext.ReturnSceneName;
            // Après une fuite, on laisse le temps au joueur de s'éloigner avant qu'un ennemi le ré-attaque.
            BattleContext.TriggerGraceSeconds = fled ? 1.5f : 0.3f;
            BattleContext.EnemyId = null;
            SceneManager.LoadScene(scene);
        }

        private void Retry()
        {
            PartyState.RestoreFull();
            BattleContext.HasReturnPosition = false; // on recommence au point de départ du niveau
            BattleContext.EnemyId = null;
            string scene = string.IsNullOrEmpty(BattleContext.ReturnSceneName) ? fallbackReturnScene : BattleContext.ReturnSceneName;
            SceneManager.LoadScene(scene);
        }

        // ------------------------------------------------------------------ Aides
        private void DamageHero(int dmg, Color color)
        {
            PartyState.Hp = Mathf.Max(0, PartyState.Hp - dmg);
            _hero.Hp = PartyState.Hp;
            AddPopup(_hero.HeadPosition, dmg.ToString(), color);
        }

        private void HealHero(int amount)
        {
            PartyState.Heal(amount);
            _hero.Hp = PartyState.Hp;
            AddPopup(_hero.HeadPosition, "+" + amount, PixelGUI.HpColor);
        }

        private void AddPopup(Vector3 world, string text, Color color)
        {
            _popups.Add(new Popup { World = world, Text = text, Color = color, Age = 0f });
        }

        /// <summary>Affiche un message, attend un délai (Entrée pour accélérer).</summary>
        private IEnumerator Say(string text, float duration = 1.1f)
        {
            _message = text;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                if (t > 0.25f && GameInput.Confirm) break;
                yield return null;
            }
            _message = "";
        }

        // ------------------------------------------------------------------ Entrées clavier du menu
        private void Update()
        {
            for (int i = _popups.Count - 1; i >= 0; i--)
            {
                Popup p = _popups[i];
                p.Age += Time.deltaTime;
                if (p.Age > 1.1f) _popups.RemoveAt(i); else _popups[i] = p;
            }

            if (_phase == Phase.Defeat)
            {
                if (GameInput.Up || GameInput.Down) _gameOverCursor = 1 - _gameOverCursor;
                if (GameInput.Confirm) ChooseGameOver(_gameOverCursor);
                return;
            }
            if (_phase != Phase.Command || TeamMode) return;

            int count = CurrentOptionCount();
            if (GameInput.Up) _cursor = (_cursor - 1 + count) % count;
            if (GameInput.Down) _cursor = (_cursor + 1) % count;
            if (GameInput.Confirm) Select(_cursor);
            else if (GameInput.Cancel && _menu != Menu.Main) { _menu = Menu.Main; _cursor = 0; }
        }

        private List<BattleUnit> AliveEnemies => _enemies.Where(e => e.IsAlive).ToList();

        private int CurrentOptionCount()
        {
            switch (_menu)
            {
                case Menu.Ninjutsu: return 3;
                case Menu.Items: return 3;
                case Menu.Target: return Mathf.Max(1, AliveEnemies.Count);
                default: return MainOptions.Length;
            }
        }

        private (string label, bool enabled)[] NinjutsuOptions() => new[]
        {
            ($"Shuriken de feu  {FireCost} PM", PartyState.Mp >= FireCost),
            ($"Soin             {HealCost} PM", PartyState.Mp >= HealCost),
            ($"Purification     {PurifyCost} PM", PartyState.Mp >= PurifyCost),
        };

        private (string label, bool enabled)[] ItemOptions() => new[]
        {
            ($"Potion    x{PartyState.Potions}", PartyState.Potions > 0),
            ($"Antidote  x{PartyState.Antidotes}", PartyState.Antidotes > 0),
            ($"Éther     x{PartyState.Ethers}", PartyState.Ethers > 0),
        };

        private void Select(int index)
        {
            switch (_menu)
            {
                case Menu.Main:
                    if (index == 0) { BeginTarget(ActionKind.Attack); }
                    else if (index == 1) { _menu = Menu.Ninjutsu; _cursor = 0; }
                    else if (index == 2) { _menu = Menu.Items; _cursor = 0; }
                    else if (_canFlee) _chosen = ActionKind.Flee;
                    break;
                case Menu.Ninjutsu:
                    if (!NinjutsuOptions()[index].enabled) return;
                    _chosen = index == 0 ? ActionKind.FireShuriken : index == 1 ? ActionKind.Heal : ActionKind.Purify;
                    break;
                case Menu.Items:
                    if (!ItemOptions()[index].enabled) return;
                    _chosen = index == 0 ? ActionKind.Potion : index == 1 ? ActionKind.Antidote : ActionKind.Ether;
                    break;
                case Menu.Target:
                    List<BattleUnit> alive = AliveEnemies;
                    if (alive.Count == 0) return;
                    _chosenTarget = alive[Mathf.Clamp(index, 0, alive.Count - 1)];
                    _chosen = _pendingTargetAction;
                    break;
            }
        }

        private void BeginTarget(ActionKind action)
        {
            _pendingTargetAction = action;
            List<BattleUnit> alive = AliveEnemies;
            if (alive.Count == 1) { _chosenTarget = alive[0]; _chosen = action; return; }
            _menu = Menu.Target; _cursor = 0;
        }

        private void ChooseGameOver(int index)
        {
            if (index == 0) Retry();
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        // ------------------------------------------------------------------ Interface (OnGUI)
        private void OnGUI()
        {
            float vw = PixelGUI.VirtualWidth;
            const float panelY = 262f, panelH = 94f;

            // popups de dégâts
            foreach (Popup p in _popups)
            {
                Vector2 v = PixelGUI.WorldToVirtual(p.World);
                float rise = p.Age * 22f;
                PixelGUI.Text(new Rect(v.x - 40, v.y - 20 - rise, 80, 24), p.Text, 18, p.Color, TextAnchor.MiddleCenter);
            }

            if (_phase == Phase.Defeat) { DrawGameOver(vw); return; }
            if (TeamMode) { DrawTeamModeHud(vw); return; }

            // message en haut
            if (!string.IsNullOrEmpty(_message))
            {
                PixelGUI.Window(new Rect(10, 8, vw - 20, 34));
                PixelGUI.Text(new Rect(20, 13, vw - 40, 26), _message, 14, null, TextAnchor.MiddleCenter);
            }

            // fenêtre ennemis (gauche)
            float leftW = Mathf.Min(210f, vw * 0.33f);
            PixelGUI.Window(new Rect(6, panelY, leftW, panelH));
            float y = panelY + 8;
            foreach (BattleUnit e in _enemies)
            {
                Color c = e.IsAlive ? PixelGUI.TextColor : PixelGUI.Disabled;
                PixelGUI.Text(new Rect(16, y, leftW - 20, 18), e.DisplayName, 12, c);
                if (e.IsAlive) PixelGUI.Bar(new Rect(leftW - 60, y + 5, 50, 7), (float)e.Hp / e.MaxHp, PixelGUI.HpLow);
                y += 18;
            }

            // fenêtre statut du héros (droite)
            float rightW = Mathf.Min(220f, vw * 0.34f);
            float rx = vw - rightW - 6;
            PixelGUI.Window(new Rect(rx, panelY, rightW, panelH));
            string status = PartyState.Poisoned ? "  <color=#9cff5a>[Poison]</color>" : _sleepTurns > 0 ? "  <color=#ffb0f0>[Charme]</color>" : "";
            PixelGUI.Text(new Rect(rx + 10, panelY + 6, rightW - 16, 18), $"{PartyState.HeroName}  Nv {PartyState.Level}{status}", 13);
            float hpRatio = (float)PartyState.Hp / PartyState.MaxHp;
            PixelGUI.Text(new Rect(rx + 10, panelY + 28, 120, 18), $"PV {PartyState.Hp}/{PartyState.MaxHp}", 12);
            PixelGUI.Bar(new Rect(rx + 110, panelY + 33, rightW - 122, 8), hpRatio, hpRatio < 0.3f ? PixelGUI.HpLow : PixelGUI.HpColor);
            PixelGUI.Text(new Rect(rx + 10, panelY + 48, 120, 18), $"PM {PartyState.Mp}/{PartyState.MaxMp}", 12);
            PixelGUI.Bar(new Rect(rx + 110, panelY + 53, rightW - 122, 8), (float)PartyState.Mp / PartyState.MaxMp, PixelGUI.MpColor);
            PixelGUI.Text(new Rect(rx + 10, panelY + 68, rightW - 16, 18), $"XP {PartyState.Xp}/{PartyState.XpForNext}   Or {PartyState.Gold}", 11, PixelGUI.Disabled);

            // menu de commandes (centre)
            if (_phase == Phase.Command) DrawCommandMenu(leftW + 12, panelY, rx - leftW - 18, panelH);
        }

        /// <summary>Affichage léger quand on utilise le menu de l'équipe : message, statut, vie des ennemis.</summary>
        private void DrawTeamModeHud(float vw)
        {
            if (!string.IsNullOrEmpty(_message))
            {
                PixelGUI.Window(new Rect(10, 8, vw - 20, 34));
                PixelGUI.Text(new Rect(20, 13, vw - 40, 26), _message, 14, null, TextAnchor.MiddleCenter);
            }

            const float w = 170f;
            float x = vw - w - 8, y = 48;
            PixelGUI.Window(new Rect(x, y, w, 58));
            string status = PartyState.Poisoned ? "  <color=#9cff5a>Poison</color>" : _sleepTurns > 0 ? "  <color=#ffb0f0>Charme</color>" : "";
            PixelGUI.Text(new Rect(x + 8, y + 3, w - 12, 16), $"{PartyState.HeroName}  Nv {PartyState.Level}{status}", 11);
            float ratio = (float)PartyState.Hp / PartyState.MaxHp;
            PixelGUI.Text(new Rect(x + 8, y + 19, 76, 14), $"PV {PartyState.Hp}/{PartyState.MaxHp}", 10);
            PixelGUI.Bar(new Rect(x + 84, y + 23, w - 92, 6), ratio, ratio < 0.3f ? PixelGUI.HpLow : PixelGUI.HpColor);
            PixelGUI.Text(new Rect(x + 8, y + 35, 76, 14), $"PM {PartyState.Mp}/{PartyState.MaxMp}", 10);
            PixelGUI.Bar(new Rect(x + 84, y + 39, w - 92, 6), (float)PartyState.Mp / PartyState.MaxMp, PixelGUI.MpColor);

            foreach (BattleUnit e in _enemies)
            {
                if (!e.IsAlive) continue;
                Vector2 v = PixelGUI.WorldToVirtual(e.HeadPosition);
                PixelGUI.Text(new Rect(v.x - 50, v.y - 26, 100, 14), e.DisplayName, 10, null, TextAnchor.MiddleCenter);
                PixelGUI.Bar(new Rect(v.x - 22, v.y - 11, 44, 6), (float)e.Hp / e.MaxHp, PixelGUI.HpLow);
            }
        }

        private void DrawCommandMenu(float x, float y, float w, float h)
        {
            PixelGUI.Window(new Rect(x, y, w, h));
            (string label, bool enabled)[] options;
            string title;
            switch (_menu)
            {
                case Menu.Ninjutsu: options = NinjutsuOptions(); title = "Ninjutsu"; break;
                case Menu.Items: options = ItemOptions(); title = "Objets"; break;
                case Menu.Target:
                    options = AliveEnemies.Select(e => (e.DisplayName, true)).ToArray(); title = "Cible";
                    DrawTargetArrow();
                    break;
                default: options = MainOptions.Select(o => (o, o != "Fuir" || _canFlee)).ToArray(); title = "Commande"; break;
            }

            PixelGUI.Text(new Rect(x + 10, y + 4, w - 20, 16), title, 10, PixelGUI.Disabled);
            float lineH = 17f;
            for (int i = 0; i < options.Length; i++)
            {
                Rect line = new Rect(x + 22, y + 20 + i * lineH, w - 30, lineH);
                bool selected = i == _cursor;
                Color c = !options[i].enabled ? PixelGUI.Disabled : selected ? PixelGUI.Highlight : PixelGUI.TextColor;
                if (selected) PixelGUI.Text(new Rect(x + 8, line.y, 14, lineH), "►", 12, PixelGUI.Highlight);
                PixelGUI.Text(line, options[i].label, 12, c);
                // clic souris
                if (GUI.Button(PixelGUI.R(line.x, line.y, line.width, line.height), GUIContent.none, GUIStyle.none))
                {
                    _cursor = i;
                    Select(i);
                }
            }
            if (_menu != Menu.Main)
                PixelGUI.Text(new Rect(x + 10, y + h - 16, w - 20, 14), "Échap : retour", 9, PixelGUI.Disabled, TextAnchor.LowerRight);
        }

        private void DrawTargetArrow()
        {
            List<BattleUnit> alive = AliveEnemies;
            if (_cursor >= alive.Count) return;
            Vector2 v = PixelGUI.WorldToVirtual(alive[_cursor].HeadPosition);
            float bob = Mathf.Sin(Time.time * 8f) * 2f;
            PixelGUI.Text(new Rect(v.x - 10, v.y - 22 + bob, 20, 20), "▼", 16, PixelGUI.Highlight, TextAnchor.MiddleCenter);
        }

        private void DrawGameOver(float vw)
        {
            PixelGUI.Fill(new Rect(0, 0, vw, PixelGUI.VirtualHeight), new Color(0, 0, 0, 0.75f));
            PixelGUI.Text(new Rect(0, 90, vw, 50), "GAME OVER", 36, PixelGUI.HpLow, TextAnchor.MiddleCenter);
            float w = 180f, x = (vw - w) / 2f;
            PixelGUI.Window(new Rect(x, 170, w, 64));
            string[] opts = { "Réessayer", "Quitter" };
            for (int i = 0; i < 2; i++)
            {
                Rect line = new Rect(x + 30, 180 + i * 22, w - 40, 20);
                bool sel = i == _gameOverCursor;
                if (sel) PixelGUI.Text(new Rect(x + 12, line.y, 16, 20), "►", 13, PixelGUI.Highlight);
                PixelGUI.Text(line, opts[i], 14, sel ? PixelGUI.Highlight : PixelGUI.TextColor);
                if (GUI.Button(PixelGUI.R(line.x, line.y, line.width, line.height), GUIContent.none, GUIStyle.none))
                    ChooseGameOver(i);
            }
        }
    }
}
