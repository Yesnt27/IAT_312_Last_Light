using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Full-screen picture of journal pages (Assets/Prefabs/UI/Journal View, its own canvas). Spawn it and call
// Open(pages, startPage, onClosed). The arrow keys (TurnPage) or the clickable arrows either side of the page
// number flip through the pages; a click anywhere else, or Esc, closes it.
// The player is frozen while it's open, and it destroys itself when it closes.
public class JournalView : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference closeAction;
    [SerializeField] private InputActionReference turnPageAction;

    [Header("References")]
    [SerializeField] private Image pageImage;
    [Tooltip("Page number; hidden when there's only one page.")]
    [SerializeField] private TextMeshProUGUI pageLabel;
    [Tooltip("The clickable arrows (the How To Play arrows). Each hides at its end of the journal.")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [Header("Text")]
    [SerializeField] private string pageText = "Page {0} / {1}";

    private Sprite[] pages;
    private int pageIndex;
    private Action onClosed;
    private int openedFrame;
    private bool isClosed;

    private void Awake()
    {
        previousButton.onClick.AddListener(() => ShowPage(pageIndex - 1));
        nextButton.onClick.AddListener(() => ShowPage(pageIndex + 1));
    }

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

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && Time.frameCount != openedFrame) Close();
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
        previousButton.gameObject.SetActive(pageIndex > 0);
        nextButton.gameObject.SetActive(pageIndex < pages.Length - 1);
    }

    private void OnTurnPage(InputAction.CallbackContext context)
    {
        float direction = context.ReadValue<float>();
        if (Mathf.Abs(direction) > 0.5f) ShowPage(pageIndex + (direction > 0f ? 1 : -1));
    }

    // Ignores the click that opened it, and clicks on the arrows (those turn the page instead).
    private void OnClose(InputAction.CallbackContext context)
    {
        if (Time.frameCount != openedFrame && !IsPointerOver(previousButton) && !IsPointerOver(nextButton)) Close();
    }

    private static bool IsPointerOver(Button button)
    {
        Mouse mouse = Mouse.current;
        return mouse != null && button.gameObject.activeInHierarchy
            && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)button.transform, mouse.position.ReadValue());
    }
}
