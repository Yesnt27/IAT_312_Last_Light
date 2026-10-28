using System.Collections;
using UnityEngine;

// A story beat that plays by itself: dialogue, then an outcome (new task and/or flag).
// When In Scene: plays as soon as the player is in this scene and the conditions are met.
// On Player Enter: plays when the player walks into this object's trigger collider.
// It waits for fades, cutscene cards and other dialogue to finish before it starts.
public class StoryTrigger : MonoBehaviour
{
    public enum TriggerMode { WhenInScene, OnPlayerEnter }

    [Header("When")]
    [SerializeField] private TriggerMode mode = TriggerMode.WhenInScene;
    [SerializeField] private int[] activeOnDays;
    [SerializeField] private string[] activeDuringTasks;
    [SerializeField] private string[] requiredFlags;
    [SerializeField] private string[] hiddenByFlags;
    [Tooltip("Set once it has played, so it never plays again this game. Empty = plays once per visit to the scene.")]
    [SerializeField] private string rememberAsFlag;

    [Header("What")]
    [SerializeField] private Dialogue dialogue;
    [SerializeField] private StoryOutcome outcome = new StoryOutcome();

    private bool hasPlayed;
    private bool isWaiting;

    private bool CanPlay
    {
        get
        {
            if (hasPlayed || isWaiting) return false;
            if (!string.IsNullOrEmpty(rememberAsFlag) && GameState.Instance.HasFlag(rememberAsFlag)) return false;
            return StoryConditions.AreMet(activeOnDays, activeDuringTasks, requiredFlags, hiddenByFlags);
        }
    }

    private void Start()
    {
        if (mode != TriggerMode.WhenInScene) return;
        GameState.Instance.Changed += TryPlay;
        TryPlay();
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null) GameState.Instance.Changed -= TryPlay;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (mode == TriggerMode.OnPlayerEnter && other.CompareTag("Player")) TryPlay();
    }

    private void TryPlay()
    {
        if (CanPlay) StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        isWaiting = true;
        while (SceneController.Instance.IsLoading || DialogueManager.instance.isDialogueActive || CutscenePlaceholder.Instance.IsShowing)
        {
            yield return null;
        }
        isWaiting = false;

        // Things may have changed while waiting.
        if (!CanPlay) yield break;

        hasPlayed = true;
        DialogueManager.instance.StartDialogue(dialogue, Finish);
    }

    private void Finish()
    {
        if (!string.IsNullOrEmpty(rememberAsFlag)) GameState.Instance.SetFlag(rememberAsFlag);
        outcome.Apply();
    }
}
