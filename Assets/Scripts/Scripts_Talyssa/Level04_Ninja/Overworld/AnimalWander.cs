using System.Collections;
using UnityEngine;

namespace AdventurersGuild
{
    /// <summary>
    /// Animal qui se promène case par case autour de son point de départ.
    /// Il évite l'eau et les buissons (WalkableMap) et peut fuir le joueur (le renard !).
    /// </summary>
    public class AnimalWander : MonoBehaviour
    {
        [SerializeField] private int wanderRadius = 3;
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private Vector2 pauseRange = new Vector2(0.8f, 2.5f);
        [SerializeField] private bool fleeFromPlayer = true;
        [SerializeField] private float fleeDistance = 2.2f;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteFlipbook flipbook;

        private static readonly Vector3Int[] Directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        private Vector3Int _home;
        private Vector3Int _cell;
        private Transform _player;

        private IEnumerator Start()
        {
            yield return null; // laisser WalkableMap s'initialiser
            WalkableMap map = WalkableMap.Instance;
            if (map == null) yield break;
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;

            _home = _cell = map.WorldToCell(transform.position);
            transform.position = map.CellCenter(_cell);

            while (true)
            {
                Vector3Int next = ChooseNext(map);
                if (next != _cell) yield return MoveTo(map, next);
                bool scared = IsPlayerClose();
                yield return new WaitForSeconds(scared ? 0.05f : Random.Range(pauseRange.x, pauseRange.y));
            }
        }

        private bool IsPlayerClose() =>
            fleeFromPlayer && _player != null && Vector2.Distance(_player.position, transform.position) < fleeDistance;

        private Vector3Int ChooseNext(WalkableMap map)
        {
            if (IsPlayerClose())
            {
                // fuir : la direction qui éloigne le plus du joueur
                Vector3Int best = _cell;
                float bestDist = Vector2.Distance(_player.position, transform.position);
                foreach (Vector3Int d in Directions)
                {
                    Vector3Int c = _cell + d;
                    if (!map.IsWalkable(c)) continue;
                    float dist = Vector2.Distance(_player.position, map.CellCenter(c));
                    if (dist > bestDist) { bestDist = dist; best = c; }
                }
                return best;
            }

            for (int tries = 0; tries < 6; tries++)
            {
                Vector3Int c = _cell + Directions[Random.Range(0, Directions.Length)];
                bool inRange = Mathf.Abs(c.x - _home.x) <= wanderRadius && Mathf.Abs(c.y - _home.y) <= wanderRadius;
                if (inRange && map.IsWalkable(c)) return c;
            }
            return _cell;
        }

        private IEnumerator MoveTo(WalkableMap map, Vector3Int next)
        {
            Vector3 target = map.CellCenter(next);
            if (spriteRenderer != null && next.x != _cell.x) spriteRenderer.flipX = next.x < _cell.x;
            if (flipbook != null) flipbook.Moving = true;
            float speed = IsPlayerClose() ? moveSpeed * 1.8f : moveSpeed;
            while ((transform.position - target).sqrMagnitude > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                yield return null;
            }
            _cell = next;
            if (flipbook != null) flipbook.Moving = false;
        }
    }
}
