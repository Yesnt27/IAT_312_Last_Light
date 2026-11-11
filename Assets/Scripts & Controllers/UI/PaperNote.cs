using System;
using UnityEngine;
using UnityEngine.InputSystem;

// A full-screen piece of paper to read (the message in a bottle, readable inventory items). It's a prefab
// (Assets/Prefabs/UI/Paper Note) with its own canvas: spawn it and call Open(text, image, onClosed).
// Click to put it away; it destroys itself when it closes. Journal pages use JournalView instead.
public class PaperNote : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference closeAction;
    [SerializeField] private InputActionReference clickAction;

    [Header("References")]
    [Tooltip("The paper: text, an optional picture, or a picture-only note drawn as the whole paper.")]
    [SerializeField] private NoteDisplay note;

    private Action onClosed;
    private int openedFrame;
    private bool isClosed;

    private void OnEnable()
    {
        closeAction.action.Enable();
        clickAction.action.Enable();
        closeAction.action.performed += OnClose;
        clickAction.action.performed += OnClose;
    }

    private void OnDisable()
    {
        closeAction.action.performed -= OnClose;
        clickAction.action.performed -= OnClose;
    }

    private void OnDestroy()
    {
        PlayerLock.Remove(this);
    }

    public void Open(string text, Sprite image, Action onClosedCallback)
    {
        onClosed = onClosedCallback;
        openedFrame = Time.frameCount;
        note.Show(text, image);
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

    // Ignores the press that opened it.
    private void OnClose(InputAction.CallbackContext context)
    {
        if (Time.frameCount != openedFrame) Close();
    }
}
