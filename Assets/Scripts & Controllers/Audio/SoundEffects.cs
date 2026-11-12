using UnityEngine;

// On the Systems prefab: plays one-shot sound effects (2D, not affected by where the camera is), so they keep
// playing through scene loads and objects don't each need an AudioSource. From code: SoundEffects.Play(clip).
// From the Inspector: an Interactable's Interact Sound, or a PlaySound component hooked to any event.
[RequireComponent(typeof(AudioSource))]
public class SoundEffects : MonoBehaviour
{
    public static SoundEffects Instance { get; private set; }

    [Header("Volume")]
    [Tooltip("Multiplies every sound effect (above 1 = louder than the files). While playing, change it on Systems(Clone) under DontDestroyOnLoad, not on the prefab.")]
    [Range(0f, 3f)]
    [SerializeField] private float masterVolume = 1.5f;

    private AudioSource source;

    private void Awake()
    {
        Instance = this;
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    // Safe to call with no clip (nothing plays) or before the Systems prefab exists.
    public static void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null || Instance == null) return;
        Instance.source.PlayOneShot(clip, volume * Instance.masterVolume);
    }
}
