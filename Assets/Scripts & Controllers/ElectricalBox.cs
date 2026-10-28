using UnityEngine;

// The electrical box. Opening it collects the journal page taped inside the door (only the first time),
// then opens the wire puzzle. Solving it turns the power back on, sets the solved flag and plays a dialogue.
// Use the Interactable conditions to limit it to its day (e.g. Active During Tasks = day2_power).
public class ElectricalBox : Interactable
{
    [Header("Journal Page Inside (0 = none)")]
    [SerializeField] private int pageNumber = 2;
    [SerializeField] private Dialogue pageFoundDialogue;

    [Header("Wire Puzzle")]
    [SerializeField] private WirePuzzle wirePuzzlePrefab;

    [Header("When Solved")]
    [SerializeField] private string solvedFlag = GameFlags.PowerFixed;
    [SerializeField] private bool restorePower = true;
    [SerializeField] private Dialogue solvedDialogue;

    private WirePuzzle openPuzzle;

    protected override void OnInteract()
    {
        if (openPuzzle != null) return;

        if (pageNumber > 0 && !GameState.Instance.HasPage(pageNumber))
        {
            GameState.Instance.CollectPage(pageNumber);
            DialogueManager.instance.StartDialogue(pageFoundDialogue, OpenPuzzle);
        }
        else
        {
            OpenPuzzle();
        }
    }

    private void OpenPuzzle()
    {
        openPuzzle = Instantiate(wirePuzzlePrefab);
        openPuzzle.Open(OnSolved);
    }

    private void OnSolved()
    {
        GameState state = GameState.Instance;
        if (restorePower) state.SetPower(true);
        state.SetFlag(solvedFlag);
        DialogueManager.instance.StartDialogue(solvedDialogue);
    }
}
