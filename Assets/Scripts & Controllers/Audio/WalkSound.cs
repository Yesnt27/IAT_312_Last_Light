using UnityEngine;

// On a player (the 2D Player prefab or the Night Scene's First Person Player): loops the footsteps clip while the
// player is walking on the ground, and fades it out quickly when they stop, jump or are frozen.
[RequireComponent(typeof(AudioSource))]
public class WalkSound : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private AudioClip footsteps;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;
    [Tooltip("Seconds to fade out after stopping.")]
    [SerializeField] private float fadeSeconds = 0.15f;

    private AudioSource source;
    private PlayerController player2D;
    private FirstPersonController player3D;

    private bool IsWalking => player2D != null
        ? player2D.MoveInput != 0f && player2D.IsGrounded
        : player3D != null && player3D.IsWalking;

    private void Awake()
    {
        player2D = GetComponent<PlayerController>();
        player3D = GetComponent<FirstPersonController>();
        source = GetComponent<AudioSource>();
        source.clip = footsteps;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = 0f;
    }

    private void Update()
    {
        bool walking = IsWalking;
        if (walking && !source.isPlaying) source.Play();

        float step = fadeSeconds > 0f ? volume * Time.deltaTime / fadeSeconds : volume;
        source.volume = walking ? volume : Mathf.MoveTowards(source.volume, 0f, step);
        if (!walking && source.isPlaying && source.volume <= 0f) source.Pause();
    }
}
