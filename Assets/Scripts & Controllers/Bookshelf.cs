using UnityEngine;

// A bookshelf the player can search (needs ClickToInteract or a trigger collider). It opens the Bookshelf Puzzle
// close-up; its books (titles, pictures, which one is the clue) are the Book hitboxes inside that prefab.
// Opening the clue book while carrying the Clue Item solves it: after the shelf closes, Clue Found Dialogue plays
// and After Finding Clue is applied (once; skipped when its flag is already set).
public class Bookshelf : Interactable
{
    [Header("Close-Up")]
    [SerializeField] private BookshelfPuzzle puzzlePrefab;

    [Header("Clue")]
    [Tooltip("The inventory item to compare with the clue book (the bottle's note).")]
    [SerializeField] private InventoryItem clueItem;
    [SerializeField] private Dialogue clueFoundDialogue;
    [SerializeField] private StoryOutcome afterFindingClue = new StoryOutcome();

    private BookshelfPuzzle openPuzzle;

    protected override void OnInteract()
    {
        if (openPuzzle != null) return;

        openPuzzle = Instantiate(puzzlePrefab);
        openPuzzle.Open(clueItem, OnClosed);
    }

    private void OnClosed(bool solved)
    {
        openPuzzle = null;
        if (!solved || GameState.Instance.HasFlag(afterFindingClue.setFlag)) return;

        DialogueManager.instance.StartDialogue(clueFoundDialogue, afterFindingClue.Apply);
    }
}
