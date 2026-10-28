using UnityEngine;
using UnityEngine.UI;

public class HowToPlay : MonoBehaviour
{
    public Button nextButton;
    public Image journal;
    public Button hideButton;

    private void Awake()
    {
        hideButton.gameObject.SetActive(false);
    }

    public void HideButton() 
    {
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (journal != null) journal.gameObject.SetActive(false);
    }

    public void ShowButton()
    {
        if (nextButton != null) nextButton.gameObject.SetActive(true);
        if (journal != null) journal.gameObject.SetActive(true);
    }

}
