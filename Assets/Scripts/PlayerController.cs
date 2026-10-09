using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private TileBase[] walkableTiles;
    [SerializeField] private Vector3 feetOffset = new Vector3(0f, -0.35f, 0f);

    private GridMovementRules _rules;
    private Vector2Int _cell;
    private bool _isMoving;

    private void Awake()
    {
        _rules = new GridMovementRules(IsWalkable);

        Vector3Int start = groundTilemap.WorldToCell(transform.position);
        _cell = new Vector2Int(start.x, start.y);
        transform.position = CellToWorld(_cell);
    }

    private bool IsWalkable(Vector2Int cell)
    {
        TileBase tile = groundTilemap.GetTile(new Vector3Int(cell.x, cell.y, 0));
        return tile != null && System.Array.IndexOf(walkableTiles, tile) >= 0;
    }

    private Vector3 CellToWorld(Vector2Int cell)
    {
        return groundTilemap.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0)) + feetOffset;
    }

    private Vector2Int ReadDirection()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return Vector2Int.zero;

        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) return Vector2Int.up;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) return Vector2Int.down;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) return Vector2Int.left;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) return Vector2Int.right;
        return Vector2Int.zero;
    }

    private void Update()
    {
        if (_isMoving) return;

        Vector2Int dir = ReadDirection();
        if (dir == Vector2Int.zero) return;

        if (dir.x != 0) spriteRenderer.flipX = dir.x < 0;

        Vector2Int next = _rules.GetNextCell(_cell, dir);
        if (next != _cell) StartCoroutine(MoveTo(next));
    }

    private IEnumerator MoveTo(Vector2Int next)
    {
        _isMoving = true;
        animator.SetBool("IsMoving", true);
        Vector3 target = CellToWorld(next);

        while ((transform.position - target).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        _cell = next;
        _isMoving = false;
        animator.SetBool("IsMoving", false);
    }
}