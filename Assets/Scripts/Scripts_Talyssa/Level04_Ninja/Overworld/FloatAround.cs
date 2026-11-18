using UnityEngine;

namespace AdventurersGuild
{
    /// <summary>Flotte doucement autour de sa position (papillons, fée, feu follet).</summary>
    public class FloatAround : MonoBehaviour
    {
        [SerializeField] private Vector2 amplitude = new Vector2(0.6f, 0.25f);
        [SerializeField] private float speed = 1.2f;
        [SerializeField] private bool flipWithDirection = true;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Vector3 _origin;
        private float _phase;
        private float _lastX;

        private void Start()
        {
            _origin = transform.localPosition;
            _phase = Random.value * 10f;
            _lastX = _origin.x;
        }

        private void Update()
        {
            float t = Time.time * speed + _phase;
            Vector3 p = _origin + new Vector3(Mathf.Sin(t) * amplitude.x, Mathf.Sin(t * 2.1f) * amplitude.y, 0f);
            transform.localPosition = p;
            if (flipWithDirection && spriteRenderer != null && Mathf.Abs(p.x - _lastX) > 0.0005f)
                spriteRenderer.flipX = p.x < _lastX;
            _lastX = p.x;
        }
    }
}
