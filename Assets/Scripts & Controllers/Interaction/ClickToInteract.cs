using System.Collections.Generic;
using UnityEngine;

// Add next to an Interactable (e.g. FoundPage) to use it by clicking its sprite with the mouse
// instead of walking up and pressing E. No collider needed: the clickable area is the sprite.
[RequireComponent(typeof(Interactable))]
public class ClickToInteract : MonoBehaviour
{
    public static readonly List<ClickToInteract> All = new List<ClickToInteract>();

    [Header("Click")]
    [Tooltip("0 = clickable from anywhere on screen.")]
    [SerializeField] private float maxDistanceFromPlayer = 0f;
    [SerializeField] private Color hoverTint = new Color(1f, 1f, 0.55f);

    private Interactable interactable;
    private SpriteRenderer[] sprites;
    private Color[] baseColors;
    private bool isHovered;

    public Interactable Interactable => interactable;

    public bool CanClick => interactable.IsAvailable && IsInRange();

    public int SortingOrder
    {
        get
        {
            int order = int.MinValue;
            foreach (SpriteRenderer sprite in sprites) order = Mathf.Max(order, sprite.sortingOrder);
            return order;
        }
    }

    private void Awake()
    {
        interactable = GetComponent<Interactable>();
        sprites = GetComponentsInChildren<SpriteRenderer>(true);
        baseColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++) baseColors[i] = sprites[i].color;
    }

    private void OnEnable()
    {
        All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
        SetHovered(false);
    }

    public bool Contains(Vector2 worldPoint)
    {
        foreach (SpriteRenderer sprite in sprites)
        {
            if (!sprite.enabled || !sprite.gameObject.activeInHierarchy) continue;
            Bounds bounds = sprite.bounds;
            if (worldPoint.x >= bounds.min.x && worldPoint.x <= bounds.max.x &&
                worldPoint.y >= bounds.min.y && worldPoint.y <= bounds.max.y) return true;
        }
        return false;
    }

    public void SetHovered(bool hovered)
    {
        if (isHovered == hovered) return;
        isHovered = hovered;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null) sprites[i].color = hovered ? baseColors[i] * hoverTint : baseColors[i];
        }
    }

    private bool IsInRange()
    {
        if (maxDistanceFromPlayer <= 0f) return true;
        GameObject player = GameObject.FindWithTag("Player");
        return player != null && Vector2.Distance(player.transform.position, transform.position) <= maxDistanceFromPlayer;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        All.Clear();
    }
}
