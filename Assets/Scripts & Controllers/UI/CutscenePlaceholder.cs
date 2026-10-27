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

    public void Show(string title, string description, Action onComplete = null)
    {
        onClosed = onComplete;
        titleLabel.text = $"CUTSCENE: {title}";
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
        if (IsShowing && !DialogueManager.instance.isDialogueActive) Close();
    }
}
