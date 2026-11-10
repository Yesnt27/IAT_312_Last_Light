using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    public Camera mainCamera;
    public SpriteRenderer spriteRenderer;
    public float normalSize;
    public float padding = 0f;
    public float zoomSpeed = 4f;

    private float targetSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        normalSize = mainCamera.orthographicSize;
        targetSize = normalSize;
        
    }

    // Update is called once per frame
    void Update()
    {
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            targetSize = calculateZoomedOutSize(spriteRenderer);
            mainCamera.transform.position = new Vector3(spriteRenderer.bounds.center.x, spriteRenderer.bounds.center.y - 100, mainCamera.transform.position.z);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            targetSize = normalSize;
        }
    }

    private float calculateZoomedOutSize(SpriteRenderer backgroundRenderer)
    {
       Vector2 spriteSize = spriteRenderer.bounds.size;

        // Calculate ortho size needed to fit height vs. width
        float verticalSize = (spriteSize.y * 0.3f) + padding;
        float horizontalSize = ((spriteSize.x * 0.3f) + padding) / mainCamera.aspect;

        // Pick whichever is larger so the entire image fits on screen
        return Mathf.Max(verticalSize, horizontalSize);
    }
}
