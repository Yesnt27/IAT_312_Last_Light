// The shared "is this available right now?" check used by Interactable and StoryTrigger.
// An empty list means no condition of that kind.
public static class StoryConditions
{
    public static bool AreMet(int[] activeOnDays, string[] activeDuringTasks, string[] requiredFlags, string[] hiddenByFlags)
    {
        GameState state = GameState.Instance;
        if (state == null) return false;

        if (activeOnDays != null && activeOnDays.Length > 0 && System.Array.IndexOf(activeOnDays, state.CurrentDay) < 0) return false;

        if (activeDuringTasks != null && activeDuringTasks.Length > 0)
        {
            bool taskMatches = false;
            foreach (string task in activeDuringTasks)
            {
                if (state.IsTaskActive(task)) taskMatches = true;
            }
            if (!taskMatches) return false;
        }

        if (requiredFlags != null)
        {
            foreach (string flag in requiredFlags)
            {
                if (!string.IsNullOrEmpty(flag) && !state.HasFlag(flag)) return false;
            }
        }

        if (hiddenByFlags != null)
        {
            foreach (string flag in hiddenByFlags)
            {
                if (state.HasFlag(flag)) return false;
            }
        }

        return true;
    }
}
