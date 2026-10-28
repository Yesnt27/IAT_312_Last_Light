using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Loads scenes behind a fade and puts the player at a spawn point.
// Scenes don't contain a player: one is spawned whenever a scene with spawn points loads.
public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    private bool isLoading;
    private string pendingSpawnId;

    public bool IsLoading => isLoading;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(string sceneName, string spawnId = null, Action onLoaded = null)
    {
        if (isLoading) return;
        StartCoroutine(LoadRoutine(sceneName, spawnId, onLoaded));
    }

    private IEnumerator LoadRoutine(string sceneName, string spawnId, Action onLoaded)
    {
        isLoading = true;
        PlayerLock.Add(this);

        yield return ScreenFader.Instance.FadeOut();

        pendingSpawnId = spawnId;
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        while (!load.isDone) yield return null;

        yield return ScreenFader.Instance.FadeIn();

        PlayerLock.Remove(this);
        isLoading = false;
        onLoaded?.Invoke();
    }

    // Runs for every scene load, including the scene you press Play in.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string spawnId = pendingSpawnId;
        pendingSpawnId = null;
        PlacePlayer(spawnId);
    }

    private void PlacePlayer(string spawnId)
    {
        GameObject player = GameObject.FindWithTag("Player");
        bool hasRequest = !string.IsNullOrEmpty(spawnId);

        // A player already placed in the scene by hand stays put unless a door sent us here.
        if (player != null && !hasRequest) return;

        SpawnPoint spawn = hasRequest ? SpawnPoint.Find(spawnId) : SpawnPoint.FindDefault();
        if (spawn == null)
        {
            if (hasRequest) Debug.LogWarning($"SceneController: no spawn point '{spawnId}' in this scene.");
            return;
        }

        Vector3 position = spawn.transform.position;
        if (player == null)
        {
            player = Instantiate(playerPrefab, position, Quaternion.identity);
            player.name = playerPrefab.name;
        }

        player.transform.position = position;
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.position = position;
            body.linearVelocity = Vector2.zero;
        }

        CameraManager cameraManager = FindAnyObjectByType<CameraManager>();
        if (cameraManager != null) cameraManager.SnapToPlayer();
    }
}
