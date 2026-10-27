using UnityEngine;

// Everything that changes from one day to the next. One asset per day, listed in order on the DayManager.
[CreateAssetMenu(fileName = "Day", menuName = "Lighthouse/Day Definition")]
public class DayDefinition : ScriptableObject
{
    [Header("Wake Up")]
    [SerializeField] private string startScene = "Floor 2";
    [SerializeField] private string startSpawnId = "Bed";
    [SerializeField] private bool startsWithPowerOutage;
    [SerializeField] private Dialogue wakeDialogue;

    [Header("Day Puzzle (ends when the journal page is collected)")]
    [SerializeField] private string puzzleTaskId;
    [SerializeField, TextArea(1, 3)] private string puzzleTaskText;
    [SerializeField] private int journalPage;

    [Header("Night")]
    [SerializeField] private int shipCount = 3;

    [Header("Bedtime")]
    [SerializeField] private CutsceneCard bedtimeCutscene;

    public string StartScene => startScene;
    public string StartSpawnId => startSpawnId;
    public bool StartsWithPowerOutage => startsWithPowerOutage;
    public Dialogue WakeDialogue => wakeDialogue;
    public string PuzzleTaskId => puzzleTaskId;
    public string PuzzleTaskText => puzzleTaskText;
    public int JournalPage => journalPage;
    public int ShipCount => shipCount;
    public CutsceneCard BedtimeCutscene => bedtimeCutscene;
}
