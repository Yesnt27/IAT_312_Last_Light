using UnityEngine;
using UnityEngine.EventSystems;

// Shows Target only while the mouse is over this UI element, e.g. a book's title on its spine in the bookshelf.
public class HoverReveal : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private GameObject target;

    private void OnEnable()
    {
        target.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        target.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        target.SetActive(false);
    }
}
