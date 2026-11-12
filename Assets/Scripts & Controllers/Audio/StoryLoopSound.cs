using UnityEngine;
using UnityEngine.SceneManagement;

// On the Systems prefab: a looping sound tied to the story, e.g. the Day 4 horn. It plays while the When conditions
// are met (same as an Interactable's) and the player is in one of Plays In Scenes. Leaving those scenes while it's
// playing sets Stop For Good Flag, so it never comes back that game (the horn stops once the player steps outside).
[RequireComponent(typeof(AudioSource))]
public class StoryLoopSound : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private AudioClip clip;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;
    [SerializeField] private float fadeSeconds = 1f;

    [Header("When (empty = always)")]
    [SerializeField] private int[] activeOnDays;
    [SerializeField] private string[] activeDuringTasks;
    [SerializeField] private string[] requiredFlags;
    [SerializeField] private string[] hiddenByFlags;

    [Header("Where")]
    [Tooltip("Scene names it plays in. Empty = every scene.")]
    [SerializeField] private string[] playsInScenes;
    [Tooltip("Set when the player leaves Plays In Scenes while it's playing; it won't play again this game. Empty = it can come back.")]
    [SerializeField] private string stopForGoodFlag;

    private AudioSource source;
    private bool isOn;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = 0f;
    }

    private void Update()
    {
        GameState state = GameState.Instance;
        if (state == null) return;

        bool conditionsMet = StoryConditions.AreMet(activeOnDays, activeDuringTasks, requiredFlags, hiddenByFlags) && !state.HasFlag(stopForGoodFlag);
        bool inScene = playsInScenes == null || playsInScenes.Length == 0 || System.Array.IndexOf(playsInScenes, SceneManager.GetActiveScene().name) >= 0;
        if (isOn && conditionsMet && !inScene && !string.IsNullOrEmpty(stopForGoodFlag)) state.SetFlag(stopForGoodFlag);

        isOn = conditionsMet && inScene;
        if (isOn && !source.isPlaying) source.Play();

        float step = fadeSeconds > 0f ? Time.unscaledDeltaTime / fadeSeconds : 1f;
        source.volume = Mathf.MoveTowards(source.volume, isOn ? volume : 0f, step);
        if (!isOn && source.isPlaying && source.volume <= 0f) source.Stop();
    }
}
