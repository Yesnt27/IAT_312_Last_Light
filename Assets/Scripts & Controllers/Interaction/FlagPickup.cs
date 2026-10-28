using UnityEngine;

// Interact to pick something up: sets a flag, plays an optional line, then hides the object.
// Hide it automatically once the flag is already set (e.g. coming back to the scene).
public class FlagPickup : Interactable
{
    [Header("Pickup")]
    [SerializeField] private string flag;
    [SerializeField] private Dialogue pickupDialogue;
    [SerializeField] private bool hideAfterPickup = true;

    private void Start()
    {
        if (hideAfterPickup && GameState.Instance.HasFlag(flag)) gameObject.SetActive(false);
    }

    protected override void OnInteract()
    {
        GameState.Instance.SetFlag(flag);

        if (pickupDialogue != null && pickupDialogue.dialogueLines.Count > 0)
        {
            DialogueManager.instance.StartDialogue(pickupDialogue);
        }

        if (hideAfterPickup) gameObject.SetActive(false);
    }
}
