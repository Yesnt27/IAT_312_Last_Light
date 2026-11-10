using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// A full-screen piece of paper to read (a note, a letter, journal pages...). It's a prefab (Assets/Prefabs/UI/Paper Note)
// with its own canvas: spawn it and call Open(text, image, onClosed), or Open(pages, startPage, onClosed) for several
// pages, which A/D (the Move action) flip through. Click to put it away; it destroys itself when it closes.
public class PaperNote : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference closeAction;
    [SerializeField] private InputActionReference clickAction;
    [SerializeField] private InputActionReference turnPageAction;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI noteLabel;
    [Tooltip("Optional picture on the paper; hidden when Open gets no sprite.")]
    [SerializeField] private Image noteImage;
    [Tooltip("'Page 1 / 3' with the A/D hint; hidden for a single page.")]
    [SerializeField] private TextMeshProUGUI pageLabel;

    [Header("Text")]
    [SerializeField] private string pageText = "[A] Back     Page {0} / {1}     Next [D]";

    private Action onClosed;
    private int openedFrame;
    private bool isClosed;
    private string[] pages;
    private int pageIndex;

    private void OnEnable()
    {
        closeAction.action.Enable();
        clickAction.action.Enable();
        closeAction.action.performed += OnClose;
        clickAction.action.performed += OnClose;
        if (turnPageAction != null)
        {
            turnPageAction.action.Enable();
            turnPageAction.action.performed += OnTurnPage;
        }
    }

    private void OnDisable()
    {
        closeAction.action.performed -= OnClose;
        clickAction.action.performed -= OnClose;
        if (turnPageAction != null) turnPageAction.action.performed -= OnTurnPage;
    }

    private void OnDestroy()
    {
        PlayerLock.Remove(this);
    }

    public void Open(string text, Sprite image, Action onClosedCallback)
    {
        Open(new[] { text }, 0, onClosedCallback);
        noteImage.gameObject.SetActive(image != null);
        if (image != null) noteImage.sprite = image;
    }

    public void Open(string[] notePages, int startPage, Action onClosedCallback)
    {
        onClosed = onClosedCallback;
        openedFrame = Time.frameCount;
        pages = notePages;
        noteImage.gameObject.SetActive(false);
        ShowPage(startPage);
        PlayerLock.Add(this);
    }

    public void Close()
    {
        if (isClosed) return;

        isClosed = true;
        PlayerLock.Remove(this);
        Destroy(gameObject);
        onClosed?.Invoke();
    }

    private void ShowPage(int index)
    {
        pageIndex = Mathf.Clamp(index, 0, pages.Length - 1);
        noteLabel.text = pages[pageIndex];
        if (pageLabel == null) return;
        pageLabel.gameObject.SetActive(pages.Length > 1);
        pageLabel.text = string.Format(pageText, pageIndex + 1, pages.Length);
    }

    private void OnTurnPage(InputAction.CallbackContext context)
    {
        float x = context.ReadValue<Vector2>().x;
        if (Mathf.Abs(x) > 0.5f) ShowPage(pageIndex + (x > 0f ? 1 : -1));
    }

    // Ignores the press that opened it.
    private void OnClose(InputAction.CallbackContext context)
    {
        if (Time.frameCount != openedFrame) Close();
    }
}
