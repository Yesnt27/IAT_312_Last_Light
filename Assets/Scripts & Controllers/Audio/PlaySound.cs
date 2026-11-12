using UnityEngine;

// Plays a sound effect when something calls Play(): hook it to any event in the Inspector (an Interactable's
// On Interact, a CameraTrigger's On Zoom Out, a Button's On Click...), or tick Play On Enable.
public class PlaySound : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private AudioClip clip;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Header("When")]
    [Tooltip("Also play whenever this object is switched on (e.g. a panel opening).")]
    [SerializeField] private bool playOnEnable;

    private void OnEnable()
    {
        if (playOnEnable) Play();
    }

    public void Play()
    {
        SoundEffects.Play(clip, volume);
    }
}
