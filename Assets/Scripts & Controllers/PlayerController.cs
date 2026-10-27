using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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


    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerControls = new PlayerControls();

    }

     private void OnEnable()
    {
        playerControls = new PlayerControls();
        playerControls.Enable();
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
    }

    public void FixedUpdate() 
    {
        Move();
        Jump();
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

    public void Jump()
    {
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && playerControls.Controls.Jump.triggered)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }


}
