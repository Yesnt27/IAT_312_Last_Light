using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogueArea;
    [SerializeField] private GameObject dialogueBox;
    [Tooltip("\"Click to continue\"; hidden for timed lines (cutscene videos), which go away by themselves.")]
    [SerializeField] private GameObject continueHint;

    [Header("Input")]
    [SerializeField] private InputActionReference advanceAction;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.02f;

    private Queue<DialogueLine> lines;
    private DialogueLine currentLine;
    private bool isTyping;
    private Action onFinished;
    private int startedFrame;
    public bool isDialogueActive = false;



    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        lines = new Queue<DialogueLine>();

        dialogueBox.SetActive(false);
    }

    private void OnEnable()
    {
        advanceAction.action.Enable();
        advanceAction.action.performed += OnAdvance;
    }

    private void OnDisable()
    {
        advanceAction.action.performed -= OnAdvance;
    }

    public void StartDialogue(Dialogue dialogue, Action onComplete = null)
    {
        if (dialogue == null || dialogue.dialogueLines.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        isDialogueActive = true;
        onFinished = onComplete;
        startedFrame = Time.frameCount;
        PlayerLock.Add(this);
        dialogueBox.SetActive(true);
        if (continueHint != null) continueHint.SetActive(true);
        lines.Clear();

        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }

        DisplayNextLine();
    }

    private void OnAdvance(InputAction.CallbackContext context)
    {
        // Ignore the key press that started this dialogue (e.g. the one that closed a card).
        if (!isDialogueActive || Time.frameCount == startedFrame) return;

        if (isTyping)
        {
            StopAllCoroutines();
            isTyping = false;
            dialogueArea.text = currentLine.text;
            return;
        }

        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        ShowLine(lines.Dequeue());
    }

    // A line that shows without waiting for clicks (e.g. under a cutscene video); HideTimedLine takes it away.
    // Ignored while a normal dialogue is open.
    public void ShowTimedLine(DialogueLine line)
    {
        if (isDialogueActive) return;

        dialogueBox.SetActive(true);
        if (continueHint != null) continueHint.SetActive(false);
        ShowLine(line);
    }

    public void HideTimedLine()
    {
        if (isDialogueActive) return;

        StopAllCoroutines();
        isTyping = false;
        dialogueBox.SetActive(false);
    }

    private void ShowLine(DialogueLine line)
    {
        currentLine = line;
        bool hasSpeaker = currentLine.character != null && !string.IsNullOrEmpty(currentLine.character.name);
        characterNameText.gameObject.SetActive(hasSpeaker);
        characterNameText.text = hasSpeaker ? currentLine.character.name : "";
        StopAllCoroutines();
        StartCoroutine(Typesentence(currentLine));
    }

    IEnumerator Typesentence(DialogueLine dialogueLine)
    {
        isTyping = true;
        dialogueArea.text = "";
        foreach (char letter in dialogueLine.text.ToCharArray())
        {
            dialogueArea.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        dialogueBox.SetActive(false);
        PlayerLock.Remove(this);

        Action finished = onFinished;
        onFinished = null;
        finished?.Invoke();
    }
}
