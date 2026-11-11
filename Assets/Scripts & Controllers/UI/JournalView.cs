using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Full-screen picture of journal pages (Assets/Prefabs/UI/Journal View, its own canvas). Spawn it and call
// Open(pages, startPage, onClosed): the arrow keys (TurnPage) flip through the pages and a click closes it.
// The player is frozen while it's open, and it destroys itself when it closes.
public class JournalView : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference closeAction;
    [SerializeField] private InputActionReference turnPageAction;

    [Header("References")]
    [SerializeField] private Image pageImage;
    [Tooltip("Page number and arrow-key hint; hidden when there's only one page.")]
    [SerializeField] private TextMeshProUGUI pageLabel;

    [Header("Text")]
    [SerializeField] private string pageText = "[Left Arrow] Back      Page {0} / {1}      Next [Right Arrow]";

    private Sprite[] pages;
    private int pageIndex;
    private Action onClosed;
    private int openedFrame;
    private bool isClosed;

    private void OnEnable()
    {
        closeAction.action.Enable();
        turnPageAction.action.Enable();
        closeAction.action.performed += OnClose;
        turnPageAction.action.performed += OnTurnPage;
    }

    private void OnDisable()
    {
        closeAction.action.performed -= OnClose;
        turnPageAction.action.performed -= OnTurnPage;
    }

    private void OnDestroy()
    {
        PlayerLock.Remove(this);
    }

    public void Open(Sprite[] journalPages, int startPage, Action onClosedCallback)
    {
        pages = journalPages;
        onClosed = onClosedCallback;
        openedFrame = Time.frameCount;
        PlayerLock.Add(this);
        ShowPage(startPage);
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
        pageImage.sprite = pages[pageIndex];
        pageLabel.gameObject.SetActive(pages.Length > 1);
        pageLabel.text = string.Format(pageText, pageIndex + 1, pages.Length);
    }

    private void OnTurnPage(InputAction.CallbackContext context)
    {
        float direction = context.ReadValue<float>();
        if (Mathf.Abs(direction) > 0.5f) ShowPage(pageIndex + (direction > 0f ? 1 : -1));
    }

    // Ignores the click that opened it.
    private void OnClose(InputAction.CallbackContext context)
    {
        if (Time.frameCount != openedFrame) Close();
    }
}
