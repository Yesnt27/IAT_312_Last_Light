using UnityEngine;
using UnityEngine.Events;

// Base for everything the player can press Interact on. Needs a Collider2D (usually a trigger),
// or a ClickToInteract component to be used with the mouse instead (then no collider is needed).
// The conditions below decide when the prompt shows, so puzzle-only prompts stay hidden at other times.
public class Interactable : MonoBehaviour
{
    [Header("Prompt")]
    [SerializeField] private string promptText = "Interact";

    [Header("When Available (empty = always)")]
    [SerializeField] private int[] activeOnDays;
    [SerializeField] private string[] activeDuringTasks;
    [SerializeField] private string[] requiredFlags;
    [SerializeField] private string[] hiddenByFlags;

    [Header("Lock (empty = never locked)")]
    [SerializeField] private string unlockFlag;
    [SerializeField] private Dialogue lockedDialogue;

    [Header("Events")]
    [SerializeField] private UnityEvent onInteract;

    public string PromptText => promptText;
    public bool IsLocked => !string.IsNullOrEmpty(unlockFlag) && !GameState.Instance.HasFlag(unlockFlag);

    public bool IsAvailable
    {
        get
        {
            GameState state = GameState.Instance;
            if (state == null || !isActiveAndEnabled) return false;

            if (activeOnDays != null && activeOnDays.Length > 0 && System.Array.IndexOf(activeOnDays, state.CurrentDay) < 0) return false;

            if (activeDuringTasks != null && activeDuringTasks.Length > 0)
            {
                bool taskMatches = false;
                foreach (string task in activeDuringTasks)
                {
                    if (state.IsTaskActive(task)) taskMatches = true;
                }
                if (!taskMatches) return false;
            }

            if (requiredFlags != null)
            {
                foreach (string flag in requiredFlags)
                {
                    if (!string.IsNullOrEmpty(flag) && !state.HasFlag(flag)) return false;
                }
            }

            if (hiddenByFlags != null)
            {
                foreach (string flag in hiddenByFlags)
                {
                    if (state.HasFlag(flag)) return false;
                }
            }

            return true;
        }
    }

    public void Interact()
    {
        if (IsLocked)
        {
            if (lockedDialogue != null && lockedDialogue.dialogueLines.Count > 0)
            {
                DialogueManager.instance.StartDialogue(lockedDialogue);
            }
            return;
        }

        OnInteract();
        onInteract?.Invoke();
    }

    protected virtual void OnInteract()
    {
    }
}
