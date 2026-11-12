using UnityEngine;

// Night Scene → Weather/Lightning: strikes the Lightning Strike bolt at random spots out at sea, Strikes Per Minute
// times a minute on average (NightWeather sets that for tonight). Put this object at the lighthouse: distances and
// bearings are measured from it. Bearings are compass degrees (0 = north / +Z, 90 = east / +X, -90 = west).
public class LightningStorm : MonoBehaviour
{
    [Header("How Often")]
    [Tooltip("Strikes per minute, on average. 0 = no lightning. NightWeather overwrites this with tonight's value.")]
    [SerializeField] private float strikesPerMinute = 2f;
    [Tooltip("Chance that a strike flashes a second time in the same place.")]
    [Range(0f, 1f)]
    [SerializeField] private float secondFlashChance = 0.4f;
    [Tooltip("Seconds after the strike starts that the second flash starts.")]
    [SerializeField] private float secondFlashDelay = 0.75f;

    [Header("Where")]
    [Tooltip("How far out the bolts land, in metres (X = nearest, Y = farthest). The ships are about 300 m out.")]
    [SerializeField] private Vector2 distance = new Vector2(350f, 800f);
    [Tooltip("The arc the bolts land in (X = from, Y = to). -165 to 25 = the west and north windows, where the ships are.")]
    [SerializeField] private Vector2 bearing = new Vector2(-165f, 25f);
    [Tooltip("World height the bolts come down from.")]
    [SerializeField] private float cloudHeight = 220f;
    [Tooltip("World height they come down to (the sea is at -25).")]
    [SerializeField] private float seaLevel = -25f;
    [Tooltip("How far the top may lean away from straight above the bottom, as a part of the bolt's height.")]
    [SerializeField] private float lean = 0.15f;

    [Header("References")]
    [SerializeField] private LightningStrike strike;

    private float untilNext;

    public float StrikesPerMinute
    {
        get => strikesPerMinute;
        set
        {
            strikesPerMinute = value;
            untilNext = NextWait();
        }
    }

    private void Start()
    {
        untilNext = NextWait();
    }

    private void Update()
    {
        if (strikesPerMinute <= 0f) return;

        untilNext -= Time.deltaTime;
        if (untilNext > 0f) return;

        untilNext = NextWait();
        StrikeSomewhere();
    }

    [ContextMenu("Strike Now (Play mode)")]
    public void StrikeSomewhere()
    {
        float angle = Random.Range(bearing.x, bearing.y) * Mathf.Deg2Rad;
        Vector3 away = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
        Vector3 bottom = transform.position + away * Random.Range(distance.x, distance.y);
        bottom.y = seaLevel;

        float height = cloudHeight - seaLevel;
        Vector2 offset = Random.insideUnitCircle * (height * lean);
        Vector3 top = bottom + new Vector3(offset.x, height, offset.y);

        CancelInvoke(nameof(FlashAgain));
        strike.Strike(top, bottom, transform.position);
        if (Random.value < secondFlashChance) Invoke(nameof(FlashAgain), secondFlashDelay);
    }

    private void FlashAgain()
    {
        strike.Flash();
    }

    // Between half and one and a half times the average gap, so the strikes don't tick like a clock.
    private float NextWait()
    {
        return strikesPerMinute > 0f ? 60f / strikesPerMinute * Random.Range(0.5f, 1.5f) : 0f;
    }
}
