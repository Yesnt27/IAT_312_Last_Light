using UnityEngine;
using UnityEngine.InputSystem;

// First-person player for 3D scenes (the night shift). WASD to walk, mouse to look, Space to jump.
// The camera is a child at eye height. Walls and floor come from the scene's colliders;
// Play Area Radius is an extra invisible limit around Play Area Center (0 = off).
// Freezes (and frees the mouse cursor) while PlayerLock is on, e.g. during dialogue.
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference clickAction;

    [Header("References")]
    [SerializeField] private Transform cameraPivot;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Play Area")]
    [SerializeField] private Transform playAreaCenter;
    [SerializeField] private float playAreaRadius = 0f;

    private CharacterController controller;
    private float pitch;
    private float verticalVelocity;
    private bool wasLocked;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
        clickAction.action.Enable();
        clickAction.action.performed += OnClick;
        CaptureCursor(true);
    }

    private void OnDisable()
    {
        clickAction.action.performed -= OnClick;
        CaptureCursor(false);
    }

    private void Update()
    {
        bool locked = PlayerLock.IsLocked;
        if (locked != wasLocked) CaptureCursor(!locked);
        wasLocked = locked;
        if (locked) return;

        Look();
        Move();
        KeepInPlayArea();
    }

    private void Look()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 delta = lookAction.action.ReadValue<Vector2>() * mouseSensitivity;
        transform.Rotate(0f, delta.x, 0f);
        pitch = Mathf.Clamp(pitch - delta.y, -maxLookAngle, maxLookAngle);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void Move()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3 walk = (transform.right * input.x + transform.forward * input.y) * moveSpeed;

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        if (controller.isGrounded && jumpAction.action.WasPressedThisFrame()) Jump();
        verticalVelocity += gravity * Time.deltaTime;

        controller.Move((walk + Vector3.up * verticalVelocity) * Time.deltaTime);
    }

    // The upward speed that reaches exactly Jump Height under this gravity.
    private void Jump()
    {
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void KeepInPlayArea()
    {
        if (playAreaCenter == null || playAreaRadius <= 0f) return;

        Vector3 offset = transform.position - playAreaCenter.position;
        offset.y = 0f;
        if (offset.magnitude <= playAreaRadius) return;

        Vector3 clamped = playAreaCenter.position + offset.normalized * playAreaRadius;
        clamped.y = transform.position.y;
        controller.enabled = false;
        transform.position = clamped;
        controller.enabled = true;
    }

    // Escape frees the cursor (Unity editor); clicking the game view captures it again.
    private void OnClick(InputAction.CallbackContext context)
    {
        if (!PlayerLock.IsLocked && Cursor.lockState != CursorLockMode.Locked) CaptureCursor(true);
    }

    private void CaptureCursor(bool capture)
    {
        Cursor.lockState = capture ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !capture;
    }

    private void OnDrawGizmosSelected()
    {
        if (playAreaCenter == null || playAreaRadius <= 0f) return;

        Gizmos.color = Color.green;
        Vector3 previous = playAreaCenter.position + Vector3.forward * playAreaRadius;
        for (int i = 1; i <= 48; i++)
        {
            Vector3 next = playAreaCenter.position + Quaternion.Euler(0f, i * 7.5f, 0f) * Vector3.forward * playAreaRadius;
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
}
