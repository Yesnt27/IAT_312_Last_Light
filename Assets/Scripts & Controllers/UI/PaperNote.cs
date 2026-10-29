using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// A full-screen piece of paper to read (a note, a letter...). It's a prefab (Assets/Prefabs/UI/Paper Note)
// with its own canvas: spawn it and call Open(text, image, onClosed). Click or press Enter to put it away;
// it destroys itself when it closes.
public class PaperNote : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference closeAction;
    [SerializeField] private InputActionReference clickAction;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI noteLabel;
    [Tooltip("Optional picture on the paper; hidden when Open gets no sprite.")]
    [SerializeField] private Image noteImage;

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
        noteLabel.text = text;
        noteImage.gameObject.SetActive(image != null);
        if (image != null) noteImage.sprite = image;
        PlayerLock.Add(this);
    }

    // Ignores the press that opened it.
    private void OnClose(InputAction.CallbackContext context)
    {
        if (isClosed || Time.frameCount == openedFrame) return;

        isClosed = true;
        PlayerLock.Remove(this);
        Destroy(gameObject);
        onClosed?.Invoke();
    }
}
