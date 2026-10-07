using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private GridMovementRules _rules;
    private Vector2Int _cell;
    private bool _isMoving;

    private void Awake()
    {
        _rules = new GridMovementRules(new Vector2Int(0, 0), new Vector2Int(9, 9));
        _cell = Vector2Int.RoundToInt(transform.position);
    }

    private Vector2Int ReadDirection()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return Vector2Int.zero;

        if (kb.wKey.isPressed) return Vector2Int.up;
        if (kb.sKey.isPressed) return Vector2Int.down;
        if (kb.aKey.isPressed) return Vector2Int.left;
        if (kb.dKey.isPressed) return Vector2Int.right;
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
        Vector3 target = new Vector3(next.x, next.y, 0);

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