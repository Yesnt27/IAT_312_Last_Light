using UnityEngine;
using UnityEngine.InputSystem;

// The clipboard on the lamp room desk: look at it and press F (Read) to look back through the journal pages
// collected so far, i.e. the ghost-ship notes. The journal opens over everything; Esc or a click puts it down.
// Works standing or seated in the chair. Runs after the other prompts so its own prompt wins while you look at it.
[DefaultExecutionOrder(100)]
public class NotesClipboard : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference readAction;

    [Header("References")]
    [SerializeField] private FirstPersonController player;
    [SerializeField] private Binoculars binoculars;
    [Tooltip("Looking at this collider (over the clipboard) shows the prompt.")]
    [SerializeField] private Collider lookZone;

    [Header("Prompt")]
    [SerializeField] private float reach = 5.5f;
    [SerializeField] private string promptText = "Read your journal notes";

    private bool showingPrompt;

    private bool IsAiming => !PlayerLock.IsLocked && !binoculars.IsLooking && Journal.Instance != null
        && Journal.Instance.HasPages && player.IsLookingAt(lookZone, reach);

    private void OnEnable()
    {
        readAction.action.Enable();
        readAction.action.performed += OnRead;
    }

    private void OnDisable()
    {
        readAction.action.performed -= OnRead;
    }

    private void Update()
    {
        bool show = IsAiming;
        if (show) InteractionPrompt.Instance.Show(promptText, InteractionPrompt.KeyHint(readAction));
        else if (showingPrompt) InteractionPrompt.Instance.Hide();
        showingPrompt = show;
    }

    private void OnRead(InputAction.CallbackContext context)
    {
        if (IsAiming) Journal.Instance.OpenBook();
    }
}
