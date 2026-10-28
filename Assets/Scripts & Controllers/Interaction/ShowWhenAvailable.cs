using UnityEngine;

// Hides an Interactable's sprites and labels while it isn't available (wrong day, task or flags),
// e.g. the message in a bottle only shows up on Day 3. Hidden objects can't be clicked either.
[RequireComponent(typeof(Interactable))]
public class ShowWhenAvailable : MonoBehaviour
{
    private Interactable interactable;
    private Renderer[] renderers;

    private void Awake()
    {
        interactable = GetComponent<Interactable>();
        renderers = GetComponentsInChildren<Renderer>(true);
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

    private void Refresh()
    {
        bool show = interactable.IsAvailable;
        foreach (Renderer part in renderers)
        {
            if (part != null) part.enabled = show;
        }
    }
}
