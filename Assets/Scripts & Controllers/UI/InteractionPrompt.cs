using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionPrompt : MonoBehaviour
{
    public static InteractionPrompt Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptLabel;

    [Header("Settings")]
    [SerializeField] private string keyHint = "[E]";

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    // "[E]" for whatever key the action is bound to.
    public static string KeyHint(InputActionReference action)
    {
        return $"[{action.action.GetBindingDisplayString()}]";
    }

    // An empty hint ("") shows the text alone.
    public void Show(string text, string hint = null)
    {
        string key = hint ?? keyHint;
        promptLabel.text = key.Length > 0 ? $"{key} {text}" : text;
        if (!promptPanel.activeSelf) promptPanel.SetActive(true);
    }

    public void Hide()
    {
        if (promptPanel.activeSelf) promptPanel.SetActive(false);
    }
}
