using UnityEngine;

namespace AdventurersGuild
{
    /// <summary>
    /// Vue de dessus : ce qui est plus bas à l'écran est dessiné devant.
    /// Mettre isStatic pour les décors qui ne bougent pas (calculé une fois).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class YSort : MonoBehaviour
    {
        [SerializeField] private int baseOrder = 1000;
        [SerializeField] private float yOffset;
        [SerializeField] private bool isStatic;

        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            Apply();
        }

        private void LateUpdate()
        {
            if (!isStatic) Apply();
        }

        private void Apply()
        {
            _sr.sortingOrder = baseOrder - Mathf.RoundToInt((transform.position.y + yOffset) * 10f);
        }
    }
}
