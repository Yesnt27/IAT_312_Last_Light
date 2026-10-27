using UnityEngine;

// The player's bed. Ends the day once the night shift is over; before that it plays a "not tired" line.
public class Bed : Interactable
{
    [Header("Bed")]
    [SerializeField] private Dialogue notTiredDialogue;

    protected override void OnInteract()
    {
        if (DayManager.Instance.CanSleep) DayManager.Instance.GoToSleep();
        else DialogueManager.instance.StartDialogue(notTiredDialogue);
    }
}
