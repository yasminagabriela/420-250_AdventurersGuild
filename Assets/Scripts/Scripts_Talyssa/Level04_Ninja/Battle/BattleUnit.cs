using System.Collections;
using UnityEngine;

namespace AdventurersGuild
{
    /// <summary>
    /// Un combattant affiché dans la scène de combat (le ninja ou un ennemi).
    /// Gère l'animation idle, l'élan d'attaque, le flash quand il est touché et la disparition.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BattleUnit : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float framesPerSecond = 3f;

        public string DisplayName { get; private set; }
        public bool IsHero { get; private set; }
        public EnemyData Data { get; private set; }

        public int MaxHp;
        public int Hp;
        public int Attack;
        public int Defense;
        public int Agility;

        public bool IsAlive => Hp > 0;
        public Vector3 HomePosition { get; private set; }

        private Sprite[] _idle;
        private Sprite _attack;
        private bool _posing;
        private float _timer;
        private int _frame;
        private Color _baseColor = Color.white;

        private void Reset() { spriteRenderer = GetComponent<SpriteRenderer>(); }

        public void SetupHero(Sprite[] idle, Sprite attack, float scale)
        {
            IsHero = true;
            DisplayName = PartyState.HeroName;
            MaxHp = PartyState.MaxHp; Hp = PartyState.Hp;
            Attack = PartyState.Attack; Defense = PartyState.Defense; Agility = PartyState.Agility;
            InitVisual(idle, attack, scale, Color.white, faceLeft: true);
        }

        public void SetupEnemy(EnemyData data, string label)
        {
            IsHero = false;
            Data = data;
            DisplayName = label;
            MaxHp = Hp = data.maxHp;
            Attack = data.attack; Defense = data.defense; Agility = data.agility;
            InitVisual(data.idleFrames, data.attackFrame, data.battleScale, data.tint, faceLeft: false);
        }

        private void InitVisual(Sprite[] idle, Sprite attack, float scale, Color tint, bool faceLeft)
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            _idle = idle;
            _attack = attack;
            _baseColor = tint;
            spriteRenderer.color = tint;
            spriteRenderer.flipX = faceLeft;
            transform.localScale = Vector3.one * scale;
            HomePosition = transform.position;
            _frame = Random.Range(0, 10);
            if (_idle != null && _idle.Length > 0) spriteRenderer.sprite = _idle[_frame % _idle.Length];
        }

        private void Update()
        {
            if (_posing || _idle == null || _idle.Length == 0 || !IsAlive) return;
            _timer += Time.deltaTime;
            if (_timer >= 1f / framesPerSecond)
            {
                _timer = 0f;
                _frame = (_frame + 1) % _idle.Length;
                spriteRenderer.sprite = _idle[_frame];
            }
        }

        /// <summary>Avance vers l'adversaire, montre la pose d'attaque, puis revient.</summary>
        public IEnumerator Lunge(float distance = 0.8f)
        {
            float dir = IsHero ? -1f : 1f;
            Vector3 target = HomePosition + new Vector3(dir * distance, 0f, 0f);
            yield return MoveTo(target, 0.12f);
            _posing = true;
            if (_attack != null) spriteRenderer.sprite = _attack;
            yield return new WaitForSeconds(0.18f);
            yield return MoveTo(HomePosition, 0.12f);
            _posing = false;
        }

        public IEnumerator CastPose(Color glow)
        {
            _posing = true;
            if (_attack != null) spriteRenderer.sprite = _attack;
            yield return Flash(glow, 2, 0.08f);
            _posing = false;
        }

        private IEnumerator MoveTo(Vector3 target, float duration)
        {
            Vector3 start = transform.position;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(start, target, t / duration);
                yield return null;
            }
            transform.position = target;
        }

        public IEnumerator Flash(Color color, int times = 3, float interval = 0.06f)
        {
            for (int i = 0; i < times; i++)
            {
                spriteRenderer.color = color;
                yield return new WaitForSeconds(interval);
                spriteRenderer.color = _baseColor;
                yield return new WaitForSeconds(interval);
            }
        }

        public IEnumerator Shake(float strength = 0.15f, float duration = 0.25f)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                transform.position = HomePosition + (Vector3)(Random.insideUnitCircle * strength);
                yield return null;
            }
            transform.position = HomePosition;
        }

        public IEnumerator FadeOut(float duration = 0.5f)
        {
            Color c = spriteRenderer.color;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                c.a = 1f - t / duration;
                spriteRenderer.color = new Color(1f, 0.4f, 0.6f, c.a);
                yield return null;
            }
            spriteRenderer.enabled = false;
        }

        /// <summary>Point au-dessus de la tête pour afficher les dégâts.</summary>
        public Vector3 HeadPosition
        {
            get
            {
                if (spriteRenderer == null || spriteRenderer.sprite == null) return transform.position + Vector3.up;
                return new Vector3(transform.position.x, spriteRenderer.bounds.max.y + 0.2f, 0f);
            }
        }
    }
}
