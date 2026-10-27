using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogueArea;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.02f;

    private Queue<DialogueLine> lines;
    public bool isDialogueActive = false;

    Animator animator;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        lines = new Queue<DialogueLine>();
    }

    public void StartDialogue(Dialogue dialogue)
    {

        isDialogueActive = true;
        animator.Play("show");
        lines.Clear();

        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
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

        DialogueLine currentline = lines.Dequeue();
        characterNameText.text = currentline.character.name;
        StopAllCoroutines();
        StartCoroutine(Typesentence(currentline));
    }

    IEnumerator Typesentence(DialogueLine dialogueLine)
    {
        dialogueArea.text = "";
        foreach (char letter in dialogueLine.text.ToCharArray())
        {
            dialogueArea.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        animator.Play("hide");
    }
}
