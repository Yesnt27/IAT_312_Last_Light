using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// The bookshelf close-up: a full-screen prefab (Assets/Prefabs/UI/Bookshelf Puzzle) with its own canvas.
// Bookshelf spawns it and calls Open(); it destroys itself when it closes. Spines light up under the mouse;
// click one to open that book. If the player carries the Clue Item (the bottle's note), it's shown beside
// every open book, and opening the clue book solves it. Esc puts the book back, then steps away.
public class BookshelfPuzzle : MonoBehaviour
{
    [Header("Shelf")]
    [SerializeField] private GameObject shelf;
    [SerializeField] private RectTransform spineRow;
    [SerializeField] private Button spineTemplate;
    [Tooltip("Spine colour while the mouse is over it (multiplied, like hovering the electrical box).")]
    [SerializeField] private Color hoverTint = new Color(1f, 1f, 0.55f);

    [Header("Open Book")]
    [SerializeField] private GameObject openBook;
    [SerializeField] private TextMeshProUGUI bookTitle;
    [SerializeField] private TextMeshProUGUI leftPage;
    [SerializeField] private TextMeshProUGUI rightPage;
    [SerializeField] private Button putBackButton;

    [Header("Clue Note")]
    [SerializeField] private GameObject heldNote;
    [SerializeField] private TextMeshProUGUI heldNoteText;
    [SerializeField] private Image heldNoteImage;

    [Header("References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI statusLabel;

    [Header("Text")]
    [SerializeField] private string shelfText = "Click a book to take it off the shelf";
    [SerializeField] private string bookText = "Compare the book with the note";
    [SerializeField] private string matchText = "The symbols on the note match the ones in this book!";
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color matchColor = new Color(0.45f, 1f, 0.5f);

    private ShelfBook[] books;
    private InventoryItem clueItem;
    private ShelfBook currentBook;
    private Action<bool> onClosed;
    private bool isSolved;

    private bool HasClueItem => clueItem != null && clueItem.IsHeld(GameState.Instance);

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        putBackButton.onClick.AddListener(PutBack);
        spineTemplate.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        PlayerLock.Remove(this);
    }

    public void Open(ShelfBook[] shelfBooks, InventoryItem clue, Action<bool> onClosedCallback)
    {
        books = shelfBooks;
        clueItem = clue;
        onClosed = onClosedCallback;
        PlayerLock.Add(this);

        for (int i = 0; i < books.Length; i++) AddSpine(i);
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

    public void OpenBook(int index)
    {
        currentBook = books[index];
        bookTitle.text = currentBook.title;
        leftPage.text = currentBook.leftPage;
        rightPage.text = currentBook.rightPage;
        shelf.SetActive(false);
        openBook.SetActive(true);
        heldNote.SetActive(HasClueItem);

        if (HasClueItem && currentBook.isClue)
        {
            isSolved = true;
            ShowStatus(matchText, matchColor);
        }
        else
        {
            ShowStatus(HasClueItem ? bookText : "", normalColor);
        }
    }

    public void PutBack()
    {
        currentBook = null;
        openBook.SetActive(false);
        heldNote.SetActive(false);
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

    private void AddSpine(int index)
    {
        Button spine = Instantiate(spineTemplate, spineRow);
        spine.gameObject.SetActive(true);
        spine.GetComponent<Image>().color = books[index].spineColor;
        spine.GetComponentInChildren<TextMeshProUGUI>().text = books[index].title;

        ColorBlock colors = spine.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = hoverTint;
        colors.selectedColor = Color.white;
        spine.colors = colors;

        LayoutElement size = spine.GetComponent<LayoutElement>();
        if (size != null) size.preferredHeight -= (index * 37) % 90;
        spine.onClick.AddListener(() => OpenBook(index));
    }

    // The note from the bottle, filled in once; it's shown whenever a book is open.
    private void ShowClueNote()
    {
        heldNote.SetActive(false);
        if (!HasClueItem) return;

        heldNoteText.text = clueItem.NoteText;
        heldNoteImage.gameObject.SetActive(clueItem.NoteImage != null);
        heldNoteImage.sprite = clueItem.NoteImage;
    }

    private void ShowStatus(string text, Color color)
    {
        statusLabel.text = text;
        statusLabel.color = color;
    }
}
