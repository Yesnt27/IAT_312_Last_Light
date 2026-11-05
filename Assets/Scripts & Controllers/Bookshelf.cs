using UnityEngine;

// A bookshelf the player can search (needs ClickToInteract or a trigger collider). It opens the Bookshelf Puzzle
// close-up with these books. Holding the Clue Item up next to the clue book solves it: after the shelf closes,
// Clue Found Dialogue plays and After Finding Clue is applied (once; skipped when its flag is already set).
public class Bookshelf : Interactable
{
    [Header("Books")]
    [SerializeField] private BookshelfPuzzle puzzlePrefab;
    [SerializeField] private ShelfBook[] books;

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
        openPuzzle.Open(books, clueItem, OnClosed);
    }

    private void OnClosed(bool solved)
    {
        openPuzzle = null;
        if (!solved || GameState.Instance.HasFlag(afterFindingClue.setFlag)) return;

        DialogueManager.instance.StartDialogue(clueFoundDialogue, afterFindingClue.Apply);
    }
}
