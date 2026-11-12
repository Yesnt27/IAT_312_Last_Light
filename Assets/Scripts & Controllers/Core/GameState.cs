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
    [Tooltip("The current (or last) night only; cleared when a night starts.")]
    [SerializeField] private ShipStats tonight = new ShipStats();
    [Tooltip("Every night this game; decides the ending.")]
    [SerializeField] private ShipStats total = new ShipStats();

    public event Action Changed;

    public int CurrentDay => currentDay;
    public DayPhase Phase => phase;
    public bool PowerOn => powerOn;
    public string ActiveTaskId => activeTaskId;
    public string ActiveTaskText => activeTaskText;
    public IReadOnlyList<int> CollectedPages => collectedPages;
    public ShipStats Tonight => tonight;
    public ShipStats Total => total;

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

    public void StartNightResults()
    {
        tonight.Clear();
        Changed?.Invoke();
    }

    public void RecordShipGuided() { tonight.guided++; total.guided++; Changed?.Invoke(); }
    public void RecordGhostShipCaught() { tonight.caught++; total.caught++; Changed?.Invoke(); }
    public void RecordGhostShipMissed() { tonight.missed++; total.missed++; Changed?.Invoke(); }
    public void RecordFalseAlarm() { tonight.falseAlarms++; total.falseAlarms++; Changed?.Invoke(); }
    public void RecordShipNotReported() { tonight.notReported++; total.notReported++; Changed?.Invoke(); }

    public void ResetAll()
    {
        currentDay = 1;
        phase = DayPhase.None;
        powerOn = true;
        flags.Clear();
        collectedPages.Clear();
        activeTaskId = null;
        activeTaskText = null;
        tonight.Clear();
        total.Clear();
        Changed?.Invoke();
    }
}
