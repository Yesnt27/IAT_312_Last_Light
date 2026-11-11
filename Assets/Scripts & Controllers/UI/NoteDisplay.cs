using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A piece of paper showing a note: text, with an optional picture above it. A note that is only a picture
// (like the bottle's symbols_paper) is drawn as the whole paper instead, so the plain paper behind it is hidden.
// Used by the Paper Note and by the note beside the books on the bookshelf.
public class NoteDisplay : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The plain paper behind the text.")]
    [SerializeField] private Graphic paper;
    [SerializeField] private TextMeshProUGUI text;
    [Tooltip("A picture on the paper, above the text.")]
    [SerializeField] private Image picture;
    [Tooltip("Fills the whole note: used when the note has a picture and no text.")]
    [SerializeField] private Image fullPicture;

    public void Show(string noteText, Sprite image)
    {
        bool pictureOnly = image != null && string.IsNullOrEmpty(noteText);

        paper.enabled = !pictureOnly;
        text.text = noteText;
        picture.gameObject.SetActive(image != null && !pictureOnly);
        picture.sprite = image;
        fullPicture.gameObject.SetActive(pictureOnly);
        fullPicture.sprite = image;
    }
}
