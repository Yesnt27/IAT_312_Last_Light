using UnityEngine;

// Dev tool for test scenes: starts the day loop in this scene when you press Play,
// without loading the story scenes. Disable the component to test other things here.
public class DayDebugStart : MonoBehaviour
{
    [SerializeField] private int startDay = 1;

    private void Start()
    {
        DayManager.Instance.StartTestFlow(startDay);
    }
}
