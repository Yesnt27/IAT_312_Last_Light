using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The binocular overlay on the night scene's own canvas: lens mask, crosshair and the green text.
// Top left says which ship is selected; Binoculars tells it what is under the crosshair.
public class BinocularHud : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Everything shown while looking through the binoculars.")]
    [SerializeField] private GameObject view;
    [SerializeField] private Graphic crosshair;
    [SerializeField] private TextMeshProUGUI selectedLabel;
    [SerializeField] private TextMeshProUGUI aimLabel;
    [Tooltip("Shown while holding the binoculars without looking through them.")]
    [SerializeField] private GameObject heldHint;

    [Header("Text")]
    [SerializeField] private string noShipText = "NO SHIP SELECTED";
    [SerializeField] private string shipSelectedText = "SHIP SELECTED";
    [SerializeField] private string aimText = "[Right Click] Select ship";

    [Header("Crosshair")]
    [SerializeField] private Color idleColor = new Color(0.25f, 1f, 0.35f, 0.45f);
    [SerializeField] private Color onShipColor = new Color(0.25f, 1f, 0.35f, 1f);

    private void Awake()
    {
        ShowView(false);
        ShowHeldHint(false);
    }

    private void OnEnable()
    {
        NightShip.SelectionChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        NightShip.SelectionChanged -= Refresh;
    }

    public void ShowView(bool visible)
    {
        view.SetActive(visible);
        if (visible) Refresh();
    }

    public void ShowHeldHint(bool visible)
    {
        if (heldHint.activeSelf != visible) heldHint.SetActive(visible);
    }

    public void SetAimed(NightShip ship)
    {
        crosshair.color = ship != null ? onShipColor : idleColor;
        aimLabel.text = ship != null && ship != NightShip.Selected ? aimText : "";
    }

    private void Refresh()
    {
        NightShip ship = NightShip.Selected;
        if (ship == null)
        {
            selectedLabel.text = noShipText;
            return;
        }

        selectedLabel.text = $"{shipSelectedText}: {ship.ShipName}";
        if (!string.IsNullOrEmpty(ship.Details)) selectedLabel.text += $"\n<size=70%>{ship.Details}</size>";
    }
}
