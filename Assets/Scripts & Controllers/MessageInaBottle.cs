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

    private void OpenNote()
    {
        PaperNote note = Instantiate(paperNotePrefab);
        note.Open(noteText, noteImage, () => DialogueManager.instance.StartDialogue(readDialogue, FinishReading));
    }

    private void FinishReading()
    {
        isReading = false;
        afterReading.Apply();
    }
}
