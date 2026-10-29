using UnityEngine;

// The safe on the fourth floor. Interact (E) to open its keypad (Assets/Prefabs/UI/Safe Keypad) and type the code.
// The right code opens it: the journal page inside is collected, Opened Dialogue plays and After Opened is applied.
// Use the Lock (Unlock Flag = BottleDeciphered) so it only asks for a code once the player has found it,
// and Hidden By Flags = SafeOpened so it can't be used again.
public class Safe : Interactable
{
    [Header("Safe")]
    [SerializeField] private string code = "2741";
    [SerializeField] private SafeKeypad keypadPrefab;

    [Header("Inside")]
    [SerializeField] private int pageNumber = 3;
    [SerializeField] private Dialogue openedDialogue;
    [SerializeField] private StoryOutcome afterOpened = new StoryOutcome();

    [Header("Art")]
    [SerializeField] private GameObject closedArt;
    [SerializeField] private GameObject openArt;

    private SafeKeypad openKeypad;

    private void Start()
    {
        ShowOpen(GameState.Instance.HasPage(pageNumber));
    }

    protected override void OnInteract()
    {
        if (openKeypad != null) return;

        openKeypad = Instantiate(keypadPrefab);
        openKeypad.Open(code, OnCorrectCode);
    }

    private void OnCorrectCode()
    {
        ShowOpen(true);
        GameState.Instance.CollectPage(pageNumber);
        DialogueManager.instance.StartDialogue(openedDialogue, afterOpened.Apply);
    }

    private void ShowOpen(bool open)
    {
        if (closedArt != null) closedArt.SetActive(!open);
        if (openArt != null) openArt.SetActive(open);
    }
}
