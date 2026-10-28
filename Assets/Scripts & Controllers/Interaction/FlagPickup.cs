using UnityEngine;

// Interact to pick something up: sets a flag, plays an optional line, then hides the object.
// Hide it automatically once the flag is already set (e.g. coming back to the scene).
public class FlagPickup : Interactable
{
    [Header("Pickup")]
    [SerializeField] private string flag;
    [SerializeField] private Dialogue pickupDialogue;
    [SerializeField] private bool hideAfterPickup = true;
    [Tooltip("Applied after the pickup dialogue, e.g. change the task.")]
    [SerializeField] private StoryOutcome afterPickup = new StoryOutcome();

    private void Start()
    {
        if (hideAfterPickup && GameState.Instance.HasFlag(flag)) gameObject.SetActive(false);
    }

    protected override void OnInteract()
    {
        GameState.Instance.SetFlag(flag);
        DialogueManager.instance.StartDialogue(pickupDialogue, afterPickup.Apply);

        if (hideAfterPickup) gameObject.SetActive(false);
    }
}
