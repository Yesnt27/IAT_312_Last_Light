using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// The Report / Safe buttons on the control panel. While a ship is selected, a label floats over each button.
// Q reports the selected ship as a ghost, E marks it safe; the button clicks down and NightShip.Report runs.
// Standing, you have to look at the buttons. Sitting in the chair you can report from there, even while
// looking through the binoculars (the binocular HUD shows the keys).
public class ShipReportPanel : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference reportFakeAction;
    [SerializeField] private InputActionReference reportRealAction;

    [Header("References")]
    [SerializeField] private FirstPersonController player;
    [SerializeField] private Binoculars binoculars;
    [SerializeField] private BinocularHud hud;
    [Tooltip("Looking at this collider (around the buttons) lets you press them.")]
    [SerializeField] private Collider buttonArea;
    [SerializeField] private PressableButton fakeButton;
    [SerializeField] private PressableButton realButton;
    [SerializeField] private TextMeshPro fakeLabel;
    [SerializeField] private TextMeshPro realLabel;

    [Header("Text")]
    [SerializeField] private float reach = 3f;
    [SerializeField] private string fakeText = "Report";
    [SerializeField] private string realText = "Safe";
    [SerializeField] private string noShipText = "Select a ship with the binoculars first";

    private bool showingPrompt;

    private bool IsAimingAtButtons => !binoculars.IsLooking && player.IsLookingAt(buttonArea, reach);
    private bool CanReportFromSeat => player.IsSeated && NightShip.Selected != null;
    private bool CanReport => !PlayerLock.IsLocked && (CanReportFromSeat || (!player.IsSeated && IsAimingAtButtons));
    private string FakeChoice => $"{InteractionPrompt.KeyHint(reportFakeAction)} {fakeText}";
    private string RealChoice => $"{InteractionPrompt.KeyHint(reportRealAction)} {realText}";

    private void Start()
    {
        fakeLabel.text = FakeChoice;
        realLabel.text = RealChoice;
    }

    private void OnEnable()
    {
        reportFakeAction.action.Enable();
        reportRealAction.action.Enable();
        reportFakeAction.action.performed += OnReportFake;
        reportRealAction.action.performed += OnReportReal;
    }

    private void OnDisable()
    {
        reportFakeAction.action.performed -= OnReportFake;
        reportRealAction.action.performed -= OnReportReal;
    }

    private void Update()
    {
        NightShip ship = NightShip.Selected;
        ShowLabel(fakeLabel, ship != null);
        ShowLabel(realLabel, ship != null);
        UpdatePrompt(ship);
    }

    private void ShowLabel(TextMeshPro label, bool visible)
    {
        if (label.gameObject.activeSelf != visible) label.gameObject.SetActive(visible);
        if (visible) label.transform.rotation = Quaternion.LookRotation(label.transform.position - player.Eyes.position);
    }

    private void UpdatePrompt(NightShip ship)
    {
        string choices = ship != null ? $"{ship.ShipName}:   {FakeChoice}   {RealChoice}" : noShipText;
        bool fromSeat = !PlayerLock.IsLocked && CanReportFromSeat;
        hud.SetReportHint(fromSeat ? $"{FakeChoice}   {RealChoice}" : "");

        bool show = !PlayerLock.IsLocked && ((fromSeat && !binoculars.IsLooking) || (!player.IsSeated && IsAimingAtButtons));
        if (show) InteractionPrompt.Instance.Show(choices, "");
        else if (showingPrompt) InteractionPrompt.Instance.Hide();
        showingPrompt = show;
    }

    private void OnReportFake(InputAction.CallbackContext context)
    {
        Report(fakeButton, true);
    }

    private void OnReportReal(InputAction.CallbackContext context)
    {
        Report(realButton, false);
    }

    private void Report(PressableButton button, bool asGhost)
    {
        if (!CanReport || player.SatDownThisFrame) return;

        button.Press();
        if (NightShip.Selected != null) NightShip.Selected.Report(asGhost);
    }
}
