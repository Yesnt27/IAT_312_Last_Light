using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// One ship in the night scene, with its own values. A ghost ship should be reported Fake, any other ship Real.
// Ghost ships carry a dark outline; the ship selected through the binoculars gets a green one.
// Active On Nights limits which nights it sails (empty = every night); NightShift hides it on the others.
public class NightShip : MonoBehaviour
{
    public static readonly List<NightShip> Sailing = new List<NightShip>();
    public static NightShip Selected { get; private set; }
    public static event Action SelectionChanged;
    public static event Action<NightShip> AnyReported;

    [Header("Ship")]
    [SerializeField] private string shipName = "Fishing Boat";
    [Tooltip("Shown under the name in the binoculars once selected.")]
    [TextArea(1, 3)]
    [SerializeField] private string details;
    [SerializeField] private bool isGhost;
    [Tooltip("Nights (day numbers) this ship comes. Empty = every night.")]
    [SerializeField] private int[] activeOnNights;

    [Header("Outline")]
    [SerializeField] private ModelOutline outline;
    [SerializeField] private Color selectedColor = new Color(0.25f, 1f, 0.35f);
    [SerializeField] private Color ghostColor = new Color(0.02f, 0.02f, 0.03f);

    [Header("After Reporting")]
    [Tooltip("Ships sail on after being reported...")]
    [SerializeField] private float sailAwaySpeed = 6f;
    [Tooltip("...except a ghost ship reported as fake, which sinks out of sight.")]
    [SerializeField] private float sinkSpeed = 1.5f;
    [SerializeField] private float sinkTime = 8f;

    private Renderer[] renderers;
    private ShipBob bob;

    public string ShipName => shipName;
    public string Details => details;
    public bool IsGhost => isGhost;
    public bool IsReported { get; private set; }
    public bool CanBeSelected => !IsReported && isActiveAndEnabled;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        bob = GetComponent<ShipBob>();
    }

    private void OnEnable()
    {
        Sailing.Add(this);
    }

    private void OnDisable()
    {
        Sailing.Remove(this);
        if (Selected == this) ClearSelection();
    }

    private void Start()
    {
        RefreshOutline();
    }

    public bool SailsOnNight(int night)
    {
        return activeOnNights == null || activeOnNights.Length == 0 || Array.IndexOf(activeOnNights, night) >= 0;
    }

    public Bounds GetBounds()
    {
        if (renderers.Length == 0) return new Bounds(transform.position, Vector3.one * 10f);

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer shipRenderer in renderers) bounds.Encapsulate(shipRenderer.bounds);
        return bounds;
    }

    public void Select()
    {
        if (!CanBeSelected || Selected == this) return;

        NightShip previous = Selected;
        Selected = this;
        if (previous != null) previous.RefreshOutline();
        RefreshOutline();
        SelectionChanged?.Invoke();
    }

    // Q at the panel = reported as a ghost (fake), E = reported as real.
    public void Report(bool reportedAsGhost)
    {
        if (IsReported) return;

        IsReported = true;
        if (Selected == this) ClearSelection();
        RefreshOutline();
        RecordResult(reportedAsGhost);

        if (isGhost && reportedAsGhost) StartCoroutine(Sink());
        else if (bob != null) bob.Speed = sailAwaySpeed;
        AnyReported?.Invoke(this);
    }

    private static void ClearSelection()
    {
        NightShip previous = Selected;
        Selected = null;
        if (previous != null) previous.RefreshOutline();
        SelectionChanged?.Invoke();
    }

    private void RecordResult(bool reportedAsGhost)
    {
        GameState state = GameState.Instance;
        if (isGhost)
        {
            if (reportedAsGhost) state.RecordGhostShipCaught();
            else state.RecordGhostShipMissed();
        }
        else if (reportedAsGhost) state.RecordFalseAlarm();
        else state.RecordShipGuided();
    }

    private IEnumerator Sink()
    {
        if (bob != null) bob.SinkSpeed = sinkSpeed;
        yield return new WaitForSeconds(sinkTime);
        gameObject.SetActive(false);
    }

    private void RefreshOutline()
    {
        if (outline == null) return;

        if (Selected == this) outline.Show(selectedColor);
        else if (isGhost && !IsReported) outline.Show(ghostColor);
        else outline.Hide();
    }
}
