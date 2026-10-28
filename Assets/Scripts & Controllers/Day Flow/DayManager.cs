using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Runs the day loop: wake up → day puzzle → explore → night shift → bedtime → next day, then the ending.
// Lives on the Systems prefab. Each day's content comes from a DayDefinition asset.
public class DayManager : MonoBehaviour
{
    public const string GoToLighthouseTask = "go_to_lighthouse";
    public const string NightShiftTask = "night_shift";
    public const string GoToBedTask = "go_to_bed";

    public static DayManager Instance { get; private set; }

    [Header("Days (the number of days is the list length)")]
    [SerializeField] private List<DayDefinition> days = new List<DayDefinition>();

    [Header("Starting the Game")]
    [Tooltip("Pressing Play in Day 1's start scene (Outside) starts a new game there.")]
    [SerializeField] private bool newGameOnPlayInFirstScene = true;
    [Tooltip("Where to go after the ending. Leave empty to start a new game instead (until the title screen exists).")]
    [SerializeField] private string titleScene = "";

    [Header("Intro")]
    [SerializeField] private CutsceneCard introCutscene;

    [Header("Explore")]
    [SerializeField] private Dialogue goToLighthouseDialogue;
    [SerializeField] private string goToLighthouseTaskText = "*Head to the top of the lighthouse when you're ready*";

    [Header("Night")]
    [SerializeField] private string nightShiftTaskText = "*Guide tonight's ships*";
    [Tooltip("Reaching this scene after the day's puzzle starts the night by itself. Empty = only the lamp controls start it.")]
    [SerializeField] private string startNightWhenEntering = "Top Lighthouse";
    [Tooltip("Testing until the night shift exists: the night ends straight away and the next day begins (bed not needed).")]
    [SerializeField] private bool skipNightForTesting = true;

    [Header("Bedtime")]
    [SerializeField] private string goToBedTaskText = "*Head to bed*";

    [Header("Ending")]
    [SerializeField] private int ghostShipsToLose = 3;
    [SerializeField] private CutsceneCard endingCutscene;
    [SerializeField] private CutsceneCard victoryCutscene;
    [SerializeField] private CutsceneCard deathCutscene;

    private string stayInScene;
    private bool isSleeping;

    public int DayCount => days.Count;
    public DayDefinition CurrentDay => GetDay(GameState.Instance.CurrentDay);
    public bool CanStartNight => GameState.Instance.Phase == DayPhase.Explore;
    public bool CanSleep => GameState.Instance.Phase == DayPhase.Bedtime && !isSleeping;

    private bool IsInNightScene => !string.IsNullOrEmpty(startNightWhenEntering) && SceneManager.GetActiveScene().name == startNightWhenEntering;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameState.Instance.Changed += CheckPuzzleDone;
        SceneController.Instance.SceneReady += OnSceneReady;

