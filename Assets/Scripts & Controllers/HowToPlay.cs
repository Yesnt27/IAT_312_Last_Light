using UnityEngine;
using UnityEngine.UI;

public class HowToPlay : MonoBehaviour
{
    public Image journal1;
    public Image journal2;
    public Image journal3;
    public Button leftButton;
    public Button rightButton;
    private int index = 1; 

    private void Awake()
    {
        if (leftButton != null)
        {
            leftButton.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        ShowImage();
    }

    public void ShowImage()
    {
        if (index == 1 && journal1 != null)
        {
            journal1.gameObject.SetActive(true);
            journal2.gameObject.SetActive(false);
            journal3.gameObject.SetActive(false);
            leftButton.gameObject.SetActive(false);
        }
        else if (index == 2 && journal2 != null)
        {
            journal2.gameObject.SetActive(true);
            journal1.gameObject.SetActive(false);
            journal3.gameObject.SetActive(false);
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);
        }
        else if (index == 3 && journal3 != null)
        {
            journal3.gameObject.SetActive(true);
            journal1.gameObject.SetActive(false);
            journal2.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);

        }
    }

    public void NextImage()
    {
        index++;
    }
  
  public void previousImage()
    {
        index--;
    }

    
    
}
