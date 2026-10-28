// One cutscene. Either a placeholder card (title + description) or, if Scene is set, a cutscene scene
// that DayManager loads instead. A cutscene scene must finish with a StoryTrigger that has
// "Ends Cutscene" ticked, so the story carries on. Leave everything empty for "no cutscene".
[System.Serializable]
public class CutsceneCard
{
    public string title;
    [UnityEngine.TextArea(2, 6)]
    public string description;
    [UnityEngine.Tooltip("Optional: a scene to play instead of the card (must be in Build Settings).")]
    public string scene;

    public bool IsEmpty => string.IsNullOrEmpty(title) && string.IsNullOrEmpty(scene);
    public bool IsScene => !string.IsNullOrEmpty(scene);
}
