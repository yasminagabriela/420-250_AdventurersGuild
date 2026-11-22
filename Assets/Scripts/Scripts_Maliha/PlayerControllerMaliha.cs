using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerMaliha : MonoBehaviour
{
    public float speed = 5.0f;
    public InputAction MoveAction;
    [SerializeField] private Animator animator;
    private Vector2 moveInput;
    private Rigidbody2D body;

    void Reset()
    {
        MoveAction = new InputAction("Move", InputActionType.Value);
        MoveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
    }

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Start()
    {
        MoveAction.Enable();
    }

    void Update()
    {
        moveInput = MoveAction.ReadValue<Vector2>();
        transform.Translate(CalculateVelocity(moveInput) * Time.deltaTime);

        animator.SetBool("IsMoving", moveInput.sqrMagnitude > 0.01f);
    }

    public Vector2 CalculateVelocity(Vector2 input)
    {
        return input.normalized * speed;
    }
}