using UnityEngine;

// One book on a Bookshelf: its spine on the shelf and the two pages it opens to.
[System.Serializable]
public class ShelfBook
{
    public string title = "Book";
    public Color spineColor = new Color(0.45f, 0.25f, 0.15f);
    [TextArea(3, 10)]
    public string leftPage;
    [TextArea(3, 10)]
    public string rightPage;
    [Tooltip("The book that solves the puzzle when the Clue Item is held up next to it.")]
    public bool isClue;
}
