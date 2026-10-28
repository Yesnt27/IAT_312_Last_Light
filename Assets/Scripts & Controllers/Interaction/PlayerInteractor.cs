using UnityEngine;
using UnityEngine.InputSystem;

// On the player: finds the nearest available Interactable and shows its prompt.
public class PlayerInteractor : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Detection")]
    [SerializeField] private float interactRadius = 1.2f;

    private Interactable current;
    private readonly Collider2D[] hits = new Collider2D[16];

    public Interactable Current => current;

    private void OnEnable()
    {
        interactAction.action.Enable();
        interactAction.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        interactAction.action.performed -= OnInteract;
        if (InteractionPrompt.Instance != null) InteractionPrompt.Instance.Hide();
    }

    private void Update()
    {
        current = PlayerLock.IsLocked ? null : FindNearest();

        if (InteractionPrompt.Instance == null) return;
        if (current != null) InteractionPrompt.Instance.Show(current.PromptText);
        else InteractionPrompt.Instance.Hide();
    }

    private Interactable FindNearest()
    {
        ContactFilter2D filter = ContactFilter2D.noFilter;
        filter.useTriggers = true;
        int count = Physics2D.OverlapCircle(transform.position, interactRadius, filter, hits);

        Interactable nearest = null;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            Interactable interactable = hits[i].GetComponentInParent<Interactable>();
            if (interactable == null || !interactable.IsAvailable) continue;
            if (interactable.GetComponent<ClickToInteract>() != null) continue; // used with the mouse instead

            float distance = Vector2.Distance(transform.position, hits[i].ClosestPoint(transform.position));
            if (distance < nearestDistance)
            {
                nearest = interactable;
                nearestDistance = distance;
            }
        }
        return nearest;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (current != null && !PlayerLock.IsLocked) current.Interact();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
