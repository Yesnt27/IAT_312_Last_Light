using UnityEngine;

// Fades a piece of UI in and out (needs a CanvasGroup on the same object). Call Show() / Hide(),
// e.g. from CameraTrigger's On Zoom Out / On Zoom In events.
[RequireComponent(typeof(CanvasGroup))]
public class FadeGroup : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private float fadeSeconds = 1.5f;
    [SerializeField] private bool startHidden = true;

    private CanvasGroup group;
    private float targetAlpha;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (startHidden) group.alpha = 0f;
        targetAlpha = group.alpha;
    }

    private void Update()
    {
        group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, Time.deltaTime / Mathf.Max(0.01f, fadeSeconds));
    }

    public void Show()
    {
        targetAlpha = 1f;
    }

    public void Hide()
    {
        targetAlpha = 0f;
    }
}
