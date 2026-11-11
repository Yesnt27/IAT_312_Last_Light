using System.Collections;
using UnityEngine;

// On a ghost ship: its figures flicker, blinking out for a moment every so often (the Night 4 tell:
// "abnormal ones flicker"). Figures lists the figure meshes; the rest of the ship stays put.
public class FlickerFigures : MonoBehaviour
{
    [Header("Figures")]
    [SerializeField] private GameObject[] figures;

    [Header("Timing")]
    [Tooltip("Seconds the figures stay visible between flickers (random in this range).")]
    [SerializeField] private Vector2 visibleSeconds = new Vector2(0.6f, 2f);
    [Tooltip("Seconds each blink lasts (random in this range).")]
    [SerializeField] private Vector2 blinkSeconds = new Vector2(0.06f, 0.18f);
    [Tooltip("Up to this many quick blinks in a row.")]
    [SerializeField] private int maxBlinks = 3;

    private void OnEnable()
    {
        StartCoroutine(Flicker());
    }

    private void OnDisable()
    {
        SetVisible(true);
    }

    private IEnumerator Flicker()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(visibleSeconds.x, visibleSeconds.y));

            int blinks = Random.Range(1, maxBlinks + 1);
            for (int i = 0; i < blinks; i++)
            {
                SetVisible(false);
                yield return new WaitForSeconds(Random.Range(blinkSeconds.x, blinkSeconds.y));
                SetVisible(true);
                yield return new WaitForSeconds(Random.Range(blinkSeconds.x, blinkSeconds.y));
            }
        }
    }

    private void SetVisible(bool visible)
    {
        foreach (GameObject figure in figures)
        {
            if (figure != null) figure.SetActive(visible);
        }
    }
}
