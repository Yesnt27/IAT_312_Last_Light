using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// On the Systems prefab: dims each scene's Global Light 2D while GameState says the power is out.
// Scenes listed in Unaffected Scenes (Outside) keep their normal light.
public class PowerLighting : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float outageIntensity = 0.3f;
    [SerializeField] private string[] unaffectedScenes = { "Outside", "Title Screen", "Introduction" };

    private Light2D globalLight;
    private float normalIntensity = 1f;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        GameState.Instance.Changed += Refresh;
        FindGlobalLight();
        Refresh();
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null) GameState.Instance.Changed -= Refresh;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindGlobalLight();
        Refresh();
    }

    private void FindGlobalLight()
    {
        globalLight = null;
        foreach (Light2D light in FindObjectsByType<Light2D>())
        {
            if (light.lightType != Light2D.LightType.Global) continue;
            globalLight = light;
            normalIntensity = light.intensity;
            return;
        }
    }

    private void Refresh()
    {
        if (globalLight == null || GameState.Instance == null) return;

        bool affected = System.Array.IndexOf(unaffectedScenes, SceneManager.GetActiveScene().name) < 0;
        globalLight.intensity = affected && !GameState.Instance.PowerOn ? outageIntensity : normalIntensity;
    }
}
