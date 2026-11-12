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
    [Tooltip("Applied after the locked dialogue, e.g. start the 'find the key' task.")]
    [SerializeField] private StoryOutcome whenTriedWhileLocked = new StoryOutcome();

    [Header("Sound (optional)")]
    [Tooltip("Plays every time it's used (e.g. a cupboard opening or closing).")]
    [SerializeField] private AudioClip interactSound;
    [Tooltip("Plays when it's tried while locked.")]
    [SerializeField] private AudioClip lockedSound;
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 1f;

    [Header("Events")]
    [SerializeField] private UnityEvent onInteract;

    public virtual string PromptText => promptText;
    public bool IsLocked => !string.IsNullOrEmpty(unlockFlag) && !GameState.Instance.HasFlag(unlockFlag);

    public bool IsAvailable => isActiveAndEnabled && StoryConditions.AreMet(activeOnDays, activeDuringTasks, requiredFlags, hiddenByFlags);

    public void Interact()
    {
        if (IsLocked)
        {
            SoundEffects.Play(lockedSound, soundVolume);
            DialogueManager.instance.StartDialogue(lockedDialogue, whenTriedWhileLocked.Apply);
            return;
        }

        SoundEffects.Play(interactSound, soundVolume);
        OnInteract();
        onInteract?.Invoke();
    }

    protected virtual void OnInteract()
    {
    }
}
