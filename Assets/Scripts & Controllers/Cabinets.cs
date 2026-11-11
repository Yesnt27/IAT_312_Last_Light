using UnityEngine;


public class Cabinets : MonoBehaviour
{
    public SpriteRenderer closedCabinet;
    public SpriteRenderer openCabinet;

    public void Awake()
    {
        closedCabinet.enabled = true;
        openCabinet.enabled = false;
    }

    public void OpenCabinet()
    {
        closedCabinet.enabled = false;
        openCabinet.enabled = true;
    }

    public void OnMouseDown()
    {
        OpenCabinet();
    }
}
