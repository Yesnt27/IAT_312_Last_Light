using UnityEngine;

// Buttons on the title and how-to-play screens: hook each Button's On Click to one of these.
// Play starts a new game (the intro cutscene, then Day 1); the others change scene with the usual fade.
// Use this instead of putting a SceneController in a scene: the one on the Systems prefab must stay the only one.
public class TitleMenu : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string howToPlayScene = "HowToPlay";
    [SerializeField] private string titleScene = "Title Screen";

    public void Play()
    {
        DayManager.Instance.StartNewGame();
    }

    public void OpenHowToPlay()
    {
        SceneController.Instance.LoadScene(howToPlayScene);
    }

    public void BackToTitle()
    {
        SceneController.Instance.LoadScene(titleScene);
    }
}
