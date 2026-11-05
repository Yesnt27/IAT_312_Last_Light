using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// The bookshelf close-up: a full-screen prefab (Assets/Prefabs/UI/Bookshelf Puzzle) with its own canvas.
// Bookshelf spawns it and calls Open(); it destroys itself when it closes. Click a spine to open that book,
// and press F (Inventory's Use Item) to hold the selected item up beside it. Holding the Clue Item up while
// the clue book is open solves it. Esc puts the book back, then steps away.
public class BookshelfPuzzle : MonoBehaviour, IItemUseHandler
{
    [Header("Shelf")]
    [SerializeField] private GameObject shelf;
    [SerializeField] private RectTransform spineRow;
    [SerializeField] private Button spineTemplate;

    [Header("Open Book")]
    [SerializeField] private GameObject openBook;
    [SerializeField] private TextMeshProUGUI bookTitle;
    [SerializeField] private TextMeshProUGUI leftPage;
    [SerializeField] private TextMeshProUGUI rightPage;
    [SerializeField] private Button putBackButton;

    [Header("Held Up Item")]
    [SerializeField] private GameObject heldNote;
    [SerializeField] private TextMeshProUGUI heldNoteText;
    [SerializeField] private Image heldNoteImage;

    [Header("References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI statusLabel;
    [SerializeField] private TextMeshProUGUI itemHintLabel;

    [Header("Text")]
    [SerializeField] private string shelfText = "Click a book to take it off the shelf";
    [SerializeField] private string bookText = "Compare it with something you carry";
    [SerializeField] private string wrongItemText = "That won't help with this.";
    [SerializeField] private string matchText = "The symbols on the note match the ones in this book!";
    [SerializeField] private string itemHint = "{0} Hold up: {1}   [Scroll] Switch item";
    [SerializeField] private string noItemHint = "You have nothing to compare it with.";
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color matchColor = new Color(0.45f, 1f, 0.5f);

    private readonly List<Button> spines = new List<Button>();
    private ShelfBook[] books;
    private InventoryItem clueItem;
    private InventoryItem heldItem;
    private ShelfBook currentBook;
    private Action<bool> onClosed;
    private bool isSolved;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        putBackButton.onClick.AddListener(PutBack);
        spineTemplate.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        PlayerLock.Remove(this);
        if (Inventory.Instance != null)
        {
            Inventory.Instance.RemoveUseHandler(this);
            Inventory.Instance.Changed -= RefreshItemHint;
        }
    }

    public void Open(ShelfBook[] shelfBooks, InventoryItem clue, Action<bool> onClosedCallback)
    {
        books = shelfBooks;
        clueItem = clue;
        onClosed = onClosedCallback;
        PlayerLock.Add(this);
        if (Inventory.Instance != null)
        {
            Inventory.Instance.AddUseHandler(this);
            Inventory.Instance.Changed += RefreshItemHint;
        }

        for (int i = 0; i < books.Length; i++)
        {
            int index = i;
            Button spine = Instantiate(spineTemplate, spineRow);
            spine.gameObject.SetActive(true);
            spine.GetComponent<Image>().color = books[i].spineColor;
            spine.GetComponentInChildren<TextMeshProUGUI>().text = books[i].title;
            LayoutElement size = spine.GetComponent<LayoutElement>();
            if (size != null) size.preferredHeight -= (i * 37) % 90;
            spine.onClick.AddListener(() => OpenBook(index));
            spines.Add(spine);
        }

        heldNote.SetActive(false);
        PutBack();
        RefreshItemHint();
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
        ShowStatus(bookText, normalColor);
        CheckMatch();
    }

    public void PutBack()
    {
        currentBook = null;
        openBook.SetActive(false);
        shelf.SetActive(true);
        ShowStatus(shelfText, normalColor);
    }

    // F while the shelf is open: hold the selected item up next to the book (F again puts it away).
    public bool TryUse(InventoryItem item)
    {
        if (!item.IsReadable)
        {
            ShowStatus(wrongItemText, normalColor);
            return true;
        }

        heldItem = heldItem == item ? null : item;
        heldNote.SetActive(heldItem != null);
        if (heldItem != null)
        {
            heldNoteText.text = heldItem.NoteText;
            heldNoteImage.gameObject.SetActive(heldItem.NoteImage != null);
            heldNoteImage.sprite = heldItem.NoteImage;
        }
        CheckMatch();
        return true;
    }

    public void Close()
    {
        Action<bool> closed = onClosed;
        onClosed = null;
        PlayerLock.Remove(this);
        Destroy(gameObject);
        closed?.Invoke(isSolved);
    }

    private void CheckMatch()
    {
        if (currentBook == null || !currentBook.isClue || heldItem == null || heldItem != clueItem) return;

        isSolved = true;
        ShowStatus(matchText, matchColor);
    }

    private void RefreshItemHint()
    {
        Inventory inventory = Inventory.Instance;
        InventoryItem selected = inventory != null ? inventory.Selected : null;
        itemHintLabel.text = selected != null
            ? string.Format(itemHint, InteractionPrompt.KeyHint(inventory.UseAction), selected.ItemName)
            : noItemHint;
    }

    private void ShowStatus(string text, Color color)
    {
        statusLabel.text = text;
        statusLabel.color = color;
    }
}
