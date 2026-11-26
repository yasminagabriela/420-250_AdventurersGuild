using UnityEngine;
using UnityEngine.Tilemaps;

namespace AdventurersGuild
{
    /// <summary>
    /// À mettre sur la Main Camera, à côté de CameraFollow.
    /// Empêche la caméra de sortir de la map (plus de zone noire sur les bords).
    /// </summary>
    [DefaultExecutionOrder(1000)] // passe APRÈS CameraFollow
    [RequireComponent(typeof(Camera))]
    public class CameraClamp : MonoBehaviour
    {
        [Tooltip("Le Tilemap du sol (Grid > Ground).")]
        [SerializeField] private Tilemap ground;

        private Camera _camera;
        private Bounds _mapBounds;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (ground == null) return;

            ground.CompressBounds(); // ignore les cases vides autour de la map
            Bounds local = ground.localBounds;
            _mapBounds = new Bounds(ground.transform.TransformPoint(local.center),
                                    Vector3.Scale(local.size, ground.transform.lossyScale));
        }

        private void LateUpdate()
        {
            if (ground == null) return;

            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;

            Vector3 pos = transform.position;
            pos.x = Clamp(pos.x, _mapBounds.min.x + halfWidth, _mapBounds.max.x - halfWidth, _mapBounds.center.x);
            pos.y = Clamp(pos.y, _mapBounds.min.y + halfHeight, _mapBounds.max.y - halfHeight, _mapBounds.center.y);
            transform.position = pos;
        }

        // Si la map est plus petite que l'écran, on centre la caméra sur la map.
        private static float Clamp(float value, float min, float max, float center)
        {
            return min > max ? center : Mathf.Clamp(value, min, max);
        }
    }
}
