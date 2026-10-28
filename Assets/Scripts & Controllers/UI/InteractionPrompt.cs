using TMPro;
using UnityEngine;

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

    public void Show(string text)
    {
        promptLabel.text = $"{keyHint} {text}";
        if (!promptPanel.activeSelf) promptPanel.SetActive(true);
    }

    public void Hide()
    {
        if (promptPanel.activeSelf) promptPanel.SetActive(false);
    }
}
