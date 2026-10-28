using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// A loose wire on the left side of the WirePuzzle. Drag from its plug to stretch the wire;
// let go over the socket of the same colour to connect it. Anywhere else, it springs back.
public class WireDragEnd : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private Image plugImage;
    [Tooltip("The stretchy part. Its pivot is on its left edge, so it grows towards the pointer.")]
    [SerializeField] private RectTransform wire;
    [SerializeField] private Image wireImage;

    [Header("Look")]
    [SerializeField] private float looseLength = 40f;
    [SerializeField] private float thickness = 22f;

    private WirePuzzle puzzle;
    private int colorIndex;
    private bool isConnected;

    public int ColorIndex => colorIndex;
    public bool IsConnected => isConnected;

    public void Setup(WirePuzzle owner, int index, Color color)
    {
        puzzle = owner;
        colorIndex = index;
        isConnected = false;
        plugImage.color = color;
        wireImage.color = color;
        ResetWire();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isConnected) StretchTo(eventData.position, eventData.pressEventCamera);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isConnected) return;

        GameObject hit = eventData.pointerCurrentRaycast.gameObject;
        WireSocket socket = hit != null ? hit.GetComponentInParent<WireSocket>() : null;
        if (!TryConnect(socket, eventData.pressEventCamera)) ResetWire();
    }

    // Connects to the socket if it's free and the same colour.
    public bool TryConnect(WireSocket socket, Camera eventCamera = null)
    {
        if (isConnected || socket == null || socket.IsConnected || socket.ColorIndex != colorIndex) return false;

        isConnected = true;
        socket.Connect();
        StretchTo(RectTransformUtility.WorldToScreenPoint(eventCamera, socket.ConnectPoint.position), eventCamera);
        puzzle.OnWireConnected();
        return true;
    }

    private void StretchTo(Vector2 screenPoint, Camera eventCamera)
    {
        RectTransform parent = (RectTransform)wire.parent;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, eventCamera, out Vector2 localPoint)) return;

        Vector2 direction = localPoint - (Vector2)wire.localPosition;
        wire.sizeDelta = new Vector2(direction.magnitude, thickness);
        wire.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private void ResetWire()
    {
        wire.sizeDelta = new Vector2(looseLength, thickness);
        wire.localRotation = Quaternion.identity;
    }
}
