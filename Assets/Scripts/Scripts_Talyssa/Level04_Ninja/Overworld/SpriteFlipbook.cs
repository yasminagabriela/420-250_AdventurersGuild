using UnityEngine;

namespace AdventurersGuild
{
    /// <summary>
    /// Animation image par image très simple (sans Animator) pour les animaux, PNJ et décors.
    /// Mettre Moving = true pour jouer les images de marche.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFlipbook : MonoBehaviour
    {
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private Sprite[] moveFrames;
        [SerializeField] private float framesPerSecond = 3f;
        [Tooltip("Pause aléatoire sur la 1re image (ex. le hibou qui cligne des yeux de temps en temps).")]
        [SerializeField] private Vector2 holdFirstFrame = Vector2.zero;

        public bool Moving { get; set; }

        private SpriteRenderer _sr;
        private float _timer;
        private float _hold;
        private int _frame;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _frame = Random.Range(0, 8);
            _timer = Random.value / Mathf.Max(0.01f, framesPerSecond);
        }

        private void Update()
        {
            Sprite[] frames = Moving && moveFrames != null && moveFrames.Length > 0 ? moveFrames : idleFrames;
            if (frames == null || frames.Length == 0) return;

            if (_hold > 0f) { _hold -= Time.deltaTime; return; }
            _timer += Time.deltaTime;
            if (_timer < 1f / framesPerSecond) return;
            _timer = 0f;
            _frame = (_frame + 1) % frames.Length;
            _sr.sprite = frames[_frame];
            if (_frame == 0 && holdFirstFrame.y > 0f) _hold = Random.Range(holdFirstFrame.x, holdFirstFrame.y);
        }
    }
}
