using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// On the Systems prefab: the old keeper's journal, drawn as a book. Pages: element 0 = page 1, found on Day 1.
// R opens the book at the newest collected page (arrow keys turn pages, R or a click closes it).
// ShowPage() is called when a page is found: it shows the loose page, then the book open at that day.
// The icon in the bottom-right corner shows once a page is collected.
public class Journal : MonoBehaviour
{
    public static Journal Instance { get; private set; }

    [Header("Input")]
    [SerializeField] private InputActionReference openAction;

    [Header("Pages")]
    [SerializeField] private JournalPage[] pages;
    [Tooltip("After a found loose page is put away, open the book at that day (where the day's notes are).")]
    [SerializeField] private bool openBookAfterFinding = true;

    [Header("References")]
    [SerializeField] private JournalView viewPrefab;
    [SerializeField] private GameObject icon;
    [SerializeField] private TextMeshProUGUI iconLabel;

    [Header("Text")]
    [SerializeField] private string iconText = "{0} Journal";

    private JournalView openView;

    public bool HasPages => GameState.Instance.CollectedPages.Count > 0;
    private bool CanOpen => HasPages && !PlayerLock.IsLocked && GameObject.FindWithTag("Player") != null;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        openAction.action.Enable();
        openAction.action.performed += OnOpen;
    }

    private void OnDisable()
    {
        openAction.action.performed -= OnOpen;
    }

    private void Update()
    {
        bool show = CanOpen;
        if (icon.activeSelf != show) icon.SetActive(show);
        if (show) iconLabel.text = string.Format(iconText, InteractionPrompt.KeyHint(openAction));
    }

    // The book at the newest collected page, from anywhere (the night scene's clipboard uses this; R needs the 2D player).
    public void OpenBook(Action onClosed = null)
    {
        if (openView == null) OpenBookAt(-1, onClosed);
    }

    // A page was just found: show it, then (optionally) the book open at it. onClosed runs when it's all put away.
    public void ShowPage(int pageNumber, Action onClosed)
    {
        JournalPage page = GetPage(pageNumber);
        if (page == null || page.WhenFound == null)
        {
            Debug.LogWarning($"Journal: page {pageNumber} has no art (Systems prefab → Journal → Pages).", this);
            onClosed?.Invoke();
            return;
        }

        bool bookShowsMore = openBookAfterFinding && page.InBook != page.WhenFound;
        Open(new[] { page.WhenFound }, 0, bookShowsMore ? () => OpenBookAt(pageNumber, onClosed) : onClosed);
    }

    // Any single picture shown the same way, with an optional title at the top (e.g. the key close-up on pickup).
    public void ShowCloseUp(Sprite picture, string title, Action onClosed)
    {
        Open(picture != null ? new[] { picture } : new Sprite[0], 0, onClosed, title);
    }

    private void OnOpen(InputAction.CallbackContext context)
    {
        if (openView != null) openView.Close();
        else if (CanOpen) OpenBookAt(-1, null);
    }

    // pageNumber -1 = the newest collected page.
    private void OpenBookAt(int pageNumber, Action onClosed)
    {
        List<int> numbers = new List<int>();
        List<Sprite> spreads = new List<Sprite>();
        List<int> collected = new List<int>(GameState.Instance.CollectedPages);
        collected.Sort();
        foreach (int number in collected)
        {
            JournalPage page = GetPage(number);
            if (page == null || page.InBook == null) continue;
            numbers.Add(number);
            spreads.Add(page.InBook);
        }

        int start = numbers.IndexOf(pageNumber);
        Open(spreads.ToArray(), start >= 0 ? start : spreads.Count - 1, onClosed);
    }

    private void Open(Sprite[] sprites, int startIndex, Action onClosed, string title = null)
    {
        if (openView != null || viewPrefab == null || sprites.Length == 0)
        {
            onClosed?.Invoke();
            return;
        }

        openView = Instantiate(viewPrefab);
        openView.Open(sprites, startIndex, () =>
        {
            openView = null;
            onClosed?.Invoke();
        }, title);
    }

    private JournalPage GetPage(int pageNumber)
    {
        int index = pageNumber - 1;
        return pages != null && index >= 0 && index < pages.Length ? pages[index] : null;
    }
}
