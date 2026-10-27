// The text for one placeholder cutscene card. Leave the title empty for "no cutscene".
[System.Serializable]
public class CutsceneCard
{
    public string title;
    [UnityEngine.TextArea(2, 6)]
    public string description;

    public bool IsEmpty => string.IsNullOrEmpty(title);
}
