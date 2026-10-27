using UnityEngine;

// Interact to play a dialogue, then optionally set a flag and change the task.
public class InteractDialogue : Interactable
{
    [Header("Dialogue")]
    [SerializeField] private Dialogue dialogue;

    [Header("After Dialogue (optional)")]
    [SerializeField] private string setFlag;
    [SerializeField] private string newTaskId;
    [SerializeField] private string newTaskText;

    protected override void OnInteract()
    {
        DialogueManager.instance.StartDialogue(dialogue, OnDialogueFinished);
    }

    private void OnDialogueFinished()
    {
        GameState state = GameState.Instance;
        if (!string.IsNullOrEmpty(setFlag)) state.SetFlag(setFlag);
        if (!string.IsNullOrEmpty(newTaskId)) state.SetTask(newTaskId, newTaskText);
    }
}
