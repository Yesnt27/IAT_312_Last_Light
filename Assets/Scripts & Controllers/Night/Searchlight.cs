using System.Collections.Generic;
using UnityEngine;

// Put on a Spot Light (the lighthouse beam). Anything inside its cone and range counts as lit:
// ships use this to know when the binoculars may select them.
[RequireComponent(typeof(Light))]
public class Searchlight : MonoBehaviour
{
    public static readonly List<Searchlight> All = new List<Searchlight>();

    private Light beam;

    private void Awake()
    {
        beam = GetComponent<Light>();
    }

    private void OnEnable()
    {
        All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
    }

    public bool Lights(Bounds bounds)
    {
        if (!beam.enabled) return false;

        Vector3 toTarget = bounds.center - transform.position;
        float distance = toTarget.magnitude;
        if (distance > beam.range) return false;

        float halfSize = Mathf.Atan2(bounds.extents.magnitude * 0.7f, distance) * Mathf.Rad2Deg;
        return Vector3.Angle(transform.forward, toTarget) <= beam.spotAngle * 0.5f + halfSize;
    }

    public static bool AnyLights(Bounds bounds)
    {
        foreach (Searchlight light in All)
        {
            if (light.Lights(bounds)) return true;
        }
        return false;
    }
}
