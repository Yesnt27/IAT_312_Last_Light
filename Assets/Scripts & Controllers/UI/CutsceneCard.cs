using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

// One cutscene: a placeholder card (title + description), a video, or a cutscene scene, checked in that order:
// Scene set = DayManager loads that scene (it must finish with a StoryTrigger that has "Ends Cutscene" ticked);
// else Video set = the clip plays full screen (VideoCutscene), with Video Lines in the dialogue box at their times;
// else the card. Leave everything empty for "no cutscene".
[System.Serializable]
public class CutsceneCard
{
    public string title;
    [TextArea(2, 6)]
    public string description;
    [Tooltip("Optional: a scene to play instead of the card (must be in Build Settings).")]
    public string scene;

    [Tooltip("Optional: a video to play instead of the card, e.g. Visual Assets/videos/sleep 1.")]
    public VideoClip video;
    [Tooltip("Shown in the dialogue box while the video plays, each from its Start to its End time (seconds into the clip).")]
    public List<TimedLine> videoLines = new List<TimedLine>();

    public bool IsEmpty => string.IsNullOrEmpty(title) && string.IsNullOrEmpty(scene) && video == null;
    public bool IsScene => !string.IsNullOrEmpty(scene);
    public bool IsVideo => !IsScene && video != null;
}

// A dialogue-box line that shows and hides by itself at set times during a video (no click needed).
[System.Serializable]
public class TimedLine
{
    public DialogueLine line = new DialogueLine();
    [Tooltip("Seconds into the clip when the line appears.")]
    [Min(0f)] public float startTime;
    [Tooltip("Seconds into the clip when it goes away. 0 = stays until the clip ends.")]
    [Min(0f)] public float endTime;
}
