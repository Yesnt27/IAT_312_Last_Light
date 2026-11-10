using UnityEngine;

// Fades the music on this object's AudioSource in and out. Call FadeIn() / FadeOut(),
// e.g. from CameraTrigger's On Zoom Out / On Zoom In events. Untick Play On Awake on the AudioSource;
// its Volume is how loud the music gets. A track that doesn't loop fades out by itself at its end.
[RequireComponent(typeof(AudioSource))]
public class MusicFader : MonoBehaviour
{
    [Header("Fade")]
    [Tooltip("0 = start at full volume.")]
    [SerializeField] private float fadeInSeconds = 0f;
    [SerializeField] private float fadeOutSeconds = 2f;
    [Tooltip("When the AudioSource doesn't loop, fade out over the last Fade Out Seconds of the track.")]
    [SerializeField] private bool fadeOutAtEnd = true;
    [Tooltip("Off = the track starts from the beginning every time it fades in.")]
    [SerializeField] private bool resumeWhereItStopped = false;

    private AudioSource source;
    private float fullVolume;
    private float targetVolume;
    private bool isPaused;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        fullVolume = source.volume;
        source.volume = 0f;
    }

    private void Update()
    {
        if (!source.isPlaying) return;

        float target = targetVolume;
        if (fadeOutAtEnd && !source.loop && source.clip != null && fadeOutSeconds > 0f)
        {
            float remaining = source.clip.length - source.time;
            target = Mathf.Min(target, fullVolume * Mathf.Clamp01(remaining / fadeOutSeconds));
        }

        float seconds = target > source.volume ? fadeInSeconds : fadeOutSeconds;
        source.volume = seconds <= 0f ? target : Mathf.MoveTowards(source.volume, target, fullVolume * Time.deltaTime / seconds);

        if (targetVolume == 0f && source.volume == 0f)
        {
            if (resumeWhereItStopped)
            {
                source.Pause();
                isPaused = true;
            }
            else
            {
                source.Stop();
            }
        }
    }

    public void FadeIn()
    {
        targetVolume = fullVolume;
        if (fadeInSeconds <= 0f) source.volume = fullVolume;
        else if (!source.isPlaying && !isPaused) source.volume = 0f;

        if (isPaused) source.UnPause();
        else if (!source.isPlaying) source.Play();
        isPaused = false;
    }

    public void FadeOut()
    {
        targetVolume = 0f;
    }
}
