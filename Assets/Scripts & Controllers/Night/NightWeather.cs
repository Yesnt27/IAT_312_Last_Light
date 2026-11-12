using UnityEngine;

// Night Scene → Weather: tonight's rain and lightning. Nights lists them night by night (element 0 = night 1; nights
// past the end of the list use the last entry), so the storm can get worse as the days go by. Which night it is comes
// from NightShift: the day loop's day, or its Preview Night when you press Play directly in the Night Scene.
// Changing the numbers while playing takes effect straight away.
public class NightWeather : MonoBehaviour
{
    [Header("Per Night (element 0 = night 1)")]
    [SerializeField] private NightWeatherSettings[] nights =
    {
        new NightWeatherSettings { rainPerSecond = 500f, wind = 1f, lightningPerMinute = 1f },
        new NightWeatherSettings { rainPerSecond = 1200f, wind = 2f, lightningPerMinute = 2f },
        new NightWeatherSettings { rainPerSecond = 2200f, wind = 3.5f, lightningPerMinute = 3.5f },
        new NightWeatherSettings { rainPerSecond = 4000f, wind = 5f, lightningPerMinute = 6f },
    };

    [Header("References")]
    [SerializeField] private NightShift nightShift;
    [Tooltip("The Rain particle system (set up after SpeedTutor's rain tutorial).")]
    [SerializeField] private ParticleSystem rain;
    [SerializeField] private LightningStorm lightning;

    private bool isChanged = true;

    // A number was changed in the Inspector: apply it on the next frame.
    private void OnValidate()
    {
        isChanged = true;
    }

    private void Update()
    {
        if (!isChanged) return;

        isChanged = false;
        Apply();
    }

    private void Apply()
    {
        if (nights == null || nights.Length == 0) return;

        NightWeatherSettings tonight = nights[Mathf.Clamp(nightShift.Night - 1, 0, nights.Length - 1)];

        ParticleSystem.EmissionModule emission = rain.emission;
        emission.rateOverTime = tonight.rainPerSecond;

        // The tutorial's "more angled rain": a sideways push on top of the fall.
        ParticleSystem.VelocityOverLifetimeModule velocity = rain.velocityOverLifetime;
        velocity.z = new ParticleSystem.MinMaxCurve(tonight.wind * 0.5f, tonight.wind);

        // Start over (the system prewarms), so the sky is full of tonight's rain at once.
        rain.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (tonight.rainPerSecond > 0f) rain.Play();

        lightning.StrikesPerMinute = tonight.lightningPerMinute;
    }
}

[System.Serializable]
public class NightWeatherSettings
{
    [Tooltip("Rain drops per second. 0 = a dry night. About 2200 is steady rain; 4000 and up is a downpour.")]
    [Min(0f)] public float rainPerSecond = 1200f;
    [Tooltip("How hard the wind pushes the rain sideways, in metres per second. 0 = straight down.")]
    public float wind = 2f;
    [Tooltip("Lightning strikes per minute, on average. 0 = no lightning.")]
    [Min(0f)] public float lightningPerMinute = 2f;
}