        bool inFirstScene = days.Count > 0 && SceneManager.GetActiveScene().name == days[0].StartScene;
        if (newGameOnPlayInFirstScene && inFirstScene && GameState.Instance.Phase == DayPhase.None) StartNewGame();
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null) GameState.Instance.Changed -= CheckPuzzleDone;
        if (SceneController.Instance != null) SceneController.Instance.SceneReady -= OnSceneReady;
    }

    private void OnSceneReady()
    {
        if (CanStartNight && IsInNightScene) StartNight();
    }

    // Intro cutscene card over a black screen, then Day 1 loads at its start spawn (Outside / Dock).
    public void StartNewGame()
    {
        stayInScene = null;
        isSleeping = false;
        GameState.Instance.ResetAll();
        ScreenFader.Instance.SetAlpha(1f);
        CutscenePlaceholder.Instance.Show(introCutscene, () => BeginDay(1));
    }

    // Dev tool: runs the loop in the current scene instead of loading each day's start scene.
    public void StartTestFlow(int day)
    {
        stayInScene = SceneManager.GetActiveScene().name;
        GameState.Instance.ResetAll();
        ScreenFader.Instance.SetAlpha(1f);
        BeginDay(day);
    }

    public void BeginDay(int day)
    {
        DayDefinition definition = GetDay(day);
        GameState state = GameState.Instance;
        state.SetDay(day);
        state.SetPhase(DayPhase.Wake);
        state.ClearTask();
        state.SetPower(!definition.StartsWithPowerOutage);

        if (stayInScene != null) StartCoroutine(WakeInPlace());
        else SceneController.Instance.LoadScene(definition.StartScene, definition.StartSpawnId, PlayWake);
    }

    public void StartNight()
    {
        if (!CanStartNight) return;

        DayDefinition definition = CurrentDay;
        GameState state = GameState.Instance;
        state.SetPhase(DayPhase.Night);
        state.SetTask(NightShiftTask, $"{nightShiftTaskText} ({definition.ShipCount} ships)");

        if (skipNightForTesting)
        {
            CutscenePlaceholder.Instance.ShowCard(
                $"NIGHT {state.CurrentDay}",
                $"{definition.ShipCount} ships would come tonight.\n\n(Testing: the night shift isn't built yet, so the day ends here.)",
                () => StartCoroutine(SleepRoutine()));
            return;
        }

        // Placeholder until the night shift gameplay exists (Phase G).
        CutscenePlaceholder.Instance.ShowCard(
            $"NIGHT {state.CurrentDay}",
            $"{definition.ShipCount} ships are coming tonight.\n\n(Placeholder: ship guiding is added in Phase G.)",
            EndNight);
    }

    public void EndNight()
    {
        GameState state = GameState.Instance;
        string report = $"Ships guided: {state.ShipsGuided}\nGhost ships caught: {state.GhostShipsCaught}\n" +
                        $"Ghost ships missed: {state.GhostShipsMissed}\nFalse alarms: {state.FalseAlarms}";

        CutscenePlaceholder.Instance.ShowCard($"NIGHT {state.CurrentDay} REPORT", report, () =>
        {
            state.SetPhase(DayPhase.Bedtime);
            state.SetTask(GoToBedTask, goToBedTaskText);
        });
    }

    public void GoToSleep()
    {
        if (!CanSleep) return;
        StartCoroutine(SleepRoutine());
    }

    private DayDefinition GetDay(int day)
    {
        return days[Mathf.Clamp(day - 1, 0, days.Count - 1)];
    }

    private IEnumerator WakeInPlace()
    {
        yield return ScreenFader.Instance.FadeIn();
        PlayWake();
    }

    private void PlayWake()
    {
        DialogueManager.instance.StartDialogue(CurrentDay.WakeDialogue, StartPuzzle);
    }

    private void StartPuzzle()
    {
        DayDefinition definition = CurrentDay;
        GameState.Instance.SetTask(definition.PuzzleTaskId, definition.PuzzleTaskText);
        GameState.Instance.SetPhase(DayPhase.DayPuzzle);
        CheckPuzzleDone();
    }

    private void CheckPuzzleDone()
    {
        GameState state = GameState.Instance;
        if (state.Phase != DayPhase.DayPuzzle) return;

        DayDefinition definition = CurrentDay;
        if (definition.JournalPage > 0 && !state.HasPage(definition.JournalPage)) return;
        foreach (string flag in definition.RequiredFlags)
        {
            if (!string.IsNullOrEmpty(flag) && !state.HasFlag(flag)) return;
        }

        state.SetPhase(DayPhase.Explore);
        StartCoroutine(BeginExplore());
    }

    private IEnumerator BeginExplore()
    {
        // Wait a frame so the page's own dialogue has started, then let it finish first.
        yield return null;
        while (DialogueManager.instance.isDialogueActive || CutscenePlaceholder.Instance.IsShowing) yield return null;

        DialogueManager.instance.StartDialogue(goToLighthouseDialogue, () =>
        {
            GameState.Instance.SetTask(GoToLighthouseTask, goToLighthouseTaskText);
            if (IsInNightScene) StartNight(); // finished the puzzle while already at the top
        });
    }

    private IEnumerator SleepRoutine()
    {
        isSleeping = true;
        GameState state = GameState.Instance;
        state.ClearTask();
        yield return ScreenFader.Instance.FadeOut();

        CutsceneCard bedtime = CurrentDay.BedtimeCutscene;
        if (bedtime != null && !bedtime.IsEmpty) yield return ShowAndWait(bedtime);

        isSleeping = false;
        if (state.CurrentDay >= DayCount) StartCoroutine(EndingRoutine());
        else BeginDay(state.CurrentDay + 1);
    }

    private IEnumerator EndingRoutine()
    {
        GameState state = GameState.Instance;
        state.SetPhase(DayPhase.Ending);

        yield return ShowAndWait(endingCutscene);
        yield return ShowAndWait(state.GhostShipsMissed >= ghostShipsToLose ? deathCutscene : victoryCutscene);

        stayInScene = null;
        if (string.IsNullOrEmpty(titleScene)) StartNewGame();
        else
        {
            state.ResetAll();
            SceneController.Instance.LoadScene(titleScene);
        }
    }

    private IEnumerator ShowAndWait(CutsceneCard card)
    {
        bool closed = false;
        CutscenePlaceholder.Instance.Show(card, () => closed = true);
        while (!closed) yield return null;
    }
}
