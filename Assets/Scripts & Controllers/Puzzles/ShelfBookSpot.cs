using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One book in the bookshelf close-up: an invisible hitbox over the book in the shelf picture. Move, resize or rotate it
// (Rect Tool) to fit the book; the Scene view draws it as a yellow outline. Under the mouse the book lights up: Highlight
// is a tinted copy of the shelf picture, lined up with it and only drawn inside this box (the Mask). No title is shown,
// so the player has to open the books. BookshelfPuzzle opens Open Pages on a click.
[RequireComponent(typeof(Mask))]
public class ShelfBookSpot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Book")]
    [Tooltip("The open book, e.g. Fishing_Almanac (Visual Assets/Journal Assets).")]
    [SerializeField] private Sprite openPages;
    [Tooltip("The book that solves the puzzle when it's opened while carrying the Clue Item.")]
    [SerializeField] private bool isClue;

    [Header("Hover")]
    [Tooltip("The shelf picture this book is drawn in.")]
    [SerializeField] private Image picture;
    [SerializeField] private Image highlight;
    [Tooltip("Multiplied with the picture under the mouse, like hovering the electrical box.")]
    [SerializeField] private Color hoverTint = new Color(1f, 1f, 0.55f);

    private bool isHovered;

    public event Action<ShelfBookSpot> Clicked;

    public Sprite OpenPages => openPages;
    public bool IsClue => isClue;

    private void OnDisable()
    {
        SetHovered(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHovered(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHovered(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) Clicked?.Invoke(this);
    }

    private void SetHovered(bool hovered)
    {
        if (isHovered == hovered) return;

        isHovered = hovered;
        if (hovered) LineUpHighlight();
        highlight.gameObject.SetActive(hovered);
    }

    // Covers the shelf picture exactly, whatever this box's position, size and rotation.
    private void LineUpHighlight()
    {
        RectTransform target = highlight.rectTransform;
        RectTransform source = picture.rectTransform;
        highlight.sprite = picture.sprite;
        highlight.preserveAspect = picture.preserveAspect;
        highlight.color = hoverTint;
        highlight.raycastTarget = false;

        target.anchorMin = target.anchorMax = new Vector2(0.5f, 0.5f);
        target.pivot = source.pivot;
        target.localScale = Vector3.one;
        target.SetPositionAndRotation(source.position, source.rotation);
        target.sizeDelta = source.rect.size * (source.lossyScale.x / target.lossyScale.x);
    }

    private void OnDrawGizmos()
    {
        Vector3[] corners = new Vector3[4];
        ((RectTransform)transform).GetWorldCorners(corners);
        Gizmos.color = Color.yellow;
        for (int i = 0; i < 4; i++) Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
    }
}
