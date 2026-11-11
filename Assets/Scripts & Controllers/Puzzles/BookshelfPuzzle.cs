using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// The bookshelf close-up: a full-screen prefab (Assets/Prefabs/UI/Bookshelf Puzzle) with its own canvas, showing the
// shelf picture (bookshelf_zoomed). Every book in the picture has a ShelfBookSpot hitbox under Shelf with its
// open-book picture and Is Clue. Hovering a book lights it up (no title, so the player has to look inside); clicking opens it.
// If the player carries the Clue Item (the bottle's note), it's shown beside every open book, and opening the clue book
// solves it. Esc puts the book back, then steps away. Bookshelf spawns it and calls Open(); it destroys itself when it closes.
public class BookshelfPuzzle : MonoBehaviour
{
    [Header("Shelf")]
    [SerializeField] private GameObject shelf;

    [Header("Open Book")]
    [SerializeField] private GameObject openBook;
    [Tooltip("Shows the open book's picture (Preserve Aspect).")]
    [SerializeField] private Image bookPages;
    [SerializeField] private Button putBackButton;

    [Header("Clue Note")]
    [SerializeField] private NoteDisplay clueNote;

    [Header("References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI statusLabel;

    [Header("Text")]
    [SerializeField] private string shelfText = "Click a book to take it off the shelf";
    [SerializeField] private string bookText = "Compare the book with the note";
    [SerializeField] private string matchText = "The symbols on the note match the ones in this book!";
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color matchColor = new Color(0.45f, 1f, 0.5f);

    private InventoryItem clueItem;
    private ShelfBookSpot currentBook;
    private Action<bool> onClosed;
    private bool isSolved;

    private bool HasClueItem => clueItem != null && clueItem.IsHeld(GameState.Instance);

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        putBackButton.onClick.AddListener(PutBack);
        foreach (ShelfBookSpot book in shelf.GetComponentsInChildren<ShelfBookSpot>(true)) book.Clicked += OpenBook;
    }

    private void OnDestroy()
    {
        PlayerLock.Remove(this);
    }

    public void Open(InventoryItem clue, Action<bool> onClosedCallback)
    {
        clueItem = clue;
        onClosed = onClosedCallback;
        PlayerLock.Add(this);

        ShowClueNote();
        PutBack();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;

        if (currentBook != null) PutBack();
        else Close();
    }

    public void PutBack()
    {
        currentBook = null;
        openBook.SetActive(false);
        clueNote.gameObject.SetActive(false);
        shelf.SetActive(true);
        ShowStatus(shelfText, normalColor);
    }

    public void Close()
    {
        Action<bool> closed = onClosed;
        onClosed = null;
        PlayerLock.Remove(this);
        Destroy(gameObject);
        closed?.Invoke(isSolved);
    }

    private void OpenBook(ShelfBookSpot book)
    {
        currentBook = book;
        bookPages.sprite = book.OpenPages;
        shelf.SetActive(false);
        openBook.SetActive(true);
        clueNote.gameObject.SetActive(HasClueItem);

        if (HasClueItem && book.IsClue)
        {
            isSolved = true;
            ShowStatus(matchText, matchColor);
        }
        else
        {
            ShowStatus(HasClueItem ? bookText : "", normalColor);
        }
    }

    // The note from the bottle, filled in once; it's shown whenever a book is open.
    private void ShowClueNote()
    {
        clueNote.gameObject.SetActive(false);
        if (HasClueItem) clueNote.Show(clueItem.NoteText, clueItem.NoteImage);
    }

    private void ShowStatus(string text, Color color)
    {
        statusLabel.text = text;
        statusLabel.color = color;
    }
}
