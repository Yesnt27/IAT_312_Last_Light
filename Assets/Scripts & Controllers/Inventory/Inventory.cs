using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// On the Systems prefab. Which items the player carries comes from GameState flags (see InventoryItem),
// in the order of All Items. The number keys pick one (1 = first slot), F uses it. It only works in scenes with the 2D player.
public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [Header("Input")]
    [SerializeField] private InputActionReference useAction;
    [Tooltip("One binding per slot, in slot order (1, 2, 3, 4).")]
    [SerializeField] private InputActionReference selectSlotAction;

    [Header("Items")]
    [SerializeField] private InventoryItem[] allItems;

    [Header("Using Items")]
    [SerializeField] private PaperNote paperNotePrefab;

    private readonly List<InventoryItem> held = new List<InventoryItem>();
    private readonly List<IItemUseHandler> useHandlers = new List<IItemUseHandler>();
    private int selectedIndex;
    private PaperNote openNote;

    public event Action Changed;

    public IReadOnlyList<InventoryItem> Held => held;
    public int SelectedIndex => selectedIndex;
    public InventoryItem Selected => held.Count > 0 ? held[selectedIndex] : null;
    public InputActionReference UseAction => useAction;
    public bool IsAvailableHere => GameObject.FindWithTag("Player") != null;
    // "[1-2]" when there's more than one item to pick from, otherwise "".
    public string SwitchKeys => held.Count < 2 ? "" : $"[1-{Mathf.Min(held.Count, selectSlotAction.action.bindings.Count)}]";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameState.Instance.Changed += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null) GameState.Instance.Changed -= Refresh;
    }

    private void OnEnable()
    {
        useAction.action.Enable();
        selectSlotAction.action.Enable();
        useAction.action.performed += OnUse;
        selectSlotAction.action.performed += OnSelectSlot;
    }

    private void OnDisable()
    {
        useAction.action.performed -= OnUse;
        selectSlotAction.action.performed -= OnSelectSlot;
    }

    // The binding that was pressed is the slot: 1 = the first item carried.
    private void OnSelectSlot(InputAction.CallbackContext context)
    {
        int slot = context.action.GetBindingIndexForControl(context.control);
        if (slot < 0 || slot >= held.Count || !CanUse()) return;
        Select(slot);
    }

    public void AddUseHandler(IItemUseHandler handler)
    {
        if (!useHandlers.Contains(handler)) useHandlers.Add(handler);
    }

    public void RemoveUseHandler(IItemUseHandler handler)
    {
        useHandlers.Remove(handler);
    }

    public void Select(int index)
    {
        if (held.Count == 0) return;
        selectedIndex = (index % held.Count + held.Count) % held.Count;
        Changed?.Invoke();
    }

    private void Refresh()
    {
        InventoryItem selected = Selected;
        held.Clear();
        foreach (InventoryItem item in allItems)
        {
            if (item != null && item.IsHeld(GameState.Instance)) held.Add(item);
        }

        int kept = held.IndexOf(selected);
        selectedIndex = kept >= 0 ? kept : Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, held.Count - 1));
        Changed?.Invoke();
    }

    // A puzzle that registered a handler (and is open) may use items while the player is frozen.
    private bool CanUse()
    {
        return IsAvailableHere && (useHandlers.Count > 0 || !PlayerLock.IsLocked);
    }

    private void OnUse(InputAction.CallbackContext context)
    {
        InventoryItem item = Selected;
        if (item == null || !CanUse()) return;

        for (int i = useHandlers.Count - 1; i >= 0; i--)
        {
            if (useHandlers[i].TryUse(item)) return;
        }

        if (item.IsReadable)
        {
            if (openNote != null || paperNotePrefab == null) return;
            openNote = Instantiate(paperNotePrefab);
            openNote.Open(item.NoteText, item.NoteImage, null);
        }
        else
        {
            DialogueManager.instance.StartDialogue(item.UseDialogue);
        }
    }
}
