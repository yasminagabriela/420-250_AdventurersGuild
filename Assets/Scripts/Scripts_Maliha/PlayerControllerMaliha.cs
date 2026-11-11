using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerMaliha : MonoBehaviour
{
    public float speed = 5.0f;
    public InputAction MoveAction;
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
    void Start()
    {
        MoveAction.Enable();
    }
    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        moveInput = MoveAction.ReadValue<Vector2>();
        transform.Translate(CalculateVelocity(moveInput) * Time.deltaTime);
    }
    public Vector2 CalculateVelocity(Vector2 input)
    {
        return input.normalized * speed;
    }
}