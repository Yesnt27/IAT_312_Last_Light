using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Runs the night shift in the 3D Night Scene. When the day loop sends you here it shows only the ships
// that sail tonight (each NightShip's Active On Nights), plays an optional opening line and keeps the
// task box counting. Once every ship tonight is reported Real or Fake, it hands back to DayManager
// (night report card, then the next morning in 2D).
// Pressing Play directly in this scene shows every ship so you can try the binoculars; the night never ends.
public class NightShift : MonoBehaviour
{
    [Header("Ships")]
    [Tooltip("Parent of all the NightShips.")]
    [SerializeField] private Transform ships;

    [Header("Shift")]
    [SerializeField] private Dialogue startDialogue;
    [Tooltip("{0} = ships reported so far, {1} = ships tonight.")]
    [SerializeField] private string taskText = "*Spot the ships with the binoculars and report each one at the Real / Fake buttons ({0}/{1})*";
    [Tooltip("Seconds after the last report before the night ends.")]
    [SerializeField] private float endDelay = 2f;

    private readonly List<NightShip> tonight = new List<NightShip>();
    private bool isRunning;

    private int ReportedCount => tonight.FindAll(ship => ship.IsReported).Count;

    private void Start()
    {
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
    }

    private IEnumerator RunShift()
    {
        while (SceneController.Instance.IsLoading) yield return null;

        bool talked = false;
        DialogueManager.instance.StartDialogue(startDialogue, () => talked = true);
        while (!talked) yield return null;

        ShowTask();
        if (tonight.Count == 0) DayManager.Instance.FinishNightShift();
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

    private void ShowTask()
    {
        GameState.Instance.SetTask(DayManager.NightShiftTask, string.Format(taskText, ReportedCount, tonight.Count));
    }
}
