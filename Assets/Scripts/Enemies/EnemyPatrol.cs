using UnityEngine;
using System.Collections;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 0.5f;
    [SerializeField] private bool pingPong = true;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool useBlendTrees = false;


    private WaypointCycle _cycle;

    private void Start()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        _cycle = new WaypointCycle(waypoints.Length, pingPong);
        transform.position = waypoints[0].position;
        StartCoroutine(Patrol());
    }

    public void Stop()
    {
        StopAllCoroutines();
        animator.SetBool("IsMoving", false);
    }

    private IEnumerator Patrol()
    {
        while (true)
        {
            Vector2 target = waypoints[_cycle.Next()].position;
            animator.SetBool("IsMoving", true);

            while ((body.position - target).sqrMagnitude > 0.0001f)
            {
                float dx = target.x - body.position.x;
                if (Mathf.Abs(dx) > 0.01f) spriteRenderer.flipX = dx < 0;
                {
                    if (useBlendTrees)
                        animator.SetFloat("MoveX", dx > 0 ? 1f : -1f);
                    else
                        spriteRenderer.flipX = dx < 0;
                }
                body.MovePosition(Vector2.MoveTowards(
                    body.position, target, moveSpeed * Time.fixedDeltaTime));
                yield return new WaitForFixedUpdate();
            }

            body.MovePosition(target);
            animator.SetBool("IsMoving", false);
            yield return new WaitForSeconds(waitTime);
        }
    }
}