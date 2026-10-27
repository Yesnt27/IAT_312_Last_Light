using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    private bool isLoading;

    public bool IsLoading => isLoading;

    private void Awake()
    {
        Instance = this;
    }

    public void LoadScene(string sceneName, string spawnId = null)
    {
        if (isLoading) return;
        StartCoroutine(LoadRoutine(sceneName, spawnId));
    }

    private IEnumerator LoadRoutine(string sceneName, string spawnId)
    {
        isLoading = true;
        PlayerLock.Add(this);

        yield return ScreenFader.Instance.FadeOut();

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        while (!load.isDone) yield return null;

        PlaceAtSpawn(spawnId);

        yield return ScreenFader.Instance.FadeIn();

        PlayerLock.Remove(this);
        isLoading = false;
    }

    private void PlaceAtSpawn(string spawnId)
    {
        if (string.IsNullOrEmpty(spawnId)) return;

        SpawnPoint spawn = SpawnPoint.Find(spawnId);
        GameObject player = GameObject.FindWithTag("Player");
        if (spawn == null || player == null)
        {
            Debug.LogWarning($"SceneController: couldn't place the player at spawn '{spawnId}'.");
            return;
        }

        player.transform.position = spawn.transform.position;
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.position = spawn.transform.position;
            body.linearVelocity = Vector2.zero;
        }

        CameraManager cameraManager = FindAnyObjectByType<CameraManager>();
        if (cameraManager != null) cameraManager.SnapToPlayer();
    }
}
