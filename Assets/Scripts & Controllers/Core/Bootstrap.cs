using UnityEngine;

// Spawns the Systems prefab before the first scene loads, so Play works from any scene.
public static class Bootstrap
{
    private const string SystemsPrefabName = "Systems";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateSystems()
    {
        if (GameState.Instance != null) return;

        GameObject prefab = Resources.Load<GameObject>(SystemsPrefabName);
        if (prefab == null)
        {
            Debug.LogError("Bootstrap: no Systems prefab found in a Resources folder.");
            return;
        }

        GameObject systems = Object.Instantiate(prefab);
        systems.name = SystemsPrefabName;
    }
}
