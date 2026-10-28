using System.Collections;
using UnityEngine;

// A physical button that clicks down and springs back up when Press() is called.
public class PressableButton : MonoBehaviour
{
    [Header("Press")]
    [Tooltip("Which way the button moves when pressed, in its own local axes.")]
    [SerializeField] private Vector3 pressDirection = Vector3.down;
    [Tooltip("How far it moves, in metres.")]
    [SerializeField] private float pressDepth = 0.07f;
    [SerializeField] private float downTime = 0.06f;
    [SerializeField] private float upTime = 0.18f;

    private Vector3 restPosition;
    private Coroutine pressing;

    private void Awake()
    {
        restPosition = transform.localPosition;
    }

    public void Press()
    {
        if (pressing != null) StopCoroutine(pressing);
        pressing = StartCoroutine(PressRoutine());
    }

    private IEnumerator PressRoutine()
    {
        Vector3 worldOffset = transform.TransformDirection(pressDirection).normalized * pressDepth;
        Vector3 pressed = restPosition + (transform.parent != null ? transform.parent.InverseTransformVector(worldOffset) : worldOffset);

        yield return Slide(transform.localPosition, pressed, downTime);
        yield return Slide(pressed, restPosition, upTime);
        pressing = null;
    }

    private IEnumerator Slide(Vector3 from, Vector3 to, float time)
    {
        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            transform.localPosition = Vector3.Lerp(from, to, t / time);
            yield return null;
        }
        transform.localPosition = to;
    }
}
