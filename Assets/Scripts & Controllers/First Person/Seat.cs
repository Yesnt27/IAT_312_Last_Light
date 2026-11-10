using UnityEngine;
using UnityEngine.InputSystem;

// Something the first-person player can sit on (the lamp room chair). Put it on an object with a trigger
// collider around the seat: look at it and press Interact (E) to sit with your eyes at Eye Point.
// Seated, you can still look around and use the binoculars; Space stands you up (FirstPersonController).
public class Seat : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    [Header("References")]
    [SerializeField] private FirstPersonController player;
    [Tooltip("Looking at this collider shows the prompt.")]
    [SerializeField] private Collider seatArea;
    [Tooltip("Where the player's eyes go while seated.")]
    [SerializeField] private Transform eyePoint;

    [Header("Prompt")]
    [SerializeField] private float reach = 2.5f;
    [SerializeField] private string promptText = "Sit down";

    private bool showingPrompt;

    private bool IsAiming => !player.IsSeated && player.CanWalk && !PlayerLock.IsLocked && player.IsLookingAt(seatArea, reach);

    private void OnEnable()
    {
        interactAction.action.Enable();
        interactAction.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        interactAction.action.performed -= OnInteract;
    }

    private void Update()
    {
        bool show = IsAiming;
        if (show) InteractionPrompt.Instance.Show(promptText, InteractionPrompt.KeyHint(interactAction));
        else if (showingPrompt) InteractionPrompt.Instance.Hide();
        showingPrompt = show;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (IsAiming) player.SitAt(eyePoint.position);
    }
}
