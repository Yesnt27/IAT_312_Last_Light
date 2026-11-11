using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The inventory slots on the right of the screen (on the Systems UI canvas). Shows at least Min Slots,
// one slot per item carried (up to the number of slots built), and highlights the selected one.
// Hidden in scenes without the 2D player (title, cutscenes, the 3D night).
public class InventoryHud : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image[] slotFrames;
    [SerializeField] private Image[] slotIcons;
    [Tooltip("The key number in each slot's corner; shown on slots with an item.")]
    [SerializeField] private TextMeshProUGUI[] slotNumbers;
    [SerializeField] private TextMeshProUGUI itemNameLabel;
    [SerializeField] private TextMeshProUGUI hintLabel;

    [Header("Look")]
    [SerializeField] private int minSlots = 2;
    [SerializeField] private Color frameColor = new Color(0f, 0f, 0f, 0.9f);
    [SerializeField] private Color selectedFrameColor = new Color(1f, 0.85f, 0.4f, 1f);
    [Tooltip("{0} = the switch keys (e.g. [1-2]), {1} = the use key.")]
    [SerializeField] private string hintText = "{0} Switch   {1} Use";
    [Tooltip("Used while there's only one item, so nothing to switch to.")]
    [SerializeField] private string singleItemHintText = "{0} Use";

    private Inventory inventory;

    private void Start()
    {
        inventory = Inventory.Instance;
        inventory.Changed += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (inventory != null) inventory.Changed -= Refresh;
    }

    private void Update()
    {
        bool show = inventory != null && inventory.IsAvailableHere;
        if (panel.activeSelf != show) panel.SetActive(show);
    }

    private void Refresh()
    {
        int count = inventory.Held.Count;
        int shown = Mathf.Clamp(Mathf.Max(count, minSlots), 0, slotFrames.Length);
        int first = Mathf.Clamp(inventory.SelectedIndex - shown + 1, 0, Mathf.Max(0, count - shown));

        for (int i = 0; i < slotFrames.Length; i++)
        {
            slotFrames[i].gameObject.SetActive(i < shown);
            int itemIndex = first + i;
            InventoryItem item = itemIndex < count ? inventory.Held[itemIndex] : null;
            slotIcons[i].enabled = item != null && item.Icon != null;
            if (item != null)
            {
                slotIcons[i].sprite = item.Icon;
                slotIcons[i].color = item.IconColor;
            }
            slotFrames[i].color = item != null && itemIndex == inventory.SelectedIndex ? selectedFrameColor : frameColor;
            if (slotNumbers != null && i < slotNumbers.Length)
            {
                slotNumbers[i].gameObject.SetActive(item != null);
                slotNumbers[i].text = (itemIndex + 1).ToString();
            }
        }

        InventoryItem selected = inventory.Selected;
        itemNameLabel.text = selected != null ? selected.ItemName : "";
        hintLabel.text = selected != null ? Hint() : "";
    }

    private string Hint()
    {
        string useKey = InteractionPrompt.KeyHint(inventory.UseAction);
        string switchKeys = inventory.SwitchKeys;
        return switchKeys.Length > 0 ? string.Format(hintText, switchKeys, useKey) : string.Format(singleItemHintText, useKey);
    }
}
