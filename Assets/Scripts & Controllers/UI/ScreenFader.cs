using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CanvasGroup fadeGroup;

    [Header("Settings")]
    [SerializeField] private float defaultDuration = 0.5f;

    private void Awake()
    {
        Instance = this;
        SetAlpha(0f);
    }

    public IEnumerator FadeOut(float duration = -1f)
    {
        yield return Fade(1f, duration);
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        yield return Fade(0f, duration);
    }

    public void SetAlpha(float alpha)
    {
        fadeGroup.alpha = alpha;
        fadeGroup.blocksRaycasts = alpha > 0f;
    }

    private IEnumerator Fade(float target, float duration)
    {
        if (duration < 0f) duration = defaultDuration;
        PlayerLock.Add(this);

        float start = fadeGroup.alpha;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            SetAlpha(Mathf.Lerp(start, target, t / duration));
            yield return null;
        }
        SetAlpha(target);

        PlayerLock.Remove(this);
    }
}
