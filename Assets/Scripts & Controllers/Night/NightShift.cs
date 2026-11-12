using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Runs the night shift in the 3D Night Scene. When the day loop sends you here it shows only the ships
// that sail tonight (each NightShip's Active On Nights) and plays an optional opening line. The clock starts
// the first time the player picks up the binoculars, with that night's entry in Night Seconds (nights past the
// end of the list use the last entry). The night ends when every ship is reported or marked safe, or when time
// runs out; ships nobody reported in time don't count (they're only listed as "not reported" on the night report).
// Pressing Play directly in this scene shows Preview Night's ships so you can try them; no clock, no end.
public class NightShift : MonoBehaviour
{
    [Header("Ships")]
    [Tooltip("Parent of all the NightShips.")]
    [SerializeField] private Transform ships;
    [Tooltip("Pressing Play directly in this scene shows this night's ships (0 = every ship).")]
    [SerializeField] private int previewNight = 1;

    [Header("Shift")]
    [SerializeField] private Binoculars binoculars;
    [SerializeField] private Dialogue startDialogue;
    [Tooltip("Shown until the binoculars are picked up (the clock waits until then).")]
    [SerializeField] private string waitingTaskText = "*Pick up the binoculars on the desk to start your watch*";
    [Tooltip("{0} = ships reported so far, {1} = ships tonight.")]
    [SerializeField] private string taskText = "*Spot each ship with the binoculars, then Report it or mark it Safe ({0}/{1})*";
    [Tooltip("Seconds after the last report (or after time runs out) before the night ends.")]
    [SerializeField] private float endDelay = 2f;

    [Header("Time Limit")]
    [Tooltip("Seconds on the clock each night: element 0 = night 1. Later nights use the last entry.")]
    [SerializeField] private float[] nightSeconds = { 60f, 75f, 90f, 180f };
    [SerializeField] private TextMeshProUGUI timerLabel;
    [Tooltip("{0} = the time left, e.g. 0:27.")]
    [SerializeField] private string timerText = "TIME LEFT  {0}";
    [SerializeField] private string timeUpText = "TIME'S UP";
    [SerializeField] private float warningSeconds = 10f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = new Color(1f, 0.35f, 0.3f);

    private readonly List<NightShip> tonight = new List<NightShip>();
    private bool isRunning;

    private int ReportedCount => tonight.FindAll(ship => ship.IsReported).Count;

    // Tonight's number: the day when the day loop sent you here, otherwise Preview Night (NightWeather uses it too).
    public int Night => GameState.Instance.Phase == DayPhase.Night ? GameState.Instance.CurrentDay : previewNight;

    public float TimeLimit(int night)
    {
        if (nightSeconds == null || nightSeconds.Length == 0) return 60f;
        return nightSeconds[Mathf.Clamp(night - 1, 0, nightSeconds.Length - 1)];
    }

    private void Start()
    {
        timerLabel.gameObject.SetActive(false);
        isRunning = GameState.Instance.Phase == DayPhase.Night;
        int night = Night;
        foreach (NightShip ship in ships.GetComponentsInChildren<NightShip>(true))
        {
            bool sails = night <= 0 || ship.SailsOnNight(night);
            ship.gameObject.SetActive(sails);
            if (sails) tonight.Add(ship);
        }

        NightShip.AnyReported += OnShipReported;
        if (isRunning) StartCoroutine(RunShift());
    }

    private void OnDestroy()
    {
        NightShip.AnyReported -= OnShipReported;
        PlayerLock.Remove(this);
    }

    private IEnumerator RunShift()
    {
        while (SceneController.Instance.IsLoading) yield return null;

        bool talked = false;
        DialogueManager.instance.StartDialogue(startDialogue, () => talked = true);
        while (!talked) yield return null;

        if (tonight.Count == 0)
        {
            DayManager.Instance.FinishNightShift();
            yield break;
        }

        float timeLeft = TimeLimit(GameState.Instance.CurrentDay);
        timerLabel.gameObject.SetActive(true);
        ShowTime(timeLeft);
        GameState.Instance.SetTask(DayManager.NightShiftTask, waitingTaskText);
        while (!binoculars.IsHeld) yield return null;

        ShowTask();
        while (isRunning && timeLeft > 0f)
        {
            if (!PlayerLock.IsLocked) timeLeft -= Time.deltaTime;
            ShowTime(timeLeft);
            yield return null;
        }

        if (isRunning) StartCoroutine(TimeUp());
    }

    private void OnShipReported(NightShip ship)
    {
        if (!isRunning || !tonight.Contains(ship)) return;

        ShowTask();
        if (ReportedCount == tonight.Count) StartCoroutine(EndShift());
    }

    private IEnumerator EndShift()
    {
        isRunning = false;
        yield return new WaitForSeconds(endDelay);
        DayManager.Instance.FinishNightShift();
    }

    // The ships still out there don't count. The player is frozen for a moment so nothing more gets reported.
    private IEnumerator TimeUp()
    {
        isRunning = false;
        foreach (NightShip ship in tonight)
        {
            if (!ship.IsReported) GameState.Instance.RecordShipNotReported();
        }

        timerLabel.text = timeUpText;
        timerLabel.color = warningColor;
        PlayerLock.Add(this);
        yield return new WaitForSeconds(endDelay);
        PlayerLock.Remove(this);
        DayManager.Instance.FinishNightShift();
    }

    private void ShowTime(float timeLeft)
    {
        int seconds = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
        timerLabel.text = string.Format(timerText, $"{seconds / 60}:{seconds % 60:00}");
        timerLabel.color = timeLeft <= warningSeconds ? warningColor : normalColor;
    }

    private void ShowTask()
    {
        GameState.Instance.SetTask(DayManager.NightShiftTask, string.Format(taskText, ReportedCount, tonight.Count));
    }
}
