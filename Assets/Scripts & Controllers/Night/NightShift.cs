using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Runs the night shift in the 3D Night Scene. When the day loop sends you here it shows only the ships
// that sail tonight (each NightShip's Active On Nights), plays an optional opening line, then starts the
// clock: First Night Seconds on night 1, plus Extra Seconds Per Night for each night after that.
// The night ends when every ship is reported Real or Fake, or when time runs out; ships nobody reported
// in time don't count (they're only listed as "not reported" on the night report).
// Pressing Play directly in this scene shows every ship so you can try the binoculars; no clock, no end.
public class NightShift : MonoBehaviour
{
    [Header("Ships")]
    [Tooltip("Parent of all the NightShips.")]
    [SerializeField] private Transform ships;

    [Header("Shift")]
    [SerializeField] private Dialogue startDialogue;
    [Tooltip("{0} = ships reported so far, {1} = ships tonight.")]
    [SerializeField] private string taskText = "*Spot the ships with the binoculars and report each one at the Real / Fake buttons ({0}/{1})*";
    [Tooltip("Seconds after the last report (or after time runs out) before the night ends.")]
    [SerializeField] private float endDelay = 2f;

    [Header("Time Limit")]
    [SerializeField] private float firstNightSeconds = 30f;
    [SerializeField] private float extraSecondsPerNight = 15f;
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

    public float TimeLimit(int night)
    {
        return firstNightSeconds + extraSecondsPerNight * Mathf.Max(0, night - 1);
    }

    private void Start()
    {
        timerLabel.gameObject.SetActive(false);
        isRunning = GameState.Instance.Phase == DayPhase.Night;
        int night = GameState.Instance.CurrentDay;
        foreach (NightShip ship in ships.GetComponentsInChildren<NightShip>(true))
        {
            bool sails = !isRunning || ship.SailsOnNight(night);
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

        ShowTask();
        if (tonight.Count == 0)
        {
            DayManager.Instance.FinishNightShift();
            yield break;
        }

        float timeLeft = TimeLimit(GameState.Instance.CurrentDay);
        timerLabel.gameObject.SetActive(true);
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
