using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// On the Systems prefab: finds the ClickToInteract under the mouse, highlights it,
// shows "[Click] ..." in the prompt, and uses it when the left mouse button is pressed.
public class ClickInteractor : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference pointAction;
    [SerializeField] private InputActionReference clickAction;

    [Header("Prompt")]
    [SerializeField] private string clickHint = "[Click]";

    private ClickToInteract hovered;

    public ClickToInteract Hovered => hovered;

    private void OnEnable()
    {
        pointAction.action.Enable();
        clickAction.action.Enable();
        clickAction.action.performed += OnClick;
    }

    private void OnDisable()
    {
        clickAction.action.performed -= OnClick;
    }

    private void Update()
    {
        SetHovered(PlayerLock.IsLocked ? null : FindUnderMouse());
    }

    // After PlayerInteractor's Update, so a hovered clickable owns the prompt this frame.
    private void LateUpdate()
    {
        if (hovered != null && InteractionPrompt.Instance != null) InteractionPrompt.Instance.Show(hovered.Interactable.PromptText, clickHint);
    }

    private ClickToInteract FindUnderMouse()
    {
        Camera cam = Camera.main;
        if (cam == null || ClickToInteract.All.Count == 0) return null;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return null;

        Vector2 worldPoint = cam.ScreenToWorldPoint(pointAction.action.ReadValue<Vector2>());
        ClickToInteract best = null;
        foreach (ClickToInteract clickable in ClickToInteract.All)
        {
            if (!clickable.CanClick || !clickable.Contains(worldPoint)) continue;
            if (best == null || clickable.SortingOrder > best.SortingOrder) best = clickable;
        }
        return best;
    }

    private void SetHovered(ClickToInteract next)
    {
        if (hovered == next) return;
        if (hovered != null)
        {
            hovered.SetHovered(false);
            if (InteractionPrompt.Instance != null) InteractionPrompt.Instance.Hide();
        }
        hovered = next;
        if (hovered != null) hovered.SetHovered(true);
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (hovered != null && !PlayerLock.IsLocked) hovered.Interactable.Interact();
    }
}
