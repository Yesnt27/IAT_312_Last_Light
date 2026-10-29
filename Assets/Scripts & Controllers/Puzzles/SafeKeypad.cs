using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// The safe's keypad: a full-screen prefab (Assets/Prefabs/UI/Safe Keypad) with its own canvas. Safe spawns it
// and calls Open(code, onCorrect). Click the buttons or type the digits; the code is checked as soon as
// enough digits are in. Backspace deletes, Esc or the X button walks away. It destroys itself when it closes.
public class SafeKeypad : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI display;
    [SerializeField] private TextMeshProUGUI statusLabel;
    [SerializeField] private Button closeButton;

    [Header("Text")]
    [SerializeField] private string instructions = "Enter the code";
    [SerializeField] private string wrongText = "Wrong code";
    [SerializeField] private string correctText = "*Click*";
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color wrongColor = new Color(1f, 0.35f, 0.3f);
    [SerializeField] private Color correctColor = new Color(0.35f, 1f, 0.45f);

    [Header("Settings")]
    [SerializeField] private float closeDelayAfterCorrect = 0.8f;

    private string code = "";
    private string entered = "";
    private Action onCorrect;
    private bool isSolved;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        if (Keyboard.current != null) Keyboard.current.onTextInput += OnTextInput;
    }

    private void OnDisable()
    {
        if (Keyboard.current != null) Keyboard.current.onTextInput -= OnTextInput;
    }

    private void OnDestroy()
    {
        PlayerLock.Remove(this);
    }

    public void Open(string safeCode, Action onCorrectCallback)
    {
        code = safeCode;
        onCorrect = onCorrectCallback;
        entered = "";
        isSolved = false;
        ShowStatus(instructions, normalColor);
        Refresh();
        PlayerLock.Add(this);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || isSolved) return;

        if (keyboard.backspaceKey.wasPressedThisFrame) DeleteDigit();
        if (keyboard.escapeKey.wasPressedThisFrame) Close();
    }

    // The digit buttons call this with their digit (set in each button's On Click).
    public void PressDigit(string digit)
    {
        if (isSolved) return;
        if (entered.Length >= code.Length) entered = "";

        entered += digit;
        ShowStatus(instructions, normalColor);
        Refresh();
        if (entered.Length == code.Length) Check();
    }

    public void DeleteDigit()
    {
        if (isSolved || entered.Length == 0) return;
        entered = entered.Substring(0, entered.Length - 1);
        Refresh();
    }

    public void ClearDigits()
    {
        if (isSolved) return;
        entered = "";
        Refresh();
    }

    public void Close()
    {
        PlayerLock.Remove(this);
        Destroy(gameObject);
    }

    private void OnTextInput(char character)
    {
        if (char.IsDigit(character)) PressDigit(character.ToString());
    }

    private void Check()
    {
        if (entered == code)
        {
            StartCoroutine(Solve());
            return;
        }

        entered = "";
        ShowStatus(wrongText, wrongColor);
        Refresh();
    }

    private IEnumerator Solve()
    {
        isSolved = true;
        ShowStatus(correctText, correctColor);
        closeButton.interactable = false;
        yield return new WaitForSecondsRealtime(closeDelayAfterCorrect);

        Action correct = onCorrect;
        Close();
        correct?.Invoke();
    }

    // "2 7 _ _"
    private void Refresh()
    {
        string shown = "";
        for (int i = 0; i < code.Length; i++) shown += (i < entered.Length ? entered[i].ToString() : "_") + (i < code.Length - 1 ? " " : "");
        display.text = shown;
    }

    private void ShowStatus(string text, Color color)
    {
        statusLabel.text = text;
        statusLabel.color = color;
    }
}
