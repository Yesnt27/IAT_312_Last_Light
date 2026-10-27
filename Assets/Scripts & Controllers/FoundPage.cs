using UnityEngine;

// A journal page in the world. Interact to collect it; collecting the day's page ends the day puzzle.
public class FoundPage : Interactable
{
    [Header("Page")]
    [SerializeField] private int pageNumber = 1;
    [SerializeField] private Dialogue foundDialogue;

    private void Start()
    {
        if (GameState.Instance.HasPage(pageNumber)) gameObject.SetActive(false);
    }

    protected override void OnInteract()
    {
        gameObject.SetActive(false);
        GameState.Instance.CollectPage(pageNumber);
        DialogueManager.instance.StartDialogue(foundDialogue);
    }
}
