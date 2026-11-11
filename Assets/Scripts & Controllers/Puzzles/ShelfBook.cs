using UnityEngine;

// One book on a Bookshelf: its spine on the shelf and the picture of it lying open (Visual Assets/Journal Assets).
[System.Serializable]
public class ShelfBook
{
    public string title = "Book";
    public Color spineColor = new Color(0.45f, 0.25f, 0.15f);
    [Tooltip("The open book, e.g. Fishing_Almanac. Import it as Sprite (2D and UI).")]
    public Sprite openPages;
    [Tooltip("The book that solves the puzzle when it's opened while carrying the Clue Item.")]
    public bool isClue;
}
