using UnityEngine;
using UnityEngine.Events;

// Runs On First Time the first time Play() is called in a game, then never again. It's remembered as a
// GameState flag, so starting a new game shows it once more. Outside's zoom-out title and music use this:
// CameraTrigger's On Zoom Out calls Play().
public class PlayOncePerGame : MonoBehaviour
{
    [Header("Once")]
    [Tooltip("Set the first time it plays. Use a name only this beat uses.")]
    [SerializeField] private string rememberAsFlag = GameFlags.ZoomIntroPlayed;
    [SerializeField] private UnityEvent onFirstTime = new UnityEvent();

    private bool played;

    public void Play()
    {
        GameState state = GameState.Instance;
        if (played || (state != null && state.HasFlag(rememberAsFlag))) return;

        played = true;
        if (state != null) state.SetFlag(rememberAsFlag);
        onFirstTime.Invoke();
    }
}
