using UnityEngine;

// Doors and stairs. Interact (or walk in, if loadOnTouch) to load another scene at a spawn point.
// Set the inherited Unlock Flag to make it a locked door.
public class SceneTransition : Interactable
{
    [Header("Destination")]
    [SerializeField] private string targetScene;
    [SerializeField] private string targetSpawnId;
    [SerializeField] private bool loadOnTouch;

    protected override void OnInteract()
    {
        SceneController.Instance.LoadScene(targetScene, targetSpawnId);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (loadOnTouch && other.CompareTag("Player") && IsAvailable && !IsLocked) Interact();
    }
}
