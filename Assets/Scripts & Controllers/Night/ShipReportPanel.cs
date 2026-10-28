using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// The Real / Fake buttons on the control panel. While a ship is selected, a label floats over each
// button; look at the buttons and press Q (fake) or E (real). The button clicks down and the
// selected ship is reported (NightShip.Report).
public class ShipReportPanel : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference reportFakeAction;
    [SerializeField] private InputActionReference reportRealAction;

    [Header("References")]
    [SerializeField] private FirstPersonController player;
    [SerializeField] private Binoculars binoculars;
    [Tooltip("Looking at this collider (around the buttons) lets you press them.")]
    [SerializeField] private Collider buttonArea;
    [SerializeField] private PressableButton fakeButton;
    [SerializeField] private PressableButton realButton;
    [SerializeField] private TextMeshPro fakeLabel;
    [SerializeField] private TextMeshPro realLabel;

    [Header("Text")]
    [SerializeField] private float reach = 3f;
    [SerializeField] private string fakeText = "FAKE";
    [SerializeField] private string realText = "REAL";
    [SerializeField] private string noShipText = "Select a ship with the binoculars first";

    private bool showingPrompt;

    private bool IsAiming => !PlayerLock.IsLocked && !binoculars.IsLooking && player.IsLookingAt(buttonArea, reach);
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
        bool show = IsAiming;
        if (show)
        {
            string text = ship != null ? $"Report {ship.ShipName}:   {FakeChoice}   {RealChoice}" : noShipText;
            InteractionPrompt.Instance.Show(text, "");
        }
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
        if (!IsAiming) return;

        button.Press();
        if (NightShip.Selected != null) NightShip.Selected.Report(asGhost);
    }
}
