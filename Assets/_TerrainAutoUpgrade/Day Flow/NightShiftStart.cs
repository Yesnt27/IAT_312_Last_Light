using UnityEngine;

// The lamp controls at the top of the lighthouse. Starts the night shift once the day's puzzle is done.
public class NightShiftStart : Interactable
{
    [Header("Night Shift")]
    [SerializeField] private Dialogue notReadyDialogue;

    protected override void OnInteract()
    {
        if (DayManager.Instance.CanStartNight) DayManager.Instance.StartNight();
        else DialogueManager.instance.StartDialogue(notReadyDialogue);
    }
}
