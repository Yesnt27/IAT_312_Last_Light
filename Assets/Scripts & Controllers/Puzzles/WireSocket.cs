using UnityEngine;
using UnityEngine.UI;

// A socket on the right side of the WirePuzzle. Accepts the wire of the same colour;
// its little light turns on once a wire is plugged in.
public class WireSocket : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image socketImage;
    [SerializeField] private Image statusLight;

    [Header("Look")]
    [SerializeField] private Color lightOff = new Color(0.15f, 0.15f, 0.15f);
    [SerializeField] private Color lightOn = new Color(0.3f, 1f, 0.35f);

    private int colorIndex;
    private bool isConnected;

    public int ColorIndex => colorIndex;
    public bool IsConnected => isConnected;
    public RectTransform ConnectPoint => (RectTransform)transform;

    public void Setup(int index, Color color)
    {
        colorIndex = index;
        isConnected = false;
        socketImage.color = color;
        if (statusLight != null) statusLight.color = lightOff;
    }

    public void Connect()
    {
        isConnected = true;
        if (statusLight != null) statusLight.color = lightOn;
    }
}
