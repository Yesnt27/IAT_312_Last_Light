using UnityEngine;

// Doors and stairs. Interact (or walk in, if loadOnTouch) to load another scene at a spawn point.
// Set the inherited Unlock Flag to make it a locked door.
// Touches during a scene load are ignored, so a player who arrives standing in one isn't sent straight back.
public class SceneTransition : Interactable
{
    [Header("Destination")]
    [SerializeField] private string targetScene;
    [SerializeField] private string targetSpawnId;
    [SerializeField] private bool loadOnTouch;

    public bool LoadsOnTouch => loadOnTouch;

    protected override void OnInteract()
    {
        SceneController.Instance.LoadScene(targetScene, targetSpawnId);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!loadOnTouch || !other.CompareTag("Player") || SceneController.Instance.IsLoading) return;
        if (IsAvailable && !IsLocked) Interact();
    }
}
