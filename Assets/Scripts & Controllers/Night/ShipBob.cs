using UnityEngine;

// Gentle bob, pitch and roll so a ship looks like it's sitting in the waves.
// Speed > 0 makes it sail slowly forward (along its blue Z axis).
public class ShipBob : MonoBehaviour
{
    [Header("Bob")]
    [SerializeField] private float bobHeight = 0.35f;
    [SerializeField] private float bobSpeed = 0.7f;
    [SerializeField] private float rollAngle = 2.5f;
    [SerializeField] private float pitchAngle = 1.2f;

    [Header("Sail")]
    [SerializeField] private float speed = 0f;

    private Vector3 basePosition;
    private Quaternion baseRotation;
    private float phase;

    public float Speed { get => speed; set => speed = value; }
    public float SinkSpeed { get; set; }

    private void Awake()
    {
        basePosition = transform.position;
        baseRotation = transform.rotation;
        phase = Random.value * 10f;
    }

    private void Update()
    {
        basePosition += (baseRotation * Vector3.forward * speed + Vector3.down * SinkSpeed) * Time.deltaTime;

        float t = Time.time * bobSpeed + phase;
        transform.position = basePosition + Vector3.up * Mathf.Sin(t) * bobHeight;
        transform.rotation = baseRotation * Quaternion.Euler(Mathf.Sin(t * 0.8f) * pitchAngle, 0f, Mathf.Sin(t * 0.6f + 1f) * rollAngle);
    }
}
