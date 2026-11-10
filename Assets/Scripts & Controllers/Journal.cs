using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// On the Systems prefab: the old keeper's journal. Pages holds what's written on each page (element 0 = page 1).
// Collected pages (GameState) open on a Paper Note: R opens them any time (A/D flip, click or R closes), and
// ShowPage() opens the page just found. The icon in the bottom-right corner shows once a page is collected.
public class Journal : MonoBehaviour
{
    public static Journal Instance { get; private set; }

    [Header("Input")]
    [SerializeField] private InputActionReference openAction;

    [Header("Pages")]
    [TextArea(4, 12)]
    [SerializeField] private string[] pages;

    [Header("References")]
    [SerializeField] private PaperNote paperNotePrefab;
    [SerializeField] private GameObject icon;
    [SerializeField] private TextMeshProUGUI iconLabel;

    [Header("Text")]
    [SerializeField] private string iconText = "{0} Journal";
    [SerializeField] private string missingPageText = "Page {0}";

    private PaperNote openNote;

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
        bool show = HasPages && !PlayerLock.IsLocked && GameObject.FindWithTag("Player") != null;
        if (icon.activeSelf != show) icon.SetActive(show);
        if (show) iconLabel.text = string.Format(iconText, InteractionPrompt.KeyHint(openAction));
    }

    private bool HasPages => GameState.Instance.CollectedPages.Count > 0;

    // Opens the collected pages at this one (used when a page is found). onClosed runs when the paper is put away.
    public void ShowPage(int pageNumber, Action onClosed)
    {
        List<int> collected = CollectedPages();
        Open(collected, Mathf.Max(0, collected.IndexOf(pageNumber)), onClosed);
    }

    private void OnOpen(InputAction.CallbackContext context)
    {
        if (openNote != null)
        {
            openNote.Close();
            return;
        }
        if (!HasPages || PlayerLock.IsLocked || GameObject.FindWithTag("Player") == null) return;

        List<int> collected = CollectedPages();
        Open(collected, collected.Count - 1, null);
    }

    private void Open(List<int> pageNumbers, int startIndex, Action onClosed)
    {
        if (openNote != null || paperNotePrefab == null || pageNumbers.Count == 0)
        {
            onClosed?.Invoke();
            return;
        }

        string[] texts = new string[pageNumbers.Count];
        for (int i = 0; i < texts.Length; i++) texts[i] = PageText(pageNumbers[i]);

        openNote = Instantiate(paperNotePrefab);
        openNote.Open(texts, startIndex, onClosed);
    }

    private List<int> CollectedPages()
    {
        List<int> collected = new List<int>(GameState.Instance.CollectedPages);
        collected.Sort();
        return collected;
    }

    private string PageText(int pageNumber)
    {
        int index = pageNumber - 1;
        bool written = pages != null && index >= 0 && index < pages.Length && !string.IsNullOrEmpty(pages[index]);
        return written ? pages[index] : string.Format(missingPageText, pageNumber);
    }
}
