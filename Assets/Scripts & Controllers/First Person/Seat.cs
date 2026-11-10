using UnityEngine;
using UnityEngine.InputSystem;

// Something the first-person player can sit on (the lamp room chair). Put it on an object with a trigger
// collider around the seat: look at it and press Interact (E) to sit with your eyes at Eye Point.
// Seated, you can still look around and use the binoculars; Space stands you up (FirstPersonController).
// Chair (optional) swivels with the player around Eye Point's vertical axis, so its backrest stays behind them,
// and turns back to Chair Facing when they get up.
public class Seat : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    [Header("References")]
    [SerializeField] private FirstPersonController player;
    [Tooltip("Looking at this collider shows the prompt.")]
    [SerializeField] private Collider seatArea;
    [Tooltip("Where the player's eyes go while seated. Put it on the chair's swivel axis, above the backrest.")]
    [SerializeField] private Transform eyePoint;

    [Header("Prompt")]
    [SerializeField] private float reach = 2.5f;
    [SerializeField] private string promptText = "Sit down";

    [Header("Swivel")]
    [Tooltip("The chair model that turns with the player. Empty = it doesn't turn.")]
    [SerializeField] private Transform chair;
    [Tooltip("The way the chair faces (world Y angle) as placed in the scene.")]
    [SerializeField] private float chairFacing = 0f;
    [SerializeField] private float turnSpeed = 900f;

    private bool showingPrompt;
    private bool seatedHere;
    private float facing;
    private Vector3 chairPosition;
    private Quaternion chairRotation;

    private bool IsAiming => !player.IsSeated && player.CanWalk && !PlayerLock.IsLocked && player.IsLookingAt(seatArea, reach);

    private void Awake()
    {
        facing = chairFacing;
        if (chair == null) return;
        chairPosition = chair.position;
        chairRotation = chair.rotation;
    }

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

        seatedHere &= player.IsSeated;
        Swivel(seatedHere ? player.transform.eulerAngles.y : chairFacing);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!IsAiming) return;

        player.SitAt(eyePoint.position);
        seatedHere = true;
    }

    // Turns the chair towards targetFacing around the vertical line through Eye Point. It swings back
    // when the player stands up, so the spot they stood on before sitting is free again.
    private void Swivel(float targetFacing)
    {
        if (chair == null || Mathf.DeltaAngle(facing, targetFacing) == 0f) return;

        facing = Mathf.MoveTowardsAngle(facing, targetFacing, turnSpeed * Time.deltaTime);
        Quaternion turn = Quaternion.Euler(0f, facing - chairFacing, 0f);
        Vector3 axis = eyePoint.position;
        axis.y = chairPosition.y;
        chair.SetPositionAndRotation(axis + turn * (chairPosition - axis), turn * chairRotation);
    }
}
