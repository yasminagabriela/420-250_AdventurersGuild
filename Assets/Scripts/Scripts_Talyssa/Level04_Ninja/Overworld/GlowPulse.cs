using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AdventurersGuild
{
    /// <summary>Fait respirer une lumière 2D (fleurs, eau lumineuse, arbre, feu follet).</summary>
    [RequireComponent(typeof(Light2D))]
    public class GlowPulse : MonoBehaviour
    {
        [SerializeField] private float minIntensity = 0.5f;
        [SerializeField] private float maxIntensity = 1.1f;
        [SerializeField] private float speed = 1.5f;

        private Light2D _light;
        private float _phase;

        private void Awake()
        {
            _light = GetComponent<Light2D>();
            _phase = Random.value * Mathf.PI * 2f;
        }

        private void Update()
        {
            float t = (Mathf.Sin(Time.time * speed + _phase) + 1f) * 0.5f;
            _light.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        }
    }
}
