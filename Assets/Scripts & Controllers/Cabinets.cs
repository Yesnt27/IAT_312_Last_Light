using UnityEngine;


public class Cabinets : MonoBehaviour
{
    public SpriteRenderer closedCabinet;
    public SpriteRenderer openCabinet;
    public SpriteRenderer key;
    private bool isOpen = false;

    public void Awake()
    {
        closedCabinet.enabled = true;
        openCabinet.enabled = false;
        key.enabled = false;
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
            showKey();
        }
        else
        {
            CloseCabinet();
            if (openCabinet.name == "Open right") {
                hideKey();
            }
        }

    }

    public void showKey()
    {
        if (openCabinet.name == "Open right" && isOpen) {
            key.enabled = true;
        }
    }

    public void hideKey()
    {
        key.enabled = false;
    }
}
