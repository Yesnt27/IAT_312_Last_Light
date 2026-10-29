using System;
using System.Collections.Generic;
using UnityEngine;

// Lives on the Systems prefab, which survives scene loads (DontDestroyOnLoad).
public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    [Header("Progress")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private DayPhase phase = DayPhase.None;
    [SerializeField] private bool powerOn = true;
    [SerializeField] private List<string> flags = new List<string>();
    [SerializeField] private List<int> collectedPages = new List<int>();

    [Header("Task")]
    [SerializeField] private string activeTaskId;
    [SerializeField] private string activeTaskText;

    [Header("Night Results")]
    [SerializeField] private int shipsGuided;
    [SerializeField] private int ghostShipsCaught;
    [SerializeField] private int ghostShipsMissed;
    [SerializeField] private int falseAlarms;
    [SerializeField] private int shipsNotReported;

    public event Action Changed;

    public int CurrentDay => currentDay;
    public DayPhase Phase => phase;
    public bool PowerOn => powerOn;
    public string ActiveTaskId => activeTaskId;
    public string ActiveTaskText => activeTaskText;
    public IReadOnlyList<int> CollectedPages => collectedPages;
    public int ShipsGuided => shipsGuided;
    public int GhostShipsCaught => ghostShipsCaught;
    public int GhostShipsMissed => ghostShipsMissed;
    public int FalseAlarms => falseAlarms;
    public int ShipsNotReported => shipsNotReported;

    public bool HasBasementKey => HasFlag(GameFlags.BasementKey);
    public bool HasJournal => HasFlag(GameFlags.Journal);
    public bool HasBottleMessage => HasFlag(GameFlags.BottleMessage);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool HasFlag(string flag)
    {
        return !string.IsNullOrEmpty(flag) && flags.Contains(flag);
    }

    public void SetFlag(string flag, bool value = true)
    {
        if (string.IsNullOrEmpty(flag) || HasFlag(flag) == value) return;
        if (value) flags.Add(flag);
        else flags.Remove(flag);
        Changed?.Invoke();
    }

    public bool HasPage(int page)
    {
        return collectedPages.Contains(page);
    }

    public void CollectPage(int page)
    {
        if (HasPage(page)) return;
        collectedPages.Add(page);
        collectedPages.Sort();
        Changed?.Invoke();
    }

    public void SetDay(int day)
    {
        currentDay = day;
        Changed?.Invoke();
    }

    public void SetPhase(DayPhase newPhase)
    {
        if (phase == newPhase) return;
        phase = newPhase;
        Changed?.Invoke();
    }

    public void SetPower(bool on)
    {
        if (powerOn == on) return;
        powerOn = on;
        Changed?.Invoke();
    }

    public void SetTask(string taskId, string taskText)
    {
        activeTaskId = taskId;
        activeTaskText = taskText;
        Changed?.Invoke();
    }

    public void ClearTask()
    {
        SetTask(null, null);
    }

    public bool IsTaskActive(string taskId)
    {
        return !string.IsNullOrEmpty(taskId) && activeTaskId == taskId;
    }

    public void RecordShipGuided() { shipsGuided++; Changed?.Invoke(); }
    public void RecordGhostShipCaught() { ghostShipsCaught++; Changed?.Invoke(); }
    public void RecordGhostShipMissed() { ghostShipsMissed++; Changed?.Invoke(); }
    public void RecordFalseAlarm() { falseAlarms++; Changed?.Invoke(); }
    public void RecordShipNotReported() { shipsNotReported++; Changed?.Invoke(); }

    public void ResetAll()
    {
        currentDay = 1;
        phase = DayPhase.None;
        powerOn = true;
        flags.Clear();
        collectedPages.Clear();
        activeTaskId = null;
        activeTaskText = null;
        shipsGuided = 0;
        ghostShipsCaught = 0;
        ghostShipsMissed = 0;
        falseAlarms = 0;
        shipsNotReported = 0;
        Changed?.Invoke();
    }
}
