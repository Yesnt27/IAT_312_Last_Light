using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A black screen with the upcoming day on it ("Day 2"). DayManager spawns the prefab (Assets/Prefabs/UI/Day Card,
// its own canvas above everything) before each day starts. It fades to black if the screen isn't black yet, fades the
// text in and out, then leaves the screen black (ScreenFader) so the day's scene fades in from there. Destroys itself.
public class DayCard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI label;

    [Header("Text")]
    [Tooltip("{0} = the day number.")]
    [SerializeField] private string dayText = "Day {0}";

    [Header("Timing (seconds)")]
    [SerializeField] private float fadeSeconds = 0.6f;
    [SerializeField] private float holdSeconds = 1.8f;

    public void Show(int day, Action onDone)
    {
        StartCoroutine(Play(day, onDone));
    }

    private IEnumerator Play(int day, Action onDone)
    {
        PlayerLock.Add(this);
        label.text = string.Format(dayText, day);
        label.alpha = 0f;

        bool alreadyBlack = ScreenFader.Instance == null || ScreenFader.Instance.Alpha >= 1f;
        yield return Fade(alreadyBlack ? 1f : 0f, 1f, SetBackground);
        if (ScreenFader.Instance != null) ScreenFader.Instance.SetAlpha(1f);

        yield return Fade(0f, 1f, alpha => label.alpha = alpha);
        yield return new WaitForSecondsRealtime(holdSeconds);
        yield return Fade(1f, 0f, alpha => label.alpha = alpha);

        PlayerLock.Remove(this);
        Destroy(gameObject);
        onDone?.Invoke();
    }

    private IEnumerator Fade(float from, float to, Action<float> apply)
    {
        for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
        {
            apply(Mathf.Lerp(from, to, t / fadeSeconds));
            yield return null;
        }
        apply(to);
    }

    private void SetBackground(float alpha)
    {
        Color color = background.color;
        color.a = alpha;
        background.color = color;
    }

    private void OnDestroy()
    {
        PlayerLock.Remove(this);
    }
}
