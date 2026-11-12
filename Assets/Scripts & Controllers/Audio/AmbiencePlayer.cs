using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// On the Systems prefab: looping background sound per scene (Ambiences: which scenes, which clip, how loud).
// Moving between scenes that share a clip keeps it playing without restarting (all the indoor floors);
// a different clip crossfades in over Fade Seconds; a scene in no list fades to silence (Night Scene, menus).
// Volume and Master Volume can be changed while playing.
public class AmbiencePlayer : MonoBehaviour
{
    [Header("Ambience Per Scene")]
    [SerializeField] private SceneAmbience[] ambiences = new SceneAmbience[0];

    [Header("Playback")]
    [SerializeField] private float fadeSeconds = 1.5f;
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;

    private readonly AudioSource[] sources = new AudioSource[2];
    private int current;
    private SceneAmbience playing;

    private void Awake()
    {
        for (int i = 0; i < sources.Length; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].loop = true;
            sources[i].playOnAwake = false;
            sources[i].spatialBlend = 0f;
            sources[i].volume = 0f;
        }
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void Start()
    {
        OnSceneChanged(default, SceneManager.GetActiveScene());
    }

    private void Update()
    {
        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource source = sources[i];
            float target = i == current && playing != null ? playing.volume * masterVolume : 0f;
            float step = fadeSeconds > 0f ? Time.unscaledDeltaTime / fadeSeconds : 1f;
            source.volume = Mathf.MoveTowards(source.volume, target, step);
            if (i != current && source.isPlaying && source.volume <= 0f) source.Stop();
        }
    }

    private void OnSceneChanged(Scene previous, Scene next)
    {
        SceneAmbience ambience = Find(next.name);
        AudioClip clip = ambience != null ? ambience.clip : null;
        AudioClip playingClip = playing != null ? playing.clip : null;
        playing = ambience;
        if (clip == playingClip) return;

        current = 1 - current;
        AudioSource source = sources[current];
        source.clip = clip;
        source.volume = 0f;
        if (clip != null) source.Play();
    }

    private SceneAmbience Find(string sceneName)
    {
        foreach (SceneAmbience ambience in ambiences)
        {
            if (ambience.clip != null && Array.IndexOf(ambience.scenes, sceneName) >= 0) return ambience;
        }
        return null;
    }
}

[Serializable]
public class SceneAmbience
{
    [Tooltip("Only a label for the Inspector.")]
    public string name;
    [Tooltip("Scene names, exactly as in Assets/Scenes (e.g. Floor 1).")]
    public string[] scenes = new string[0];
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 0.6f;
}
