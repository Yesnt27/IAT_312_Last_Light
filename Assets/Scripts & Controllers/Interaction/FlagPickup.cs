using UnityEngine;

// Interact to pick something up: sets a flag, plays an optional line, then hides the object.
// Hide it automatically once the flag is already set (e.g. coming back to the scene).
// Close-Up Picture (optional) is shown full screen after the line, like a found journal page, with its title on top.
public class FlagPickup : Interactable
{
    [Header("Pickup")]
    [SerializeField] private string flag;
    [SerializeField] private Dialogue pickupDialogue;
    [SerializeField] private bool hideAfterPickup = true;
    [Tooltip("Applied after the pickup dialogue (and the close-up), e.g. change the task.")]
    [SerializeField] private StoryOutcome afterPickup = new StoryOutcome();

    [Header("Close-Up (optional)")]
    [Tooltip("Shown full screen after the pickup line. Empty = no close-up.")]
    [SerializeField] private Sprite closeUpPicture;
    [SerializeField] private string closeUpTitle;

    private void Start()
    {
        if (hideAfterPickup && GameState.Instance.HasFlag(flag)) gameObject.SetActive(false);
    }

    protected override void OnInteract()
    {
        GameState.Instance.SetFlag(flag);
        DialogueManager.instance.StartDialogue(pickupDialogue, ShowCloseUp);

        if (hideAfterPickup) gameObject.SetActive(false);
    }

    private void ShowCloseUp()
    {
        if (closeUpPicture != null && Journal.Instance != null) Journal.Instance.ShowCloseUp(closeUpPicture, closeUpTitle, afterPickup.Apply);
        else afterPickup.Apply();
    }
}
