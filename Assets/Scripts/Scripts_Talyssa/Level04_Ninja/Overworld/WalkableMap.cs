using UnityEngine;
using UnityEngine.Tilemaps;

namespace AdventurersGuild
{
    /// <summary>
    /// Dit quelles cases de la carte sont marchables (même règle que le PlayerController).
    /// Utilisé par les animaux pour se promener sans entrer dans l'eau ou les buissons.
    /// À placer sur l'objet Grid de la scène.
    /// </summary>
    public class WalkableMap : MonoBehaviour
    {
        public static WalkableMap Instance { get; private set; }

        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private TileBase[] walkableTiles;

        public Tilemap Ground => groundTilemap;

        private void Awake() { Instance = this; }
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public bool IsWalkable(Vector3Int cell)
        {
            if (groundTilemap == null) return false;
            TileBase t = groundTilemap.GetTile(cell);
            return t != null && System.Array.IndexOf(walkableTiles, t) >= 0;
        }

        public Vector3Int WorldToCell(Vector3 world) => groundTilemap.WorldToCell(world);
        public Vector3 CellCenter(Vector3Int cell) => groundTilemap.GetCellCenterWorld(cell);
    }
}
