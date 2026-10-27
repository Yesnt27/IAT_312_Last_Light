using System.Collections.Generic;
using UnityEngine;

// Anything that should freeze the player (dialogue, UI, cutscenes, fades) adds itself here.
public static class PlayerLock
{
    private static readonly HashSet<object> holders = new HashSet<object>();

    public static bool IsLocked => holders.Count > 0;

    public static void Add(object holder)
    {
        holders.Add(holder);
    }

    public static void Remove(object holder)
    {
        holders.Remove(holder);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        holders.Clear();
    }
}
