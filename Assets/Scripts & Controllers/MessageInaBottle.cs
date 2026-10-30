using UnityEngine;

// The message in a bottle floating by the dock. Click it (needs ClickToInteract next to it) to fish it out:
// Open Dialogue plays, then the note inside opens on a Paper Note to read. Putting the note away plays
// Read Dialogue and applies After Reading (e.g. set BottleMessage and start the "find the book" task).
public class MessageInaBottle : Interactable
{
    [Header("Note")]
    [SerializeField] private PaperNote paperNotePrefab;
    [TextArea(3, 8)]
    [SerializeField] private string noteText;
    [Tooltip("Optional picture on the note.")]
    [SerializeField] private Sprite noteImage;

    [Header("Dialogue")]
    [SerializeField] private Dialogue openDialogue;
    [SerializeField] private Dialogue readDialogue;
    [SerializeField] private StoryOutcome afterReading = new StoryOutcome();

    private bool isReading;

    protected override void OnInteract()
    {
        if (isReading) return;

        isReading = true;
        DialogueManager.instance.StartDialogue(openDialogue, OpenNote);
    }

    // Without a Paper Note Prefab the note is skipped, so the quest can still go on.
    private void OpenNote()
    {
        if (paperNotePrefab == null)
        {
            Debug.LogError($"{name}: Paper Note Prefab is empty, so the note can't open. Drag Assets/Prefabs/UI/Paper Note into it.", this);
            ReadNote();
            return;
        }

        PaperNote note = Instantiate(paperNotePrefab);
        note.Open(noteText, noteImage, ReadNote);
    }

    private void ReadNote()
    {
        DialogueManager.instance.StartDialogue(readDialogue, FinishReading);
    }
    

    private void FinishReading()
    {
        isReading = false;
        afterReading.Apply();
    }
}
