using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private bool artFacesRight = false;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Stairs and Slopes")]
    [Tooltip("Used while moving or in the air: no friction, so the player can walk up stairs.")]
    [SerializeField] private PhysicsMaterial2D movingMaterial;
    [Tooltip("Used while standing still on the ground: high friction, so the player doesn't slide down stairs.")]
    [SerializeField] private PhysicsMaterial2D standingMaterial;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    private bool CanControl => !PlayerLock.IsLocked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        jumpAction.action.performed += OnJump;
    }

    private void OnDisable()
    {
        jumpAction.action.performed -= OnJump;
    }

    private void Update()
    {
        moveInput = CanControl ? moveAction.action.ReadValue<Vector2>().x : 0f;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (moveInput != 0f) Face(Mathf.Sign(moveInput));
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        PhysicsMaterial2D material = moveInput == 0f && isGrounded ? standingMaterial : movingMaterial;
        if (material != null && rb.sharedMaterial != material) rb.sharedMaterial = material;
    }

    // Flips left/right by changing only the sign of X, so any size set on the player is kept.
    private void Face(float direction)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (artFacesRight ? direction : -direction);
        transform.localScale = scale;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && CanControl) rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
