// What happens after something finishes (a dialogue, a pickup, a locked door): optionally change
// the task and/or set a flag. Every field is optional. The task changes first, so a flag that
// completes the day's puzzle still gets the last word.
[System.Serializable]
public class StoryOutcome
{
    public string newTaskId;
    [UnityEngine.TextArea(1, 3)]
    public string newTaskText;
    public string setFlag;

    public void Apply()
    {
        GameState state = GameState.Instance;
        if (state == null) return;

        if (!string.IsNullOrEmpty(newTaskId)) state.SetTask(newTaskId, newTaskText);
        if (!string.IsNullOrEmpty(setFlag)) state.SetFlag(setFlag);
    }
}
