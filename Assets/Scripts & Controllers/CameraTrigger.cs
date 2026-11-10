using UnityEngine;
using UnityEngine.Events;

// Zooms the camera out while the player is inside this trigger. Sprite Renderer = the scene's whole background
// (Outside: Outside_0): the zoom never goes past its edges, and CameraManager keeps the camera inside it while zoomed.
// On Zoom Out / On Zoom In run when the player walks in / out (e.g. fade music or a title text in and out).
public class CameraTrigger : MonoBehaviour
{
    public Camera mainCamera;
    public SpriteRenderer spriteRenderer;
    public float normalSize;
    [Tooltip("World units kept back from the background's edges. 0 = zoom out as far as the background allows.")]
    public float padding = 0f;
    public float zoomSpeed = 4f;

    [Header("Events")]
    public UnityEvent onZoomOut;
    public UnityEvent onZoomIn;

    private float targetSize;
    private CameraManager cameraManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        normalSize = mainCamera.orthographicSize;
        targetSize = normalSize;
        cameraManager = mainCamera.GetComponent<CameraManager>();
    }

    // Update is called once per frame
    void Update()
    {
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);

        // Back to normal: hand the camera back to the RoomBounds only once the zoom has finished.
        if (targetSize == normalSize && Mathf.Abs(mainCamera.orthographicSize - normalSize) < 0.01f)
        {
            mainCamera.orthographicSize = normalSize;
            if (cameraManager != null) cameraManager.ClearBoundsOverride();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"{name}: CameraTrigger has no Sprite Renderer, so it can't zoom. Assign the scene's background sprite.", this);
                return;
            }

            targetSize = calculateZoomedOutSize(spriteRenderer);
            Bounds background = spriteRenderer.bounds;
            if (cameraManager != null) cameraManager.SetBoundsOverride(new Rect(background.min, background.size));
            onZoomOut?.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            targetSize = normalSize;
            onZoomIn?.Invoke();
        }
    }

    // The largest size that still fits inside the background, so the view never shows past the map.
    private float calculateZoomedOutSize(SpriteRenderer backgroundRenderer)
    {
        Vector2 spriteSize = backgroundRenderer.bounds.size;

        float verticalSize = spriteSize.y / 2f - padding;
        float horizontalSize = (spriteSize.x / 2f - padding) / mainCamera.aspect;

        // Pick whichever is smaller so the view stays inside the image
        return Mathf.Max(normalSize, Mathf.Min(verticalSize, horizontalSize));
    }
}
