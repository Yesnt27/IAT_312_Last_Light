using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

// On the Systems prefab's UI canvas: plays a cutscene video full screen (a CutsceneCard with a Video), under the
// dialogue box (its Panel sits just before the Dialogue Box in the canvas). The black fade is lifted while it plays,
// since the Panel is black itself, and put back afterwards. Video Lines show in the dialogue box at their times.
// Esc skips (Can Skip). The player is frozen while it plays. DayManager.PlayCutscene calls Play().
public class VideoCutscene : MonoBehaviour
{
    public static VideoCutscene Instance { get; private set; }

    [Header("References")]
    [Tooltip("Full-screen black background; on while a video plays.")]
    [SerializeField] private GameObject panel;
    [SerializeField] private RawImage screen;
    [SerializeField] private AspectRatioFitter screenFitter;
    [SerializeField] private VideoPlayer player;

    [Header("Playback")]
    [SerializeField] private bool canSkip = true;

    private RenderTexture texture;
    private Action onDone;
    private bool reachedEnd;
    private float fadeBefore;

    public bool IsPlaying => panel.activeSelf;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        player.playOnAwake = false;
        player.isLooping = false;
        player.renderMode = VideoRenderMode.RenderTexture;
        player.audioOutputMode = VideoAudioOutputMode.Direct;
        player.loopPointReached += _ => reachedEnd = true;
    }

    private void OnDestroy()
    {
        if (texture != null) texture.Release();
    }

    public void Play(CutsceneCard card, Action onComplete)
    {
        StopAllCoroutines();
        onDone = onComplete;
        StartCoroutine(PlayRoutine(card));
    }

    private IEnumerator PlayRoutine(CutsceneCard card)
    {
        VideoClip clip = card.video;
        PlayerLock.Add(this);
        panel.SetActive(true);
        fadeBefore = ScreenFader.Instance.Alpha;
        ScreenFader.Instance.SetAlpha(0f);
        screen.enabled = false;
        UseTexture(clip);

        reachedEnd = false;
        player.clip = clip;
        player.Prepare();
        while (!player.isPrepared) yield return null;

        screen.enabled = true;
        player.Play();

        float length = (float)clip.length;
        float elapsed = 0f;
        int shownLine = -1;
        while (!reachedEnd && elapsed < length + 1f && !SkipPressed())
        {
            elapsed += Time.unscaledDeltaTime;
            int line = LineAt(card, (float)player.time, length);
            if (line != shownLine) ShowLine(card, line);
            shownLine = line;
            yield return null;
        }

        Finish();
    }

    private void Finish()
    {
        DialogueManager.instance.HideTimedLine();
        player.Stop();
        ScreenFader.Instance.SetAlpha(fadeBefore);
        panel.SetActive(false);
        PlayerLock.Remove(this);

        Action done = onDone;
        onDone = null;
        done?.Invoke();
    }

    // The last line whose time window contains t, or -1.
    private static int LineAt(CutsceneCard card, float t, float length)
    {
        int found = -1;
        for (int i = 0; i < card.videoLines.Count; i++)
        {
            TimedLine line = card.videoLines[i];
            float end = line.endTime > 0f ? line.endTime : length;
            if (t >= line.startTime && t < end) found = i;
        }
        return found;
    }

    private static void ShowLine(CutsceneCard card, int index)
    {
        if (index < 0) DialogueManager.instance.HideTimedLine();
        else DialogueManager.instance.ShowTimedLine(card.videoLines[index].line);
    }

    private void UseTexture(VideoClip clip)
    {
        int width = Mathf.Max(1, (int)clip.width);
        int height = Mathf.Max(1, (int)clip.height);
        if (texture == null || texture.width != width || texture.height != height)
        {
            if (texture != null) texture.Release();
            texture = new RenderTexture(width, height, 0);
        }

        player.targetTexture = texture;
        screen.texture = texture;
        screenFitter.aspectRatio = (float)width / height;
    }

    private bool SkipPressed()
    {
        Keyboard keyboard = Keyboard.current;
        return canSkip && keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
    }
}
