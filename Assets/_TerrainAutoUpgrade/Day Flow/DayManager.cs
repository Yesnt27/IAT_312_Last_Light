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
    [Tooltip("Testing, Editor only: pressing Play in the first scene skips to this day, as if the earlier days were done " +
             "(their journal pages and Required Flags are given to you). 1 = a normal new game. Builds always start on Day 1.")]
    [SerializeField] private int startOnDay = 1;
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
    [Tooltip("The 3D scene the night shift happens in (fades in from the 2D game). Its NightShift ends the night. Empty = no 3D night.")]
    [SerializeField] private string nightShiftScene = "Night Scene";
    [Tooltip("Only used when Night Shift Scene is empty: the night ends straight away and the next day begins.")]
    [SerializeField] private bool skipNightForTesting = true;

    [Header("Bedtime")]
    [SerializeField] private string goToBedTaskText = "*Head to bed*";
    [Tooltip("After the 3D night shift the player is brought back here to sleep (the bed). Empty = straight to the next morning.")]
    [SerializeField] private string afterNightScene = "Floor 4";
    [SerializeField] private string afterNightSpawnId = "Bed";
    [Tooltip("Plays when the player arrives at the bed after the night.")]
    [SerializeField] private Dialogue afterNightDialogue;

    [Header("Ending")]
    [SerializeField] private int ghostShipsToLose = 3;
    [SerializeField] private CutsceneCard endingCutscene;
    [SerializeField] private CutsceneCard victoryCutscene;
    [SerializeField] private CutsceneCard deathCutscene;

    private string stayInScene;
    private bool isSleeping;
    private System.Action cutsceneFinished;

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
        if (!newGameOnPlayInFirstScene || !inFirstScene || GameState.Instance.Phase != DayPhase.None) return;

        if (Application.isEditor && startOnDay > 1) StartOnDay(startOnDay);
        else StartNewGame();
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

    // The intro cutscene (a card over a black screen, or a cutscene scene faded into), then Day 1 at its start spawn.
    public void StartNewGame()
    {
        stayInScene = null;
        isSleeping = false;
        GameState.Instance.ResetAll();
        if (introCutscene == null || !introCutscene.IsScene) ScreenFader.Instance.SetAlpha(1f);
        PlayCutscene(introCutscene, () => BeginDay(1));
    }

    // Testing: no intro, the earlier days count as done (their pages and Required Flags), then that day starts.
    public void StartOnDay(int day)
    {
        day = Mathf.Clamp(day, 1, days.Count);
        Debug.LogWarning($"DayManager: starting on Day {day} for testing (Start On Day on the Systems prefab). Set it back to 1 for a normal new game.");

        stayInScene = null;
        isSleeping = false;
        GameState.Instance.ResetAll();
        for (int earlier = 1; earlier < day; earlier++) GiveResults(GetDay(earlier));
        ScreenFader.Instance.SetAlpha(1f);
        BeginDay(day);
    }

    private void GiveResults(DayDefinition definition)
    {
        if (definition.JournalPage > 0) GameState.Instance.CollectPage(definition.JournalPage);
        foreach (string flag in definition.RequiredFlags) GameState.Instance.SetFlag(flag);
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

        if (stayInScene == null) SceneController.Instance.LoadScene(definition.StartScene, definition.StartSpawnId, PlayWake);
        else if (SceneManager.GetActiveScene().name == stayInScene) StartCoroutine(WakeInPlace());
        else SceneController.Instance.LoadScene(stayInScene, null, PlayWake);   // e.g. back from the 3D night
    }

    public void StartNight()
    {
        if (!CanStartNight) return;

        DayDefinition definition = CurrentDay;
        GameState state = GameState.Instance;
        state.SetPhase(DayPhase.Night);
        state.SetTask(NightShiftTask, $"{nightShiftTaskText} ({definition.ShipCount} ships)");

        // 2D -> 3D: fade into the night shift scene; its NightShift calls FinishNightShift().
        if (!string.IsNullOrEmpty(nightShiftScene))
        {
            SceneController.Instance.LoadScene(nightShiftScene);
            return;
        }

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

    // Called by the NightShift in the 3D scene: night report, then back to 2D next to the bed (or straight to the morning).
    public void FinishNightShift()
    {
        if (GameState.Instance.Phase != DayPhase.Night) return;

        GameState state = GameState.Instance;
        string report = $"Ships guided: {state.ShipsGuided}\nAbnormal ships caught: {state.GhostShipsCaught}\n" +
                        $"Abnormal ships missed: {state.GhostShipsMissed}\nNot reported in time: {state.ShipsNotReported}";
        CutscenePlaceholder.Instance.ShowCard($"NIGHT {state.CurrentDay} REPORT", report, ReturnToBed);
    }

    // The player wakes back up in 2D beside the bed with the "Head to bed" task; the Bed then ends the day.
    private void ReturnToBed()
    {
        if (stayInScene != null || string.IsNullOrEmpty(afterNightScene))
        {
            StartCoroutine(SleepRoutine());
            return;
        }

        GameState state = GameState.Instance;
        state.SetPhase(DayPhase.Bedtime);
        state.SetTask(GoToBedTask, goToBedTaskText);
        SceneController.Instance.LoadScene(afterNightScene, afterNightSpawnId, () => DialogueManager.instance.StartDialogue(afterNightDialogue));
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

    // A card, or a cutscene scene if the card names one. onDone runs when it's over.
    public void PlayCutscene(CutsceneCard card, System.Action onDone)
    {
        if (card == null || card.IsEmpty)
        {
            onDone?.Invoke();
            return;
        }

        if (!card.IsScene)
        {
            CutscenePlaceholder.Instance.Show(card, onDone);
            return;
        }

        cutsceneFinished = onDone;
        SceneController.Instance.LoadScene(card.scene);
    }

    // Called by the StoryTrigger in a cutscene scene that has "Ends Cutscene" ticked.
    public void FinishCutscene()
    {
        System.Action done = cutsceneFinished;
        cutsceneFinished = null;
        if (done != null) done();
        else Debug.LogWarning("DayManager: a cutscene scene ended, but no cutscene was playing (pressed Play in that scene?).");
    }

    private IEnumerator ShowAndWait(CutsceneCard card)
    {
        bool closed = false;
        PlayCutscene(card, () => closed = true);
        while (!closed) yield return null;
    }
}
