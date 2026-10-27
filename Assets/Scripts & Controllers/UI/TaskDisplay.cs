using TMPro;
using UnityEngine;

// The "Task Dialogue Box": shows GameState's active task and hides when there is none.
public class TaskDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject taskPanel;
    [SerializeField] private TextMeshProUGUI taskLabel;

    private void Start()
    {
        GameState.Instance.Changed += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null) GameState.Instance.Changed -= Refresh;
    }

    private void Refresh()
    {
        string text = GameState.Instance.ActiveTaskText;
        bool hasTask = !string.IsNullOrEmpty(text);
        taskPanel.SetActive(hasTask);
        if (hasTask) taskLabel.text = text;
    }
}
