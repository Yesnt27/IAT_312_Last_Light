using UnityEngine;

// One kind of item the player can carry (create with Assets > Create > Lighthouse > Inventory Item, then add it to
// Inventory's All Items on the Systems prefab). The player has it while Held While Flag is set, so a pickup only
// has to set that flag (e.g. a StoryOutcome or FlagPickup), and new games and Start On Day work without saving.
// F uses it: a readable item opens its text on a Paper Note, any other item plays its Use Dialogue.
[CreateAssetMenu(menuName = "Lighthouse/Inventory Item", fileName = "New Item")]
public class InventoryItem : ScriptableObject
{
    [Header("Display")]
    [SerializeField] private string itemName = "Item";
    [SerializeField] private Sprite icon;
    [SerializeField] private Color iconColor = Color.white;

    [Header("Held While")]
    [Tooltip("The player has the item while this flag is set.")]
    [SerializeField] private string heldWhileFlag;
    [Tooltip("Optional: the item is gone again once this flag is set (e.g. used up).")]
    [SerializeField] private string goneWhenFlag;

    [Header("Use (F)")]
    [Tooltip("Makes the item readable: F opens this text on a Paper Note.")]
    [TextArea(3, 8)]
    [SerializeField] private string noteText;
    [SerializeField] private Sprite noteImage;
    [Tooltip("Played on F when the item isn't readable.")]
    [SerializeField] private Dialogue useDialogue;

    public string ItemName => itemName;
    public Sprite Icon => icon;
    public Color IconColor => iconColor;
    public string NoteText => noteText;
    public Sprite NoteImage => noteImage;
    public Dialogue UseDialogue => useDialogue;
    public bool IsReadable => !string.IsNullOrEmpty(noteText) || noteImage != null;

    public bool IsHeld(GameState state)
    {
        return state.HasFlag(heldWhileFlag) && !state.HasFlag(goneWhenFlag);
    }
}
