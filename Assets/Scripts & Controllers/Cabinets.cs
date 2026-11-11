using UnityEngine;


public class Cabinets : MonoBehaviour
{
    public SpriteRenderer closedCabinet;
    public SpriteRenderer openCabinet;
    private bool isOpen = false;

    public void Awake()
    {
        closedCabinet.enabled = true;
        openCabinet.enabled = false;
    }

    public void OpenCabinet()
    {
        closedCabinet.enabled = false;
        openCabinet.enabled = true;
        isOpen = true;
    }

    public void CloseCabinet()
    {
        closedCabinet.enabled = true;
        openCabinet.enabled = false;
        isOpen = false;
    }

    public void OnMouseDown()
    {
        if (!isOpen) 
        {
            OpenCabinet();
        }
        else
        {
            CloseCabinet();
        }

    }
}
