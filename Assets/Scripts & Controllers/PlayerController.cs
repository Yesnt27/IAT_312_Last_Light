using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private bool artFacesRight = false;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private PlayerControls playerControls;
    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    private bool CanControl => !PlayerLock.IsLocked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        playerControls.Controls.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        playerControls.Controls.Jump.performed -= OnJump;
        playerControls.Disable();
    }

    private void Update()
    {
        moveInput = CanControl ? playerControls.Controls.Move.ReadValue<Vector2>().x : 0f;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (moveInput != 0f) Face(Mathf.Sign(moveInput));
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    private void Face(float direction)
    {
        transform.localScale = new Vector3(artFacesRight ? direction : -direction, 1f, 1f);
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
