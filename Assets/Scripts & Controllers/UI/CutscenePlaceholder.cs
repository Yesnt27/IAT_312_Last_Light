using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Stands in for a real cutscene: a full-screen card with the cutscene's name and description.
public class CutscenePlaceholder : MonoBehaviour
{
    public static CutscenePlaceholder Instance { get; private set; }

    [Header("Input")]
    [SerializeField] private InputActionReference continueAction;

    [Header("References")]
    [SerializeField] private GameObject cutscenePanel;
    [SerializeField] private TextMeshProUGUI titleLabel;
    [SerializeField] private TextMeshProUGUI descriptionLabel;
    [SerializeField] private Button continueButton;

    private Action onClosed;
    private int shownFrame;

    public bool IsShowing => cutscenePanel.activeSelf;

    private void Awake()
    {
        Instance = this;
        continueButton.onClick.AddListener(Close);
        cutscenePanel.SetActive(false);
    }

    private void OnEnable()
    {
        continueAction.action.Enable();
        continueAction.action.performed += OnContinue;
    }

    private void OnDisable()
    {
        continueAction.action.performed -= OnContinue;
    }

    public void Show(CutsceneCard card, Action onComplete = null)
    {
        Show(card.title, card.description, onComplete);
    }

    public void Show(string title, string description, Action onComplete = null)
    {
        ShowCard($"CUTSCENE: {title}", description, onComplete);
    }

    // A full-screen card without the "CUTSCENE:" prefix (night placeholders, reports).
    public void ShowCard(string heading, string description, Action onComplete = null)
    {
        onClosed = onComplete;
        shownFrame = Time.frameCount;
        titleLabel.text = heading;
        descriptionLabel.text = description;
        cutscenePanel.SetActive(true);
        PlayerLock.Add(this);
    }

    public void Close()
    {
        if (!IsShowing) return;

        cutscenePanel.SetActive(false);
        PlayerLock.Remove(this);

        Action closed = onClosed;
        onClosed = null;
        closed?.Invoke();
    }

    private void OnContinue(InputAction.CallbackContext context)
    {
        // Ignore the key press that opened this card (e.g. the one that ended a dialogue).
        if (IsShowing && Time.frameCount > shownFrame && !DialogueManager.instance.isDialogueActive) Close();
    }
}
