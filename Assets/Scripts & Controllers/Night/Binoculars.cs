using UnityEngine;
using UnityEngine.InputSystem;

// The binoculars on the lamp room desk. Look at their spot and press E to pick them up (E at the
// same spot puts them back). While holding them, click to look through them: the camera zooms in,
// you stand still and the binocular HUD shows. Right click selects the ship under the crosshair
// (green outline); then report it at the Real / Fake buttons (ShipReportPanel).
public class Binoculars : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference useAction;
    [SerializeField] private InputActionReference selectAction;

    [Header("References")]
    [SerializeField] private FirstPersonController player;
    [SerializeField] private Camera viewCamera;
    [SerializeField] private Transform model;
    [Tooltip("Where the binoculars lie. Looking at this collider shows the pick up / put down prompt.")]
    [SerializeField] private Collider restSpot;
    [SerializeField] private BinocularHud hud;

    [Header("Holding")]
    [SerializeField] private float reach = 2.5f;
    [Tooltip("Where the model sits in front of the camera while held.")]
    [SerializeField] private Vector3 holdPosition = new Vector3(0.22f, -0.2f, 0.42f);
    [SerializeField] private Vector3 holdRotation = Vector3.zero;
    [SerializeField] private float holdScale = 0.6f;
    [SerializeField] private string pickUpText = "Pick up binoculars";
    [SerializeField] private string putDownText = "Put down binoculars";

    [Header("Looking")]
    [SerializeField] private float zoomFieldOfView = 12f;
    [SerializeField] private float zoomTime = 0.2f;
    [Tooltip("Extra degrees around a ship that still count as aiming at it.")]
    [SerializeField] private float aimTolerance = 1f;
    [Tooltip("Seconds after selecting a ship before the binoculars lower by themselves. 0 = stay up until you click.")]
    [SerializeField] private float lowerAfterSelecting = 0f;

    private Transform restParent;
    private Vector3 restPosition;
    private Quaternion restRotation;
    private Vector3 restScale;
    private float normalFieldOfView;
    private bool cursorWasCaptured;
    private bool showingPrompt;
    private float lowerTimer;
    private NightShip aimedShip;

    public bool IsHeld { get; private set; }
    public bool IsLooking { get; private set; }

    private bool IsAimingAtSpot => !IsLooking && !PlayerLock.IsLocked && player.IsLookingAt(restSpot, reach);

    private void Awake()
    {
        restParent = model.parent;
        restPosition = model.localPosition;
        restRotation = model.localRotation;
        restScale = model.localScale;
        normalFieldOfView = viewCamera.fieldOfView;
    }

    private void OnEnable()
    {
        interactAction.action.Enable();
        useAction.action.Enable();
        selectAction.action.Enable();
        interactAction.action.performed += OnInteract;
        useAction.action.performed += OnUse;
        selectAction.action.performed += OnSelect;
    }

    private void OnDisable()
    {
        interactAction.action.performed -= OnInteract;
        useAction.action.performed -= OnUse;
        selectAction.action.performed -= OnSelect;
    }

    private void Update()
    {
        if (IsLooking && PlayerLock.IsLocked) SetLooking(false);

        aimedShip = IsLooking ? FindAimedShip() : null;
        hud.SetAimed(aimedShip);
        hud.ShowHeldHint(IsHeld && !IsLooking && !PlayerLock.IsLocked);
        UpdatePrompt();
        UpdateLowerTimer();
        Zoom();
    }

    // The click that re-captures a freed cursor shouldn't also raise the binoculars.
    private void LateUpdate()
    {
        cursorWasCaptured = Cursor.lockState == CursorLockMode.Locked;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!IsAimingAtSpot) return;

        if (IsHeld) PutDown();
        else PickUp();
    }

    private void OnUse(InputAction.CallbackContext context)
    {
        if (!IsHeld || PlayerLock.IsLocked || !cursorWasCaptured) return;
        SetLooking(!IsLooking);
    }

    private void OnSelect(InputAction.CallbackContext context)
    {
        if (!IsLooking || aimedShip == null) return;

        aimedShip.Select();
        if (lowerAfterSelecting > 0f) lowerTimer = lowerAfterSelecting;
    }

    private void PickUp()
    {
        IsHeld = true;
        model.SetParent(viewCamera.transform, false);
        model.localPosition = holdPosition;
        model.localRotation = Quaternion.Euler(holdRotation);
        model.localScale = restScale * holdScale;
    }

    private void PutDown()
    {
        IsHeld = false;
        model.SetParent(restParent, false);
        model.localPosition = restPosition;
        model.localRotation = restRotation;
        model.localScale = restScale;
    }

    private void SetLooking(bool looking)
    {
        IsLooking = looking;
        lowerTimer = 0f;
        model.gameObject.SetActive(!looking);
        player.CanWalk = !looking;
        player.LookSensitivityScale = looking ? zoomFieldOfView / normalFieldOfView : 1f;
        hud.ShowView(looking);
    }

    private void UpdatePrompt()
    {
        bool show = IsAimingAtSpot;
        if (show) InteractionPrompt.Instance.Show(IsHeld ? putDownText : pickUpText, InteractionPrompt.KeyHint(interactAction));
        else if (showingPrompt) InteractionPrompt.Instance.Hide();
        showingPrompt = show;
    }

    private void UpdateLowerTimer()
    {
        if (lowerTimer <= 0f) return;

        lowerTimer -= Time.deltaTime;
        if (lowerTimer <= 0f && IsLooking) SetLooking(false);
    }

    private void Zoom()
    {
        float target = IsLooking ? zoomFieldOfView : normalFieldOfView;
        float speed = (normalFieldOfView - zoomFieldOfView) / Mathf.Max(zoomTime, 0.01f);
        viewCamera.fieldOfView = Mathf.MoveTowards(viewCamera.fieldOfView, target, speed * Time.deltaTime);
    }

    // The ship closest to the crosshair, if the crosshair is on (or just around) it.
    private NightShip FindAimedShip()
    {
        Transform eye = viewCamera.transform;
        NightShip best = null;
        float bestAngle = float.MaxValue;

        foreach (NightShip ship in NightShip.Sailing)
        {
            if (!ship.CanBeSelected) continue;

            Bounds bounds = ship.GetBounds();
            Vector3 toShip = bounds.center - eye.position;
            float angle = Vector3.Angle(eye.forward, toShip);
            float size = Mathf.Atan2(bounds.extents.magnitude * 0.7f, toShip.magnitude) * Mathf.Rad2Deg + aimTolerance;
            if (angle < size && angle < bestAngle)
            {
                best = ship;
                bestAngle = angle;
            }
        }
        return best;
    }
}
