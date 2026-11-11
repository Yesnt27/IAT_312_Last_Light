using UnityEngine;

// A cupboard or drawer the player clicks to open and close (add ClickToInteract; its Sprites = both arts).
// Open Flag (optional) is set while it's open, so whatever is inside can list it in its Required Flags
// and only show up (and be clickable) while it's open, e.g. the basement key in the Floor 4 table drawer.
// The flag also remembers the drawer as open when the player comes back to the room.
public class Cabinets : Interactable
{
    [Header("Art")]
    [SerializeField] private SpriteRenderer closedCabinet;
    [SerializeField] private SpriteRenderer openCabinet;

    [Header("Contents")]
    [Tooltip("Set while open, cleared when closed. Empty = it just opens and closes.")]
    [SerializeField] private string openFlag;

    [Header("Text")]
    [SerializeField] private string openPrompt = "Open";
    [SerializeField] private string closePrompt = "Close";

    private bool isOpen;

    public override string PromptText => isOpen ? closePrompt : openPrompt;

    private void Start()
    {
        ShowOpen(GameState.Instance.HasFlag(openFlag));
    }

    protected override void OnInteract()
    {
        ShowOpen(!isOpen);
        GameState.Instance.SetFlag(openFlag, isOpen);
    }

    private void ShowOpen(bool open)
    {
        isOpen = open;
        closedCabinet.enabled = !open;
        openCabinet.enabled = open;
    }
}
