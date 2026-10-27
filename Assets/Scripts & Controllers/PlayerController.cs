using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float facingDirection = 1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private bool isGrounded;

    public void Awake()
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
        playerControls.Disable();
    }

    public void PlayerInput() 
    {
        movement = playerControls.Controls.Move.ReadValue<Vector2>();
    }

    public void Update()
    {
        PlayerInput();
        CheckIsGrounded();
    }

    public void FixedUpdate() 
    {
        Move();
    }

    public void Move()
    {
        if (movement.x != 0)
        {
            facingDirection = Mathf.Sign(movement.x);
        }

        rb.linearVelocity = new Vector2(movement.x * moveSpeed, rb.linearVelocity.y);

        if (movement.x > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (movement.x < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void CheckIsGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    } 

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
}
